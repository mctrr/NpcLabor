# CONTEXT — NPC Labor

## What this is

Elin mod workspace for **NPC Labor**: make ally life skills useful without leaving vanilla power budget.

**Current code scope: slice A co-craft + slice B processor (加工) + slice D dungeon dispatch + slice E town shop labor.**

Product glue (v1): systems link via **skills** (home co-craft / town work both grant life-skill exp), not a shared economy.
Feedback tone: lean player messages; no spreadsheet dumps; single duration unit.

## Domain terms

| Term | Player name (CN) | Meaning |
|------|------------------|---------|
| Co-craft | 共同制造 / 协助 | PC crafts at workbench with an NPC assistant |
| Assistant pin | — | Manual selection of co-craft NPC from craft UI |
| Auto assistant | — | Fallback: best relevant recipe skill among party/residents |
| `eff` | — | Effective craft skill: `pc + Floor(npc * 0.5)` on recipe req skill |
| Session | — | Temporary co-craft state opened on craft start; always cleared on complete/cancel |
| SP assist | — | Co-craft stamina cut via `GetCostSp` |
| Processor job | **加工** | PC machine conversion; operator self / auto / pin NPC; product on ground |
| Process button | — | `LayerDragGrid` chip for operator mode (not LayerCraft) |
| Dispatch | 派遣 | Home board entry covering region outing + dungeon explore |
| Dungeon explore | 地牢探索 | AFK send resident/party to known nearby dungeon; mailbox rewards; unlock branch **lv≥5** |
| Region dispatch | 地区派遣 | AFK region outing; unlock branch **lv≥3** |
| Town shop labor | 店铺帮工 | Non-base town quest-board job rows; 1 NPC works a merchant post for hours |
| Invest level | 店等级 / 投资等级 | Merchant client `c_invest`. **This is the only player-facing shop level.** Wages, skill-pay bar, and skill-warn UI all use it. |
| Vanilla ShopLv | — (internal) | Engine `Trait.ShopLv` = `development/10 + c_invest*(100+guildBonus)/100 + 1`. **Not** 店等级. Used only inside the soft town invest-raise **ceiling** math (not the skill > level check). _Avoid calling this shop level in player text or product docs._ |

Player-facing names live in `NpcLabor/LaborTerms.cs` (via `LaborText` CN/EN). Balance numbers live in `NpcLabor/package/labor_config.json` (`LaborConfig`). Critique / polish backlog: `docs/mod-critique-and-gaps.md`.

## Locked v1 product rules (slice A)

- UI: craft / manufacturing **panel 协助 button** (not console-first).
- Presence: teleport assistant to **PC cell** on craft start when valid; do not restore after.
- Skill: duration + quality/fail use `eff`.
- Exp: PC vanilla path; NPC share raw*0.5; human finish messages.
- SP: reduce GetCostSp by Floor(npcSkill*0.25), min 1.
- Pin non-save; party + residents; skill <= 0 hidden.
- Skill UI: `制造 X / 有效 Y`.
- Version **1.14.514** never change.
- Uninstall hygiene: **recall all dispatch + town labor before disabling the mod** (do not rely on tracker strip/rebuild).

- Boundary: do not fork Auto Act; do not merge into BetterCustomSprites.

## Workshop metadata (2026-08-07)

- Internal code/assembly/folder name stays `NpcLabor`; only the publish surface changes.
- Steam Workshop display title: **NPC 帮工 (NPC Labor)**.
- Package id: `mctrr.npclabor`; author: `mctrr`.
- Keep the package `<id>` stable after the first Workshop upload; Elin matches updates by id.

## Slice B (MVP implemented)

**In scope:**
- Machines: non-factory drag-grid processors (Saw/Mill/WoodMill/StoneCutter/Spinner/GemCutter/Kiln/Smelter/…).
- One conversion cycle per tick via `TraitCrafter.Craft(AI_UseCrafter)` with `recipe == null`.
- Flow: set 加工 operator (自己/自动/某人) first → fill drag grid → intercept TryStartCraft → claim full stacks → close UI → NPC goto machine → product on ground.
- Runtime job only (no save). Single active job.
- NPC process jobs **spend no SP** (costSp still used only for exp scale). Duration uses NPC skill then **half cut** (`max(1, floor(base*0.5))`); live progress interval `2` (vanilla craft uses 5 for anime cadence). NPC gets craft ModExp.
- Claimed ingredients **park on the machine tile** (not PC bag) for bulk jobs; leftovers return to PC on Clear.
- **Zone leave (simple hold):** party worker -> Clear(zone-change) + rejoin. Resident worker -> Suspend (ings stay on machine, AI dropped, **no hour-tick craft**). On return to work zone: rebind parked ings by uid/cell, estimate finished crafts from elapsed game minutes at **half away speed** (duration*2 minutes/craft), run catch-up only on live stacks (no product without consume), then resume AI if Remaining > 0. Still runtime-only (not saved; quit mid-hold loses job).
- Party hop: companions leave party on start; **set `c_wasInPcParty` after `RemoveMember`** (vanilla clears the flag inside RemoveMember). Clear rejoins and clears the flag; if Clear never runs, vanilla `FactionBranch.OnAfterSimulate` can auto-rejoin. Still runtime-only (no process-job save).

