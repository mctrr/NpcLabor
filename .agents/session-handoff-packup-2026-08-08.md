# Handoff — NPC Labor (2026-08-08)

## Purpose for next session
Pick up **NpcLabor** after a completed **vanilla-API reuse audit**. No collapse/refactor was implemented yet. Next work starts only if the user green-lights the ranked replacements.

## Workspace
- Mod root: `D:\work\Elin\5\2`
- Code: `NpcLabor/` (BepInEx/Harmony)
- Game: `E:\SteamLibrary\steamapps\common\Elin`
- Deploy: `Package\Mod_NpcLabor\` (DLL + package.xml)
- Version string **1.14.514** — never change
- Git: not a usable repo root for status/log in this workspace (do not rely on git history)

## Always read first
1. `docs/CONTEXT.md` — product locks + slice A/B/D/E rules
2. `.agents/handoff.md` → latest session handoff
3. Detailed audit: `.agents/session-handoff-vanilla-reuse-audit-2026-08-08.md`
4. Prior beach/lockpick pass: `.agents/session-handoff-beach-lockpick-2026-08-08.md`
5. Decompile policy: `.research/README.md` + parent `D:\work\Elin\5\AGENTS.md`

## Key decisions (locked / current)

### Product locks (do not reopen unless asked)
- Version **1.14.514** fixed
- Town wage: `(50 + c_invest) * hours * skillFactor` — do not invent new formula
- Region rewards: **manual 4 pools only** — never reintroduce biome scan
- Lockpick mail: **chest contents only**, no chest body (`FlattenInto keepContainer: false`)
- Co-craft: `eff = pc + Floor(npc * 0.5)`; SP cut Floor(npcSkill*0.25) min 1; pin non-save
- Do not fork Auto Act; do not merge BetterCustomSprites; kill-quest outsourcing out of scope
- Workshop surface: title **NPC 帮工 (NPC Labor)**; package id `mctrr.npclabor`; internal folder stays `NpcLabor`

### Audit method decision (this workstream)
- Use **Elin MCP only** (index **23.333.1**); no local ILSpy for the audit
- Report ranked findings; **do not implement** mass factory collapse without explicit user OK
- Co-craft / town labor / processor already on good vanilla paths — no high-value reinvent there

## Completed work

### Vanilla-API reuse audit (done, no code change)
Already correctly using vanilla:
- Mail: `ThingGen.CreateParcel` + `FactionBranch.PutInMailBox`
- Currency: `ThingGen.CreateCurrency`
- Lockpick/boss chest: `CreateTreasure` / `CreateTreasureContent` + `TreasureType.*`
- Dungeon filter lottery: `CreateFromFilter` / `CreateFromCategory` / `CreateFromTag`
- Processor: `TraitCrafter.Craft` via throwaway `AI_UseCrafter`
- Co-craft: Harmony on `Card.Evalue` + `ElementContainer.ModExp` only
- Affinity/invest: `ModAffinity`, `c_invest`
- Named meat: thin wrapper over `MakeFoodFrom` / `MakeRefFrom`

Ranked replace targets (not yet done):
1. **Must** — collapse `RewardMaterialFactory.cs` (~79KB) to CreateRawMaterial-first single helper
2. **Must** — beach fish via `ThingGen.Create("fish", -1, skillLv)` instead of manual id list + weight^1.5 (do **not** call full `AI_Fish.Makefish`)
3. **Must** — delete dead region normalizers in `DungeonRewardPlans.cs` (`TopUpRegionToTarget`, `NormalizeBeach/Forest/Mountain/PlainThings`, `AddRegionGatherFood`) + soft-dead scan stubs in `DispatchRewardPools`
4. **Should** — simplify KeepFoodFresh / ForceFresh to `decay=0` + DefaultMaterial
5. **Should** — lockpick empty fallback → treasure re-roll, drop custom 85/12/3 currency table if chest path stable
6. **Should** — single CreateBeachSand path; drop Hard/MandatoryFallback/DigFloorMaterial duplicates

### Earlier beach/lockpick pass (code on disk, built)
- Beach pool planGen **29**: sand/salt/seaweed/bait + multi-fish only
- Lockpick: contents only, no empty chest body
- `CreateFromPlanEntry` special-id fallbacks hardened
- Release build OK ~2026-08-08 00:33 (~358KB DLL); game had been launched for verify

### Scope still implemented
- A co-craft, B processor outsource, D dungeon+region dispatch, E town shop labor

## Unfinished / blocked on user

| Item | Status | Notes |
|------|--------|-------|
| Material factory collapse | **Waiting user OK** | Large blast radius on sand/ore identity |
| Beach fish → Create("fish") | Waiting user OK | Keep weight filter only if lv path too narrow |
| Delete dead Normalize*/TopUp* | Waiting user OK | Unreferenced from live CollectRegionThingsManual |
| Lockpick currency table drop | Waiting user OK | Prefer treasure re-roll |
| In-game re-verify after any collapse | Not started | Beach 4w, lockpick no chest body, forest/mountain/plain real mats |
| Workshop first upload extras | Older backlog | author confirm / preview.jpg if still needed |

## Modified files this workstream

### Audit session
- **No source edits.** Artifacts only:
  - `.agents/session-handoff-vanilla-reuse-audit-2026-08-08.md`
  - `.agents/handoff.md` (pointer)

### Last code-touch session (beach/lockpick, already shipped to source)
- `NpcLabor/Dispatch/RegionManualPools.cs` — pools + fish roll (planGen **29**)
- `NpcLabor/Dispatch/DungeonDispatchRewards.cs` — FlattenInto chest-body fix
- `NpcLabor/Dispatch/DungeonRewardPlans.cs` — CreateFromPlanEntry fallbacks
- Prior: `DispatchRewardPools.cs` region scan path retired (dungeon scan kept)

### Prime collapse targets (not yet edited for audit)
- `NpcLabor/Dispatch/RewardMaterialFactory.cs` (~79KB)
- `NpcLabor/Dispatch/DungeonRewardPlans.cs` (~117KB, dead normalizers)
- `NpcLabor/Dispatch/RegionManualPools.cs` (fish list)
- `NpcLabor/Dispatch/DispatchRewardPools.cs` (scan stubs + currency fallback)

## Current region pool source of truth
`RegionManualPools.cs` planGen **29**:
- **Beach** explore: sand 55 / salt 25 / seaweed 12 / bait 8; food: multi-fish only
- **Forest** explore: wood/birch/vine/resin/branch/bark; food: mushroom/berry/fruit/apple/grape
- **Mountain** explore: copper/iron/stone/sulfur/gem/rock; food: mushroom/berry
- **Plain** explore: pasture/grass/flower/herb; food: flower/herb/egg

## Build / deploy
```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```
- Do **not** start the game unless asked
- Close game if DLL locked before rebuild/copy
- Full game restart after deploy

## Suggested next implementation order (only if user says go)
1. Collapse material create → `CreateRawMaterial` single helper; retest sand/copper/iron/wood/sulfur
2. Beach fish → `ThingGen.Create("fish", -1, skillLv)` (+ optional weight filter)
3. Delete dead Normalize*/TopUp*/AddRegionGatherFood + soft scan stubs
4. Lockpick empty fallback → treasure re-roll
5. Release build only; no game start unless asked

## Do not
- Reintroduce region biome scan
- Change wage formula or version **1.14.514**
- Call full `AI_Fish.Makefish` (PC fever / fishArtifact / zone side effects)
- Mass-edit RewardMaterialFactory without explicit user OK
- Blind full-assembly redecompile; search `.research/` first
- Fork Auto Act / BCS / kill-quest outsourcing

## Suggested skills
- **elin-modding** — any vanilla API / Harmony / package work (MCP source index)
- **diagnose** — if post-collapse sand/ore/fish identity bugs appear
- **handoff** — end of next implementation session

## Residual risk
- Material factory has many sand_sea / mud / gold lottery edge cases; single-path collapse can regress beach sand identity
- Fish Create-by-lv may be narrower than current multi-id weight pool
- Dead code deletion is low risk if call-graph rechecked before delete
- Old in-progress region missions rebuild under planGen 29 on ensure — verify after any pool/gen bump
