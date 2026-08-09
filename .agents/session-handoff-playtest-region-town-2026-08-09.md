# Handoff — region/town playtest residual bugs (2026-08-09)

## Purpose for next session
Diagnose and fix **residual region-dispatch + town-labor playtest failures** that still appear after planGen **34** code landed. Do **not** restart from the old planGen 31 backlog; most requested levers already exist on disk. Root-cause why playtest still fails, patch the leak paths, bump planGen if plan shape changes, Release build, re-verify in game.

## Workspace
- Mod root: `D:\work\Elin\5\2`
- Code: `NpcLabor/` (BepInEx/Harmony)
- Game: `E:\SteamLibrary\steamapps\common\Elin`
- Deploy: `Package\Mod_NpcLabor\` (DLL + package.xml)
- Version string **1.14.514** — never change
- Always read first:
  1. `docs/CONTEXT.md`
  2. `.agents/handoff.md` → this file
  3. Prior safety locks: `.agents/session-handoff-grill-safety-cleanup-2026-08-09.md`
  4. Earlier region balance intent: `.agents/session-handoff-region-balance-feedback-2026-08-08.md`
- Decompile policy: `.research/README.md` + parent `D:\work\Elin\5\AGENTS.md`
- OS temp copy (skill target): `%TEMP%\npc-labor-handoff-playtest-2026-08-09.md`

## Player playtest feedback (latest, authoritative)

These are the open player bugs for the next session:

| # | Area | Symptom | Expected |
|---|------|---------|----------|
| 1 | Plain region | 炼金灰的蛋 (alchemy-ash egg) still appears | Domestic eggs only; never ash/alchemy/monster eggs |
| 2 | Plain (and likely forest) | Flower kinds too many | Lock **2–4** flower kinds per mission |
| 3 | Plain | Herb kinds too many | Lock **2–4** herb kinds per mission |
| 4 | Beach | Still no 帕露露 (`palulu`) | Fixed coastal staple; must appear every beach haul |
| 5 | Beach fish | Max-weight / heavy fish still refresh? | Heavy fish rare (~÷10); confirm dampen still works |
| 6 | Mountain | 硫磺 qty still too low | Sulfur ≈ stone count |
| 7 | Town labor | Free shop invest raise has no visible log | Log like `xxx店铺更受欢迎了。。。` on invest +1 |

## What is already on disk (planGen 34)

Source timestamps ~2026-08-09 15:44–15:47. Treat as **attempted fix, not verified green**.

### Region pools / plan gen
- `ManualRegionPlanGenVersion = 34` in [`NpcLabor/Dispatch/RegionManualPools.cs`](NpcLabor/Dispatch/RegionManualPools.cs)
- Current tables (edit here only):

| Region | Explore | Food |
|--------|---------|------|
| Beach | sand 48 / salt 22 / wood:palm 14 / seaweed 10 / bait 6 | fish 78 / palulu 22 |
| Forest | wood:wood 34 / vine 34 / birch 12 / flower 10 / resin 5 / branch 3 / bark 2 | mushroom 36 / mushroom_rare 6 / berry 20 / fruit 12 / apple 10 / grape 8 / flower 8 |
| Mountain | mat:stone 30 / sulfur 30 / copper 14 / iron 10 / rock 12 / gem 6 | mushroom 60 / berry 40 |
| Plain | pasture 70 / flower 16 / grass 8 / herb 6 | meat 34 / flower 28 / egg 22 / herb 16 |

Hard guarantees already coded in `CollectRegionThingsManual`:
- `EnsureBeachPalulu` — force palulu qty ≈ `max(1, foodTarget/5)` if missing
- `EnsureMountainSulfurParity` — top sulfur up to stone count
- Beach fish: `DistributeFishPool` + `RollBeachFish` lv cap + heavy re-roll (~1/10 keep)

### Flower / herb lock
In [`NpcLabor/Dispatch/DispatchRewardPools.cs`](NpcLabor/Dispatch/DispatchRewardPools.cs):
- `BeginMissionFlowerLock` / `BeginMissionHerbLock` → `PickMissionIdSubset(..., minKinds:2, maxKinds:4)`
- `CreatePlainFlower` / `CreatePlainHerb` prefer mission lock arrays
- `RebalanceEvenPlainFlowers` in `DungeonRewardPlans.cs` also uses lock

**Likely residual causes if playtest still shows too many kinds:**
- Plan **recreate / mail materialize** path bypasses lock (`EndMissionFlowerLock` already cleared, or exact planned flower_/herb_ ids expand)
- Old missions with `planGenVersion < 34` still carry many kinds until rebuild
- Flower lock comment in RegionManualPools still says “2-3” while code allows 2–4 (cosmetic only)

### Alchemy-ash eggs
In `DispatchRewardPools.CreateNamedEgg` / `IsForbiddenPlainEgg`:
- Species whitelist: chicken / chicken_wild / bird / sheep / cow / pig (+ goat/lamb/bull/ox/cattle)
- Ban ash / alchemy / mana / ether / chaos materials and 炼金灰 name
- Explicit: never `TryMakeRandomItem` on eggs

**Likely residual causes:**
- Plan recreate path (`DungeonRewardPlans`) may restore `egg:`/`refCard` without re-running forbid filter
- `MakeFoodFrom` may leave material/name as 炼金灰 even when ref is domestic — filter may miss if name/mat differ from expected
- Stale planned entries from older gens
- Fallback plain egg without successful MakeFoodFrom still slipping through some path

### Town invest log
Already implemented:
- CN: `reward.investRaised` = `{0}店铺更受欢迎了。。。`
- EN: `{0}'s shop is more popular now...`
- Added in `TownLaborRewards` **before** money/plat when free `c_invest+1` succeeds
- Settle prints `rewardLog.Take(3)` via `Msg.Say`

