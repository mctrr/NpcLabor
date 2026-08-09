# Session handoff — 2026-08-05 polish (anti-farm / dual-duration / lean settle)

## Goal
Continue polish after feedback pass: land highest-value critique leftovers that do not need in-game play.

## Done

### E shop invest anti-farm
- `TownLaborRewards`: grant `c_invest+1` only when `TownLaborManager.CanGrantShopInvest(client.uid)`.
- Cooldown: `InvestCooldownRaw = 3 * 24 * 60` (~3 game days via `Date.GetRaw()`).
- Persist in `npclabor_townlabor.json` as `investHistory` (`TownLaborInvestRecord`: uidClient + lastInvestRaw).
- Load/Save/ClearAllRuntime keep history; settle calls `Save()`.

### E affinity quiet
- `ModAffinity(EClass.pc, aff, show: false)` — no pop spam; still applied.

### D dual-duration cleanup (region)
- Week picker labels: `探索N周` only (no `· 约M天`).
- `PreviewText` region / `RegionInfoLine`: weeks only.
- Dungeon still uses days as its primary unit (multi-day missions); tracker remains days-only.

### Lean settle / narrative
- Dispatch settle: drop empty `奖励已入邮箱` when loot summary empty.
- Co-craft finish with exp: `在共同制造中熟练了一些` (no exp number).
- Process outsource finish with exp: `完成了加工外包，手更熟了`.

### Build
```
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```
**Release OK, 0 warnings** → `E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll`

## Files touched
- `NpcLabor/TownLabor/TownLaborMission.cs` — invest history DTO
- `NpcLabor/TownLabor/TownLaborManager.cs` — cooldown API + save/load + settle Save
- `NpcLabor/TownLabor/TownLaborRewards.cs` — silent affinity + gated invest
- `NpcLabor/Dispatch/DungeonDispatchTargets.cs` — region duration unit
- `NpcLabor/Dispatch/DungeonDispatchUi.cs` — week picker labels
- `NpcLabor/Dispatch/DungeonDispatchManager.cs` — settle empty-loot line
- `NpcLabor/CoCraft/CoCraftSession.cs`
- `NpcLabor/Process/ProcessorJobSession.cs`
- `docs/mod-critique-and-gaps.md`
- `docs/CONTEXT.md`
- `.agents/handoff.md`
- this file

## Not done / next
### P0 smoke (in-game)
1. Same merchant labor twice within ~3 days → second success money/item still, **no second invest**.
2. After ~3 days raw, invest grants again.
3. Region week menu shows only `探索N周`.
4. Affinity no visible spam on town labor success.
5. Empty-loot dispatch settle does not say 奖励已入邮箱.

### P1 still open
- Localization key table + EN
- Cross-system numeric scale (A/B/D/E time-risk-reward)
- Config toggles
- Stronger result narrative (still lean)

### Explicit non-goals
- Version lock **1.14.514**
- No Auto Act / BCS / kill-quest outsourcing
- No failure-punishment redesign

## Constraints
- Co-craft default Off; busy mutual exclusion A/B/D/E
- E: no PC-base labor; max 2 concurrent; board 2–3; cash ~400; party rejoin
- D unlock: region ≥3 / dungeon ≥5
