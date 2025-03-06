#include "DrGrandetTaskPlugin.h"

#include "Config/TaskData.h"
#include "Controller/Controller.h"
#include "Task/Fight/MedicineCounterTaskPlugin.h"
#include "Utils/Logger.hpp"
#include "Vision/OCRer.h"

#include <regex>

bool asst::DrGrandetTaskPlugin::verify(AsstMsg msg, const json::value& details) const
{
    if (msg != AsstMsg::SubTaskStart || details.get("subtask", std::string()) != "ProcessTask") {
        return false;
    }

    const std::string task = details.at("details").at("task").as_string();
    if (task == "Fight@StoneConfirm") {
        return true;
    }
    else {
        return false;
    }
}

bool asst::DrGrandetTaskPlugin::_run()
{
    LogTraceFunction;

    const auto& image = ctrler()->get_image();

    auto sanity_target = MedicineCounterTaskPlugin::get_target_of_sanity(image);
    auto sanity_max = MedicineCounterTaskPlugin::get_maximun_of_sanity(image);
    if (!sanity_target || !sanity_max) [[unlikely]] {
        return false;
    }

    if (*sanity_target < *sanity_max) {
        return true;
    }

    int cur_ms = analyze_time_left(image);
    if (cur_ms < 0) {
        return false;
    }
    auto task_ptr = Task.get("DrGrandetUseOriginiums");
    int threshold = task_ptr->post_delay;
    if (threshold <= cur_ms) {
        return true;
    }

    int delay = cur_ms - task_ptr->pre_delay;
    if (delay > 0) {
        sleep(delay);
    }

    // 直接 return false, 下次进来会重试的
    return false;
}

int asst::DrGrandetTaskPlugin::analyze_time_left(const cv::Mat& image)
{
    LogTraceFunction;

    OCRer analyzer(image);
    analyzer.set_task_info("DrGrandetUseOriginiums");
    analyzer.set_replace(Task.get<OcrTaskInfo>("NumberOcrReplace")->replace_map);
    // 这里是汉字和数字混合的，用不了单独的en模型
    analyzer.set_use_char_model(false);

    if (!analyzer.analyze()) {
        Log.warn(__FUNCTION__, "unable to analyze time");
        return -1;
    }
    auto text = analyzer.get_result().front().text;

    auto regex = std::regex(R"((\d):(\d?)(\d?))");
    std::smatch match;
    if (!std::regex_search(text, match, regex)) {
        Log.warn(__FUNCTION__, "Unable to match time:", text);
        return -1;
    }
    std::string time = match.str(0);
    Log.info("Time:", time);

    int min = std::stoi(match.str(1));
    int sec1 = match.str(2).empty() ? 5 : std::stoi(match.str(2));
    int sec2 = match.str(3).empty() ? 9 : std::stoi(match.str(3));
    int millis = (min * 60 + sec1 * 10 + sec2) * 1000;
    if (millis < 0 || millis > 6 * 60 * 1000) {
        Log.warn("Invalid time:", millis);
        return -1;
    }
    Log.info("Time left ms:", millis);
    return millis;
}