**Likely residual causes if player saw no log:**
1. Invest never raised (skill ≤ c_invest, or not CanInvest trait, or town cap → only `reward.investTownCap`)
2. Player tested build **before** string/order landed
3. Less likely: Take(3) truncation — invest is first, so money+plat still leave invest visible
4. Need separate dedicated `Msg.Say` for invest (if settle line is too easy to miss / scrolled)

## Product locks (do not reopen)
- Region rewards: **manual 4 pools only** (`RegionManualPools`) — no biome scan / Normalize* / TopUp*
- Beach fish: `ThingGen.Create("fish", -1, lv)` — not full `AI_Fish.Makefish`
- Town wage: `(50 + c_invest) * hours * skillFactor`
- 店等级 = `c_invest`; vanilla ShopLv only for invest ceiling
- Free invest raise, **no cooldown**
- Dispatch settle: home only, **no auto party rejoin**
- Town labor: **no difficulty axis**
- Board weekly full re-roll batch (mod offers only)
- Version **1.14.514** never change
- Out of scope: Auto Act fork, BCS, kill-quest outsourcing

## Files most relevant next
1. [`NpcLabor/Dispatch/RegionManualPools.cs`](NpcLabor/Dispatch/RegionManualPools.cs) — pools, EnsureBeachPalulu, EnsureMountainSulfurParity, planGen 34→35 if needed
2. [`NpcLabor/Dispatch/DispatchRewardPools.cs`](NpcLabor/Dispatch/DispatchRewardPools.cs) — flower/herb lock, CreateNamedEgg ban, fish roll
3. [`NpcLabor/Dispatch/DungeonRewardPlans.cs`](NpcLabor/Dispatch/DungeonRewardPlans.cs) — plan lock/recreate/repair; beach palulu + mountain sulfur post-plan hardens already exist ~L1943+ / ~L1991+
4. [`NpcLabor/Dispatch/DungeonDispatchRewards.cs`](NpcLabor/Dispatch/DungeonDispatchRewards.cs) — deliver path if mail drops lock
5. [`NpcLabor/TownLabor/TownLaborRewards.cs`](NpcLabor/TownLabor/TownLaborRewards.cs) + [`NpcLabor/TownLabor/TownLaborManager.cs`](NpcLabor/TownLabor/TownLaborManager.cs) settle — invest log visibility
6. [`NpcLabor/LaborText.cs`](NpcLabor/LaborText.cs) — `reward.investRaised` already present

