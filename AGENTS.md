# Repository Guidelines

## Project Structure & Module Organization

This workspace is the **NPC Labor** Elin mod (co-craft + dispatch + town labor). Code lives under `NpcLabor/` (BepInEx/Harmony).

- `docs/` — product/domain docs (`CONTEXT.md`, `feasibility-npc-labor.md`, critique notes)
- `.agents/` — session handoffs (`handoff.md` pointer + `session-handoff-*.md`)
- `.research/` — **durable vanilla decompile cache** (read `README.md` first)
  - `types/` — craft / AI / trait hooks
  - `types_live/` — UI / layer / recipe dumps
  - `decompile_tmp/`, `extra/` — kept one-off dumps
  - `KNOWN_TYPES.txt`, `TYPE_INDEX.csv` — inventory; search these before any dump
- `.research/AutoAct/` — Auto Act reference only (do not fork)
- Deploy output: `Package\Mod_NpcLabor\` (DLL + package.xml)

Parent multi-mod map + shared decompile guide: `D:\work\Elin\5\AGENTS.md`.

## Build, Test, and Development Commands

```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```

Deploy the built DLL into the game `Package\` mod folder, then verify in-game. Full game restart after deploy.

## Coding Style & Naming Conventions

- C# / Harmony patches; prefer ASCII in new code.
- 4-space indent; `PascalCase` types/methods, `camelCase` locals.
- Patch classes: `*Patches.cs`; keep co-craft / job session state restore-safe.
- Do not permanently mutate PC skills without guaranteed restore on complete/cancel/death/zone change.
- Version string **1.14.514** never changes.

## Testing Guidelines

No automated suite yet. Manual checks by slice: co-craft pin/off/auto + restore; processor outsource; dungeon/region dispatch rewards; town shop labor board rows.

## Commit & Pull Request Guidelines

Git may be uninitialized here. Prefer short imperative commits scoped to one concern. Note player-facing behavior, restore/SP risks, and in-game verify notes.

## Agent-Specific Instructions

### Always read first

1. `docs/CONTEXT.md`
2. `.agents/handoff.md` (then the linked latest session handoff)
3. `.research/README.md` if the task needs vanilla hooks

### Decompile policy (do not redecompile blindly)

**Existing dumps already cover the craft/dispatch/town-labor surface.** Reuse them.

1. Search `.research/KNOWN_TYPES.txt` / `TYPE_INDEX.csv`.
2. Open the listed file under `types/`, `types_live/`, `decompile_tmp/`, or `extra/`.
3. If still missing, search parent workspace:
   - `D:\work\Elin\5\.tools\decompile\`
   - `D:\work\Elin\5\.tmp_dump\`
   - `D:\work\Elin\5\.research\` (render/ACS; rarely needed here)
4. Only then decompile **one type or one method** from live DLLs.
5. Write new keepers to `.research/extra/<Type>.decompiled.cs` and refresh the index files.
6. **Never** recreate full-assembly dumps. **Never** leave root `_tmp_*`, `tmp_decompile/`, or reflect `bin/obj` as permanent tree clutter.

Scratch belongs in `D:\work\Elin\5\.tmp_dump\` or OS temp — not scattered at the mod root.

### Scope boundaries

- In scope: co-craft, processor outsource, dungeon/region dispatch, town shop labor.
- Out of scope unless asked: BetterCustomSprites, Seamless Floors, Auto Act fork, kill-quest outsourcing.
