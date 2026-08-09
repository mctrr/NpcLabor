# Session handoff — town labor polish + region/dungeon feedback (2026-08-08)

## Workspace
- Mod: `D:\work\Elin\5\2` · code `NpcLabor/` · version **1.14.514** (never change)
- Game: `E:\SteamLibrary\steamapps\common\Elin`
- Deploy: `Package\Mod_NpcLabor\NpcLabor.dll` (Release **green** this session)
- Always read: `docs/CONTEXT.md`, this handoff via `.agents/handoff.md`
- Skill: **elin-modding** (MCP)

## Product locks (do not reopen)
- Region rewards: **manual 4 pools only** (`RegionManualPools`, planGen **33**)
- Beach fish: `ThingGen.Create("fish", -1, lv)` — not full `AI_Fish.Makefish`
- Lockpick mail: contents only
- Town wage: `(50 + c_invest) * hours * skillFactor`
- Out of scope: Auto Act fork, BCS, kill-quest outsourcing

## Player feedback — this batch (status)

| # | Feedback | Status |
|---|----------|--------|
| 1 | Beach 帕露露木 but no 帕露露 | **Done** — food pool + guarantee ≥1 palulu / id 790 |
| 2 | Mountain 硫磺 still low → ×2 | **Done** — weight 28→56, planGen 33 |
| 3 | Forest flowers too many kinds → 2–3 | **Done** — `BeginMissionFlowerLock` |
| 4 | Plain flowers same; egg should be named like meat | **Done** — flower lock + `CreateNamedEgg` |
| 5 | Ban chaos mushroom (1122 furniture) | **Done** — ban helpers on create/roll paths |
| 6 | Dungeon loot junk → vanilla ground drop | **Done** — `biome.spawn.GetRandomThingId` + `CreateFromFilter` |
| 7 | Town labor PC exp half NPC each hour | **Done** — hourly PC self `ExpPerHour/2`; settle skips bulk for PC self |
| 8 | Realistic shop titles (鱼店需要人手) | **Done** — `ResolveBoardTitle` wired board/UI/mission + i18n keys |
| 9 | Board refresh weekly not daily | **Done** — `BoardSeed` week bucket (`dayIndex/7`) |

## Code changes this session

### Town labor (primary remaining work)
- [`NpcLabor/TownLabor/TownLaborManager.cs`](NpcLabor/TownLabor/TownLaborManager.cs)
  - `BoardSeed`: calendar week (`year*360 + (month-1)*30 + day) / 7`) instead of daily
  - `TryStartSelf` / `TryStart`: `jobTitle = ResolveBoardTitle(client, def)`
  - Hour tick after SP drain: PC self `ModExp(skillId, ExpPerHour/2)` each worked hour
- [`NpcLabor/TownLabor/TownLaborRewards.cs`](NpcLabor/TownLabor/TownLaborRewards.cs)
  - Success settle exp: **companions only** full `ExpPerHour * hours`
  - PC self success: **no** bulk settle exp (already hourly half)
  - Recall still tiny consolation for both
- [`NpcLabor/TownLabor/TownLaborJobs.cs`](NpcLabor/TownLabor/TownLaborJobs.cs)
  - `J(key, params)` overload for `shop.generic.need`
  - `ResolveBoardTitle` (already present) maps fish/meat/inn/… traits → shop need keys
- [`NpcLabor/TownLabor/QuestNpcLaborTownLaborOffer.cs`](NpcLabor/TownLabor/QuestNpcLaborTownLaborOffer.cs)
  - `GetTitle` uses mission.jobTitle / `ResolveBoardTitle`
  - `TitlePrefix`: free rows empty (no "店铺帮工·"); active rows short "进行中"
- [`NpcLabor/TownLabor/TownLaborUi.cs`](NpcLabor/TownLabor/TownLaborUi.cs)
  - Accept / pick / confirm headers use `ResolveBoardTitle`
- [`NpcLabor/LaborText.cs`](NpcLabor/LaborText.cs)
  - CN/EN `job.shop.*.need` keys (fish/meat/fruit/bread/milk/booze/food/inn/kitchen/book/scholar/smith/general/junk/souvenir/generic)

### Region / dungeon (prior agent + nullable fix)
- [`NpcLabor/Dispatch/RegionManualPools.cs`](NpcLabor/Dispatch/RegionManualPools.cs) planGen **33**, sulfur 56, palulu guarantee, flower lock, named egg, ban chaos
- [`NpcLabor/Dispatch/DispatchRewardPools.cs`](NpcLabor/Dispatch/DispatchRewardPools.cs) flower lock, ban helpers, named egg, vanilla dungeon ground path; nullable CS8602 fixed this session
- [`NpcLabor/Dispatch/DungeonDispatchRewards.cs`](NpcLabor/Dispatch/DungeonDispatchRewards.cs) / [`DungeonRewardPlans.cs`](NpcLabor/Dispatch/DungeonRewardPlans.cs) junk filter → dungeon/eq; egg:ref tokens

## Exp model (town)
| Path | Hourly | Success settle |
|------|--------|----------------|
| PC self | `ExpPerHour / 2` each worked hour | 0 (avoid double) |
| Companion | none | `ExpPerHour * hours` |
| Recall (either) | n/a | `max(6, hours)` consolation |

Default `ExpPerHour = 12` → PC ~6/h, companion 12*h on finish.

## Build
```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
# -> Package\Mod_NpcLabor\NpcLabor.dll  OK (0 warn / 0 err)
```
Game not launched this session.

## In-game verify checklist (next session)
1. **Town board titles**: fishmonger → "鱼店需要人手" (not 店铺帮工·食品店员); free row has no generic prefix
2. **Weekly refresh**: same town offers stable within week; change after 7 in-game days
3. **PC self exp**: each hour tick grants half-rate life skill; full job should not dump bulk exp again
4. **Companion exp**: still full settle only
5. **Beach**: 帕露露木 + at least one 帕露露 in food parcel
6. **Mountain**: 硫磺 roughly stone-tier volume (weight 56)
7. **Forest/plain**: only 2–3 flower kinds per mission; eggs named (chicken/etc.)
8. **Mushroom zones**: no 混沌蘑菇 / id 1122 furniture
9. **Dungeon explore**: ground haul from biome spawn filter (not junk lottery)

## Open / residual
- No automated tests
- Optional: cleanup root `_tmp_patch_*.py` if still present
- Companion workers: user wording could be read as "PC always gets half when any town job runs"; implemented as **PC self only** half hourly. Revisit if they want PC exp while NPC works.
- Tracker quest title still uses `town.ui.header` with jobTitle — should already show realistic title once mission stores ResolveBoardTitle

## Constraints
- Prefer ASCII in new code; 4-space indent
- Search `.research/` before redecompile
- Version string never changes
- Manual region pools only; planGen already 33
