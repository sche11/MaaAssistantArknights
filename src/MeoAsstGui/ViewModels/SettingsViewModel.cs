// MeoAsstGui - A part of the MeoAssistantArknights project
// Copyright (C) 2021 MistEO and Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Notification.Wpf.Constants;
using Notification.Wpf.Controls;
using Stylet;
using StyletIoC;

namespace MeoAsstGui
{
    public class SettingsViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IContainer _container;

        [DllImport("MeoAssistant.dll")] private static extern IntPtr AsstGetVersion();

        private readonly string _versionInfo = "版本号：" + Marshal.PtrToStringAnsi(AsstGetVersion());

        public string VersionInfo
        {
            get { return _versionInfo; }
        }

        public SettingsViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            DisplayName = "设置";

            _listTitle.Add("基建设置");
            _listTitle.Add("肉鸽设置");
            _listTitle.Add("自动公招");
            _listTitle.Add("信用商店");
            _listTitle.Add("企鹅数据");
            _listTitle.Add("连接设置");
            _listTitle.Add("启动设置");
            _listTitle.Add("定时执行");
            _listTitle.Add("界面设置");
            _listTitle.Add("软件更新");
            _listTitle.Add("关于我们");

            InfrastInit();

            var trayObj = _container.Get<TrayIcon>();
            trayObj.SetVisible(UseTray);
            trayObj.SetMinimizeToTaskbar(MinimizeToTray);
            Bootstrapper.SetTrayIconInSettingsViewModel(this);
        }

        private List<string> _listTitle = new List<string>();

        public List<string> ListTitle
        {
            get { return _listTitle; }
            set
            {
                SetAndNotify(ref _listTitle, value);
            }
        }

        private void InfrastInit()
        {
            /* 基建设置 */
            string[] facility_list = new string[] { "制造站", "贸易站", "控制中枢", "发电站", "会客室", "办公室", "宿舍" };

            var temp_order_list = new List<DragItemViewModel>(new DragItemViewModel[facility_list.Length]);
            for (int i = 0; i != facility_list.Length; ++i)
            {
                var facility = facility_list[i];
                int order = -1;
                bool parsed = int.TryParse(ViewStatusStorage.Get("Infrast.Order." + facility, "-1"), out order);

                if (!parsed || order < 0)
                {
                    temp_order_list[i] = new DragItemViewModel(facility, "Infrast.");
                }
                else
                {
                    temp_order_list[order] = new DragItemViewModel(facility, "Infrast.");
                }
            }
            InfrastItemViewModels = new ObservableCollection<DragItemViewModel>(temp_order_list);

            FacilityKey.Add("宿舍", "Dorm");
            FacilityKey.Add("制造站", "Mfg");
            FacilityKey.Add("贸易站", "Trade");
            FacilityKey.Add("发电站", "Power");
            FacilityKey.Add("会客室", "Reception");
            FacilityKey.Add("办公室", "Office");
            FacilityKey.Add("控制中枢", "Control");

            UsesOfDronesList = new List<CombData>
            {
                new CombData { Display = "不使用无人机", Value = "_NotUse" },
                new CombData { Display = "贸易站-龙门币", Value = "Money" },
                new CombData { Display = "贸易站-合成玉", Value = "SyntheticJade" },
                new CombData { Display = "制造站-经验书", Value = "CombatRecord" },
                new CombData { Display = "制造站-赤金", Value = "PureGold" },
                new CombData { Display = "制造站-源石碎片", Value = "OriginStone" },
                new CombData { Display = "制造站-芯片组", Value = "Chip" }
            };

            ConnectConfigList = new List<CombData>
            {
                new CombData { Display = "通用模式", Value = "General" },
                new CombData { Display = "蓝叠模拟器", Value = "BlueStacks" },
                new CombData { Display = "MuMu模拟器", Value = "MuMuEmulator" },
                new CombData { Display = "雷电模拟器", Value = "LDPlayer" },
                new CombData { Display = "夜神模拟器", Value = "Nox" },
                new CombData { Display = "逍遥模拟器", Value = "XYAZ" },
                new CombData { Display = "WSA 旧版本", Value = "WSA" },
                new CombData { Display = "兼容模式", Value = "Compatible" },
            };

            _dormThresholdLabel = "基建工作心情阈值：" + _dormThreshold + "%";

            RoguelikeModeList = new List<CombData>
            {
                new CombData { Display = "刷蜡烛，尽可能稳定的打更多层数", Value = "0" },
                new CombData { Display = "刷源石锭，第一层商店后直接退出", Value = "1" },
                //new CombData { Display = "两者兼顾，投资过后退出", Value = "2" } // 弃用
                //new CombData { Display = "尝试通关，贪B策略", Value = "3" },  // 开发中
            };

            ClientTypeList = new List<CombData>
            {
                new CombData { Display = "不选择", Value = "" },
                new CombData { Display = "官服", Value = "Official" },
                new CombData { Display = "Bilibili服", Value = "Bilibili" },
                new CombData { Display = "国际服 (YoStarEN)", Value = "YoStarEN" },
                new CombData { Display = "日服 (YoStarJP)", Value = "YoStarJP" },
                new CombData { Display = "韩服 (YoStarKR)", Value = "YoStarKR" },
                new CombData { Display = "繁中服 (txwy)", Value = "txwy" }
            };

            InverseClearModeList = new List<CombData>
            {
                new CombData { Display = "清空", Value = "Clear" },
                new CombData { Display = "反选", Value = "Inverse" },
                new CombData { Display = "可切换", Value = "ClearInverse" }
            };
        }

        private bool _idle = true;

        public bool Idle
        {
            get { return _idle; }
            set
            {
                SetAndNotify(ref _idle, value);
            }
        }

        /* 启动设置 */
        private bool _startSelf = StartSelfModel.CheckStart();

        public bool StartSelf
        {
            get { return _startSelf; }
            set
            {
                SetAndNotify(ref _startSelf, value);
                StartSelfModel.SetStart(value);
            }
        }

        private bool _runDirectly = Convert.ToBoolean(ViewStatusStorage.Get("Start.RunDirectly", bool.FalseString));

        public bool RunDirectly
        {
            get { return _runDirectly; }
            set
            {
                SetAndNotify(ref _runDirectly, value);
                ViewStatusStorage.Set("Start.RunDirectly", value.ToString());
            }
        }

        private bool _startEmulator = Convert.ToBoolean(ViewStatusStorage.Get("Start.StartEmulator", bool.FalseString));

        public bool StartEmulator
        {
            get { return _startEmulator; }
            set
            {
                SetAndNotify(ref _startEmulator, value);
                ViewStatusStorage.Set("Start.StartEmulator", value.ToString());
                if (ClientType == "" && Idle)
                {
                    ClientType = "Official";
                }
            }
        }

        private string _emulatorPath = ViewStatusStorage.Get("Start.EmulatorPath", string.Empty);

        public string EmulatorPath
        {
            get { return _emulatorPath; }
            set
            {
                SetAndNotify(ref _emulatorPath, value);
                ViewStatusStorage.Set("Start.EmulatorPath", value);
            }
        }

        private string _emulatorAddCommand = ViewStatusStorage.Get("Start.EmulatorAddCommand", string.Empty);

        public string EmulatorAddCommand
        {
            get { return _emulatorAddCommand; }
            set
            {
                SetAndNotify(ref _emulatorAddCommand, value);
                ViewStatusStorage.Set("Start.EmulatorAddCommand", value);
            }
        }

        private string _emulatorWaitSeconds = ViewStatusStorage.Get("Start.EmulatorWaitSeconds", "60");

        public string EmulatorWaitSeconds
        {
            get { return _emulatorWaitSeconds; }
            set
            {
                SetAndNotify(ref _emulatorWaitSeconds, value);
                ViewStatusStorage.Set("Start.EmulatorWaitSeconds", value);
            }
        }

        public void TryToStartEmulator(bool manual = false)
        {
            if ((EmulatorPath.Length == 0
                || !File.Exists(EmulatorPath))
                || !(StartEmulator
                || manual))
            {
                return;
            }
            if (EmulatorAddCommand.Length != 0)
            {
                Process.Start(EmulatorPath, EmulatorAddCommand);
            }
            else
            {
                Process.Start(EmulatorPath);
            }
            int delay = 0;
            if (!int.TryParse(EmulatorWaitSeconds, out delay))
            {
                delay = 60;
            }
            Thread.Sleep(delay * 1000);
        }

        public void SelectEmulatorExec()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Filter = "可执行文件|*.exe;*.bat;*.lnk";

            if (dialog.ShowDialog() == true)
            {
                EmulatorPath = dialog.FileName;
            }
        }

        private string _clientType = ViewStatusStorage.Get("Start.ClientType", "");

        public string ClientType
        {
            get { return _clientType; }
            set
            {
                SetAndNotify(ref _clientType, value);
                ViewStatusStorage.Set("Start.ClientType", value);
            }
        }

        /* 基建设置 */
        public Dictionary<string, string> FacilityKey = new Dictionary<string, string>();
        public ObservableCollection<DragItemViewModel> InfrastItemViewModels { get; set; }

        public List<CombData> UsesOfDronesList { get; set; }
        public List<CombData> RoguelikeModeList { get; set; }
        public List<CombData> ClientTypeList { get; set; }
        public List<CombData> ConnectConfigList { get; set; }
        public List<CombData> InverseClearModeList { get; set; }

        private int _dormThreshold = Convert.ToInt32(ViewStatusStorage.Get("Infrast.DormThreshold", "30"));

        public int DormThreshold
        {
            get { return _dormThreshold; }
            set
            {
                DormThresholdLabel = "基建工作心情阈值：" + _dormThreshold + "%";
                SetAndNotify(ref _dormThreshold, value);
                ViewStatusStorage.Set("Infrast.DormThreshold", value.ToString());
            }
        }

        private string _dormThresholdLabel;

        public string DormThresholdLabel
        {
            get { return _dormThresholdLabel; }
            set
            {
                SetAndNotify(ref _dormThresholdLabel, value);
            }
        }

        public List<string> GetInfrastOrderList()
        {
            var orderList = new List<string>();
            foreach (var item in InfrastItemViewModels)
            {
                if (item.IsChecked == false)
                {
                    continue;
                }

                orderList.Add(FacilityKey[item.Name]);
            }
            return orderList;
        }

        public void SaveInfrastOrderList()
        {
            for (int i = 0; i < InfrastItemViewModels.Count; i++)
            {
                ViewStatusStorage.Set("Infrast.Order." + InfrastItemViewModels[i].Name, i.ToString());
            }
        }

        private string _usesOfDrones = ViewStatusStorage.Get("Infrast.UsesOfDrones", "Money");

        public string UsesOfDrones
        {
            get { return _usesOfDrones; }
            set
            {
                SetAndNotify(ref _usesOfDrones, value);
                ViewStatusStorage.Set("Infrast.UsesOfDrones", value);
            }
        }

        #region 设置页面列表和滚动视图联动绑定

        private enum NotifyType
        {
            None,
            SelectedIndex,
            ScrollOffset
        }

        private NotifyType _notifySource = NotifyType.None;

        private System.Timers.Timer _resetNotifyTimer;

        private void ResetNotifySource()
        {
            if (_resetNotifyTimer != null)
            {
                _resetNotifyTimer.Stop();
                _resetNotifyTimer.Close();
            }
            _resetNotifyTimer = new System.Timers.Timer(20);
            _resetNotifyTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate (object source, System.Timers.ElapsedEventArgs e)
            {
                _notifySource = NotifyType.None;
            });
            _resetNotifyTimer.AutoReset = false;
            _resetNotifyTimer.Enabled = true;
            _resetNotifyTimer.Start();
        }

        public double ScrollViewportHeight { get; set; }

        public double ScrollExtentHeight { get; set; }

        public List<double> RectangleVerticalOffsetList { get; set; }

        private int _selectedIndex = 0;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                switch (_notifySource)
                {
                    case NotifyType.None:
                        _notifySource = NotifyType.SelectedIndex;
                        SetAndNotify(ref _selectedIndex, value);

                        var isInRange = RectangleVerticalOffsetList != null
                            && RectangleVerticalOffsetList.Count > 0
                            && value < RectangleVerticalOffsetList.Count;

                        if (isInRange)
                            ScrollOffset = RectangleVerticalOffsetList[value];

                        ResetNotifySource();
                        break;

                    case NotifyType.ScrollOffset:
                        SetAndNotify(ref _selectedIndex, value);
                        break;
                }
            }
        }

        private double _scrollOffset = 0;

        public double ScrollOffset
        {
            get { return _scrollOffset; }
            set
            {
                switch (_notifySource)
                {
                    case NotifyType.None:
                        _notifySource = NotifyType.ScrollOffset;
                        SetAndNotify(ref _scrollOffset, value);

                        // 设置 ListBox SelectedIndex 为当前 ScrollOffset 索引
                        var isInRange = RectangleVerticalOffsetList != null && RectangleVerticalOffsetList.Count > 0;

                        if (isInRange)
                        {
                            // 滚动条滚动到底部，返回最后一个 Rectangle 索引
                            if (value + ScrollViewportHeight >= ScrollExtentHeight)
                            {
                                SelectedIndex = RectangleVerticalOffsetList.Count - 1;
                                ResetNotifySource();
                                break;
                            }

                            // 根据出当前 ScrollOffset 选出最后一个在可视范围的 Rectangle 索引
                            var rectangleSelect = RectangleVerticalOffsetList.Select((n, i) => (
                            rectangleAppeared: value >= n,
                            index: i
                            ));

                            var index = rectangleSelect.LastOrDefault(n => n.rectangleAppeared).index;
                            SelectedIndex = index;
                        }

                        ResetNotifySource();
                        break;

                    case NotifyType.SelectedIndex:
                        SetAndNotify(ref _scrollOffset, value);
                        break;
                }
            }
        }

        #endregion 设置页面列表和滚动视图联动绑定

        /* 肉鸽设置 */

        private string _roguelikeMode = ViewStatusStorage.Get("Roguelike.Mode", "0");

        public string RoguelikeMode
        {
            get { return _roguelikeMode; }
            set
            {
                SetAndNotify(ref _roguelikeMode, value);
                ViewStatusStorage.Set("Roguelike.Mode", value);
            }
        }

        private string _roguelikeStartsCount = ViewStatusStorage.Get("Roguelike.StartsCount", "9999999");

        public int RoguelikeStartsCount
        {
            get { return int.Parse(_roguelikeStartsCount); }
            set
            {
                SetAndNotify(ref _roguelikeStartsCount, value.ToString());
                ViewStatusStorage.Set("Roguelike.StartsCount", value.ToString());
            }
        }

        private string _roguelikeInvestsCount = ViewStatusStorage.Get("Roguelike.InvestsCount", "9999999");

        public int RoguelikeInvestsCount
        {
            get { return int.Parse(_roguelikeInvestsCount); }
            set
            {
                SetAndNotify(ref _roguelikeInvestsCount, value.ToString());
                ViewStatusStorage.Set("Roguelike.InvestsCount", value.ToString());
            }
        }

        private string _roguelikeStopWhenInvestmentFull = ViewStatusStorage.Get("Roguelike.StopWhenInvestmentFull", false.ToString());

        public bool RoguelikeStopWhenInvestmentFull
        {
            get { return bool.Parse(_roguelikeStopWhenInvestmentFull); }
            set
            {
                SetAndNotify(ref _roguelikeStopWhenInvestmentFull, value.ToString());
                ViewStatusStorage.Set("Roguelike.StopWhenInvestmentFull", value.ToString());
            }
        }

        /* 信用商店设置 */

        private bool _creditShopping = Convert.ToBoolean(ViewStatusStorage.Get("Mall.CreditShopping", bool.TrueString));

        public bool CreditShopping
        {
            get { return _creditShopping; }
            set
            {
                SetAndNotify(ref _creditShopping, value);
                ViewStatusStorage.Set("Mall.CreditShopping", value.ToString());
            }
        }

        private string _creditFirstList = ViewStatusStorage.Get("Mall.CreditFirstList", "招聘许可 龙门币");

        public string CreditFirstList
        {
            get { return _creditFirstList; }
            set
            {
                SetAndNotify(ref _creditFirstList, value);
                ViewStatusStorage.Set("Mall.CreditFirstList", value);
            }
        }

        private string _creditBlackList = ViewStatusStorage.Get("Mall.CreditBlackList", "碳 家具");

        public string CreditBlackList
        {
            get { return _creditBlackList; }
            set
            {
                SetAndNotify(ref _creditBlackList, value);
                ViewStatusStorage.Set("Mall.CreditBlackList", value);
            }
        }

        /* 定时设置 */

        private bool _timer1 = ViewStatusStorage.Get("Timer.Timer1", bool.FalseString) == bool.TrueString;
        private bool _timer2 = ViewStatusStorage.Get("Timer.Timer2", bool.FalseString) == bool.TrueString;
        private bool _timer3 = ViewStatusStorage.Get("Timer.Timer3", bool.FalseString) == bool.TrueString;
        private bool _timer4 = ViewStatusStorage.Get("Timer.Timer4", bool.FalseString) == bool.TrueString;
        private bool _timer5 = ViewStatusStorage.Get("Timer.Timer5", bool.FalseString) == bool.TrueString;
        private bool _timer6 = ViewStatusStorage.Get("Timer.Timer6", bool.FalseString) == bool.TrueString;
        private bool _timer7 = ViewStatusStorage.Get("Timer.Timer7", bool.FalseString) == bool.TrueString;
        private bool _timer8 = ViewStatusStorage.Get("Timer.Timer8", bool.FalseString) == bool.TrueString;

        private int _timer1hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer1Hour", "0"));
        private int _timer2hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer2Hour", "6"));
        private int _timer3hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer3Hour", "12"));
        private int _timer4hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer4Hour", "18"));
        private int _timer5hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer5Hour", "3"));
        private int _timer6hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer6Hour", "9"));
        private int _timer7hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer7Hour", "15"));
        private int _timer8hour = Int32.Parse(ViewStatusStorage.Get("Timer.Timer8Hour", "21"));

        public bool Timer1
        {
            get { return _timer1; }
            set
            {
                SetAndNotify(ref _timer1, value);
                ViewStatusStorage.Set("Timer.Timer1", value.ToString());
            }
        }

        public bool Timer2
        {
            get { return _timer2; }
            set
            {
                SetAndNotify(ref _timer2, value);
                ViewStatusStorage.Set("Timer.Timer2", value.ToString());
            }
        }

        public bool Timer3
        {
            get { return _timer3; }
            set
            {
                SetAndNotify(ref _timer3, value);
                ViewStatusStorage.Set("Timer.Timer3", value.ToString());
            }
        }

        public bool Timer4
        {
            get { return _timer4; }
            set
            {
                SetAndNotify(ref _timer4, value);
                ViewStatusStorage.Set("Timer.Timer4", value.ToString());
            }
        }

        public bool Timer5
        {
            get { return _timer5; }
            set
            {
                SetAndNotify(ref _timer5, value);
                ViewStatusStorage.Set("Timer.Timer5", value.ToString());
            }
        }

        public bool Timer6
        {
            get { return _timer6; }
            set
            {
                SetAndNotify(ref _timer6, value);
                ViewStatusStorage.Set("Timer.Timer6", value.ToString());
            }
        }

        public bool Timer7
        {
            get { return _timer7; }
            set
            {
                SetAndNotify(ref _timer7, value);
                ViewStatusStorage.Set("Timer.Timer7", value.ToString());
            }
        }

        public bool Timer8
        {
            get { return _timer8; }
            set
            {
                SetAndNotify(ref _timer8, value);
                ViewStatusStorage.Set("Timer.Timer8", value.ToString());
            }
        }

        public int Timer1Hour
        {
            get { return _timer1hour; }
            set
            {
                SetAndNotify(ref _timer1hour, value);
                ViewStatusStorage.Set("Timer.Timer1Hour", value.ToString());
            }
        }

        public int Timer2Hour
        {
            get { return _timer2hour; }
            set
            {
                SetAndNotify(ref _timer2hour, value);
                ViewStatusStorage.Set("Timer.Timer2Hour", value.ToString());
            }
        }

        public int Timer3Hour
        {
            get { return _timer3hour; }
            set
            {
                SetAndNotify(ref _timer3hour, value);
                ViewStatusStorage.Set("Timer.Timer3Hour", value.ToString());
            }
        }

        public int Timer4Hour
        {
            get { return _timer4hour; }
            set
            {
                SetAndNotify(ref _timer4hour, value);
                ViewStatusStorage.Set("Timer.Timer4Hour", value.ToString());
            }
        }

        public int Timer5Hour
        {
            get { return _timer5hour; }
            set
            {
                SetAndNotify(ref _timer5hour, value);
                ViewStatusStorage.Set("Timer.Timer5Hour", value.ToString());
            }
        }

        public int Timer6Hour
        {
            get { return _timer6hour; }
            set
            {
                SetAndNotify(ref _timer6hour, value);
                ViewStatusStorage.Set("Timer.Timer6Hour", value.ToString());
            }
        }

        public int Timer7Hour
        {
            get { return _timer7hour; }
            set
            {
                SetAndNotify(ref _timer7hour, value);
                ViewStatusStorage.Set("Timer.Timer7Hour", value.ToString());
            }
        }

        public int Timer8Hour
        {
            get { return _timer8hour; }
            set
            {
                SetAndNotify(ref _timer8hour, value);
                ViewStatusStorage.Set("Timer.Timer8Hour", value.ToString());
            }
        }

        /* 企鹅数据设置 */

        private string _penguinId = ViewStatusStorage.Get("Penguin.Id", string.Empty);

        public string PenguinId
        {
            get { return _penguinId; }
            set
            {
                SetAndNotify(ref _penguinId, value);
                ViewStatusStorage.Set("Penguin.Id", value);
            }
        }

        /* 自动公招设置 */
        private string _recruitMaxTimes = ViewStatusStorage.Get("AutoRecruit.MaxTimes", "4");

        public string RecruitMaxTimes
        {
            get { return _recruitMaxTimes; }
            set
            {
                SetAndNotify(ref _recruitMaxTimes, value);
                ViewStatusStorage.Set("AutoRecruit.MaxTimes", value);
            }
        }

        private bool _refreshLevel3 = Convert.ToBoolean(ViewStatusStorage.Get("AutoRecruit.RefreshLevel3", bool.TrueString));

        public bool RefreshLevel3
        {
            get { return _refreshLevel3; }
            set
            {
                SetAndNotify(ref _refreshLevel3, value);
                ViewStatusStorage.Set("AutoRecruit.RefreshLevel3", value.ToString());
            }
        }

        private bool _useExpedited = false;

        public bool UseExpedited
        {
            get { return _useExpedited; }
            set { SetAndNotify(ref _useExpedited, value); }
        }

        private bool _notChooseLevel1 = Convert.ToBoolean(ViewStatusStorage.Get("AutoRecruit.NotChooseLevel1", bool.TrueString));

        public bool NotChooseLevel1
        {
            get { return _notChooseLevel1; }
            set
            {
                SetAndNotify(ref _notChooseLevel1, value);
                ViewStatusStorage.Set("AutoRecruit.NotChooseLevel1", value.ToString());
            }
        }

        private bool _chooseLevel3 = Convert.ToBoolean(ViewStatusStorage.Get("AutoRecruit.ChooseLevel3", bool.TrueString));

        public bool ChooseLevel3
        {
            get { return _chooseLevel3; }
            set
            {
                SetAndNotify(ref _chooseLevel3, value);
                ViewStatusStorage.Set("AutoRecruit.ChooseLevel3", value.ToString());
            }
        }

        private bool _chooseLevel4 = Convert.ToBoolean(ViewStatusStorage.Get("AutoRecruit.ChooseLevel4", bool.TrueString));

        public bool ChooseLevel4
        {
            get { return _chooseLevel4; }
            set
            {
                SetAndNotify(ref _chooseLevel4, value);
                ViewStatusStorage.Set("AutoRecruit.ChooseLevel4", value.ToString());
            }
        }

        private bool _chooseLevel5 = Convert.ToBoolean(ViewStatusStorage.Get("AutoRecruit.ChooseLevel5", bool.FalseString));

        public bool ChooseLevel5
        {
            get { return _chooseLevel5; }
            set
            {
                SetAndNotify(ref _chooseLevel5, value);
                ViewStatusStorage.Set("AutoRecruit.ChooseLevel5", value.ToString());
            }
        }

        /* 软件更新设置 */
        private bool _updateBeta = Convert.ToBoolean(ViewStatusStorage.Get("VersionUpdate.UpdateBeta", bool.FalseString));

        public bool UpdateBeta
        {
            get { return _updateBeta; }
            set
            {
                SetAndNotify(ref _updateBeta, value);
                ViewStatusStorage.Set("VersionUpdate.UpdateBeta", value.ToString());
            }
        }

        private bool _updateCheck = Convert.ToBoolean(ViewStatusStorage.Get("VersionUpdate.UpdateCheck", bool.TrueString));

        public bool UpdateCheck
        {
            get { return _updateCheck; }
            set
            {
                SetAndNotify(ref _updateCheck, value);
                ViewStatusStorage.Set("VersionUpdate.UpdateCheck", value.ToString());
            }
        }

        private string _proxy = ViewStatusStorage.Get("VersionUpdate.Proxy", string.Empty);

        public string Proxy
        {
            get { return _proxy; }
            set
            {
                SetAndNotify(ref _proxy, value);
                ViewStatusStorage.Set("VersionUpdate.Proxy", value);
            }
        }

        private bool _useAria2 = Convert.ToBoolean(ViewStatusStorage.Get("VersionUpdate.UseAria2", bool.TrueString));

        public bool UseAria2
        {
            get { return _useAria2; }
            set
            {
                SetAndNotify(ref _useAria2, value);
                ViewStatusStorage.Set("VersionUpdate.UseAria2", value.ToString());
            }
        }

        private bool _autoDownloadUpdatePackage = Convert.ToBoolean(ViewStatusStorage.Get("VersionUpdate.AutoDownloadUpdatePackage", bool.TrueString));

        public bool AutoDownloadUpdatePackage
        {
            get { return _autoDownloadUpdatePackage; }
            set
            {
                SetAndNotify(ref _autoDownloadUpdatePackage, value);
                ViewStatusStorage.Set("VersionUpdate.AutoDownloadUpdatePackage", value.ToString());
            }
        }

        public async void ManualUpdate()
        {
            var updateModle = _container.Get<VersionUpdateViewModel>();
            var task = Task.Run(() =>
            {
                if (!updateModle.CheckAndDownloadUpdate(true))
                {
                    Execute.OnUIThread(() =>
                    {
                        using (var toast = new ToastNotification("已是最新版本~"))
                        {
                            toast.Show();
                        }
                    });
                }
            });
            await task;
        }

        /* 连接设置 */

        private string _connectAddress = ViewStatusStorage.Get("Connect.Address", string.Empty);

        public string ConnectAddress
        {
            get { return _connectAddress; }
            set
            {
                SetAndNotify(ref _connectAddress, value);
                ViewStatusStorage.Set("Connect.Address", value);
                UpdateWindowTitle(); /* 每次修改连接地址时更新WIndowTitle */
            }
        }

        private string _adbPath = ViewStatusStorage.Get("Connect.AdbPath", string.Empty);

        public string AdbPath
        {
            get { return _adbPath; }
            set
            {
                SetAndNotify(ref _adbPath, value);
                ViewStatusStorage.Set("Connect.AdbPath", value);
            }
        }

        private string _connectConfig = ViewStatusStorage.Get("Connect.ConnectConfig", "General");

        public string ConnectConfig
        {
            get { return _connectConfig; }
            set
            {
                SetAndNotify(ref _connectConfig, value);
                ViewStatusStorage.Set("Connect.ConnectConfig", value);
            }
        }

        public readonly Dictionary<string, List<string>> DefaultAddress = new Dictionary<string, List<string>>
            {
                { "General", new List<string>{ "" } },
                { "BlueStacks", new List<string>{ "127.0.0.1:5555", "127.0.0.1:5556", "127.0.0.1:5565", "127.0.0.1:5554" } },
                { "MuMuEmulator", new List<string>{ "127.0.0.1:7555" } },
                { "LDPlayer", new List<string>{ "emulator-5554", "127.0.0.1:5555", "127.0.0.1:5556", "127.0.0.1:5554" } },
                { "Nox", new List<string>{ "127.0.0.1:62001", "127.0.0.1:59865" } },
                { "XYAZ", new List<string>{ "127.0.0.1:21503"  } },
                { "WSA", new List<string>{ "127.0.0.1:58526" } },
            };

        public bool RefreshAdbConfig(ref string error)
        {
            var adapter = new WinAdapter();
            List<string> emulators;
            try
            {
                emulators = adapter.RefreshEmulatorsInfo();
            }
            catch (Exception)
            {
                error = "检测模拟器出错\n请使用管理员权限打开本软件\n或手动设置连接";
                return false;
            }
            if (emulators.Count == 0)
            {
                error = "未检测到任何模拟器\n请使用管理员权限打开本软件\n或手动设置连接";
                return false;
            }
            else if (emulators.Count > 1)
            {
                error = "检测到多个模拟器\n请关闭不需要的模拟器\n或手动设置连接";
                return false;
            }
            ConnectConfig = emulators.First();
            AdbPath = adapter.GetAdbPathByEmulatorName(ConnectConfig) ?? AdbPath;
            if (ConnectAddress.Length == 0)
            {
                var addresses = adapter.GetAdbAddresses(AdbPath);
                // 傻逼雷电已经关掉了，用别的 adb 还能检测出来这个端口 device
                if (addresses.Count == 1 && addresses.First() != "emulator-5554")
                {
                    ConnectAddress = addresses.First();
                }
                else if (addresses.Count > 1)
                {
                    foreach (var address in addresses)
                    {
                        if (address == "emulator-5554" && ConnectConfig != "LDPlayer")
                        {
                            continue;
                        }
                        ConnectAddress = address;
                        break;
                    }
                }

                if (ConnectAddress.Length == 0)
                {
                    ConnectAddress = DefaultAddress[ConnectConfig][0];
                }
            }

            return true;
        }

        public void SelectFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Filter = "adb程序|*.exe";

            if (dialog.ShowDialog() == true)
            {
                AdbPath = dialog.FileName;
            }
        }

        /*  标题栏显示模拟器名称和IP端口  */

        public void UpdateWindowTitle()
        {
            var rvm = (RootViewModel)this.Parent;
            string ConnectConfigName = "";
            foreach (CombData data in ConnectConfigList)
            {
                if (data.Value == ConnectConfig)
                {
                    ConnectConfigName = data.Display;
                }
            }
            rvm.WindowTitle = string.Format("MaaAssistantArknights - {0} ({1})", ConnectConfigName, ConnectAddress);
        }

        private string _bluestacksConfig = ViewStatusStorage.Get("Bluestacks.Config.Path", string.Empty);

        public void TryToSetBlueStacksHyperVAddress()
        {
            if (_bluestacksConfig.Length == 0)
            {
                return;
            }
            if (!File.Exists(_bluestacksConfig))
            {
                ViewStatusStorage.Set("Bluestacks.Config.Error", "File not exists");
                return;
            }

            var all_lines = File.ReadAllLines(_bluestacksConfig);
            foreach (var line in all_lines)
            {
                if (line.StartsWith("bst.instance.Nougat64.status.adb_port"))
                {
                    var sp = line.Split('"');
                    ConnectAddress = "127.0.0.1:" + sp[1];
                }
            }
        }

        /* 界面设置 */
        public bool UseTray = true;

        //private bool _usetray = Convert.ToBoolean(ViewStatusStorage.Get("GUI.UseTray", bool.TrueString));

        //public bool UseTray
        //{
        //    get { return _usetray; }
        //    set
        //    {
        //        SetAndNotify(ref _usetray, value);
        //        ViewStatusStorage.Set("GUI.UseTray", value.ToString());
        //        var trayObj = _container.Get<TrayIcon>();
        //        trayObj.SetVisible(value);

        //        if (!Convert.ToBoolean(value))
        //        {
        //            MinimizeToTray = false;
        //        }
        //    }
        //}

        private bool _minimizeToTray = Convert.ToBoolean(ViewStatusStorage.Get("GUI.MinimizeToTray", bool.FalseString));

        public bool MinimizeToTray
        {
            get { return _minimizeToTray; }
            set
            {
                SetAndNotify(ref _minimizeToTray, value);
                ViewStatusStorage.Set("GUI.MinimizeToTray", value.ToString());
                var trayObj = _container.Get<TrayIcon>();
                trayObj.SetMinimizeToTaskbar(value);
            }
        }

        private bool _useNotify = Convert.ToBoolean(ViewStatusStorage.Get("GUI.UseNotify", bool.TrueString));

        public bool UseNotify
        {
            get { return _useNotify; }
            set
            {
                SetAndNotify(ref _useNotify, value);
                ViewStatusStorage.Set("GUI.UseNotify", value.ToString());
                if (value)
                {
                    Execute.OnUIThread(() =>
                    {
                        using (var toast = new ToastNotification("通知演示"))
                        {
                            toast.Show();
                        }
                    });
                }
            }
        }

        private bool _hideUnavailableStage = Convert.ToBoolean(ViewStatusStorage.Get("GUI.HideUnavailableStage", bool.TrueString));

        public bool HideUnavailableStage
        {
            get { return _hideUnavailableStage; }
            set
            {
                SetAndNotify(ref _hideUnavailableStage, value);
                ViewStatusStorage.Set("GUI.HideUnavailableStage", value.ToString());
                var mainModel = _container.Get<TaskQueueViewModel>();
                mainModel.UpdateStageList();
            }
        }

        private enum InverseClearType
        {
            Clear,
            Inverse,
            ClearInverse
        };

        private InverseClearType _inverseClearMode =
            InverseClearType.TryParse(ViewStatusStorage.Get("GUI.InverseClearMode", InverseClearType.Clear.ToString()),
                out InverseClearType temp)
            ? temp : InverseClearType.Clear;

        public string InverseClearMode
        {
            get { return _inverseClearMode.ToString(); }
            set
            {
                bool parsed = InverseClearType.TryParse(value, out InverseClearType tempEnumValue);
                if (!parsed)
                {
                    return;
                }
                SetAndNotify(ref _inverseClearMode, tempEnumValue);
                ViewStatusStorage.Set("GUI.InverseClearMode", value);

                var taskQueueModel = _container.Get<TaskQueueViewModel>();
                switch (tempEnumValue)
                {
                    case InverseClearType.Clear:
                        taskQueueModel.InverseMode = false;
                        taskQueueModel.InverseShowVisibility = Visibility.Collapsed;
                        taskQueueModel.NotInverseShowVisibility = Visibility.Visible;
                        taskQueueModel.SelectedAllWidth = 90;
                        break;

                    case InverseClearType.Inverse:
                        taskQueueModel.InverseMode = true;
                        taskQueueModel.InverseShowVisibility = Visibility.Collapsed;
                        taskQueueModel.NotInverseShowVisibility = Visibility.Visible;
                        taskQueueModel.SelectedAllWidth = 90;
                        break;

                    case InverseClearType.ClearInverse:
                        taskQueueModel.InverseShowVisibility = Visibility.Visible;
                        taskQueueModel.NotInverseShowVisibility = Visibility.Collapsed;
                        taskQueueModel.SelectedAllWidth = TaskQueueViewModel.SelectedAllWidthWhenBoth;
                        break;
                }
            }
        }
    }
}
