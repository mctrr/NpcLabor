# Session handoff — flower qty half + meat lock/half (2026-08-09)

## Status
Release build OK → `Package\Mod_NpcLabor\NpcLabor.dll`
`ManualRegionPlanGenVersion = 36`

## Changes
### Flower quantity half (kinds stay 2-4)
- `RebalanceEvenPlainFlowers`: after collecting flower total, `flowerTotal /= 2` then redistribute onto mission flower lock.
- Applies plain + forest (explore+food flowers combined).

### Meat quantity half + kinds 2-4
- New `PlainMeatSpeciesIds` + `_missionMeatSpecies` lock via `BeginMissionMeatLock` (2-4).
- `CreateManualThing` meat path rolls only locked species.
- `RebalanceEvenPlainMeats`: strip meats, half total, recreate even stacks across lock.
- Plan repair: `CollapsePlanMeatKinds(maxKinds: 4)`; anon meat rewrite uses lock.
- No re-half on repair (only full rebuild via planGen 36).

## Files
- `NpcLabor/Dispatch/RegionManualPools.cs`
- `NpcLabor/Dispatch/DispatchRewardPools.cs`
- `NpcLabor/Dispatch/DungeonRewardPlans.cs`

## Verify
Full restart; **re-accept** region missions (planGen ≥ 36).
- Plain/forest flowers: ~half previous qty, still 2-4 kinds
- Plain meat: ~half qty, 2-4 species only
