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
            // Leave/unload cancels freestanding AI — hold resident job instead of wiping.
            ProcessorJobSession.HandleAiInterrupted("ai-cancel");
        }
    }

    public override void OnSuccess()
    {
        if (ProcessorJobSession.Active && owner != null && owner.uid == ProcessorJobSession.NpcUid)
        {
            if (ProcessorJobSession.Suspended)
            {
                return;
            }

            if (ProcessorJobSession.Remaining <= 0)
            {
                ProcessorJobSession.Clear("ai-end");
            }
        }
    }

    public override IEnumerable<Status> Run()
    {
        if (!ProcessorJobSession.Active || ProcessorJobSession.Suspended
            || owner == null || owner.uid != ProcessorJobSession.NpcUid)
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

        while (ProcessorJobSession.Active && !ProcessorJobSession.Suspended && ProcessorJobSession.Remaining > 0)
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
                        // Split may copy hide flags from parked bulk; re-apply craft-visible rules.
                        ProcessorJobSession.PrepareCraftPiece(card as Thing ?? piece, crafter);
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.LogWarn("place ingredient failed: " + ex.Message);
                    }
                }
                else
                {
                    // Non-consume path still keeps the piece out of PC bag.
                    ProcessorJobSession.PrepareCraftPiece(piece, crafter);
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
            ProcessorJobSession.RememberCraftTiming(duration, costSp);

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
            }.SetDuration(duration, 2);

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
                ProcessorJobSession.HandleAiInterrupted("ai-fail");
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

        // Clean end of loop (suspend exits while via !Active/Suspended — do not Clear held jobs).
        if (ProcessorJobSession.Active && !ProcessorJobSession.Suspended && ProcessorJobSession.Remaining <= 0)
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
        // Mirror TraitCrafter.GetDuration but use NPC skill instead of EClass.pc,
        // then cut NPC process duration in half vs the skill-scaled base.
        try
        {
            SourceRecipe.Row? src = crafter.GetSource(shell);
            if (src == null)
            {
                return ProcessorJobSession.ApplyNpcDurationCut(10);
            }

            int skill = ProcessorJobSession.NpcSkill;
            return ProcessorJobSession.ApplyNpcDurationCut(src.time * 100 / (80 + skill * 5));
        }
        catch
        {
            try
            {
                return ProcessorJobSession.ApplyNpcDurationCut(crafter.GetDuration(shell, costSp));
            }
            catch
            {
                return ProcessorJobSession.ApplyNpcDurationCut(10);
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
        bool craftedOk = false;
        Thing? spawnedProduct = null;
        try
        {
            // recipe is always null for processors - conversion path.
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
                // Slice B rule: product on ground - do not auto-pick to NPC or PC.
                spawnedProduct = product;
                craftedOk = true;
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogWarn("processor Craft failed: " + ex.Message);
            craftedOk = false;
        }

        // Consume craft pieces. MVP processors are IsConsumeIng; always Destroy pieces
        // so a null GetSource cannot skip consume and leave free mats / free products.
        bool consumedOk = true;
        try
        {
            bool isConsume;
            try { isConsume = crafter.IsConsumeIng; } catch { isConsume = true; }

            if (isConsume)
            {
                consumedOk = ings.Count > 0;
                for (int m = 0; m < ings.Count; m++)
                {
                    Thing? piece = ings[m];
                    if (piece == null)
                    {
                        consumedOk = false;
                        continue;
                    }

                    if (piece.isDestroyed)
                    {
                        continue;
                    }

                    try
                    {
                        piece.Destroy();
                    }
                    catch (System.Exception __e)
                    {
                        Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message);
                        consumedOk = false;
                    }

                    if (!piece.isDestroyed)
                    {
                        consumedOk = false;
                    }
                }
            }
            else
            {
                // Non-consume machines: return leftover pieces to PC.
                foreach (Thing ing3 in ings)
                {
                    if (ing3 != null && !ing3.isDestroyed && ing3.ExistsOnMap)
                    {
                        try { ProcessorJobSession.ReturnOneIngredientToPc(ing3); }
                        catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("consume ings: " + ex.Message);
            consumedOk = false;
        }

        // Integrity: never credit a craft that did not both produce and consume when required.
        if (craftedOk && !consumedOk)
        {
            Plugin.LogWarn("processor product without consume - destroying product, no NoteCompleted");
            try
            {
                if (spawnedProduct != null && !spawnedProduct.isDestroyed)
                {
                    spawnedProduct.Destroy();
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
            ProcessorJobSession.SyncIngredientMarksAfterCraft();
            return;
        }

        if (!craftedOk)
        {
            // No product: do not advance Remaining (mats already Split from bulk; pieces
            // destroyed above if isConsume - rare GetSource miss). Avoid free progress.
            ProcessorJobSession.SyncIngredientMarksAfterCraft();
            return;
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
                // costSp is kept only for exp scaling; process jobs no longer spend SP.
                exp = costSp * 12 * (100 + duration * 2) / 100;
                owner.elements.ModExp(skillId, exp);
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("exp failed: " + ex.Message);
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
        ProcessorJobSession.SyncIngredientMarksAfterCraft();
        Plugin.LogDebug(
            $"processor tick done={ProcessorJobSession.Completed} left={ProcessorJobSession.Remaining} exp+{exp}");
    }

    /// <summary>
    /// One conversion cycle for return catch-up (no progress anime). Uses parked bulk on machine.
    /// Returns false if ingredients/fuel/machine cannot complete another craft.
    /// </summary>
    internal static bool TryCatchUpOne(Chara worker, TraitCrafter crafter, Card machine)
    {
        if (worker == null || worker.isDead || crafter == null || machine == null || machine.isDestroyed)
        {
            return false;
        }

        if (!ProcessorJobSession.Active || ProcessorJobSession.Remaining <= 0)
        {
            return false;
        }

        List<Thing> sources = ProcessorJobSession.Ingredients;
        if (sources == null || sources.Count == 0)
        {
            return false;
        }

        // Catch-up must use live map stacks only. Ghost refs after unload would Split
        // in memory and leave the real parked bulk untouched (= free product / dupe).
        int[] numsBefore = new int[sources.Count];
        for (int i = 0; i < sources.Count; i++)
        {
            Thing? s = sources[i];
            if (s == null || s.isDestroyed || s.Num <= 0)
            {
                return false;
            }

            try
            {
                if (!s.ExistsOnMap && crafter.IsConsumeIng)
                {
                    Plugin.LogDebug("catch-up abort: ingredient not on map uid=" + s.uid);
                    return false;
                }
            }
            catch
            {
                return false;
            }

            numsBefore[i] = s.Num;
        }

        List<Thing> targets = new List<Thing>(sources);
        try
        {
            if (!crafter.IsFuelEnough(1, targets))
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        int remainingBefore = ProcessorJobSession.Remaining;
        int completedBefore = ProcessorJobSession.Completed;
        List<Thing> ings = new List<Thing>();
        BlessedState blessed = BlessedState.Normal;
        try
        {
            for (int i = 0; i < sources.Count; i++)
            {
                Thing src = sources[i];
                int numBeforeSplit = src.Num;
                Thing? piece = null;
                try { piece = src.Split(1); } catch { piece = null; }
                if (piece == null || piece.isDestroyed)
                {
                    CleanupPartialIngs(ings);
                    return false;
                }

                // If Split returned the same full stack object without decreasing Num,
                // bulk accounting is wrong - abort rather than craft free goods.
                if (ReferenceEquals(piece, src) && numBeforeSplit <= 1)
                {
                    // Last unit: vanilla Split returns the same Thing; Num stays 1 until Destroy.
                }
                else if (!ReferenceEquals(piece, src) && src.Num != numBeforeSplit - 1 && src.Num != numBeforeSplit)
                {
                    // Unexpected; continue with Destroy path but log.
                    Plugin.LogDebug($"catch-up split num odd before={numBeforeSplit} after={src.Num}");
                }
                else if (ReferenceEquals(piece, src) && numBeforeSplit > 1)
                {
                    CleanupPartialIngs(ings);
                    Plugin.LogDebug("catch-up abort: Split returned whole stack unexpectedly");
                    return false;
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
                        Point place = machine.ExistsOnMap ? machine.pos : worker.pos;
                        if (EClass._zone != null)
                        {
                            // Last-unit Split returns the parked bulk itself; it is already on map.
                            if (!piece.ExistsOnMap)
                            {
                                Card card = EClass._zone.AddCard(piece, place);
                                try { card.altitude = machine.ExistsOnMap ? 0 : 1; } catch { }
                                ProcessorJobSession.PrepareCraftPiece(card as Thing ?? piece, crafter);
                            }
                            else
                            {
                                ProcessorJobSession.PrepareCraftPiece(piece, crafter);
                            }
                        }
                    }
                    catch
                    {
                        CleanupPartialIngs(ings);
                        return false;
                    }
                }
                else
                {
                    ProcessorJobSession.PrepareCraftPiece(piece, crafter);
                }
            }

            bool requireOn = crafter.IsRequireFuel || crafter.ToggleType != ToggleType.None;
            if (requireOn && !machine.isOn)
            {
                try { crafter.Toggle(on: true); } catch { }
            }

            AI_UseCrafter shell = new AI_UseCrafter
            {
                crafter = crafter,
                recipe = null,
                num = 1,
                ings = ings,
                owner = worker
            };

            int baseSp;
            try { baseSp = crafter.GetCostSp(shell); }
            catch { baseSp = crafter.CostSP; }
            int costSp = ProcessorJobSession.AdjustCostSp(baseSp);
            int duration = ComputeDuration(crafter, shell, costSp);
            ProcessorJobSession.RememberCraftTiming(duration, costSp);

            CompleteOne(worker, crafter, machine, shell, ings, blessed, costSp, duration);

            if (!ProcessorJobSession.Active)
            {
                return false;
            }

            // CompleteOne must have advanced the job; otherwise treat as failed cycle.
            if (ProcessorJobSession.Completed <= completedBefore
                && ProcessorJobSession.Remaining >= remainingBefore)
            {
                Plugin.LogDebug("catch-up one made no progress");
                return false;
            }

            // Bulk must have decreased (or last unit destroyed) for consume machines.
            if (crafter.IsConsumeIng)
            {
                bool anyDecreased = false;
                for (int i = 0; i < sources.Count; i++)
                {
                    Thing? s = sources[i];
                    if (s == null || s.isDestroyed)
                    {
                        anyDecreased = true;
                        break;
                    }

                    if (s.Num < numsBefore[i])
                    {
                        anyDecreased = true;
                        break;
                    }
                }

                if (!anyDecreased)
                {
                    Plugin.LogWarn("catch-up crafted without bulk decrease - abort further catch-up");
                    return false;
                }
            }

            ProcessorJobSession.SyncIngredientMarksAfterCraft();
            return true;
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("processor catch-up one failed: " + ex.Message);
            CleanupPartialIngs(ings);
            return false;
        }
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
                ProcessorJobSession.ReturnOneIngredientToPc(t);
            }
            catch (System.Exception __e) { Plugin.LogDebug("AI_NpcProcess.cs silent catch: " + __e.Message); }
        }
    }

    static void ReturnIngs(List<Thing> ings)
    {
        CleanupPartialIngs(ings);
    }
}


