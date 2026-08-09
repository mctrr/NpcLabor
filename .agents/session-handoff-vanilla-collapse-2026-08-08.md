# Session handoff — vanilla-reuse collapse (2026-08-08)

## Goal
Implement ranked replacements from the vanilla-API reuse audit. No game launch.

## Done (Release build 0 warn/0 err)
Deployed: `E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll` (~339KB, 2026-08-08 12:47)

### 1. Material factory → CreateRawMaterial-first
- `RewardMaterialFactory.cs`: new `CreateMaterialFromRow` + `AcceptCreatedMaterial`
- `CreateMaterialThing(alias, preferredCarriers?)` routes through it
- `CreateByMaterialId` / `CreatePinnedMaterialThing` / `CreateMetalHard` thin wrappers
- `CreateBeachSand` single path; `CreateBeachSandHard` → `CreateBeachSand()`; MandatoryFallback kept as last chunk pin only
- Removed `TryCreateDigFloorMaterial` multi-path
- `KeepFoodFresh`: `decay=0` + foodish `ForceFreshProduceMaterial` only

### 2. Beach fish → vanilla origin
- `RegionManualPools.DistributeFishPool`: `ThingGen.Create("fish", -1, lv)` with `lv = rnd(gatherSkill*2)+1` (explore fallback); **no** `AI_Fish.Makefish`
- Optional `IsBeachFishCandidate` post-filter
- `ManualRegionPlanGenVersion = **31**` (old plans rebuild)

### 3. Dead region staples deleted (`DungeonRewardPlans.cs`)
- `TopUpRegionToTarget`, `AddGuaranteedRegionStaples`
- `NormalizeMountain/Plain/Forest/BeachThings`
- `ApplyRegionSpecialRewards`, `AddRegionGatherFood`
- Live path still: manual pools + `EnforceRegionDeliverContract` + plan freeze
- `ForceFreshProduceMaterial` simplified to fresh alias / DefaultMaterial

### 4. Lockpick empty fallback
- `CollectLockpickChestThings`: empty extract → **treasure re-roll** + FlattenInto contents-only
- Last resort: `ThingGen.CreateCurrency(n)` (not custom 85/12/3 table)
- `RollLockpickChestCurrency` thinned to CreateCurrency wrapper

## File size delta (approx)
| File | Before | After |
|------|--------|-------|
| RewardMaterialFactory | ~79KB / 2478L | ~70KB / 2230L |
| DungeonRewardPlans | ~117KB / 3134L | ~86KB / 2313L |
| RegionManualPools | ~21KB / 675L | ~20KB / 652L |

## In-game verify (player; not done this turn)
1. Beach 4w: 海沙/盐/鱼(多品种)/海藻/鱼饵; no mud; planGen 31 rebuild
2. Mountain copper/iron/sulfur identity (not gold lottery)
3. Forest wood logs real material pin
4. Lockpick parcel: contents only, no chest body; empty path still pays something
5. Co-craft / processor / town labor untouched smoke

## Do not
- Reintroduce biome scan / Normalize* / wage change / version **1.14.514**
- Call full `Makefish`
- Start game unless asked

## Next if needed
- If fish diversity too narrow under Create-by-lv, add light weight filter only
- If sand identity regresses, inspect `CreateMaterialFromRow` accept rules + `AcceptSeaSandThing`
- Optional: delete `CreateBeachSandHard` / MandatoryFallback names entirely after stable play
- Workshop extras still older backlog
