# Session handoff — playtest residual fix + Release build (2026-08-09)

## Status
**Code fixed + Release build OK.** Deployed to:
`E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll`

Version remains **1.14.514**.

## Player bugs addressed
1. Plain alchemy-ash eggs → `SanitizePlainEggs` + stronger `IsForbiddenPlainEgg` + chicken fallback; plan eggs → `SanitizePlanEggs`
2. Flower kinds too many → `RebalanceEvenPlainFlowers` now called after plain/forest top-up; plan collapse ≤4
3. Herb kinds too many → **added missing** `RebalanceEvenPlainHerbs` (was called but not defined); plan collapse ≤4
4. Beach missing 帕露露 → `CreateBeachPaluluFruit` only (no ForestFruit fallback); hard qty ≥2; plan repair force
5. Heavy/max-weight fish → lv cap 12, heavy keep ~1/20, weight threshold ≥12 or lv≥10
6. Mountain sulfur low → pool weight 30=stone + `EnsureMountainSulfurParity` + plan repair
7. Town invest log → dedicated `Msg.Say(reward.investRaised)` + settle rewardLog Take(5)

## Key files
- `NpcLabor/Dispatch/RegionManualPools.cs` — planGen **35**, palulu/eggs/herbs/flowers/sulfur/fish
- `NpcLabor/Dispatch/DungeonRewardPlans.cs` — `RebalanceEvenPlainHerbs`, plan repair collapse/sanitize
- `NpcLabor/Dispatch/DispatchRewardPools.cs` — egg forbid + fresh mat; fixed broken try/catch
- `NpcLabor/TownLabor/TownLaborRewards.cs` — invest Msg.Say
- `NpcLabor/TownLabor/TownLaborManager.cs` — settle log take 5

## Build
```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```
Result: success, 0 warnings, 0 errors. DLL written to game Package folder.

## In-game verify (next)
Full game restart. Old region missions need **new accept** (planGen ≥ 35 rebuild) to pick up locks.
- Plain: no 炼金灰蛋; flowers/herbs only 2–4 kinds
- Beach: always 帕露露; heavy fish rare
- Mountain: sulfur ≈ stone
- Town free invest raise: chat log shows `xxx店铺更受欢迎了。。。`

## Do not reopen
- Manual 4 pools only; no biome scan/Normalize/TopUp rewrites
- Beach fish = `ThingGen.Create("fish", -1, lv)` not full AI_Fish
- Town wage formula / version string
