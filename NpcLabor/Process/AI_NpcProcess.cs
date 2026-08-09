using System.Collections.Generic;
using UnityEngine;

namespace NpcLabor.Process;

/// <summary>
/// NPC freestanding processor AI (no LayerDragGrid).
/// Goto machine → convert ings via TraitCrafter.Craft → drop product on ground → repeat.
/// </summary>
internal class AI_NpcProcess : AIAct
{
    /// <summary>Unused marker so the type is distinct per start; real state lives in ProcessorJobSession.</summary>
    internal int jobToken;

    public override bool CanManualCancel() => true;

    public override bool CancelWhenDamaged => true;

    public override void OnCancel()
    {
        if (ProcessorJobSession.Active && owner != null && owner.uid == ProcessorJobSession.NpcUid)
        {
            ProcessorJobSession.Clear("ai-cancel");
        }
    }

    public override void OnSuccess()
    {
        if (ProcessorJobSession.Active && owner != null && owner.uid == ProcessorJobSession.NpcUid)
        {
            // Only clear if the session still belongs to us and remaining is 0 or we finished cleanly.
            if (ProcessorJobSession.Remaining <= 0 || !ProcessorJobSession.Active)
            {
                ProcessorJobSession.Clear("ai-end");
            }
        }
    }

    public override IEnumerable<Status> Run()
    {
        if (!ProcessorJobSession.Active || owner == null || owner.uid != ProcessorJobSession.NpcUid)
        {
            yield return Success();
            yield break;
        }

        TraitCrafter? crafter = ProcessorJobSession.GetCrafter();
        Card? machine = crafter?.owner;
        if (crafter == null || machine == null || machine.isDestroyed)
        {
            ProcessorJobSession.Clear("machine-gone");
            yield return Cancel();
            yield break;
        }

        // Walk next to the machine.
        if (machine.ExistsOnMap)
        {
            yield return DoGoto(machine, 1);
            if (owner == null || owner.isDead)
            {
                ProcessorJobSession.Clear("ai-fail");
                yield return Cancel();
                yield break;
            }

            if (machine.isDestroyed || owner.Dist(machine) > 1)
            {
                ProcessorJobSession.Clear("machine-gone");
                yield return Cancel();
                yield break;
            }

            owner.LookAt(machine);
        }

        if (crafter.Icon != 0)
        {
            try
            {
                owner.ShowEmo(crafter.Icon);
            }
            catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}

        while (ProcessorJobSession.Active && ProcessorJobSession.Remaining > 0)
        {
            crafter = ProcessorJobSession.GetCrafter();
            machine = crafter?.owner;
            if (crafter == null || machine == null || machine.isDestroyed)
            {
                ProcessorJobSession.Clear("machine-gone");
                yield return Cancel();
                yield break;
            }

            if (owner.stamina != null && owner.stamina.value <= 0)
            {
                ProcessorJobSession.Clear("stamina");
                yield return Cancel();
                yield break;
            }

            // Keep adjacency if we drifted.
            if (machine.ExistsOnMap && owner.Dist(machine) > 1)
            {
                yield return DoGoto(machine, 1);
                if (owner == null || owner.isDead || machine.isDestroyed || owner.Dist(machine) > 1)
                {
                    ProcessorJobSession.Clear("machine-gone");
                    yield return Cancel();
                    yield break;
                }

                owner.LookAt(machine);
            }

            List<Thing> sources = ProcessorJobSession.Ingredients;
            if (sources == null || sources.Count == 0)
            {
                ProcessorJobSession.Clear("no-ings");
                yield return Cancel();
                yield break;
            }

            // Validate source stacks still usable.
            for (int i = 0; i < sources.Count; i++)
            {
                Thing? s = sources[i];
                if (s == null || s.isDestroyed || s.Num <= 0)
                {
                    ProcessorJobSession.Clear("no-ings");
                    yield return Cancel();
                    yield break;
                }
            }

            List<Thing> targets = new List<Thing>(sources);
            if (!crafter.IsFuelEnough(1, targets))
            {
                ProcessorJobSession.Clear("no-fuel");
                yield return Cancel();
                yield break;
            }

            // Split one set of ingredients for this craft.
            List<Thing> ings = new List<Thing>();
            BlessedState blessed = BlessedState.Normal;
            for (int i = 0; i < sources.Count; i++)
            {
                Thing src = sources[i];
                if (src == null || src.isDestroyed || src.Num <= 0)
                {
                    CleanupPartialIngs(ings);
                    ProcessorJobSession.Clear("no-ings");
                    yield return Cancel();
                    yield break;
                }

                Thing? piece = null;
                bool splitFail = false;
                try
                {
                    piece = src.Split(1);
                }
                catch
                {
                    splitFail = true;
                }

                if (splitFail || piece == null)
                {
                    CleanupPartialIngs(ings);
                    ProcessorJobSession.Clear("no-ings");
                    yield return Cancel();
                    yield break;
                }

                ings.Add(piece);
                if (piece.blessedState <= BlessedState.Cursed && blessed > piece.blessedState)
                {
                    blessed = piece.blessedState;
                }

                if (piece.blessedState > BlessedState.Normal && blessed == BlessedState.Normal)
                {
                    blessed = piece.blessedState;
                }

                if (crafter.IsConsumeIng)
                {
                    try
                    {
                        Point place = machine.ExistsOnMap ? machine.pos : owner.pos;
                        Card card = EClass._zone.AddCard(piece, place);
                        card.altitude = machine.ExistsOnMap ? 0 : 1;
                        if (crafter.animeType == TraitCrafter.AnimeType.Microwave)
                        {
                            card.isHidden = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.LogWarn("place ingredient failed: " + ex.Message);
                    }
                }
            }

            bool requireOn = crafter.IsRequireFuel || crafter.ToggleType != ToggleType.None;
            if (requireOn && !machine.isOn)
            {
                try
                {
                    crafter.Toggle(on: true);
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}

            // Build a throwaway AI_UseCrafter shell so GetSource / Craft / GetCostSp keep working.
            AI_UseCrafter shell = new AI_UseCrafter
            {
                crafter = crafter,
                recipe = null,
                num = 1,
                ings = ings,
                owner = owner
            };

            int baseSp;
            try
            {
                baseSp = crafter.GetCostSp(shell);
            }
            catch
            {
                baseSp = crafter.CostSP;
            }

            int costSp = ProcessorJobSession.AdjustCostSp(baseSp);
            int duration = ComputeDuration(crafter, shell, costSp);

            if (!crafter.idSoundBG.IsEmpty())
            {
                try
                {
                    SE.Play(crafter.idSoundBG);
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}

            Progress_Custom progress = new Progress_Custom
            {
                canProgress = () =>
                {
                    if (requireOn && (machine == null || !machine.isOn))
                    {
                        return false;
                    }

                    if (machine == null || machine.isDestroyed)
                    {
                        return false;
                    }

                    foreach (Thing ing in ings)
                    {
                        if (ing == null || ing.isDestroyed)
                        {
                            return false;
                        }

                        if (crafter.IsConsumeIng && !ing.ExistsOnMap)
                        {
                            return false;
                        }
                    }

                    return owner != null && !owner.isDead;
                },
                onProgress = _ =>
                {
                    if (owner == null)
                    {
                        return;
                    }

                    if (machine != null && machine.ExistsOnMap && !owner.pos.Equals(machine.pos))
                    {
                        owner.LookAt(machine);
                    }

                    try
                    {
                        owner.PlaySound(crafter.idSoundProgress);
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
try
                    {
                        if (machine != null && machine.ExistsOnMap)
                        {
                            TraitCrafter.AnimeType animeType = crafter.animeType;
                            if ((uint)(animeType - 1) <= 1u)
                            {
                                machine.renderer.PlayAnime(crafter.IdAnimeProgress);
                            }
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
foreach (Thing ing2 in ings)
                    {
                        try
                        {
                            if (ing2 != null && ing2.renderer != null)
                            {
                                ing2.renderer.PlayAnime(crafter.IdAnimeProgress);
                            }
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}
                },
                onProgressComplete = () =>
                {
                    CompleteOne(owner, crafter, machine, shell, ings, blessed, costSp, duration);
                }
            }.SetDuration(duration, 5);

            try
            {
                owner.SetTempHand(-1, -1);
            }
            catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
yield return Do(progress);

            if (progress.status == Status.Fail)
            {
                // Return leftover non-destroyed ings to PC.
                ReturnIngs(ings);
                ProcessorJobSession.Clear("ai-fail");
                yield return Cancel();
                yield break;
            }

            if (!ProcessorJobSession.Active)
            {
                yield return Success();
                yield break;
            }

            if (ProcessorJobSession.Remaining <= 0)
            {
                break;
            }
        }

        // Clean end of loop.
        if (ProcessorJobSession.Active)
        {
            TraitCrafter? c2 = ProcessorJobSession.GetCrafter();
            if (c2 != null && c2.AutoTurnOff && c2.owner != null && c2.owner.isOn)
            {
                try
                {
                    c2.Toggle(on: false);
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}

            if (c2 != null && !c2.idSoundBG.IsEmpty())
            {
                try
                {
                    EClass.Sound.Stop(c2.idSoundBG);
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}

            ProcessorJobSession.Clear("ai-end");
        }

        yield return Success();
    }

    static int ComputeDuration(TraitCrafter crafter, AI_UseCrafter shell, int costSp)
    {
        // Mirror TraitCrafter.GetDuration but use NPC skill instead of EClass.pc.
        try
        {
            SourceRecipe.Row? src = crafter.GetSource(shell);
            if (src == null)
            {
                return Mathf.Max(1, 10);
            }

            int skill = ProcessorJobSession.NpcSkill;
            return Mathf.Max(1, src.time * 100 / (80 + skill * 5));
        }
        catch
        {
            try
            {
                return Mathf.Max(1, crafter.GetDuration(shell, costSp));
            }
            catch
            {
                return 10;
            }
        }
    }

    static void CompleteOne(
        Chara owner,
        TraitCrafter crafter,
        Card machine,
        AI_UseCrafter shell,
        List<Thing> ings,
        BlessedState blessed,
        int costSp,
        int duration)
    {
        try
        {
            if (crafter.StopSoundProgress)
            {
                EClass.Sound.Stop(crafter.idSoundProgress);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
try
        {
            owner.PlaySound(crafter.idSoundComplete);
        }
        catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
int exp = 0;
        try
        {
            // recipe is always null for processors — conversion path.
            Thing? product = crafter.Craft(shell);
            if (product != null)
            {
                try
                {
                    if (product.category != null && product.category.ignoreBless == 0)
                    {
                        product.SetBlessedState(blessed);
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
try
                {
                    product.PlaySoundDrop(spatial: false);
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
Point dropAt = (machine != null && machine.ExistsOnMap) ? machine.pos : owner.pos;
                EClass._zone.AddCard(product, dropAt);
                try
                {
                    product.Identify(show: false);
                }
                catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
// Slice B rule: product on ground — do not auto-pick to NPC or PC.
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogWarn("processor Craft failed: " + ex.Message);
        }

        // Consume ingredients that should be consumed.
        try
        {
            SourceRecipe.Row? source = crafter.GetSource(shell);
            for (int m = 0; m < ings.Count; m++)
            {
                if (crafter.ShouldConsumeIng(source, m))
                {
                    try
                    {
                        ings[m].Destroy();
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}
            }

            foreach (Thing ing3 in ings)
            {
                if (ing3 != null && !ing3.isDestroyed && ing3.ExistsOnMap)
                {
                    // Leftover non-consumed (rare for MVP machines) — leave on ground near machine,
                    // or pick to PC via session return list is not tracking these pieces.
                    // Prefer pick to PC so they are not lost.
                    try
                    {
                        if (EClass.pc != null)
                        {
                            EClass.pc.Pick(ing3);
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("consume ings: " + ex.Message);
        }

        if (crafter.IsRequireFuel && machine != null)
        {
            try
            {
                machine.ModCharge(-crafter.FuelCost * 1);
                if (machine.c_charges <= 0)
                {
                    machine.c_charges = 0;
                    crafter.Toggle(on: false);
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}

        try
        {
            int skillId = ProcessorJobSession.SkillId;
            if (skillId == 0)
            {
                string alias = crafter.IDReqEle(null);
                if (!alias.IsEmpty())
                {
                    skillId = EClass.sources.elements.alias[alias].id;
                }
            }

            if (skillId != 0 && owner.elements != null)
            {
                exp = costSp * 12 * (100 + duration * 2) / 100;
                owner.elements.ModExp(skillId, exp);
            }

            owner.stamina.Mod(-costSp);
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("exp/sp failed: " + ex.Message);
            exp = 0;
        }

        try
        {
            Point from = (machine != null && machine.ExistsOnMap) ? machine.pos : owner.pos;
            Effect.Get("smoke").Play(from);
            owner.renderer.PlayAnime(AnimeID.JumpSmall);
        }
        catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
ProcessorJobSession.NoteCompleted(exp);
        Plugin.LogDebug(
            $"processor tick done={ProcessorJobSession.Completed} left={ProcessorJobSession.Remaining} exp+{exp}");
    }

    static void CleanupPartialIngs(List<Thing> ings)
    {
        if (ings == null)
        {
            return;
        }

        foreach (Thing t in ings)
        {
            if (t == null || t.isDestroyed)
            {
                continue;
            }

            try
            {
                if (EClass.pc != null)
                {
                    EClass.pc.Pick(t);
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
}
    }

    static void ReturnIngs(List<Thing> ings)
    {
        CleanupPartialIngs(ings);
    }
}