**Out of scope / rejected:**
- `LayerCraft` factory synthesis multi-ingredient recipes / craft-panel 委派.
- `TraitGrindstone` (polish / socket eject, not simple conversion) for this MVP.
- Auto Act fork; BCS; kill-quest outsourcing.

## Slice D (地牢探索 / 地区派遣 — implemented, feedback pass)

**In scope:**
- Entry: PC-faction home quest board button **派遣** (covers region + dungeon; LayerList UI; inner headers still 地区派遣 / 地牢探索).
- Targets: nearby dungeons radius `clamp(5+branch.lv,5,10)`; random nefia + fixed + region outing.
- Team: **up to 4 members per dungeon** (one mission per zone); residents + party (**leave party on start**; settle/recall **home only, no auto rejoin** — player re-adds). Unlike town labor/process.
- Duration / combatPower / progress floors as before.
- Tracker: `QuestNpcLaborDispatch`; abandon/recall via dispatch UI.
- Rewards simulated mailbox; general money still stripped, **lockpick chest currencies allowed** (`money`/`money2`/`medal`).
- **Haul formula (v21 skill split):** `explore` = item quantity; `gather` = food (region, append-only) / gather-node qty (dungeon); `lockpick` = chest sim (`money` common / `money2` rare / `medal` very rare). Region explore total = `Round(explore * weeks * variance)`; region food = `Round(gather * weeks * variance * 0.35)`; dungeon explore = `Round(explore * variance)`; dungeon gather nodes = `Round(gather * variance * 0.55)`; lockpick chests = unified `lockpick/20` (100=>5, clamp 0..12; **not** × weeks). Variance locked once in `0.8..1.2`.
- **Region rewards:** explore materials only in plan staples (~70%+ sand/logs/ore/pasture); gather food append-only outside explore total; lockpick may add light currency; **fresh not rotten**; dye banned; bone low non-zero weight; staples beach sand+salt / forest logs+vine / mountain ore+sulfur / plain pasture.
- **Plan vs tracker:** `plannedLootEntries` + `plannedHaulTotal` locked at start; tracker/mail use **HarvestProgressPercent** (0 until after travelHours/路程, then explore portion); **mail qty = details qty**; region success pays full plan; fail/recall pay current harvest only (no free 0.05 / no 1.0/0.55).
- Quest pin force-cleared via `track=false` + `WidgetQuestTracker.Refresh`.
- **Dungeon fail:** full ground haul still paid; only Boss chest / fixed-dungeon success extras are gated by success roll. Fail settle appends educational hint (boss spoils/fame or fuller haul); fixed-dungeon RewardLv math unchanged (no 1.2 on fixed extras).
- **Success chance (v19):** `raw = 65 + (power - danger*6.5)*0.05 + explore/15 + lockpick/25 + gather/30`, clamp 15..100; floors at power?danger*8/10/12/15/20. Anchor: danger150 / power1000 ? 60-70%.
- **Region no danger:** region dispatch ignores dangerLv for duration/success/UI/rewards; natural complete always succeeds; haul is skillScore*weeks*variance only.
- **Skill roles (v21):** explore=qty, gather=region food / dungeon nodes, lockpick=chest currencies.
- **Exp (balance pass):** base explore/lockpick/gather × **weeks** (region 1-4; dungeon 1). Region specialty ×2: plain explore / mountain mining / forest lumber / beach digging. Console `NpcLaborExp` previews gains + to-next.
- **Dungeon rewards (v22):** ground haul = target-zone biome ground-item pool; gather = biome harvest/obj pool; success adds Boss chest (random) or fixed extras/specials. Banned fixed staples: sulfur / gold ore / scrap. No generic `material`/`ore` filter lottery for dungeon floor loot.
- **Dungeon pool quality:** `RewardLv = dangerLv * 2` for ground/gather; **Boss chest lv = Round(RewardLv * 1.2)** on success. Success also grants PC fame `rndHalf(30+danger*2)` (region skip).
- Success can reach **100%** when power heavily dominates (~danger*20).
- Debug: Reflex console `NpcLaborHarvest` / `NpcLaborComplete` / `NpcLaborCompleteOne` / `NpcLaborExp` (exact BCS `[ConsoleCommand("")]` + assemblies.Add + Rebuild in Plugin Awake/Start).
- Save: `npclabor_dispatch.json`; hour tick on home `FactionBranch.OnSimulateHour`.
- Region map markers: **tinkerCamp zone icon tile** (SourceZone `tinkerCamp` pos[2], fallback 334). Never pass `iconFlag` as an AddLight prefab. Legacy elolight pins are still scrubbed.

