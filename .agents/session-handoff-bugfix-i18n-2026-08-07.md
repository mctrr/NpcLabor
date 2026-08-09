# Session handoff — C4 状态泄漏 + i18n 收口 (2026-08-07)

## Goal this session
完成清单：
- C4: 修复 `actionsNextFrame` 双层延迟状态泄漏
- M1: 硬编码中文字符串迁移到 `LaborText.T()`
- M5: `TownLaborInvestHistory` 添加清理机制
- m1: 地区种类匹配大小写一致化
- m4: `AI_TownLabor` 添加安全计数器防死循环
- m5: `ProcessorWhitelist` 回退分支加警告日志
- 编译验证

## Done
### C4 — LayerDragGridPatches 状态泄漏
- 问题：`StartJob` 先入队 `FinishStart`，其 `finally` 再入队一个 reset，`SuppressUiRefresh`
  跨两级延迟保持 true；reset 动作一旦丢失（队列清空 / 层被销毁）标志永久泄漏，
  会抑制后续任意 LayerDragGrid 的 RefreshCost/RedrawButton。
- 修复（`NpcLabor/Patches/LayerDragGridPatches.cs`）：
  - 单层延迟：`FinishStart` 的 `finally` 在 `Close()` 后**同步** `ClearStartState()`，
    不再嵌套 `actionsNextFrame`。
  - 帧戳兜底：置 true 时记录 `_suppressFrame`；`RefreshCost`/`RedrawButton` 前缀先
    `ExpireStaleSuppress()`，超 2 帧自动清除。
  - `_startPending` 防同帧二次 `StartJob` 重复入队。

### M5 — TownLaborInvestHistory 清理
- `TownLaborManager.PruneInvestHistory()`：清除无效记录（uid<=0 / raw<=0）与已过冷却
  （`now - lastInvestRaw >= InvestCooldownRaw`）的记录。
- 调用点：`Load()` 合并后、`Save()` 序列化前（先清再写）。
- 存档不再无限增长；冷却逻辑不受影响（过期记录本就可再次发放）。

### m1 — 地区种类大小写一致化
- 新增 `DungeonDispatchTargets.NormalizeRegionKind(string?)`（trim + lowercase）。
- `BuildRegionTarget` 存储即归一化；`TryStartRegion` 落档 `regionKind` 归一化；
  `fieldId = "region:" + NormalizeRegionKind(...)` 修复 zone id 大小写分叉；
  `FindByRegion`/`TargetKey`/奖励侧全部改走同一归一化入口。
- 特殊说明：`SpecialRewardConfig` 仍允许 field/plain 互为别名（保持原逻辑）。

### m4 — AI_TownLabor 安全计数器
- `MaxChaseAttempts = 12`：追不到委托人时停止追赶，原地 idle，防 DoGoto thrash。
- `MaxFastIterations = 600`：循环迭代不推进帧（零时长 yield）则 `Cancel()`，防死循环。
- 需要在游戏内确认：离远时仍会走到客户附近，追丢后停在原地工作而非抖动。

### m5 — ProcessorWhitelist 回退警告
- 未列名机器经 `IdSource` 回退支持时，`Plugin.LogWarn` 一次（按类型 HashSet 去重），
  便于后续评审是否加入白名单。

### M1 — 硬编码中文字符串 → LaborText.T()
- `NpcLabor/LaborText.cs` 词表大幅扩展（CN+EN 均补齐，约 250+ 键）：
  - `co.*` 共同制造、`proc.*` 加工、`craft.*` 配方有效值行
  - `dis.*` 派遣（错误/出发/包裹/统计行/预览/UI/任务追踪/调试命令）
  - `town.*` 店铺帮工（错误/消息/UI/忙碌原因/交谈拦截）
  - `job.*` 岗位目录（标题/技能标签/24 条第一人称理由，含 EN 翻译）
- 迁移覆盖文件：CoCraft、Process、Patches（LayerCraft/LayerDragGrid/Dispatch/
  TownLabor/UIRecipeInfo）、Dispatch（Manager/Targets/Rewards/UI/Quest/DispatchMission）、
  TownLabor（Manager/UI/Jobs/Mission/Rewards/LaborBusy/Quest*）。
- 保留不迁移：物品名匹配启发式（`Contains("海沙")` 等，匹配游戏内物品名）、注释、
  `"派遣"` 任务标题启发式、`'个'` 数量解析字符。
- 注意：`DungeonDispatchManager` 结算对比 `loot != "暂无收获"` 已同步改为
  `loot != LaborText.T("dis.loot.none")`，两处必须一起改。

## Build
```
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```
**Release OK, 0 warnings, 0 errors**，DLL 已部署到
`E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll`（本次游戏未锁定 Package）。

