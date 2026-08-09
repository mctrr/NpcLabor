# Session handoff — polish pay/config/i18n (2026-08-06)

## Goal this session
Continue balance polish from prior pass:
1. E skill under bar → **proportional pay** + remind
2. Invest blocked by town cap → soft remind `似乎城镇投资等级需要提升。。。`
3. D failure → success-feel hint (boss/fame or fuller haul); fixed dungeon math untouched
7. Region lockpick × weeks — already correct, left alone
Last: localization + config de-hardcoding

## Done
### E town labor
- Cash: `money = fullWage * clamp(skill/shopLv, 0..1)` (0 skill → 0; skill≥shop → full)
- Under-skill: rewardLog + settle line includes `手艺不足，报酬打折`
- Invest: skill>shop but town cap blocks → `似乎城镇投资等级需要提升。。。` (no c_invest++)
- Accept UI: self/companion rows warn when skill < shopLv
- Hours/wage/plat/ticket/exp/headroom read from `LaborConfig`

### D dispatch
- Failure settle appends:
  - dungeon: `若成功或许还能带回首领战利品与声望。`
  - region: `若成功或许还能带回更完整的收获。`
- Boss mult / lockpick divisor / exp bases / fame / specialty mult from config
- Fixed dungeon extras still not forced through BossRewardLv×1.2 path beyond existing boss chest logic

### Config + i18n
- NEW `NpcLabor/package/labor_config.json` (copied by existing CopyAssets)
- NEW `NpcLabor/LaborConfig.cs` loader (Plugin.Awake EnsureLoaded)
- NEW `NpcLabor/LaborText.cs` CN default + EN when lang probe looks English
- `LaborTerms` product names + unlock denials go through LaborText; unlock LVs from config

### Build
- **Compile succeeded** → `NpcLabor/obj/Release/NpcLabor.dll` (2026-08-06 ~13:45)
- **Deploy failed**: game Package path access denied (Elin likely running)
  - Close game, then:
    ```powershell
    $env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
    dotnet build NpcLabor\NpcLabor.csproj -c Release
    ```
  - Confirm package folder has `NpcLabor.dll` + `labor_config.json` + `special_rewards.json`
  - Full game restart after deploy

## Key files touched
- `NpcLabor/LaborConfig.cs` (new)
- `NpcLabor/LaborText.cs` (new)
- `NpcLabor/package/labor_config.json` (new)
- `NpcLabor/LaborTerms.cs`
- `NpcLabor/Plugin.cs`
- `NpcLabor/TownLabor/TownLaborRewards.cs`
- `NpcLabor/TownLabor/TownLaborJobs.cs`
- `NpcLabor/TownLabor/TownLaborManager.cs`
- `NpcLabor/TownLabor/TownLaborUi.cs`
- `NpcLabor/Dispatch/DungeonDispatchRewards.cs`
- `NpcLabor/Dispatch/DungeonDispatchManager.cs`
- `NpcLabor/Dispatch/DispatchConsole.cs`
- `docs/CONTEXT.md`
- `.agents/handoff.md` + this file

## Explicit non-goals (still)
- Do not rename B back to 外包
- Do not rework region lockpick × weeks
- Do not Buff fixed-dungeon RewardLv to 1.2 globally
- No drama-talk accept; no Slice C / kill-quest outsourcing

## Verify in-game after deploy
1. Town board: skill < shopLv → accept subtext warn; settle pay < full + 手艺不足
2. Skill > shop but town invest cap → settle shows 城镇投资等级需要提升; c_invest unchanged
3. Dungeon fail settle includes boss/fame hint; success still gives boss×1.2 + fame
4. Region fail hint is haul-only (no fame talk)
5. Log on boot: `labor config loaded from ...`
6. `NpcLaborExp` shows pay factor / preview money for town missions

## Next optional polish
- Move more residual CN UI strings (picker headers, job catalog details) into LaborText
- Expose more dispatch variance/haul constants in labor_config if further balance needed
- Soft-fail deploy path when game locks Package (copy to sidecar folder)
