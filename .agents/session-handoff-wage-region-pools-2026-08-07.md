# Handoff — wage formula + manual region pool table (2026-08-07 b)

## This turn
1. Town wage restored to player formula: **(50 + c_invest) * hours * skillFactor**
   - Board/lock/settle all use `client.c_invest` (投资等级), NOT vanilla ShopLv / difficulty
   - Invest-raise gate still uses raw ShopLv
   - invest=2, 12h full skill => (50+2)*12 = **624** gold
2. Region pools simplified to one editable table in `NpcLabor/Dispatch/RegionManualPools.cs`
   - planGen **27**
   - Only edit the `BeachExplore/BeachFood/Forest*/Mountain*/Plain*` arrays
   - Lockpick chests remain outside these lists

## Current 4-region pool (for player review)

### 沙滩 beach
Explore: sand 55, salt 20, shell 8, shell2 5, coral 4, seaweed 5, bait 3
Food: fish 50, shell 20, shell2 10, seaweed 15, bait 5

### 森林 forest
Explore: wood:wood 45, wood:wood_birch 20, vine 15, resin 8, branch 7, bark 5
Food: mushroom 35, berry 25, fruit 20, apple 10, grape 10

### 山地 mountain
Explore: mat:copper 30, mat:iron 22, mat:stone 18, sulfur 12, gem 8, rock 10
Food: mushroom 60, berry 40 (half gather budget)

### 平原 plain
Explore: pasture 55, grass 15, flower 20, herb 10
Food: flower 40, herb 30, egg 30

## Build
Release OK, game restarted. DLL ~latest.

## Next
User will manually tweak the four pool arrays one by one.