## 追加：防御收敛 + DungeonDispatchRewards.cs 拆分（同日第二轮）
### 防御收敛（DungeonDispatchRewards.cs 全文件）
- 删除 238 处空 catch / 静默 LogDebug catch（脚本：仅移除 catch 为空或
  `Plugin.LogDebug` 单语句、try 体无 return/throw 出口、且不嵌套重叠的构造）。
- 保留 213 处有真实兜底逻辑的 catch（fallback 赋值 / return null / LogWarn 入口守卫）
  + 38 处因嵌套重叠保留的方法级守卫。
- 效果：9076 → 8870 行；try 453 → 214；catch 550 → 239；空 catch 156 → 1；
  静默 catch 99 → 26（保留的是方法级唯一守卫）。
- 工具脚本在 OS temp（`converge_defense3.py`），关键规则：try 体含 return/throw 出口
  必须跳过（否则产生不可达代码 CS0162）；嵌套 try 只删内层、保留外层守卫。

### 拆分（partial class，无调用方改动）
- `DungeonDispatchRewards.cs`（1,603 行）：结算编排 + 通用工具 + enum DispatchSettleKind
- `DungeonRewardPlans.cs`（2,971 行）：数值公式 + 计划锁定/修复 + 地区契约/均衡
- `DispatchRewardPools.cs`（2,091 行）：地区/地牢战利品池 + 掷骰 + 池缓存 + id 数组
  + RegionLootEntry/RegionLootPool
- `RewardMaterialFactory.cs`（2,478 行）：材料/物品工厂 + 判定
- 4 个文件均为 `internal static partial class DungeonDispatchRewards`，
  外部调用 `DungeonDispatchRewards.X` 全部不变；编译 0 警告 0 错误。
- 注意：表达式体成员（`=>` 单行）不会被脚本解析器抓到，手工补了
  `EnsureRegionPlanPublic` / `RepairRegionPlanEntries` / `RegionHaulUnits` /
  `RegionSkillScore`（都进 Plans 文件）。

### 真实 id 优先（RewardMaterialFactory.cs）
- 新增 `MaterialIdCache`（按 alias 缓存已解析材料 id，运行时源数据静态）。
- 新增 `HasMaterialAlias(Thing, params aliases)`：先按 `LiveMaterialId` 与
  `ResolveMaterialId` 相等判定。
- `IsGoldLikeThing` / `IsSulfurLikeThing` / `IsSandLikeThing` / `IsSaltLikeThing` /
  `IsMudLikeThing` 均改为 id 判定优先，文本 Contains 降级为 fallback。
- 确认自游戏常量：`MATERIAL.sand_sea=97`、`gold=12`、`sand=8`、`mud=4`、`soil=45`。

### 计划锁定/重放 —— 原版核查结论（未重写）
- vanilla `Expedition`/`ExpeditionManager` 只做「计时 + 传走再传回」，不产战利品；
  vanilla `Quest` 奖励是完成时现生成。原版没有可复用的战利品计划机制。
- 计划锁定是产品需求「详情数量 == 邮件数量」+ 失败/召回按进度结算逼出来的自研机制，
  不能直接换原版。
- 可行简化（下一步，未实施）：把地区计划生成改为「存 seed + 技能快照」确定性重算，
  可删掉 `plannedLootEntries` 存储、`ThingsToPlanEntries`/`CreateFromPlanEntry`/
  `Repair*PlanEntries`/`ParseLootLogQtys` 等约 1,000+ 行；风险中等（涉及核心经济与旧档兼容），
  建议单独一轮做。

## 追加：反馈渠道收敛（同日第三轮，用户三项要求）
### 1. 奖励写进任务描述，完成不再弹窗
- 店铺帮工：`TownLaborRewards.LockRewardPreview` 在接受时锁定确定性奖励
  （`rewardMoney` / `rewardPlat` / `rewardTicketGranted`，去除结算时的小随机），
  `RewardPreviewLine` 拼成「报酬 金币×N / 白金币×N / 家具兑换券」写入
  `QuestNpcLaborTownLabor.GetTrackerText`（任务描述）与活动委托行
  `QuestNpcLaborTownLaborOffer.GetDetail`；旧档在 Load 时迁移补锁。
- 结算改用锁定值（描述 == 实付）；leftover 仍结算时随机，不入描述。
- 技能不达标：`reward.skillDiscount` 只进结算日志（Msg），无弹窗；接受界面保留
  红字提示（非弹窗）。
- 地区派遣：`QuestNpcLaborDispatch.GetTrackerText` 追加「计划收获: ...」
  （`FormatPlannedSuccessHarvest`，成功时锁定计划）。

