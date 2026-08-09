# Session handoff — gitignore + GitHub upload (2026-08-10)

## Goal
1. Review/tighten `.gitignore`.
2. Publish this NpcLabor workspace to GitHub under `mctrr`.
3. Leave a structured handoff so the next session can continue.

## Status
**Done (this session):** gitignore tightened; empty broken `.git` placeholders rebuilt; repo pushed to GitHub.

## Decisions
- Root empty `.git/` and `NpcLabor/.git/` were **broken placeholders** (0 files) — removed and re-inited at workspace root only.
- Keep durable research cache (`.research/types*`, `decompile_tmp`, `extra`, indexes) **in git**; only tooling leftovers stay ignored.
- Keep `.agents/session-handoff-*.md` in git (agent continuity); ignore only `.agents/_*.py` / `_*.csfrag`.
- Ignore root scratch: `_tmp_*`, `tmp_*`, `.tmp_*`, `tmp/`, `Package/`, `bin/obj`, `*.dll`/`*.pdb`/`*.exe`.
- Ignore nested `NpcLabor/.git/` and `NpcLabor/.agents/`.
- Public repo name: **NpcLabor** (matches assembly / Workshop internal folder; Workshop display title remains `NPC 帮工 (NPC Labor)`).
- Auth via Windows credential manager for `https://github.com` as user `mctrr` (no `gh` CLI installed).

## Files changed this session
- `.gitignore` — add `.tmp_*`, binaries, IDE/OS noise, nested empty git dirs
- `.agents/handoff.md` — pointer to this file
- `.agents/session-handoff-github-upload-2026-08-10.md` — this handoff
- git history recreated at repo root (previous local `.git` was empty)

## Prior product state (still valid)
Latest feature handoff: `.agents/session-handoff-region-map-icon-2026-08-09.md`
- Region dispatch pins = tinkerCamp zone icon (not elolight); quest text drops `(gx,gy)`.
- Town PC self-work leave menu / invest wage / flower-meat plan gen — shipped earlier.
- Version string **1.14.514** never changes.
- Scope: co-craft + processor + dungeon/region dispatch + town shop labor.

## Next session suggestions
1. In-game verify region map icon after full restart (if not already).
2. Optional: add a short root `README.md` for GitHub visitors (Workshop blurb + build steps).
3. Optional: install `gh` CLI for easier repo maintenance.
4. Do **not** full-assembly redecompile; search `.research` first.

## Do not
- Commit root `_tmp_*` / `.tmp_*` patch scripts
- Commit `Package/` deploy output or `bin/obj`
- Change version away from 1.14.514
- Fork Auto Act / merge BCS
