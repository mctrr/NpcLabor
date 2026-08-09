# Parent AGENTS.md decompile patch — APPLIED

Target: `D:\work\Elin\5\AGENTS.md`  
Applied: 2026-08-05

## Landed changes
1. Task table → craft/labor research points at `2/.research/README.md` + `KNOWN_TYPES.txt` + `types/` `types_live/` `extra/`
2. Preferred methods bullet → NpcLabor cache-first (`README` → index → dumps)
3. Heading → “existing dumps first (do not redecompile if present)”
4. Decompile hygiene → search caches first; no mod-root `_tmp_*` / `tmp_decompile/` clutter; re-dump only on real API drift

NpcLabor-local rules remain authoritative for this mod:
- `2/AGENTS.md`
- `2/.research/README.md`
- `2/.research/KNOWN_TYPES.txt`
- `2/.research/TYPE_INDEX.csv`
