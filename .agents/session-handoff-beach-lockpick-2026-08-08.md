# Handoff — beach pool strict + lockpick extract (2026-08-08)

## Workspace
- Mod: **NpcLabor** at `D:\work\Elin\5\2`
- Game: `E:\SteamLibrary\steamapps\common\Elin`
- Deploy: `Package\Mod_NpcLabor\NpcLabor.dll` (built 2026-08-08 00:33, ~358KB)
- Version string **1.14.514** never changes

## This turn (completed)
1. **Beach 4w only 5 kinds** (manual pool planGen **29**):
   - Explore: sand 55 / salt 25 / seaweed 12 / bait 8
   - Food: multi-fish only (weight^-1.5 non-linear)
   - No shell/coral/starfish/bread/wood
2. **Lockpick chests extract contents only**
   - `CollectLockpickChestThings` still opens treasure chests
   - `FlattenInto(..., keepContainer: false)` **no longer adds empty chest body**
   - Empty extract falls back to currency via caller
3. **CreateFromPlanEntry** special-id fallbacks hardened (mail recreate path):
   - seaweed/bait/fish/vine/branch/bark/log/mushroom/berry/fruit/flower/herb/pasture/grass/egg/gem/resin
4. Dead helper `AddRegionGatherFood` beach branch cleaned (fish-only; shell removed)
5. Release build OK (0 warn/err), game launched (`Elin.exe`)

## Current 4-region pool (source of truth: `RegionManualPools.cs`)

### 沙滩 beach
| Slot | Items |
|------|--------|
| Explore | sand 55, salt 25, seaweed 12, bait 8 |
| Food | multi fish only (inverse weight^1.5) |
| Lockpick | chest **contents** only (no chest body) |

### 森林 forest
| Slot | Items |
|------|--------|
| Explore | wood:wood 45, wood:wood_birch 20, vine 15, resin 8, branch 7, bark 5 |
| Food | mushroom 40, berry 25, fruit 15, apple 10, grape 10 |

### 山地 mountain
| Slot | Items |
|------|--------|
| Explore | mat:copper 30, mat:iron 22, mat:stone 18, sulfur 12, gem 8, rock 10 |
| Food | mushroom 60, berry 40 (half gather budget) |

### 平原 plain
| Slot | Items |
|------|--------|
| Explore | pasture 55, grass 15, flower 20, herb 10 |
| Food | flower 40, herb 30, egg 30 |

## Key files touched
- `NpcLabor/Dispatch/RegionManualPools.cs` — pools + CreateManualThing + fish roll (planGen **29**)
- `NpcLabor/Dispatch/DungeonDispatchRewards.cs` — FlattenInto chest-body fix
- `NpcLabor/Dispatch/DungeonRewardPlans.cs` — CreateFromPlanEntry fallbacks + beach food helper cleanup
- Prior: `DispatchRewardPools.cs` region scan path retired (dungeon scan kept)

## Still true from prior sessions
- Town wage formula: `(50 + c_invest) * hours * skillFactor` — do not invent new formula
- Region tracker hides haul preview; mail settles from locked plan
- Home crash was BetterCustomSprites, not NpcLabor
- Do not reintroduce biome scan for region plans

## In-game verify (player)
1. Beach 4 weeks: only 海沙/盐/多种鱼/海藻/鱼饵 (+ lockpick contents if skill)
2. Lockpick: parcel has loot pieces, **no chest item**
3. Forest/mountain/plain create real items (not empty stacks / mud)
4. Old in-progress region missions rebuild under planGen 29 on ensure

## Next if needed
- If fish rolls still produce non-fish junk, tighten `IsBeachFishCandidate` / numeric id list
- Optional: delete more unused dead region helpers (`TopUpRegionToTarget`, `AddRegionGatherFood`, Normalize* ) if file size is a pain — they are currently unreferenced from live CollectRegionThingsManual path except EnforceRegionDeliverContract on deliver
- Player may still want pool weight tweaks one-by-one in `RegionManualPools.cs` arrays only
