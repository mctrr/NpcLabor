# NpcLabor research / decompile cache

> **Agents: read this before any decompile.**  
> This folder is the durable vanilla-hook cache for the NPC Labor mod.  
> Do **not** re-decompile a type that already exists here.

Last index rebuild: 2026-08-05  
Known types in cache: **126** (see `KNOWN_TYPES.txt` / `TYPE_INDEX.csv`)

## Lookup order (mandatory)

Before touching game DLLs, search in this order:

1. `docs/CONTEXT.md` and product notes under `docs/`
2. This cache:
   - `.research/types/` — curated craft / AI / trait dumps (preferred for co-craft & processors)
   - `.research/types_live/` — broader live UI / layer / recipe dumps
   - `.research/decompile_tmp/` — kept one-off dumps still useful (Quest, Zone, Thing, …)
   - `.research/extra/` — promoted session dumps (town labor, harvest, merchants, …)
3. Workspace shared research (parent of this mod):
   - `D:\work\Elin\5\.research\` — render/ACS notes (usually **not** needed for NpcLabor)
   - `D:\work\Elin\5\.tools\decompile\` — kept sample type dumps
   - `D:\work\Elin\5\.tmp_dump\` — scratch IL/C# dumps for other mods; search before recreating
4. Only if missing after a real search: decompile **one type** (or one method) into:
   - `.research/extra/<Type>.decompiled.cs` (keep if useful), **or**
   - `D:\work\Elin\5\.tmp_dump\` (ephemeral scratch)

**Hard rule:** if `KNOWN_TYPES.txt` already lists the type, open that file. Do not spawn `tmp_decompile/`, root `_tmp_*`, or a new reflect project for it.

## Quick search (pwsh)

```powershell
# From D:\work\Elin\5\2
Select-String -Path .research\KNOWN_TYPES.txt -Pattern 'LayerCraft'
# or full path index
Import-Csv .research\TYPE_INDEX.csv | Where-Object Type -eq 'LayerCraft'
# content search across cache
Get-ChildItem .research\types,.research\types_live,.research\decompile_tmp,.research\extra -Recurse -Filter *.cs |
  Select-String -Pattern 'TryStartCraft' | Select-Object -First 20
