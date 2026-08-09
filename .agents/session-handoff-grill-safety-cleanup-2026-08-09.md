# Session handoff — grill safety + town cleanup (2026-08-09)

## Workspace
- Mod: `D:\work\Elin\5\2` · code `NpcLabor/` · version **1.14.514** (never change)
- Game: `E:\SteamLibrary\steamapps\common\Elin`
- Deploy: `Package\Mod_NpcLabor\NpcLabor.dll` (Release **green** this session after difficulty purge)
- Always read: `docs/CONTEXT.md`, this handoff via `.agents/handoff.md`
- Skills used: **grill-with-docs** + **elin-modding** (MCP available)
- Parent map: `D:\work\Elin\5\AGENTS.md`
- OS temp handoff (skill target): `C:\Users\123\AppData\Local\Temp\npc-labor-handoff-2026-08-09.md`

## What this session did

### Player / design Q&A (locked)
1. **PC+NPC co-work + uninstall safety** answered (table below).
2. Town board weekly re-roll is **mod-only** (`dayIndex/7`) — does **not** patch vanilla `Zone.UpdateQuests`.
3. **店等级** = merchant `c_invest` only (not vanilla `Trait.ShopLv`).
4. Wage `(50 + c_invest) × hours × skillFactor`.
5. Free `c_invest+1` on success when skill > c_invest and under town cap — **no gold cost**, parallel to paid vanilla invest.
6. **No invest cooldown** — weekly board + skill/cap only.
7. Home board button label **「派遣」** (2 chars); inner headers still 地区/地牢.
8. Dispatch settle: **home only, no auto party rejoin** (player re-adds).
9. Reinstall heal: `lastSeenWorldRaw` gap → auto-recall on Load; legacy raw 0 resume; no force-heal dirty old saves.
10. **Delete town labor difficulty axis** entirely.
11. Dead `wagePerShopLv` deleted.

### Code landed (Release green)

| Area | Change |
|------|--------|
| Cleanup | Removed `wagePerShopLv` from `LaborConfig` / json / rewards / CONTEXT |
| Invest cooldown | Removed `CanGrantShopInvest` / `MarkShopInvest` / `InvestHistory` / `TownLaborInvestRecord` / json field |
| Reinstall heal | `lastSeenWorldRaw` on dispatch + town save; write on `Save()` + hour tick; Load auto-recall if `now > lastSeen` |
| Dispatch label | Button/panel caption/header + `dis.msg.baseOnly` use `LaborTerms.Dispatch` (“派遣”) |
| Difficulty purge | Removed Min/MaxDifficulty, HoursForDifficulty, ClampDifficulty, DifficultyFromClient, mission.difficulty, offer.laborDifficulty, `affinityPerDifficulty`; hours random 6–12 only |
| Affinity | Fixed `AffinityBase` only (default 10) |
| Leftover | `RollLeftover(def)` qty always 1 — **final CS fix this session** in `TownLaborRewards.cs` |

### Build
```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
# -> Package\Mod_NpcLabor\NpcLabor.dll  OK (0 warn / 0 err)
```
**In-game not launched** after this batch.

## Co-work / uninstall safety (player-facing lock)

| System | Co-work | Risk |
|--------|---------|------|
| Co-craft | PC crafts, NPC assist | Safest; runtime only; no party leave |
| Process | NPC operates machine | Temp leave party; rejoin on clear; crash mid-job can leave party dirty (`WasPartyMember` not saved) |
| Town labor | Self or companion | Companion leave+rejoin on settle; designed |
| Dispatch | AFK team | **Leave party on start; settle home only, NO auto rejoin** |

**Uninstall:** not anytime-safe mid-mission. `OnDestroy` unpatches + clears runtime lists only — does **not** settle/rejoin/clear `noMove`/`isRestrained`. Safe path: recall all → quit game → disable mod. Permanent grants (affinity, free invest, exp, mail items) stay.

**Reinstall heal:** if world time advanced past `lastSeenWorldRaw` without a mod save (played with mod off), Load auto-recalls open dispatch + town missions (home/return; town party rejoin; no main success prize). Normal same-mod load resumes. Legacy `lastSeen==0` resumes (no force).

