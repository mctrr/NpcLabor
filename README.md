# NPC 帮工 (NPC Labor)

Elin BepInEx/Harmony mod: co-craft, processor outsource, region/dungeon dispatch, town shop labor.

- Workshop package id: `mctrr.npclabor` (keep stable)
- Internal assembly/folder: `NpcLabor`
- Version string: **1.14.514** (do not change)

## Build

```powershell
$env:ElinGamePath = "E:\SteamLibrary\steamapps\common\Elin"
dotnet build NpcLabor\NpcLabor.csproj -c Release
```

Deployed DLL lands under the game `Package\Mod_NpcLabor\` folder. Full game restart after deploy.

## Layout

| Path | Role |
|------|------|
| `NpcLabor/` | Mod source (Harmony patches + systems) |
| `docs/` | Product / domain notes (`CONTEXT.md`) |
| `.agents/` | Session handoffs |
| `.research/` | Durable vanilla decompile cache (read `README.md` before dumping) |

## Agent notes

Read `AGENTS.md`, then `docs/CONTEXT.md`, then `.agents/handoff.md`.
