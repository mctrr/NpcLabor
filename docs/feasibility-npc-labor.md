# Elin NPC Labor / Co-Craft — Technical Feasibility

> Saved: 2026-08-01 (Asia/Shanghai); decisions updated 2026-08-02
> Sources: decompile `Elin.dll`; Workshop Auto Act `3370686923`
> Decompile cache (this repo): `.research/types/`
> Handoff: `.agents/handoff.md`
> Domain short context: `docs/CONTEXT.md`

## Goal (product)

NPC life skills become useful via:
- A co-craft with PC (craft/cook) ← **v1 / slice A**
- B timed job outsourcing (e.g. saw boards → drop on ground)
- C base dispatch (~1 month)
- D dungeon expedition (settle first; floor encounter later)
- E town shop labor (new job rows on town quest board; not kill-quest outsourcing)

Tone: vanilla-adjacent, not overpowered.

## External mod boundary

**Auto Act** (Shift repeat actions): PC-centric harvest/mine/build/etc. Party used only for held-item/seed helpers. **Does not** co-craft at workbenches. Do not fork it; stay off world-gather pipeline.

**Do not** put Labor logic into BetterCustomSprites. This workspace is the Labor mod only.

## Vanilla anchors

| Need | Types / flow |
|------|----------------|
| Craft UI | `LayerCraft.OnClickCraft` → `pc.SetAI(AI_UseCrafter)` |
| Craft AI | `AI_UseCrafter` + `Progress_Custom`; exp/stamina on **AI owner** |
| Duration/SP | `TraitCrafter.GetDuration` / `GetCostSp` (**duration uses `EClass.pc` skill**) |
| Quality/fail | `Recipe.GetQualityBonus`, `RecipeCard.Craft` (**hardcoded `EClass.pc`**) |
| Output | Usually `EClass.pc.HoldCard` / AddCard |
| Resident work | `GoalWork` → `Hobby` → `AIWork_*` (separate from PC craft UI) |
| Dispatch | `FactionBranch.expeditions` / `Expedition` / `ExpeditionManager`; `MoveZone("somewhere")`; hour tick on `OnSimulateHour` |
| Branch lv | `FactionBranch.lv` (1..7), `members`, efficiency |
| Party | `Party.members`, `ModExp`, `GetBestSkill` |

Local dumps: `.research/types/<TypeName>.cs`.

## Slice feasibility

| Slice | Feasible | Notes |
|-------|----------|-------|
| A co-craft | Yes, med-high | Must bypass/fix PC hardcodes |
| B processor outsource (saw/mill/wood mill) | Yes, med | MVP implemented 2026-08-02: LayerDragGrid 外包 + AI_NpcProcess; TraitSawMill/Mill/WoodMill; product ground drop; runtime only. Grindstone deferred. |
| C expedition | Yes, low-med | Extend vanilla Expedition |
| D dungeon AFK | Yes settle / hard encounter | v1 settle only |
| E town shop labor | Yes | Implemented: board rows + hour tick + rewards; kill-quest still no |

## Slice A — PC-centric hardcodes (why simple buff helps)

Paths that read **PC only**:
- `TraitCrafter.GetDuration` ← `EClass.pc.Evalue(IDReqEle)`
- `Recipe.GetQualityBonus` ← pc vs req skill
- `RecipeCard.Craft` fail/quality/cook ← pc
- Product hold ← pc

Exp/stamina in `AI_UseCrafter.onProgressComplete` use **owner** (PC if PC runs AI).

## Open risks

- Restore must survive cancel/UI close/death
- Temp `ModBase` may interact badly with exp; prefer bonus channel or save/restore exact values
- SP assist has stamina **sync risks** if top-up/restore is sloppy
- Economic power if bonus uncapped
- Do not merge with Auto Act

---

## Slice A decisions (2026-08-02, latest)

### Locked

