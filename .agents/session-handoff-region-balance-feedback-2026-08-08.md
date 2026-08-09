# Handoff — NPC Labor region balance playtest (2026-08-08)

## Purpose for next session
Apply **in-game region-dispatch balance feedback** on top of the already-shipped vanilla-reuse collapse (planGen **31**). No code was changed in the handoff-only wrap session; implement the feedback list below, bump planGen, Release build, then re-verify in game.

## Workspace
- Mod root: `D:\work\Elin\5\2`
- Code: `NpcLabor/` (BepInEx/Harmony)
- Game: `E:\SteamLibrary\steamapps\common\Elin`
- Deploy: `Package\Mod_NpcLabor\` (DLL + package.xml)
- Version string **1.14.514** — never change
- Latest prior collapse handoff: `.agents/session-handoff-vanilla-collapse-2026-08-08.md`
- Pointer: `.agents/handoff.md`

## Always read first
1. `docs/CONTEXT.md`
2. `.agents/handoff.md` → this file
3. Collapse details: `.agents/session-handoff-vanilla-collapse-2026-08-08.md`
4. Decompile policy: `.research/README.md` + parent `D:\work\Elin\5\AGENTS.md`

## Key decisions (still locked)
- Region rewards: **manual 4 pools only** — never reintroduce biome scan / Normalize* / TopUp*
- Beach fish: `ThingGen.Create("fish", -1, lv)` path stays; **do not** call full `AI_Fish.Makefish`
- Lockpick mail: chest **contents only**
- Town wage formula unchanged
- Version **1.14.514** fixed
- Co-craft / processor / town labor out of scope unless regression

## Completed work (already on disk / deployed earlier today)
Vanilla-reuse collapse shipped (Release 0 warn/0 err ~12:47):
- `RewardMaterialFactory` → CreateRawMaterial-first
- Beach fish → Create("fish", -1, lv); planGen **31**
- Dead region Normalize*/TopUp* deleted from live path
- Lockpick empty → treasure re-roll

**This wrap session:** no source edits. Player playtested planGen 31 region rewards and filed balance bugs (below).

## Current pool source of truth (planGen 31)
File: `NpcLabor/Dispatch/RegionManualPools.cs`

| Region | Explore weights | Food |
|--------|-----------------|------|
| Beach | sand 55 / salt 25 / seaweed 12 / bait 8 | fish 100 (Create-by-lv) |
| Forest | wood:wood 45 / birch 20 / vine 15 / resin 8 / branch 7 / bark 5 | mushroom 40 / berry 25 / fruit 15 / apple 10 / grape 10 |
| Mountain | copper 30 / iron 22 / stone 18 / sulfur 12 / gem 8 / rock 10 | mushroom 60 / berry 40 |
| Plain | pasture 55 / grass 15 / flower 20 / herb 10 | flower 40 / herb 30 / egg 30 |

Helpers of note:
- Mushroom create always `CreateForestMushroom(common: true)` — rare path unused in manual pool
- `palulu` supported in CreateManualThing fruit branch, but **not listed** in beach/forest pools
- Fish: `lv = rnd(gatherSkill*2)+1` — heavy/high-quality fish common (player complaint)

## Player feedback → implementation backlog (NOT done)

### 1. Beach
- High-weight (~high quality) fish too many → **dampen by ~÷10**
  - Likely: post-filter by `t.Weight` / quality, re-roll heavy fish, or lower lv formula, or keep only light fish with inverse-weight odds
  - Do **not** restore old multi-id list unless Create-by-lv cannot be tamed
- **Add 帕露露 (`palulu`)** and **帕露露木** (coconut/palm wood — confirm live id via MCP/`.research`; candidates `wood:palulu` / palm log alias)
  - Place in Beach explore and/or food as appropriate (fruit vs wood log)

### 2. `PickMaxHarvestMembers` region differentiation
- File: `DungeonDispatchManager.PickMaxHarvestMembers` (~L526)
- Region score today: `explore + gather + lockpick` with gather bias:
  - forest → lumber
  - mountain → max(mining, digging)
  - beach → digging
  - **plain → plain gather only**
- Related: `DungeonDispatchTargets.AggregateRegionGather` (~L428) has forest/mountain bias but **no beach digging branch** (inconsistent with picker)
- Player: each region should prefer different skills. Suggested direction:
  - beach: digging (+ fishing if skill id exists for food)
  - forest: lumber
  - mountain: mining
  - plain: gather / farming-ish
  - Weight explore vs specialty vs lockpick differently per region (not flat sum)
- Also re-check start-mission skill aggregation (~L722) for same consistency

### 3. Mountain
- **海沙 must not appear** (sand leak — inspect CreateManualThing / material accept / wrong id fallback)
- 硫磺 **≈ 石头** weight
- 石头 **> 矿** (ores currently dominate: copper30+iron22 vs stone18)
- 矿 **too many overall** — cut copper/iron share
- Suggested starting table (tune in play): stone ~28–32, sulfur ~28–32, copper+iron combined lower, rock/gem keep light

### 4. Forest
- **No mushrooms dropping** — debug `CreateForestMushroom` / `CreateManualThing("mushroom")` / food distribute path; may be create failure or foodTarget 0
- Need common mushrooms **plus low-rate rare/other** (`CreateForestMushroom(common:false)` / mix RareMushroomIds)
- **No flowers** — add flower entries to Forest explore or food
- **Wood too low** — raise wood share
- **藤蔓 = 木头** weight (vine == wood total or primary wood id)

### 5. Plain (草原)
- **No meat** — add meat to PlainFood (`meat` / MakeFoodFrom path if needed)
- Only one flower type observed — `CreatePlainFlower` already randomizes id list; verify list not collapsing to one id, or broaden
- **牧草 ×2** — pasture weight 55 → ~110 relative (or double share vs others)

### 6. Mail parcel name
- Current: `LaborText["dis.parcel.harvest"] = "{0}收获"` with `LaborTerms.DungeonExplore` → reads like 地牢探索收获
- Player wants package name **派遣收获**
- Touch: `DungeonDispatchRewards` deliver title (~L152) + `LaborText` keys; region vs dungeon may need split (region: 派遣收获 / dungeon keep or also 派遣*)

## Files most likely to edit next
1. `NpcLabor/Dispatch/RegionManualPools.cs` — weights, beach palulu/wood, fish dampen, forest/plain food, planGen **32+**
2. `NpcLabor/Dispatch/DispatchRewardPools.cs` — CreateForestMushroom / CreatePlainFlower / fruit helpers if create fails
3. `NpcLabor/Dispatch/RewardMaterialFactory.cs` — only if mountain sand leak / wood:palulu identity
4. `NpcLabor/Dispatch/DungeonDispatchManager.cs` — PickMaxHarvestMembers scoring
5. `NpcLabor/Dispatch/DungeonDispatchTargets.cs` — AggregateRegionGather beach + per-region specialty
6. `NpcLabor/LaborText.cs` (+ maybe LaborTerms) — parcel title 派遣收获
7. Possibly `DungeonDispatchRewards.cs` title switch for region vs dungeon

## Modified files this wrap session
- **None** (handoff only). Also writing:
  - `.agents/session-handoff-region-balance-feedback-2026-08-08.md`
  - `.agents/handoff.md` pointer update
  - OS temp copy of this handoff (skill requirement)

## Build / deploy reminder
```powershell
E:\SteamLibrary\steamapps\common\Elin = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```
- Close game if DLL locked; full restart after deploy
- Bump `ManualRegionPlanGenVersion` so in-progress region plans rebuild

## Suggested implementation order
1. Parcel rename (small, player-visible)
2. Pool weight retune mountain/forest/plain + planGen bump
3. Beach fish ÷10 dampen + palulu / palulu wood
4. Forest mushroom/flower create path fix + rare mix
5. Plain meat + pasture double + flower diversity check
6. PickMaxHarvestMembers + AggregateRegionGather region specialty pass
7. Release build → in-game verify all four regions + mail title

## Do not
- Reintroduce biome scan / Normalize* / TopUp*
- Call full Makefish
- Change version **1.14.514** or town wage formula
- Touch co-craft / processor / town labor unless asked
- Blind full-assembly redecompile

## Suggested skills
- **elin-modding** — confirm palulu wood / meat / mushroom / flower live ids via MCP
- **diagnose** — if forest mushrooms still empty after pool edit (create-path failure)
- **handoff** — after the balance pass ships

## Residual risk
- Fish Create-by-lv may still skew heavy even after ÷10 filter; may need lv cap
- Mountain sand leak may be material factory accept, not pool weights
- Forest foodTarget half only applies to mountain; if forest food empty, create helpers are the bug not weights
- Old missions need planGen bump or they keep stale tables
- PickMaxHarvest scoring change affects auto-pick UX and haul size; keep skill roles (explore qty / gather food / lockpick chests) aligned with CONTEXT.md