**Out of scope:**
- Slice C Expedition Search/Hunt; real combat sim; co-op recall into party; true map pickups.

## Slice E (店铺帮工 / town shop labor — implemented)

**In scope:**
- Entry: non-PC-faction town `LayerQuestBoard` tab 0; labor rows **appended under** vanilla quests.
- Base / `IsPCFaction`: **no** labor rows (user lock).
- Job catalog (trait-matched): inn chore / kitchen help / general clerk / smith assist / food clerk / bookstore clerk / scholar assist.
  - Bookstore: `TraitMerchantBook` / `ShopType.Book|RedBook` -> 书店店员.
  - Scholar/desk: guild clerks, healer desk, plat/magic shops -> 学者助手 (not 杂货店员).
  - General clerk only for true general/junk/souvenir/goods shops (no broad merchant fallback).
- Board free offers: random **2-3** per town/**week** (active rows always shown); sample seed is weekly (`dayIndex/7`). **Intentional weekly full re-roll batch** — slower/stabler than vanilla `Zone.UpdateQuests` (which retries about daily and tops up empty slots). Not every merchant; not daily; not empty-slot top-up.
- Detail text: multi-variant first-person reasons + duration only (no reward dump; no difficulty axis).
- 1 worker per client; max **2 concurrent per town**; duration **random 6-12h** (no difficulty axis; board hours locked into start).
- Accept UI (plan A): board click → **1 自己做 / 2 交给同伴 / 取消** (not vanilla drama talk).
- **PC self-work:** auto-walk to client (no teleport) then work; hours/SP tick only after arrive; same rewards as companion; no party hop; exhaustion / approach cancel / hard fail = no main prize + vanilla `Quest.Fail()` (fame); recall soft-removes tracker; `isPcSelf` + `workStarted` saved.
- Candidates (companion): residents + party; party members temporarily leave party on start and **rejoin party on settle**.
- Board offer rows: unique synthetic `uid` (not Init), `person` temp-cache client; resolve via `ResolveChara`.
- Off-map: labor **keeps ticking** when PC leaves town; unloaded map-local client is not treated as dead/fail.
- Visible work: soft presence near client + cosmetic `AI_TownLabor` (wide radius idle/DoGoto).
- Progress: `GameDate.AdvanceHour` tick; save `npclabor_townlabor.json`.
- Success rewards (no `Quest.Complete`): silent affinity = `AffinityBase` (default 10; no difficulty axis); cash **proportional** to `clamp(skill/invest, 0..1)` × full wage **`(50 + c_invest) × hours`** (6-12h); under-skill settles with **手艺不足，报酬打折**; plat `max(2, 2+2×floor(c_invest/20))`; **20%** local `ticket_furniture`; optional themed leftover; **`c_invest+1` free (no extra gold cost)** only if skill > c_invest (店等级) and under town invest cap; no per-shop cooldown (weekly board + skill/cap rate-limit). Parallel to paid vanilla town invest, not a replacement., else soft remind **似乎城镇投资等级需要提升。。。**.
- Player settle text lean: `金币×N / 白金币×N / 家具兑换券 / 打工奖励 X` + skill/invest warns (affinity/exp silent). **Exp:** companion success = `ExpPerHour×hours` (default 12×h); **PC self = ExpPerHour/2 each worked hour, no bulk settle exp**. Snapshots `clientInvest`/`workerSkill` at start (vanilla ShopLv only for town invest cap math). Duration hours-only. Accept UI subtext warns when skill < **c_invest**.
- Delivery: same-map Drop near PC; off-map home mailbox parcel.
- Recall / death / confirmed-dead client / SP exhaustion: settle fail/recall, no main prize; **Failed** calls tracker `Quest.Fail()` (sound+fame); Success/Recall quiet remove; tracker `QuestNpcLaborTownLabor`.
- Busy mutual exclusion via `LaborBusy` across A/B/D/E.
- **Uninstall / reinstall:** Not safe mid-mission without recall first. Co-craft/process are runtime-only. Dispatch/town persist in `npclabor_*.json`. **Reinstall heal:** if world time advanced past `lastSeenWorldRaw` without a mod save (played with mod off), Load **auto-recalls** open dispatch + town missions (home/return; town party rejoin; no main success prize). Normal same-mod load resumes. Legacy saves with raw 0 resume (no force). Prefer: recall all → quit game → disable mod.

- Console: `NpcLaborComplete` / `NpcLaborCompleteOne` / `NpcLaborExp` (dispatch + town labor); `NpcLaborTownComplete` alias.
- D dispatch button/tab remains independent (home-only).

**Out of scope / rejected:**
- Kill-quest outsourcing; vanilla Supply/Meal/Bulk/Deliver hijack.
- Vanilla drama talk accept (plan B) — not used; plan A LayerList dual choice only.
- Auto Act fork; BCS merge; version change (lock **1.14.514**).