| Topic | Decision |
|-------|----------|
| Assistant | **Both** auto (best relevant skill in party/range) **and** manual pin |
| UI | **Craft / manufacturing panel button** (primary). Console is **not** primary control (earlier console-first idea superseded) |
| Effective skill | `eff = pc + Floor(npc * 0.5)` on recipe req skill |
| Where `eff` applies | Duration + quality/fail paths that currently read PC skill |
| Exp v1 | PC keeps **vanilla** exp path; NPC basic share (`raw * k`); **no double-exp yet** |
| SP | **Must change** (vanilla SP is flat table). Formula locked: max(1, base - Floor(npcSkill*0.25)) via GetCostSp; no temp SP top-up |
| Presence | On craft start: **teleport assistant to PC cell** when valid |
| Implementation style | Session flag / rewrite skill reads preferred; temp PC skill buff OK if restore-safe |
| Power budget | Vanilla enhancement, not OP |

### Locked during implement (2026-08-02)

1. NPC exp share `k = 0.5`
2. SP: `max(1, base - Floor(npcSkill*0.25))`
3. After craft: leave NPC on PC cell
4. Pin: non-save runtime only
5. Session clear on cancel / end / PC death / zone change (no skill restore needed — rewrite-only)

### Still open / polish

1. Teleport edge cases — multi-tile / combat / busy AI
2. Assist button layout polish
3. Save pin later if asked

### Suggested runtime shape

1. Resolve assistant: pin if valid → else auto best skill → else none.
2. Teleport NPC to PC cell.
3. Open session (`eff`, SP plan, restore bookkeeping).
4. Duration/quality use `eff`; SP assist applied.
5. Complete: PC vanilla exp + SP settle; NPC `ModExp(req, raw * k)`; clear session.
6. Cancel/fail: always restore session state.

### Patch targets

| Hook | Intent |
|------|--------|
| `LayerCraft` | Co-craft button + start wiring |
| `AI_UseCrafter` OnStart / complete / cancel | Session open/close; teleport; restore; NPC exp |
| `TraitCrafter.GetDuration` | Use `eff` |
| `Recipe.GetQualityBonus` / craft fail | Use `eff` |
| `TraitCrafter.GetCostSp` | SP assist path (**required now**, not optional) |
| Stamina | Explicit Mod + restore bookkeeping |

---

## Vanilla craft timing (AI_UseCrafter + TraitCrafter)

```text
costSP   = GetCostSp(ai)           // = SourceRecipe.Row.sp  (NOT skill-based)
duration = GetDuration(ai, costSP)
         = max(1, source.time * 100 / (80 + EClass.pc.Evalue(reqSkill) * 5))
progress.SetDuration(duration, interval: 5)
```

- `source.time` / `source.sp` come from recipe/factory row.
- Higher PC skill → larger denominator → **smaller duration** → faster bar.
- SP cost is **flat** from table; skill does not reduce SP in vanilla — hence co-craft must change SP deliberately.

## Vanilla craft exp (on each of `num` items in batch)

```text
raw = costSP * 12 * (100 + duration * 2) / 100
owner.elements.ModExp(reqSkillId, raw)
owner.stamina.Mod(-costSP)
```

Then `ElementContainer.ModExp`:
- days-together bonus on chara
- if UseExpMod: `a *= clamp(Potential,10,1000) / (100 + max(0, ValueWithoutLink)*25)`
- parent skill drip via parentFactor
- `vExp += (int)a`; level when `vExp >= ExpToNext` (default 1000)

**Coupling:** faster craft (lower duration) → **lower raw exp**. Skill assist that only shortens duration also shrinks PC raw exp unless compensated. v1 accepts vanilla PC exp path.

## Quality (Recipe.GetQualityBonus) — also PC-only in vanilla

```text
diff = reqSkill.Value - EClass.pc.Evalue(reqSkill.id)
if diff > 0:  // under-skilled
  diff < 5 → 0
  else → -(diff-4)*10
else:         // at/above req
  curve(-diff, 10, 20, 80)/10*10 + 10 (+ recipe lv term)
```

Feeding `eff` into these PC reads improves quality/fail and speed together.

## Manual pin notes

- Store uid or name in static/player flag (non-save OK for v1; save later).
- Invalid pin → fall back auto → none.
- Primary UX is craft-panel button, not console.