## Prior feedback still done (previous session; do not reopen)

Region/dungeon planGen 33: beach 帕露露+木, mountain sulfur ×2, forest/plain flower lock 2–3, named eggs, ban chaos mushroom 1122, dungeon vanilla ground pool, town realistic shop titles, weekly board, PC self half exp/hour.

See `.agents/session-handoff-town-titles-exp-week-2026-08-08.md`.

## Product locks (do not reopen unless user reopens)

- Region rewards: **manual 4 pools only** (`RegionManualPools`)
- Beach fish: `ThingGen.Create("fish", -1, lv)` — not full `AI_Fish.Makefish`
- Town wage: `(50 + c_invest) * hours * skillFactor`
- 店等级 = `c_invest`; vanilla ShopLv only for invest **ceiling** math
- Free invest raise, **no cooldown**
- Dispatch no party rejoin on settle/recall
- Town labor: **no difficulty axis**
- Board weekly full re-roll batch (mod offers only)
- Version **1.14.514** never change
- Out of scope: Auto Act fork, BCS, kill-quest outsourcing

## Key files touched this arc

- `NpcLabor/TownLabor/TownLaborRewards.cs` — affinity base only; leftover no difficulty (**build fix**)
- `NpcLabor/TownLabor/TownLaborJobs.cs` — difficulty helpers gone; duration random hours
- `NpcLabor/TownLabor/TownLaborMission.cs` — no difficulty; `lastSeenWorldRaw`
- `NpcLabor/TownLabor/TownLaborManager.cs` — no difficulty on offer/mission; reinstall heal; hourly Save stamp
- `NpcLabor/TownLabor/QuestNpcLaborTownLaborOffer.cs` — seed uses hours; no laborDifficulty
- `NpcLabor/Patches/TownLaborPatches.cs` — BuildOffer no difficulty
- `NpcLabor/LaborConfig.cs` + `package/labor_config.json` — no wagePerShopLv / affinityPerDifficulty / investHistory
- `NpcLabor/Dispatch/DungeonDispatchManager.cs` + `DungeonDispatchMission` save — lastSeen + no rejoin
- `NpcLabor/Dispatch/DungeonDispatchUi.cs` — 派遣 labels
- `NpcLabor/LaborTerms.cs` / `LaborText.cs` — Dispatch = 派遣
- `docs/CONTEXT.md` — glossary + slice D/E locks updated

## Residual greps (should stay clean)

Only intentional leftover comment:
- `TownLaborJobs.cs` summary: “no difficulty axis”

Should be **absent**: `ClampDifficulty`, `AffinityPerDifficulty`, `laborDifficulty`, `wagePerShopLv`, `InvestHistory`, `mission.difficulty`.

## Immediate next steps

1. **In-game verify** (never done for recent batches):
   - Home board button shows **派遣**
   - Dispatch party leave on start; settle home **without** auto rejoin
   - Town board titles realistic; free rows no “店铺帮工·” prefix
   - Weekly offers stable within week
   - PC self: half exp each hour; no bulk settle exp
   - Companion: full settle exp; free invest when skill > c_invest
   - Affinity silent ~10 (no difficulty scaling)
   - Region: 帕露露+木, sulfur, flower lock, named eggs, no chaos mushroom
   - Dungeon: biome ground pool not junk lottery
2. Optional grill follow-ups (one question at a time if user resumes grill):
   - CONTEXT still implementation-heavy vs pure glossary (skill preference)
   - Dispatch rejoin intentionally asymmetric vs town — any UI copy implying rejoin?
   - Hourly `Save()` on tick I/O cost
   - Dead API sweep beyond difficulty
3. Do **not** redecompile full assemblies; use `.research/` + Elin MCP first.

## Constraints
- Prefer ASCII; 4-space indent
- Search `.research/` before redecompile
- Manual region pools; planGen 33 already landed
- grill-with-docs: one question at a time; update CONTEXT as decisions lock; ADR only if hard/surprising/tradeoff