## Suggested fix order (next session)
1. **Confirm deployed DLL** matches planGen 34 source (build/deploy if game still on older DLL). Bump to **35** only if plan payload shape changes.
2. **Plain egg ash leak** — instrument `CreateNamedEgg` + plan recreate; destroy+reroll any forbidden egg after materialize; never keep plan line that fails ban.
3. **Flower/herb diversity** — ensure lock is active during plan build **and** recreate; optionally persist locked id list on mission/plan so mail path cannot free-roll; after materialize, collapse extras into locked set.
4. **Beach palulu** — check BepInEx log for `dispatch beach hard-palulu FAILED`; verify create id (`palulu` / `790`); if Ensure runs only on live collect but plan path skips, mirror hard-ensure on plan lock (there is already a plan-side palulu block ~L1943 — verify it always runs for beach).
5. **Heavy fish** — re-check `IsHeavyBeachFish` / `FishWeightScore` thresholds against real max-weight fish; tighten if still common.
6. **Mountain sulfur** — verify stone counter is not under-counting (ore false-positive skip?) or sulfur create fails; log `dispatch mountain sulfur-parity` / hard-sulfur lines.
7. **Town invest log** — if raise happens silently, force a standalone `Msg.Say(investRaised)` in addition to rewardLog, or reorder/ensure settle always includes it; verify raise condition with console/self-work high skill.

## Build / deploy
```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
# close game if DLL locked
dotnet build NpcLabor\NpcLabor.csproj -c Release
# deploy Package\Mod_NpcLabor\NpcLabor.dll then full game restart
```
Old region missions need `planGenVersion >= ManualRegionPlanGenVersion` or they keep stale tables.

## Debug consoles (already wired)
- `NpcLaborHarvest` / `NpcLaborComplete` / `NpcLaborCompleteOne` / `NpcLaborExp`
- `NpcLaborTownComplete` alias for town labor

## Modified files this arc (source on disk; playtest residual)
- `NpcLabor/Dispatch/RegionManualPools.cs`
- `NpcLabor/Dispatch/DispatchRewardPools.cs`
- `NpcLabor/Dispatch/DungeonRewardPlans.cs`
- `NpcLabor/LaborText.cs`
- `NpcLabor/TownLabor/TownLaborRewards.cs`
- (earlier same-day safety cleanup still landed; see grill handoff)

**This wrap session:** handoff only — no further source edits beyond writing this document + pointer.

## Do not
- Reintroduce biome scan / Normalize* / TopUp*
- Call full `AI_Fish.Makefish`
- Change version **1.14.514** or town wage formula
- Touch co-craft / processor unless regression
- Blind full-assembly redecompile (search `.research/` / parent dumps / Elin MCP first)

## Suggested skills
- **elin-modding** — confirm live ids for palulu / egg material / sulfur create; inspect MakeFoodFrom egg material bleed
- **diagnose** — plain ash egg + beach missing palulu (create vs plan-path skip)
- **handoff** — after residual pass ships and in-game verifies

## Residual risk
- Player may still be running pre-34 DLL; always confirm deploy first
- ThreadStatic flower/herb locks can be empty on recreate if Begin* not called
- PlanGen bump required or old missions keep bad eggs/flowers
- Invest log only fires when skill > c_invest **and** under town cap **and** trait allows invest — document that in settle UX if still “missing”
- Heavy-fish threshold may not match vanilla SelfWeight scale for “max weight” fish the player cares about
