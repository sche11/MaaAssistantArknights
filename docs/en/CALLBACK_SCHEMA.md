## Callback Schema

### Prototype

```c++
typedef void(ASST_CALL* AsstCallback)(int msg, const char* details, void* custom_arg);
```

### Overview

- `int msg`  
    The message type

    ```c++
    enum class AsstMsg
    {
        /* Global Info */
        InternalError = 0,          // Internal error
        InitFailed,                 // Initialization failure
        ConnectionInfo,             // Connection info
        AllTasksCompleted,          // Whether all tasks have been completed
        /* TaskChain Info */
        TaskChainError = 10000,     // Errors in task chain execution/recognition
        TaskChainStart,             // Task chain starts
        TaskChainCompleted,         // Task chain completes
        TaskChainExtraInfo,         // Extra information about task chain
        /* SubTask Info */
        SubTaskError = 20000,       // Errors in subtask execution/recognition
        SubTaskStart,               // Subtask starts
        SubTaskCompleted,           // Subtask completes
        SubTaskExtraInfo            // Extra information about subtask
    };
    ```

- `const char* details`  
    Message details, JSON. See also: [Field Description](#field-description)
- `void* custom_arg`  
    Custom arguments of the caller, will pass the `custom_arg` argument of `AsstCreateEx` interface. C-like languages can pass the `this` pointer with it.

### Field Description

#### InternalError

Todo

#### InitFailed

```jsonc
{
    "what": string,     // Error type
    "why": string,      // Error reason
    "details": object   // Error details
}
```

### ConnectionInfo

```jsonc
{
    "what": string,     // Info type
    "why": string,      // Info reason
    "uuid": string,     // UUID (empty when connection fails)
    "details": {
        "adb": string,     // adb_path argument of AsstConnect interface
        "address": string, // address argument of AsstConnect interface
        "config": string   // config argument of AsstConnect interface
    }
      
}
```

#### Commonly-used `What` Field Values

- `ConnectFailed`  
    Connection failed.
- `Connected`  
    Connected. Note that the `uuid` field is empty now (and will be retrieved in the next step)
- `UuidGot`  
    UUID has been retrieved.
- `UnsupportedResolution`  
    The resolution is not supported.
- `ResolutionError`
    Cannot get the resolution.
- `Reconnecting`
    Disconnected (adb/emulator crashed), and reconnecting
- `Reconnected`
    Disconnected (adb/emulator crashed), and reconnected successfully
- `Disconnect`
    Disconnected (adb/emulator crashed), and failed to reconnect

#### AllTasksCompleted

```jsonc
{
    "taskchain": string,            // Last task chain
    "uuid": string                  // UUID
    "runned_tasks": [               // ID of the last task run
        int,
        ...
    ]
}
```

##### Commonly-used `taskchain` Field Values

- `StartUp`  
    Start-up.
- `Fight`  
    Fighting.
- `Mall`  
    Mall.
- `Recruit`  
    Auto-recruitment.
- `RecruitCalc`  
    Recruitment calculation.
- `Infrast`  
    Infrastructure.
- `Roguelike`  
    Roguelike.
- `Debug`  
    Debug.

#### Information Related to TaskChain

```jsonc
{
    "taskchain": string,            // Current task chain
    "taskid": int,                  // Current task ID
    "uuid": string                  // UUID
}
```

#### TaskChainExtraInfo

Todo

#### Information Related to SubTask

```jsonc
{
    "subtask": string,             // Subtask name
    "class": string,               // Subtask class
    "taskchain": string,           // Current task chain
    "taskid": int,                 // Current task ID
    "details": object,             // Details
    "uuid": string                 // UUID
}
```

##### Commonly-used `subtask` Field Values

- `ProcessTask`  

    ```jsonc
    // Example of corresponding details field
    {
        "task": "StartButton2",     // Task name
        "action": 512,
        "exec_times": 1,            // Execution times
        "max_times": 999,           // Maximum execution times
        "algorithm": 0
    }
    ```

- Todo Other

###### Commonly-used `task` Field Values

- `StartButton2`  
    Starting.
- `MedicineConfirm`  
    Confirmation of using Sanity Potion.
- `StoneConfirm`  
    Confirmation of using Originite Prime.
- `RecruitRefreshConfirm`  
    Confirmation of refreshing recruitment list.
- `RecruitConfirm`  
    Confirmation of recruitment.
- `RecruitNowConfirm`  
    Confirmation of expedited recruitment.
- `ReportToPenguinStats`  
    Reporting to Penguin Stats.
- `InfrastDormDoubleConfirmButton`  
    Double confirmation at infrastructure, only happens when there is a conflict with other operators.
- `Roguelike1Start`  
    Integrated Strategy: starting.
- `Roguelike1StageTraderInvestConfirm`  
    Integrated Strategy: trading items with Originium Ingot.
- `Roguelike1StageTraderInvestSystemFull`  
    Integrated Strategy: trading system full.
- `Roguelike1ExitThenAbandon`  
    Integrated Strategy: exit confirmation.
- `Roguelike1MissionCompletedFlag`  
    Integrated Strategy: mission completed.
- `Roguelike1MissionFailedFlag`  
    Integrated Strategy: mission failed.
- `Roguelike1StageSafeHouseEnter`  
    Integrated Strategy: Cunning Merchant
- `Roguelike1StageSafeHouseEnter`  
    Integrated Strategy: Safehouse
- `Roguelike1StageEncounterEnter`  
    Integrated Strategy: Chance Meeting
- `Roguelike1StageCambatDpsEnter`  
    Integrated Strategy: Operation
- `Roguelike1StageEmergencyDps`  
    Integrated Strategy: Emergency Operation
- `Roguelike1StageDreadfulFoe`  
    Integrated Strategy: Dreadful Foe
- `StartGameTask`
    Failed to launch client (incompatible config file with client_type)
- Todo Other

#### SubTaskExtraInfo

```jsonc
{
    "taskchain": string,           // Current task chain
    "class": string,               // Subtask class
    "what": string,                // Information type
    "details": object,             // Information details
    "uuid": string,                // UUID
}
```

##### Commonly-used `what` And `details` Field Values

- `StageDrops`  
    Stage drop information

    ```jsonc
    // Example of corresponding details field
    {
        "drops": [              // dropped items
            {
                "itemId": "3301",
                "quantity": 2,
                "itemName": "技巧概要·卷1" // Skill Summary - 1
            },
            {
                "itemId": "3302",
                "quantity": 1,
                "itemName": "技巧概要·卷2" // Skill Summary - 2
            },
            {
                "itemId": "3303",
                "quantity": 2,
                "itemName": "技巧概要·卷3" // Skill Summary - 3
            }
        ],
        "stage": {              // 关卡信息
            "stageCode": "CA-5",
            "stageId": "wk_fly_5"
        },
        "stars": 3,             // Stage clear ★
        "stats": [              // Statistics of drops
            {
                "itemId": "3301",
                "itemName": "技巧概要·卷1", // Skill Summary - 1
                "quantity": 4
            },
            {
                "itemId": "3302",
                "itemName": "技巧概要·卷2", // Skill Summary - 2
                "quantity": 3
            },
            {
                "itemId": "3303",
                "itemName": "技巧概要·卷3", // Skill Summary - 3
                "quantity": 4
            }
        ]
    }
    ```

- `RecruitTagsDetected`  
    Recruitment tags detected

    ```jsonc
    // Example of corresponding details field
    {
        "tags": [
            "费用回复", // DP-Recovery
            "防护", // Defense
            "先锋干员", // Vanguard
            "辅助干员", // Support
            "近战位" // Melee
        ]
    }
    ```

- `RecruitSpecialTag`  
    Special recruitment tags detected

    ```jsonc
    // Example of corresponding details field
    {
        "tag": "高级资深干员" // Senior operator
    }
    ```

- `RecruitRobotTag`
    Recruitment tag recognized as robot and skip (i.e. user needs it)

    ```jsonc
    // Example of corresponding details field
    {
        "tag": "支援机械" // Support robot
    }
    ```

- `RecruitResult`  
    Recruitment result

    ```jsonc
    // Example of corresponding details field
    {
        "level": 4,                 // ★ in total
        "result": [
            {
                "tags": [
                    "削弱" // Debuff
                ],
                "level": 4,         // Rarity of these tags
                "opers": [
                    {
                        "name": "初雪", // Pramanix
                        "level": 5  // ★ of the operator
                    },
                    {
                        "name": "陨星", // Meteorite
                        "level": 5
                    },
                    {
                        "name": "槐琥", // Waai Fu
                        "level": 5
                    },
                    {
                        "name": "夜烟", // Haze
                        "level": 4
                    },
                    {
                        "name": "流星", // Meteor
                        "level": 4
                    }
                ]
            },
            {
                "tags": [
                    "减速", // Slow
                    "术师干员" // Caster
                ],
                "level": 4,
                "opers": [
                    {
                        "name": "夜魔", // Nightmare
                        "level": 5
                    },
                    {
                        "name": "格雷伊", // Greyy
                        "level": 4
                    }
                ]
            },
            {
                "tags": [
                    "削弱", // Debuff
                    "术师干员" // Caster
                ],
                "level": 4,
                "opers": [
                    {
                        "name": "夜烟", // Haze
                        "level": 4
                    }
                ]
            }
        ],
        "robot": True               // Recruitment tag recognized and skip (i.e. user needs it)
    }
    ```

- `RecruitTagsRefreshed`  
    Recruitment tags refreshed

    ```jsonc
    // Example of corresponding details field
    {
        "count": 1,               // Number of times that the slot has been refreshed
        "refresh_limit": 3        // Limits of the number of times of refreshing
    }
    ```

- `RecruitTagsSelected`  
    Recruitment tags selected

    ```jsonc
    // Example of corresponding details field
    {
        "tags": [
            "减速", // Slow
            "术师干员" // Caster
        ]
    }
    ```

- `RecruitSlotCompleted`  
    Recruitment slot completed

- `RecruitError`  
    Error when recognizing recruitment

- `EnterFacility`  
    Entering the facility

    ```jsonc
    // Example of corresponding details field
    {
        "facility": "Mfg",  // Facility name
        "index": 0          // Facility ID
    }
    ```

- `NotEnoughStaff`  
    Available operators not enough

    ```jsonc
    // Example of corresponding details field
    {
        "facility": "Mfg",  // Facility name
        "index": 0          // Facility ID
    }
    ```

- `ProductOfFacility`  
    Production of the facility

    ```jsonc
    // Example of corresponding details field
    {
        "product": "Money", // Product
        "facility": "Mfg",  // Facility name
        "index": 0          // Facility ID
    }
    ```

- `StageInfo`  
    Auto-battle stage info

    ```jsonc
    // Example of corresponding details field
    {
        "name": string  // Stage name
    }
    ```

- `StageInfoError`  
    Auto-battle stage info error

- `PenguinId`  
    Penguin ID

    ```jsonc
    // Example of corresponding details field
    {
        "id": string
    }
    ```

- `DepotInfo`  
    Recognition result of depot

    ```jsonc
    // Example of corresponding details field
    // Supports ArkPlanner format only. More formats may be supported in future.
    "arkplanner": {
        "object": {
            "items": [
                {
                    "id": "2004",
                    "have": 4,
                    "name": "高级作战记录" // Strategic Battle Record
                },
                {
                    "id": "mod_unlock_token",
                    "have": 25,
                    "name": "模组数据块" // Module Data Block
                },
                {
                    "id": "2003",
                    "have": 20,
                    "name": "中级作战记录" // Tactical Battle Record
                }
            ],
            "@type": "@penguin-statistics/depot"
        },
        "data": "{\"@type\":\"@penguin-statistics/depot\",\"items\":[{\"id\":\"2004\",\"have\":4,\"name\":\"高级作战记录\"},{\"id\":\"mod_unlock_token\",\"have\":25,\"name\":\"模组数据块\"},{\"id\":\"2003\",\"have\":20,\"name\":\"中级作战记录\"}]}"
    }
    ```

- `UnsupportedLevel`  
    Unsupported level name
