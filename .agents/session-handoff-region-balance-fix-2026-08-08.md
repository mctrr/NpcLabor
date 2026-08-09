# Handoff — region balance fix (2026-08-08)

## Goal
Ship player region-dispatch balance feedback on planGen **32**. Release build OK.

## Done
Deployed: `E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll` (Release, 0 warn/0 err)

### Pools (`RegionManualPools.cs`, planGen **32**)
- **Beach explore:** sand 48 / salt 22 / wood:palm 14 / seaweed 10 / bait 6
- **Beach food:** fish 78 + palulu 22; fish heavy dampen (~1/10 keep heavy; lv cap 18; skill cap 40)
- **Forest explore:** wood 34 / vine 34 / birch 12 / flower 10 / resin 5 / branch 3 / bark 2
- **Forest food:** mushroom 36 / mushroom_rare 6 / berry / fruit / apple / grape / flower
- **Mountain:** stone 30 / sulfur 28 / copper 14 / iron 10 / rock 12 / gem 6
- **Plain explore:** pasture 70 / flower 16 / grass 8 / herb 6
- **Plain food:** meat 34 / flower 28 / egg 22 / herb 16
- Per-unit roll for flower/mushroom/meat/fruit/berry/herb variety
- Non-beach CreateManualThing("sand*") → stone (no 海沙 leak)
- meat → CreateNamedMeat random chicken/sheep/cow/putty/bird

### Mail name
- `LaborText` CN: **派遣收获** / partial/recall/loot/package likewise; EN: Dispatch harvest…
- Deliver titles no longer prefix 地牢探索

### Pick / gather specialty
- `SkillFishing = 245`
- `PickMaxHarvestMembers` region score: `explore*2 + specialty*3 + gather + lockpick`
  - beach: dig/fish; forest: lumber; mountain: mine/dig; plain: gather
- `AggregateRegionGather` + mission start gather: beach dig/fish added

### Deliver contract
- Non-beach `EnforceRegionDeliverContract` strips any sea sand

## In-game verify (player)
1. Beach: lighter fish majority; palulu + palm wood; sand/salt still present
2. Mountain: no 海沙; stone≈sulfur > ores
3. Forest: mushrooms (some rare), flowers, wood≈vine
4. Plain: meat + mixed flowers + more pasture
5. Mail parcel name = 派遣收获
6. Max-harvest pick prefers region specialty skills

## Do not
- Biome scan / Normalize* / wage / version **1.14.514** / Makefish full

## Residual
- Fish weight threshold (SelfWeight>=18 / LV>=14) may need live tune
- palm wood identity depends on CreateWoodLog("palm") material pin
- Old missions rebuild on planGen 32 ensure
