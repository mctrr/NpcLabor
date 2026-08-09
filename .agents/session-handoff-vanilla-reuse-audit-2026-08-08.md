# Session handoff — vanilla-API reuse audit (2026-08-08)

## Goal
Audit NpcLabor for custom logic that reimplements or can be simulated by vanilla Elin APIs. Use Elin MCP only (no local ILSpy). Report ranked findings; do **not** implement replacements unless asked.

## Method
- MCP index version: **23.333.1**
- Tools: `elin_find` / `elin_outline` / `elin_read` / `elin_callers` / `elin_ids`
- Cross-checked against `NpcLabor/Dispatch/*`, TownLabor, CoCraft, Process

## Already using vanilla correctly
| Area | Vanilla used |
|------|--------------|
| Mail delivery | `ThingGen.CreateParcel` + `FactionBranch.PutInMailBox` |
| Currency | `ThingGen.CreateCurrency` (town money path) |
| Lockpick / boss chest | `ThingGen.CreateTreasure` + `CreateTreasureContent` + `TreasureType.RandomChest` / `BossNefia` |
| Dungeon filter lottery | `ThingGen.CreateFromFilter` / `CreateFromCategory` / `CreateFromTag` |
| Processor conversion | `TraitCrafter.Craft` via throwaway `AI_UseCrafter` shell |
| Co-craft skill/exp | Harmony on `Card.Evalue` + `ElementContainer.ModExp` (no craft engine fork) |
| Affinity / invest | `Chara.ModAffinity`, `c_invest` |
| Named meat | `Card.MakeFoodFrom` / `MakeRefFrom` (wrapper only) |

## Ranked findings

### Must replace (or collapse hard)
1. **Material chunk factory sprawl** — `RewardMaterialFactory.cs` (~79KB)
   - Custom: `CreateMaterialThing`, `CreatePinnedMaterialThing`, `CreateByMaterialId`, `CreateBeachSand*` (4 paths), `CreateMetalHard`, `CreateWoodLog`, `CreateMountain*`, many `Is*Like` detectors
   - Vanilla: region dig is literally  
     `ThingGen.CreateRawMaterial(row)` + `ChangeMaterial(row.alias)` (`Map.MineFloor` region branch)  
     and material by-products: `SourceMaterial.Row.CreateByProduct`
   - `CreateRawMaterial` itself is only `Create(m.thing)` + `ChangeMaterial(m.id)`
   - Keep **one** thin helper: resolve material row → CreateRawMaterial → optional carrier pin. Drop multi-carrier fallback forests.
   - Reason custom grew: sand_sea / mud leaks / gold lottery — still fixable with strict post-check, not 4 dig paths.

2. **Beach fish pool reinvents fishing** — `RegionManualPools.cs` manual id list + weight^1.5
   - Vanilla: `AI_Fish.Makefish` already does `ThingGen.Create("fish", -1, lv)` where `lv = rnd(fishingSkill*2)+1`, plus rare junk table
   - User wants multi-fish + heavier rarer: closest vanilla is **Create("fish", -1, skill-based lv)** (origin resolves species by lv). Manual id probe list is optional; keep weight filter only if lv path is too narrow.
   - Do **not** call full `Makefish` (PC fever / fishArtifact / zone side effects).

3. **Dead region plan normalizers** still live in `DungeonRewardPlans.cs` (~117KB)
   - Unreferenced / superseded by `CollectRegionThingsManual`:  
     `TopUpRegionToTarget`, `NormalizeBeach/Forest/Mountain/PlainThings`, `AddRegionGatherFood`
   - Also soft-dead scan path in `DispatchRewardPools`: `RollRegionScanned` / `RollRegionDrop` → only `FallbackRegionBasic`
   - Action: delete dead region staples code; keep plan lock + CreateFromPlanEntry only if still used by dungeon/mail identity.

### Should replace
4. **KeepFoodFresh / ForceFreshProduceMaterial**
   - No vanilla “set fresh” API; only `Card.decay` prop + `Decay` / `DecayNatural`
   - Custom ForceFresh (`ChangeMaterial("fresh")` or DefaultMaterial) is OK for mail edible contract
   - Simplify to: `t.decay = 0` + `ChangeMaterial(t.DefaultMaterial)` when food; drop long alias guess lists

5. **CreateNamedMeat**
   - Already calls vanilla `MakeFoodFrom`; species fallback id lists are local policy — keep thin wrapper, drop extra probe noise if plain food path covers plain eggs

6. **RollLockpickChestCurrency** (`DispatchRewardPools`)
   - Still used as fallback when treasure flatten yields nothing
   - Vanilla RandomChest already rolls money/money2/plat/medal inside `CreateTreasureContent`
   - Prefer: empty flatten → re-roll one `CreateTreasure` contents, or single `CreateCurrency`; drop custom 85/12/3 table if chest path is stable

7. **CreateBeachSand** multi-path
   - Should be: resolve `sand_sea` row → `CreateRawMaterial(row)` → `ChangeMaterial(row.alias)` once; reject non-sea-sand; one hard fallback `Create("chunk", row.id)` + ChangeMaterial
   - Delete Hard / MandatoryFallback / DigFloorMaterial duplicates if single path works

### Keep custom (with reason)
| Custom | Why keep |
|--------|----------|
| Region manual 4 pools | Product lock: no biome scan; player-set lists |
| FlattenInto lockpick | Product lock: mail **contents only**, no chest body |
| Co-craft eff rewrite | No vanilla multi-NPC craft skill merge |
| Town wage `(50+invest)*hours*skillFactor` | Product lock; not a vanilla shop wage |
| Mission/quest tracker glue | Mod systems (QuestNpcLabor*) |
| Dungeon ground roll via CreateFromFilter | Already vanilla; custom is only qty/filter mix |
| Processor AI_NpcProcess | Orchestration only; conversion is TraitCrafter.Craft |
| Plan freeze + HarvestProgressPercent | Dispatch UX / anti-reward-drift |

## Co-craft / town labor / processor
- **No high-value reinvent.** Town labor money uses CreateCurrency/CreateParcel; processor uses Craft; co-craft patches Evalue/ModExp only.
- Do not chase shop invest APIs for wage (user locked formula).

## What not to do
- Do not reintroduce region biome scan
- Do not change wage formula or version **1.14.514**
- Do not launch game unless asked
- Do not implement mass factory collapse without explicit user OK (large blast radius on sand/ore identity)

## Suggested next implementation order (if user says go)
1. Collapse material create to CreateRawMaterial-first single helper; retest sand/copper/iron/wood/sulfur
2. Beach fish → `ThingGen.Create("fish", -1, skillLv)` (+ optional weight filter)
3. Delete dead Normalize*/TopUp*/AddRegionGatherFood + soft scan stubs
4. Lockpick empty fallback → treasure re-roll not custom currency table
5. Build Release only; no game start unless asked

## Key files
- `NpcLabor/Dispatch/RewardMaterialFactory.cs` — prime collapse target
- `NpcLabor/Dispatch/DungeonRewardPlans.cs` — dead region normalizers
- `NpcLabor/Dispatch/RegionManualPools.cs` — fish list
- `NpcLabor/Dispatch/DungeonDispatchRewards.cs` — treasure + FlattenInto (mostly good)
- `NpcLabor/Dispatch/DispatchRewardPools.cs` — scan stubs + currency fallback
