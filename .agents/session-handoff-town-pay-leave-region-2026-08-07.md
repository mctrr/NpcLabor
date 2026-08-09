# Handoff — town pay / leave confirm / manual region pools (2026-08-07)

## Goal (this turn)
1. Soften skill-pay wording (no “手艺不足，报酬打折”)
2. Fix town labor pay explosion (shop “lv2” → 6000+)
3. Confirm when leaving while town labor is active
4. Crash going home — diagnosed as BetterCustomSprites, not NpcLabor
5. Replace region dispatch item generation with fixed 4-biome manual pools
6. Build + launch game

## Status
**Implemented + Release built + game launched.**

DLL: `E:\SteamLibrary\steamapps\common\Elin\Package\Mod_NpcLabor\NpcLabor.dll` (2026-08-07 23:35)
Elin process started 23:35.

## Changes

### 1. Skill wording
- CN `reward.skillDiscount` → “技能再熟练一点，报酬会更好…”
- CN `town.ui.skillWarn` → “这活儿还得再练练手，报酬会更好…”
- EN equivalents softened similarly
- File: `NpcLabor/LaborText.cs`

### 2. Town labor wage fix
Root cause: board/lock used raw vanilla `TraitMerchant.ShopLv` (development/10 + invest curve), not the UI “店铺等级/difficulty 1–6”. Early invest shops can hit ShopLv 50–100 → thousands of gold.

Fix:
- Added `TownLaborJobs.GetWageShopLv(client)` = `DifficultyFromClient` (1..6)
- Mission start / board preview / UI skill-warn / reward lock use wage tier
- `CalcFullWagePerHour` / `CalcPayFactor` clamp to difficulty range
- Invest raise gate still uses **raw** `GetShopLv` (skill must beat real shop bar)

Expected early pay: `(50 + 10 * wageLv) * hours * skillFactor`
- wageLv 2 × 12h full skill ≈ **840** gold (not 6000+)

Files:
- `NpcLabor/TownLabor/TownLaborJobs.cs`
- `NpcLabor/TownLabor/TownLaborManager.cs`
- `NpcLabor/TownLabor/TownLaborRewards.cs`
- `NpcLabor/TownLabor/QuestNpcLaborTownLaborOffer.cs`
- `NpcLabor/TownLabor/TownLaborUi.cs`

### 3. Leave confirm
- Real leave is `Chara.MoveZone(Zone, ZoneTransition)` (not `Player.MoveZone`)
- Prefix `TownLaborLeaveConfirmPatch`: Dialog.YesNo when leaving work zone or any leave during PC self labor
- One-shot `_allowNextPcMove` avoids reentry
- Strings: `town.leave.confirm` / `town.leave.confirmSelf`
- Files: `NpcLabor/Patches/LifecyclePatches.cs`, `TownLaborManager.ShouldConfirmLeave`, `LaborText.cs`

### 4. Crash (home flash)
Player.log stack is **BetterCustomSprites** (`SkinLoadPatches` / `PcSkinBinder` / `IO.LoadPNG` native Texture2D crash on zone enter). **Not NpcLabor.** No NpcLabor fix for this crash.

### 5. Region manual pools
- New `NpcLabor/Dispatch/RegionManualPools.cs`
- `CollectRegionThings` now only calls `CollectRegionThingsManual`
- planGen bumped to **26** (`ManualRegionPlanGenVersion`) so old plans rebuild
- Beach: sand/salt/shell/coral/seaweed/bait/fish — no wood/bread
- Forest: logs/vine/resin + mushroom/fruit food
- Mountain: copper/iron/stone/sulfur/gem + light forage
- Plain: pasture/grass/flower/herb
- Qty still from explore/gather × weeks × variance; lockpick chests still appended
- Old scan/prune code left dead but unreferenced from region path; dungeon path untouched

## Not done / verify in-game
- [ ] Board offer on low-lv shop shows hundreds, not thousands
- [ ] Accept + settle money matches board money-only line
- [ ] Leave town / open map while labor → YesNo confirm; No cancels
- [ ] New region beach dispatch: only beach-ish materials/food; no bread/oak flood
- [ ] Old in-progress region missions replan on next EnsureRegionPlan (gen 26)
- [ ] Home crash if still present → disable/fix BetterCustomSprites PNG

## Constraints
- Version **1.14.514** unchanged
- Scope: co-craft / process / dungeon+region / town labor only

## Next steps
1. In-game verify pay, leave dialog, beach loot
2. If ACS crash still hits home, fix/disable BetterCustomSprites skin reload path
3. Optional cleanup: delete dead region scan/prune methods later