```

## Cache layout

| Path | Role | Keep? |
|------|------|-------|
| `types/` | Stable craft/AI/trait anchors | **Yes** |
| `types_live/` | Live UI/layer/recipe dumps | **Yes** |
| `decompile_tmp/` | Older kept dumps (Quest/Zone/Thing…) | **Yes** until promoted/deduped |
| `extra/` | Promoted session dumps | **Yes** |
| `AutoAct/` | Auto Act reference only | **Yes** (do not fork) |
| `TYPE_INDEX.csv` / `KNOWN_TYPES.txt` | Machine-readable inventory | **Yes** — refresh when adding dumps |
| Root `_tmp_*`, `tmp_decompile/`, reflect `bin/obj` | Session scratch | **No** — delete after use |
| `.research` root `_patch_*.py` / `_reflect_*.exe` | Old agent scaffolding | **No** |

## When re-decompile IS allowed

Only if one of these is true:

- Type is **absent** from `KNOWN_TYPES.txt` and workspace dumps
- Game updated and member names/behavior clearly drifted (note the game build)
- Existing dump is empty, truncated, or wrong type

Then dump **minimally** (one type / one method), write under `.research/extra/` or workspace `.tmp_dump/`, and refresh the index:

```powershell
# after adding a file under .research/extra
# re-run the index rebuild snippet in AGENTS.md or ask agent to refresh TYPE_INDEX.csv
```

## Workspace (outside this mod)

Parent map: `D:\work\Elin\5\AGENTS.md` (Decompile guide section).

| Path | Use for NpcLabor |
|------|------------------|
| `5/.research/` | Render/ACS — usually skip |
| `5/.tools/decompile/` | Shared sample C# dumps |
| `5/.tmp_dump/` | Large scratch; search `scale_search` etc. before new dumps |
| Game DLLs | `E:\SteamLibrary\steamapps\common\Elin\Elin_Data\Managed\Elin.dll` |

Preferred light dump: Mono.Cecil IL from parent guide. Preferred C# text: existing `.tmp_dump/Decompile.csproj` pattern — **do not** recreate full-assembly decompiles.

## Type index (best path per type)

| Type | Cache | Path | Bytes |
|------|-------|------|-------|
| Affinity | extra | .research/extra/Affinity.decompiled.cs | 4371 |
| AI_Cook | extra | .research/extra/AI_Cook.decompiled.cs | 1165 |
| AI_Goto | types | .research/types/AI_Goto.cs | 5264 |
| AI_TargetCard | types | .research/types/AI_TargetCard.cs | 1169 |
| AI_UseCrafter | types | .research/types/AI_UseCrafter.cs | 7950 |
| AIAct | types | .research/types/AIAct.cs | 11455 |
| AIWork | extra | .research/extra/AIWork.decompiled.cs | 3107 |
| AIWork_Chore | extra | .research/extra/AIWork_Chore.decompiled.cs | 503 |
| BaseTaskHarvest | extra | .research/extra/BaseTaskHarvest.decompiled.cs | 7079 |
| ButtonGrid | types | .research/types/ButtonGrid.cs | 17851 |
| Card | types_live | .research/types_live/Card.decompiled.cs | 165884 |
| CardManager | extra | .research/extra/CardManager.decompiled.cs | 2206 |
| CardRow | extra | .research/extra/CardRow.decompiled.cs | 3159 |
| Chara | types_live | .research/types_live/Chara.decompiled.cs | 245249 |
| Chara_snip | extra | .research/extra/Chara_snip.decompiled.cs | 12206 |
| CraftUtil | types | .research/types/CraftUtil.cs | 14678 |
| Date | extra | .research/extra/Date.decompiled.cs | 6835 |
| DynamicAIAct | types | .research/types/DynamicAIAct.cs | 1293 |
| ELayer | types_live | .research/types_live/ELayer.decompiled.cs | 3903 |
| Element | types | .research/types/Element.cs | 28954 |
| ElementContainer | types | .research/types/ElementContainer.cs | 18510 |
| EloMap | decompile_tmp | .research/decompile_tmp/EloMap.decompiled.cs | 10745 |
| EloMapActor | decompile_tmp | .research/decompile_tmp/EloMapActor.decompiled.cs | 2395 |
| EloMapLight | decompile_tmp | .research/decompile_tmp/EloMapLight.decompiled.cs | 129 |
| EMono | types_live | .research/types_live/EMono.decompiled.cs | 1273 |
| Expedition | types | .research/types/Expedition.cs | 2544 |
| ExpeditionManager | types | .research/types/ExpeditionManager.cs | 643 |
| FactionBranch | types | .research/types/FactionBranch.cs | 43539 |
| GameDate | decompile_tmp | .research/decompile_tmp/GameDate.decompiled.cs | 15210 |
| GoalHobby | extra | .research/extra/GoalHobby.decompiled.cs | 200 |
| GoalWork | types | .research/types/GoalWork.cs | 2688 |
| GuildMerchant | extra | .research/extra/GuildMerchant.decompiled.cs | 444 |
| Hobby | types | .research/types/Hobby.cs | 1834 |
| InvOwner | types | .research/types/InvOwner.cs | 39009 |
| InvOwnerCraft | types | .research/types/InvOwnerCraft.cs | 1701 |
| InvOwnerDraglet | types | .research/types/InvOwnerDraglet.cs | 3843 |
| ItemGeneral | types_live | .research/types_live/ItemGeneral.decompiled.cs | 4879 |
| ItemQuest | extra | .research/extra/ItemQuest.decompiled.cs | 1301 |
| ItemQuestTracker | decompile_tmp | .research/decompile_tmp/ItemQuestTracker.decompiled.cs/ItemQuestTracker.decompiled.cs | 2095 |
| Layer | types_live | .research/types_live/Layer.decompiled.cs | 13814 |
| LayerAbility | types_live | .research/types_live/LayerAbility.decompiled.cs | 7848 |
| LayerBaseCraft | types | .research/types/LayerBaseCraft.cs | 588 |
| LayerCraft | types | .research/types/LayerCraft.cs | 11187 |
| LayerCraftFloat | types | .research/types/LayerCraftFloat.cs | 5802 |
| LayerDragGrid | types | .research/types/LayerDragGrid.cs | 13383 |
| LayerEditPlaylist | types_live | .research/types_live/LayerEditPlaylist.decompiled.cs | 5363 |
| LayerInventory | types_live | .research/types_live/LayerInventory.decompiled.cs | 16401 |
| LayerList | types_live | .research/types_live/LayerList.decompiled.cs | 8136 |
| LayerMod | types_live | .research/types_live/LayerMod.decompiled.cs | 5334 |
| LayerQuestBoard | types_live | .research/types_live/LayerQuestBoard.decompiled.cs | 5728 |
| Map | extra | .research/extra/Map.decompiled.cs | 67444 |
| MATERIAL | extra | .research/extra/MATERIAL.decompiled.cs | 3171 |
| Party | types | .research/types/Party.cs | 4645 |
| Person | decompile_tmp | .research/decompile_tmp/Person.decompiled.cs | 3112 |
| Player | decompile_tmp | .research/decompile_tmp/Player.decompiled.cs | 53002 |
| Progress_Custom | types | .research/types/Progress_Custom.cs | 1479 |
| Quest | decompile_tmp | .research/decompile_tmp/Quest.decompiled.cs | 13933 |
| QuestCraft | types | .research/types/QuestCraft.cs | 3074 |
| QuestDeliver | extra | .research/extra/QuestDeliver.decompiled.cs | 5833 |
| QuestDestZone | extra | .research/extra/QuestDestZone.decompiled.cs | 1107 |
| QuestEscort | extra | .research/extra/QuestEscort.decompiled.cs | 1442 |
| QuestHunt | extra | .research/extra/QuestHunt.decompiled.cs | 256 |
| QuestManager | decompile_tmp | .research/decompile_tmp/QuestManager.decompiled.cs | 6242 |
| QuestMeal | extra | .research/extra/QuestMeal.decompiled.cs | 169 |
| QuestMusic | extra | .research/extra/QuestMusic.decompiled.cs | 1035 |
| QuestRandom | extra | .research/extra/QuestRandom.decompiled.cs | 1626 |
| QuestSupply | extra | .research/extra/QuestSupply.decompiled.cs | 592 |
| QuestSupplyBulk | extra | .research/extra/QuestSupplyBulk.decompiled.cs | 332 |
| QuestSupplySpecific | extra | .research/extra/QuestSupplySpecific.decompiled.cs | 1333 |
| QuestTaskHunt | extra | .research/extra/QuestTaskHunt.decompiled.cs | 3059 |
| Recipe | types | .research/types/Recipe.cs | 24471 |
| RecipeCard | types | .research/types/RecipeCard.cs | 12476 |
| RecipeSource | types_live | .research/types_live/RecipeSource.decompiled.cs | 4499 |
| RefChara | extra | .research/extra/RefChara.decompiled.cs | 708 |
| Region | types_live | .research/types_live/Region.decompiled.cs | 10236 |
| SourceMaterial | extra | .research/extra/SourceMaterial.decompiled.cs | 12919 |
| SourceQuest | extra | .research/extra/SourceQuest.decompiled.cs | 4171 |
| SourceRecipe | types_live | .research/types_live/SourceRecipe.decompiled.cs | 2723 |
| SourceThing+Row | extra | .research/extra/SourceThing+Row.decompiled.cs | 11228 |
| TaskCraft | types | .research/types/TaskCraft.cs | 3425 |
| TaskDig | extra | .research/extra/TaskDig.decompiled.cs | 5786 |
| TaskMine | extra | .research/extra/TaskMine.decompiled.cs | 4416 |
| Thing | decompile_tmp | .research/decompile_tmp/Thing.decompiled.cs | 60131 |
| ThingGen | decompile_tmp | .research/decompile_tmp/ThingGen.decompiled.cs | 11124 |
| Trait_shop_snip | extra | .research/extra/Trait_shop_snip.decompiled.cs | 114577 |
| TraitBarrelMaker | types | .research/types/TraitBarrelMaker.cs | 710 |
| TraitButcher | types | .research/types/TraitButcher.cs | 463 |
| TraitChef | extra | .research/extra/TraitChef.decompiled.cs | 152 |
| TraitCitizen | extra | .research/extra/TraitCitizen.decompiled.cs | 88 |
| TraitCrafter | types | .research/types/TraitCrafter.cs | 14924 |
| TraitDyeMaker | types | .research/types/TraitDyeMaker.cs | 697 |
| TraitFactory | types | .research/types/TraitFactory.cs | 660 |
| TraitGemCutter | types | .research/types/TraitGemCutter.cs | 525 |
| TraitGrindstone | types | .research/types/TraitGrindstone.cs | 710 |
| TraitInnkeeper | extra | .research/extra/TraitInnkeeper.decompiled.cs | 100 |
| TraitKiln | types | .research/types/TraitKiln.cs | 465 |
| TraitMerchant | extra | .research/extra/TraitMerchant.decompiled.cs | 335 |
| TraitMerchantBlack | extra | .research/extra/TraitMerchantBlack.decompiled.cs | 426 |
| TraitMerchantFood | extra | .research/extra/TraitMerchantFood.decompiled.cs | 108 |
| TraitMerchantGeneral | extra | .research/extra/TraitMerchantGeneral.decompiled.cs | 114 |
| TraitMerchantWeapon | extra | .research/extra/TraitMerchantWeapon.decompiled.cs | 167 |
| TraitMill | types | .research/types/TraitMill.cs | 439 |
| TraitOre | extra | .research/extra/TraitOre.decompiled.cs | 1442 |
| TraitRationMaker | types | .research/types/TraitRationMaker.cs | 453 |
| TraitSawMill | types | .research/types/TraitSawMill.cs | 410 |
| TraitScratchMachine | types | .research/types/TraitScratchMachine.cs | 525 |
| TraitSculpture | types | .research/types/TraitSculpture.cs | 416 |
| TraitSelfFactory | types | .research/types/TraitSelfFactory.cs | 269 |
| TraitSmelter | types | .research/types/TraitSmelter.cs | 474 |
| TraitSpinner | types | .research/types/TraitSpinner.cs | 411 |
| TraitStoneCutter | types | .research/types/TraitStoneCutter.cs | 422 |
| TraitWoodMill | types | .research/types/TraitWoodMill.cs | 734 |
| TreasureType | decompile_tmp | .research/decompile_tmp/TreasureType.decompiled.cs | 103 |
| UIButton | types_live | .research/types_live/UIButton.decompiled.cs | 17815 |
| UIDragGridIngredients | types | .research/types/UIDragGridIngredients.cs | 2453 |
| UIItem | types_live | .research/types_live/UIItem.decompiled.cs | 1104 |
| UIList | extra | .research/extra/UIList.decompiled.cs | 18519 |
| UIRecipeInfo | types_live | .research/types_live/UIRecipeInfo.decompiled.cs | 11109 |
| VirtualDate | decompile_tmp | .research/decompile_tmp/VirtualDate.decompiled.cs | 1695 |
| WidgetQuestTracker | decompile_tmp | .research/decompile_tmp/WidgetQuestTracker.decompiled.cs/WidgetQuestTracker.decompiled.cs | 1957 |
| Window | types_live | .research/types_live/Window.decompiled.cs | 44827 |
| WindowMenu | types_live | .research/types_live/WindowMenu.decompiled.cs | 3416 |
| WorkSession | types | .research/types/WorkSession.cs | 857 |
| WorkSummary | types | .research/types/WorkSummary.cs | 1116 |
| Zone | decompile_tmp | .research/decompile_tmp/Zone.decompiled.cs | 98639 |
| ZoneEvent | types_live | .research/types_live/ZoneEvent.decompiled.cs | 1860 |

