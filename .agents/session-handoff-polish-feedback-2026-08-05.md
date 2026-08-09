# Session handoff — 2026-08-05 polish (feedback / naming / unlock)

## Goal
1. Save the “拷问” critique as durable docs.
2. Fix easy-fatigue items with locked user decisions (#1–#6).
3. Keep product names / unlock / lean feedback consistent across A/B/D/E.
4. Leave a clean handoff for the next session.

## Locked decisions (this pass)

| # | Topic | Decision |
|---|--------|----------|
| 1 | (unspecified fatigue) | Leave alone |
| 2 | Feedback tone | Settle text only key rewards; no spreadsheet dumps; **no dual duration** like `12小时（0.5天）` |
| 3 | Failure punishment | Skip |
| 4 | Cross-system glue | **Skills only** (town work + co-craft both grow life skills); no new shared economy |
| 5 | Unlock | **地区派遣** branch lv≥3; **地牢探索** branch lv≥5 |
| 6 | Naming | **地牢探索 / 地区派遣 / 店铺帮工** (+ 共同制造 / 加工外包) via `LaborTerms` |

Town name: **店铺帮工** (not 城镇帮工). Alternatives kept in `docs/mod-critique-and-gaps.md` and `LaborTerms` comment.

Town settle feedback example: `金币×400 / 打工奖励 绳子`  
Affinity / `c_invest` / skill exp still apply silently.

## Done

### Docs
- `docs/mod-critique-and-gaps.md` — full critique + decisions + backlog
- `docs/CONTEXT.md` — domain terms with CN names; unlock; skill-center; lean feedback; E/D headers

### Code — naming / unlock hub
- `NpcLabor/LaborTerms.cs`
  - `DungeonExplore` / `RegionDispatch` / `TownWork` / `CoCraft` / `Assist` / `ProcessOutsource`
  - `RegionUnlockBranchLv = 3`, `DungeonUnlockBranchLv = 5`
  - `CanRegionDispatch` / `CanDungeonExplore` / `HomeBranchLv`

### Code — lean player text
- Town labor start: hours-only (`约 N 小时`), not days
- Town labor settle: already money + 打工奖励 only in `rewardLog`
- Town tracker: shorter progress line via `ProgressLine()`
- Co-craft finish: `获得了技能经验` / `共同制造结束了` (no raw exp number dump)
- Co-craft menu msgs: drop `共同制造：` prefix noise; titles use `LaborTerms`
- Process outsource menu msgs: shortened; titles use `LaborTerms`
- Processor start string stays short count form
- Dispatch start/settle strings use 地牢探索/地区派遣; settle drops “奖励已入据点邮箱” when loot present
- Dispatch detail text leaner

### Code — unlock wiring (already present / confirmed)
- `DungeonDispatchUi` list gating
- `DungeonDispatchManager` start gates for region + dungeon

### Build
```
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```
**Release build OK** → deploys `E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll`

## Files touched this polish pass
- `NpcLabor/LaborTerms.cs` (existing hub)
- `NpcLabor/TownLabor/TownLaborManager.cs`
- `NpcLabor/TownLabor/QuestNpcLaborTownLabor.cs`
- `NpcLabor/TownLabor/TownLaborRewards.cs` (lean rewardLog from earlier)
- `NpcLabor/CoCraft/CoCraftSession.cs`
- `NpcLabor/Process/ProcessorJobSession.cs`
- `NpcLabor/Patches/LayerCraftPatches.cs`
- `NpcLabor/Patches/LayerDragGridPatches.cs`
- `NpcLabor/Dispatch/DungeonDispatchManager.cs`
- `NpcLabor/Dispatch/DungeonDispatchUi.cs`
- `docs/mod-critique-and-gaps.md`
- `docs/CONTEXT.md`
- `.agents/handoff.md` (this pointer)
- `.agents/session-handoff-polish-feedback-2026-08-05.md` (this file)

## Not done / next suggestions

### P0 (if still itch in-game)
1. Smoke-test branch lv 2/3/5 dispatch list + start deny.
2. Smoke-test town labor settle line = only money + 打工奖励.
3. Smoke-test co-craft/outsource button messages are short.
4. Grep leftover player-facing spreadsheet tone after play:
   - raw exp numbers, dual duration, 好感+/投资+ in Msg
5. Confirm party rejoin after town labor (previous bug path) still holds.

### P1 product (from critique, not this pass)
- Localization key table + EN (hardcoded CN still)
- Cross-system numeric scale (time/risk/reward between A/B/D/E)
- `c_invest+1` anti-farm for town work
- Stronger result narrative (still lean, not spreadsheet)
- Config toggles

### Explicit non-goals this pass
- No version bump (lock **1.14.514**)
- No Auto Act fork / BCS / kill-quest outsourcing
- No failure-punishment redesign
- No new economy linking systems beyond skill exp

## Constraints (do not regress)
- Version **1.14.514**
- E: no labor on PC base; max 2 concurrent; board sample 2–3; cash ~400; party rejoin on settle; off-map continues
- D button independent of E board rows
- Co-craft default Off
- Busy mutual exclusion A/B/D/E via `LaborBusy`

## How to continue
1. Read `docs/mod-critique-and-gaps.md` + `docs/CONTEXT.md` + this handoff.
2. Prefer edit player strings through `LaborTerms` + lean Msg rules above.
3. Build with `ElinGamePath` set; DLL lands in game Package folder.
