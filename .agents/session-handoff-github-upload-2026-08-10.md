# Session handoff - gitignore + GitHub upload (2026-08-10)

## Goal
1. Review/tighten .gitignore.
2. Publish this NpcLabor workspace to GitHub under mctrr.
3. Leave a structured handoff so the next session can continue.

## Status
**Done.** Repo live at https://github.com/mctrr/NpcLabor (public, main @ 78bef1d).

## Decisions
- Root empty .git/ was a broken placeholder (0 files) - re-inited cleanly at workspace root.
- Keep durable research cache (.research/types*, decompile_tmp, xtra, indexes) in git; only tooling leftovers stay ignored.
- Keep .agents/session-handoff-*.md in git; ignore only .agents/_*.py / _*.csfrag.
- Ignore root scratch: _tmp_*, 	mp_*, .tmp_*, 	mp/, Package/, in/obj, *.dll/*.pdb/*.exe, IDE/OS noise.
- Ignore nested NpcLabor/.git/ and NpcLabor/.agents/.
- Public repo name: **NpcLabor** (matches assembly / Workshop internal folder).
- Auth: Windows credential manager for https://github.com as mctrr (no gh CLI).
- Global git http(s).proxy = 127.0.0.1:10808 blocked git push; fixed with repo-local empty http.proxy / https.proxy.

## Files changed this session
- .gitignore - .tmp_*, 	mp/, binaries, IDE/OS, nested empty git dirs
- README.md - short GitHub landing (build + layout)
- .agents/handoff.md - pointer
- .agents/session-handoff-github-upload-2026-08-10.md - this file
- git root re-init + initial commit 78bef1d (221 files)

## Prior product state (still valid)
Latest feature handoff: .agents/session-handoff-region-map-icon-2026-08-09.md
- Region dispatch pins = tinkerCamp zone icon; quest text drops (gx,gy).
- Version **1.14.514** never changes.
- Scope: co-craft + processor + dungeon/region dispatch + town shop labor.

## Next session suggestions
1. In-game verify region map icon after full restart (if not already).
2. Optional: expand root README with Workshop link once published.
3. Optional: install gh CLI.
4. Do not full-assembly redecompile; search .research first.
5. If git push fails on proxy again, keep repo-local empty proxy or fix the system 10808 proxy.

## Do not
- Commit root _tmp_* / .tmp_* patch scripts or 	mp/
- Commit Package/ / in / obj / dll-pdb
- Change version away from 1.14.514
- Fork Auto Act / merge BCS
