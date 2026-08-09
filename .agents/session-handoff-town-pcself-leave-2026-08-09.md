# Session handoff — town labor PC self speed/leave (2026-08-09)

## Status
Release build OK → `E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll` (2026-08-09 22:37)
Version string **1.14.514** unchanged.

## Player questions answered
### 打工奖励会随投资等级提升吗？
**会。** 已有逻辑，无需改公式：
- 满额时薪 = `(50 + c_invest) × hours`（`TownLaborRewards.CalcFullWagePerHour`）
- 实发 = `clamp(skill/c_invest, 0..1) × 满额`
- 白金 = `max(2, 2 + 2×floor(c_invest/20))`（config: platBase / platPerInvestTier / investTierSize）
- 成功且 skill > c_invest 且未触城镇投资 cap 时免费 `c_invest+1`，并有 Msg.Say 反馈
- reward pins（money/plat/ticket）接单时锁定，看板与结算一致

### PC 打工时间太慢
- `AI_TownLaborPcSelf.UseTurbo => true`（像休息/制造一样加速时间）
- `DoIdle(40 + rnd(30))` 缩短空闲块，小时 tick 更跟手

### PC 打工有时停住（寻路卡）
- 接近阶段 path fail：`PlaceWorkerNearClient(force)` 软拉一次，仍过远才取消
- 工作阶段 path stuck streak ≥2：强制拉到客户旁，不再静默停住

### PC 自己脱离无影响
离开城镇且有 PC 亲自帮工时：
1. **退出打工（任务失败）** → `AbortAllPcSelfLabor` / `Settle(Failed)`
2. **换 NPC 继续** → `TryHandOffPcSelfTo`（保留 hoursLeft + locked rewards）
3. **取消** → 留在当前图
- 仅同伴帮工时仍是 YesNo「继续计时」
- 中止/换人后**不自动 MoveZone**；玩家再点一次离开即可

## Files this session
- `NpcLabor/TownLabor/AI_TownLaborPcSelf.cs` — turbo + path recover + shorter idle
- `NpcLabor/LaborText.cs` — leave menu CN/EN（EN 引号已转义）
- `NpcLabor/Patches/LifecyclePatches.cs` — self leave intercept menu
- `NpcLabor/TownLabor/TownLaborUi.cs` — `OpenPcSelfLeaveMenu` / swap picker
- `NpcLabor/TownLabor/TownLaborManager.cs` — HasActivePcSelf / Abort / HandOff

## Build note
先前 EN 字符串未转义导致编译失败；已修：
- `"{0} takes over \"{1}\"."`
- `"You quit \"{0}\"."`

## In-game verify
1. 完整重启游戏加载新 DLL
2. PC 亲自帮工：时间应明显快于旧版；卡住应被拉回客户旁
3. 亲自帮工中离开城镇 → 三选一菜单
4. 换 NPC：剩余小时继续；结算用接单锁定金额
5. 仅同伴帮工离开 → 旧 YesNo
6. 投资等级更高的店：看板/结算 money + plat 更高（技能不足仍打折）

## Do not
- 改 version / 工资公式
- 再砍花/肉（planGen 36 已 ship）
- 全量反编译
- abort/swap 后自动 MoveZone

## Prior still valid
- planGen **36**: flower qty half; meat qty half + 2–4 species
- planGen **35**: no alchemy-ash eggs; palulu lock; sulfur≈stone; invest Msg.Say
