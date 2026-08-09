# Session handoff — 派遣 / 地区奖励 / 加工队友 / 城镇帮工文案反馈 (2026-08-07)

## Goal this session

处理玩家反馈五条，全部限定在 NPC Labor 现有 scope：

1. 家园任务板「地牢探索 / 地区派遣」入口按钮文字改为 **派遣**
2. 地区派遣奖励按 biome 锁定：沙滩不该出面包 / 橡木等跨区物
3. 地区派遣任务列表不预告收获；停止 AFK 时假膨胀（如泥土×2500，实际结算没有）
4. 队友可以帮忙 **加工**（居民本来就正常）
5. 城镇帮工任务描述：去掉「(按技能)」、不单独列报酬种类/白金币；只写会付多少钱

## Key decisions

- **按钮短名**：`term.dispatch` = `派遣` / `Dispatch`；`LaborTerms.Dispatch` 专供家园任务板入口。地区全称仍用 `term.regionDispatch` = 地区派遣。
- **地区 tracker 瘦身**：`QuestNpcLaborDispatch` + `DungeonDispatchManager.MissionTrackerText` 对 `isRegion` **隐藏** `dis.q.harvest` / plan 行；地牢仍显示 haul。玩家靠 biome 直觉即可。
- **假膨胀根因**：小时 tick 仍 `EnsureRegionPlan`，但 **不再** 每小时 `RefreshRegionProgressLog` 改写 `lootLog`。结算仍走 planned entries（`TryTakeRegionPlanThings`），UI 只是不预览。
- **泥土问题**：结算路径本就 ban mud/soil；膨胀主要是 tracker 进度缩放 + lootLog 聚合观感。隐藏 region harvest 后不再误导。
- **地区食物**：禁止 `TryCreateFromCategorySafe("food")` 类 cooked 路径；沙滩 gather 仅 `fish/shell/shell2/seaweed/bait`；`IsCookedOrProcessedFood` + `StripRegionJunk` 二次剥离。
- **地区 pool**：`DispatchRewardPools` cache key **v18**；禁 bread/cooked；沙滩 prune 木/矿/果/肉；跨 biome 再 prune。
- **计划版本**：`CurrentPlanGenVersion = 25`；旧 mission 在 load/tick 会因 gen 过低重建 plan。
- **加工队友**：镜像城镇帮工 — 开工 `party.RemoveMember`，清任务 `party.AddMemeber`（注意 vanilla 拼写）。
- **城镇 board 文案**：新增 `town.reward.moneyOnly` = `报酬：金币×{0}`；offer 未接单用 full wage×hours；进行中用 `MoneyOnlyPreviewLine`。**结算 log 仍可含白金/票**，仅 board/tracker 不写。
- **版本号 1.14.514 不变**；不 blind redecompile；不 fork Auto Act。

## Done (code on disk)

| Area | Files | Change |
|------|-------|--------|
| Button label | `NpcLabor/LaborText.cs`, `NpcLabor/LaborTerms.cs`, `NpcLabor/Patches/DispatchPatches.cs` | `term.dispatch`；board `LaborTerms.Dispatch` (+ count) |
| Region tracker | `NpcLabor/Dispatch/QuestNpcLaborDispatch.cs`, `NpcLabor/Dispatch/DungeonDispatchManager.cs` | Region 隐藏 harvest/plan；dungeon 仍显示 |
| Region hour tick | `NpcLabor/Dispatch/DungeonDispatchRewards.cs` `TickPartialLoot` | `EnsureRegionPlan` only；**无** hourly lootLog refresh |
| Region food/plan | `NpcLabor/Dispatch/DungeonRewardPlans.cs` | planGen **25**；beach food 锁定；`IsCookedOrProcessedFood`；`StripRegionJunk` |
| Region pools | `NpcLabor/Dispatch/DispatchRewardPools.cs` | cache **v18**；forbid cooked；beach/cross-biome prune |
| Processor party | `NpcLabor/Process/ProcessorJobSession.cs` | `WasPartyMember` detach/rejoin |
| Town board text | Offer/tracker + Rewards + LaborText | `MoneyOnlyPreviewLine` / free-offer full gold line |

Source mtimes ~ **2026-08-07 22:53–22:58**。

## Build / deploy status

**本轮未完成 Release 构建。**

- 已部署 DLL：`E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll`
  - LastWrite **2026-08-07 08:17**（更早 bugfix/i18n 轮）
  - **不含** 本轮 22:5x 源码改动
- 本地 `NpcLabor\bin\Release` 未见更新后的 DLL

下一会话 **必须先 build** 再游戏内验证：

```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```

若游戏锁 DLL，关游戏后再 build / 复制。

## Unfinished / next steps

1. **Build + 修编译错误**（若有 party hop、partial、新字符串问题）。
2. **全量游戏重启** 后验证：
   - 家园任务板按钮 **派遣** / `派遣 (n)`
   - 沙滩地区：沙/盐/贝/鱼等；**无** 面包、木材、橡木
   - 地区 tracker：成员/进度/剩余时间；**无** 收获清单；AFK 不堆泥土显示
   - `NpcLaborComplete` / 自然结算邮件与 biome 一致，无 dirt flood
   - 队友 pin 锯/磨等可加工；完成/取消后重回队伍
   - 城镇帮工 board：理由 + 时长 + `报酬：金币×N`；无「(按技能)」、无白金币预告
3. 旧 region mission：依赖 planGen 25 / pool v18 重建；load 后确认。
4. 可选：清理仍写 `lootLog` 的 debug/console 路径（`FormatPlannedSuccessHarvest` 等）以免调试误读。
5. **并行未完成（上轮工坊）**：author 确认、`preview.jpg`、工坊首次上传；与本反馈无关，见 previous handoff。

## Root causes (review)

- Beach gather 曾走 generic food category → cooked bread
- Biome scan + 弱 prune → 木/橡木类串区
- Tracker 显示 progress 缩放 plan + 小时 refresh lootLog → 泥土×2500 观感；结算本就不发 mud
- Party AI 把队友粘在 PC 上；加工未 detach（城镇帮工已做）

## Do not

- 改版本字符串 `1.14.514`
- 全量 redecompile / 在 mod 根堆 `_tmp_*`
- 把 region live haul 重新挂回 tracker
- 在城镇 **board 描述** 再写白金/票/「按技能」（结算 log 可保留真实掉落）
- 扩大 scope 到 BCS / Seamless / Auto Act fork / 击杀任务外包

## Previous handoffs

- Workshop publish：`.agents/session-handoff-workshop-publish-2026-08-07.md`
- Bugfix + i18n：`.agents/session-handoff-bugfix-i18n-2026-08-07.md`

## Read first next session

1. `docs/CONTEXT.md`
2. `.agents/handoff.md` → 本文件
3. 若碰 vanilla hook：`.research/README.md` + `KNOWN_TYPES.txt` / `TYPE_INDEX.csv`