### 2. PC 自工改为「站到委托人旁边自动耗时间」（类休息）
- `AI_TownLaborPcSelf` 增加 `WorkPhase()`：到达后持续站在客户旁
  （WorkRadius=2，过远自动走回）、`LookAt` + `WorkEmo` 表情、DoIdle，
  随游戏小时自然推进并扣 SP（保持原有 GameDate.AdvanceHour 挂钩）。
- 手动取消/移动打断 AI → `NotifyPcSelfApproachCancelled`：未开工=失败，
  已开工=Recall（无主奖），与「停下」菜单一致。
- 结算成功路径 `ClearPcSelfApproachAi` 用 `ai.Success()` 正常结束工作态。

### 3. 全功能去除弹窗
- 删除全部 9 处 `WidgetPopText.Say`（TownLaborManager 4 处、DungeonDispatchManager 5 处），
  保留同位置的 `Msg.Say`（游戏日志）。全仓已无 WidgetPopText/Toast/Popup 类调用。

### 本轮 Build
**Release OK, 0 warnings, 0 errors**；DLL 已部署。
游戏内验证点：
- 接单后任务描述显示确切报酬；完成后无浮动弹窗，仅日志一行。
- PC 自工：角色走到客户旁站立工作，时间按游戏小时推进、每小时扣 SP；
  移动/取消 → 无主奖结算；休息/睡觉会推进完成（时间挂钩）。
- 地区派遣任务描述显示计划收获。

## 本轮最终 Build
**Release OK, 0 warnings, 0 errors**；DLL 已部署。
游戏内验证点同上一轮（C4/加工/店铺帮工/地区派遣英文文案），另加：
- 沙滩/山脉地区收获仍只出真实海沙/硫磺（id 判定生效，无泥/金泄漏）。
- `NpcLaborHarvest` 控制台输出正常（Plan 方法搬入 Plans 文件后引用仍解析）。

## Files touched
- `NpcLabor/Patches/LayerDragGridPatches.cs` (C4 + M1)
- `NpcLabor/Process/ProcessorWhitelist.cs` (m5)
- `NpcLabor/Process/ProcessorJobSession.cs` / `ProcessorOutsourceMode.cs` (M1)
- `NpcLabor/CoCraft/AssistantResolver.cs` / `CoCraftSession.cs` (M1)
- `NpcLabor/LaborText.cs` (M1 词表)
- `NpcLabor/Dispatch/DungeonDispatchTargets.cs` (m1 + M1)
- `NpcLabor/Dispatch/DungeonDispatchManager.cs` (m1 + M1)
- `NpcLabor/Dispatch/DungeonDispatchRewards.cs` / `DungeonDispatchMission.cs` / `DungeonDispatchUi.cs` / `QuestNpcLaborDispatch.cs` / `DispatchConsole.cs` (m1/M1)
- `NpcLabor/Patches/DispatchPatches.cs` / `LayerCraftPatches.cs` / `TownLaborPatches.cs` / `UIRecipeInfoPatches.cs` (M1)
- `NpcLabor/TownLabor/TownLaborManager.cs` (M5 + M1)
- `NpcLabor/TownLabor/TownLaborMission.cs` / `TownLaborJobs.cs` / `TownLaborUi.cs` / `TownLaborRewards.cs` / `LaborBusy.cs` / `QuestNpcLaborTownLabor.cs` / `QuestNpcLaborTownLaborOffer.cs` / `AI_TownLabor.cs` (M1/m4)
- `.agents/handoff.md` + this file

## Verify in-game after deploy (next session)
1. 加工：连开两台机器 / 快速重复开始，不再出现按钮或费用栏不刷新（C4 泄漏路径）。
2. 加工：未列名机器（如模组新增 TraitCrafter）启动时日志出现一次 whitelist fallback warn。
3. 店铺帮工：存档 `npclabor_townlabor.json` 的 `investHistory` 只保留 3 天内记录；
   同一委托人冷却判定不变。
4. 地区派遣：英文语言下区域名/统计行/预览为英文；region 场区 zone id 始终小写
   （`region:plain` 等）。
5. 店铺帮工：工人追不到委托人（隔墙/卡点）时原地待机不抖动；英文下 24 条理由为英文。
6. `NpcLaborExp` / `NpcLaborHarvest` 等调试命令输出走词表。

## Explicit non-goals (unchanged)
- Version lock **1.14.514**；不 fork Auto Act / BCS；不做 kill-quest 外包。
- 物品名匹配启发式中的中文不迁移（那是游戏数据匹配，不是 UI 文案）。

## Next optional polish
- 若英文语言下游戏自身物品名/技能名仍是中文（Elin 部分数据只带 CN），需要接受现状；
  纯 mod 文案已全部 EN 化。
- 若有更进一步的区域平衡需求，可把 `dis.*` 更多常数（成功率锚点等）挪进
  `labor_config.json`。
