using System.Collections.Generic;
using UnityEngine;

namespace NpcLabor.Process;

/// <summary>
/// Runtime-only processor outsourcing jobs (slice B). Not saved.
/// One NPC runs conversion at a TraitCrafter (saw / mill / wood mill).
/// Product is dropped on the ground at the machine; leftovers returned to PC.
/// </summary>
internal static class ProcessorJobSession
{
    internal const float NpcSpPerSkill = 0.25f;

    internal static bool Active;
    internal static int NpcUid;
    internal static int MachineUid;
    internal static int SkillId;
    internal static int NpcSkill;
    internal static int Remaining;
    internal static int Completed;
    internal static int ExpGranted;
    internal static string? NpcName;
    internal static string? MachineName;
    internal static string LastClearReason = "";
    /// <summary>Party companions leave party for the job so AI is free, then rejoin on Clear. Flag order matches town labor / vanilla auto-rejoin.</summary>
    internal static bool WasPartyMember;

    // Strong refs: installed furniture is not always found via map uid lookup alone.
    internal static TraitCrafter? CrafterRef;
    internal static Card? MachineRef;

    // Snapshot of ingredients assigned to the job (Things may be split copies).
    internal static readonly List<Thing> Ingredients = new List<Thing>();

    internal static Chara? GetWorker()
    {
        if (!Active || NpcUid == 0)
        {
            return null;
        }

        Chara? c = RefChara.Get(NpcUid);
        if (c == null || c.isDead || !c.IsAliveInCurrentZone)
        {
            return null;
        }

        return c;
    }

    internal static TraitCrafter? GetCrafter()
    {
        if (!Active)
        {
            return null;
        }

        if (CrafterRef != null)
        {
            Card? owner = CrafterRef.owner;
            if (owner != null && !owner.isDestroyed)
            {
                MachineRef = owner;
                return CrafterRef;
            }
        }

        Card? card = GetMachineCard();
        TraitCrafter? trait = card?.trait as TraitCrafter;
        if (trait != null)
        {
            CrafterRef = trait;
        }

        return trait;
    }

    internal static Card? GetMachineCard()
    {
        if (MachineRef != null && !MachineRef.isDestroyed)
        {
            return MachineRef;
        }

        if (CrafterRef != null)
        {
            Card? owner = CrafterRef.owner;
            if (owner != null && !owner.isDestroyed)
            {
                MachineRef = owner;
                return owner;
            }
        }

        if (MachineUid == 0)
        {
            return null;
        }

        Card? found = FindCardByUid(MachineUid);
        if (found != null)
        {
            MachineRef = found;
            if (found.trait is TraitCrafter trait)
            {
                CrafterRef = trait;
            }
        }

        return found;
    }

    static Card? FindCardByUid(int uid)
    {
        if (uid == 0 || EClass._map == null)
        {
            return null;
        }

        try
        {
            Thing? t = EClass._map.FindThing(uid);
            if (t != null)
            {
                return t;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("ProcessorJobSession.cs silent catch: " + __e.Message); }
try
        {
            if (EClass._map.things != null)
            {
                foreach (Thing t in EClass._map.things)
                {
                    if (t != null && t.uid == uid)
                    {
                        return t;
                    }
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("ProcessorJobSession.cs silent catch: " + __e.Message); }
// Installed furniture is held via CrafterRef/MachineRef at job start.
        // Do not walk Installed by uid: PropsInstalled.Find is id-based, not uid-based.
        return null;
    }

    internal static bool TryStart(
        Chara worker,
        TraitCrafter crafter,
        List<Thing> ings,
        int count)
    {
        Clear("reopen", announce: false);

        if (worker == null || crafter == null || ings == null || ings.Count == 0 || count <= 0)
        {
            return false;
        }

        if (!ProcessorWhitelist.IsSupported(crafter))
        {
            Plugin.LogWarn("processor start rejected: unsupported crafter " + crafter.GetType().Name);
            return false;
        }

        if (crafter.owner == null || crafter.owner.isDestroyed)
        {
            return false;
        }

        int skillId = ProcessorWhitelist.ResolveSkillId(crafter);
        int skill = skillId > 0 ? worker.Evalue(skillId) : 0;
        if (skillId > 0 && skill <= 0)
        {
            Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.cannotOperate", CoCraft.AssistantResolver.NameOf(worker)));
            return false;
        }

        // Claim ingredient stacks (split so PC inventory keeps remainder).
        Ingredients.Clear();
        for (int i = 0; i < ings.Count; i++)
        {
            Thing? src = ings[i];
            if (src == null || src.isDestroyed || src.Num <= 0)
            {
                ReturnIngredientsToPc();
                Ingredients.Clear();
                return false;
            }

            // For multi-batch, we keep the full stack and consume per craft via Split.
            Ingredients.Add(src);
        }

        Active = true;
        NpcUid = worker.uid;
        MachineUid = crafter.owner.uid;
        CrafterRef = crafter;
        MachineRef = crafter.owner;
        SkillId = skillId;
        NpcSkill = skill;
        Remaining = count;
        Completed = 0;
        ExpGranted = 0;
        NpcName = CoCraft.AssistantResolver.NameOf(worker);
        MachineName = ProcessorWhitelist.DisplayName(crafter);
        LastClearReason = "";
        WasPartyMember = false;

        // Party AI can keep companions glued to the PC; temporarily detach so they can work.
        // Order matters: vanilla Party.RemoveMember always clears c_wasInPcParty=false.
        // Set the flag AFTER RemoveMember so FactionBranch.OnAfterSimulate can auto-rejoin
        // if this runtime-only job is lost (crash / force quit) before Clear().
        // Same pattern as town labor; still no process-job save.
        try
        {
            bool inParty = worker.IsPCParty
                || (EClass.pc?.party?.members != null && EClass.pc.party.members.Contains(worker));
            if (inParty && EClass.pc?.party != null)
            {
                WasPartyMember = true;
                EClass.pc.party.RemoveMember(worker);
                try { worker.c_wasInPcParty = true; } catch { }
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("processor leave party: " + ex.Message);
        }

        Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.start", NpcName, MachineName, Remaining));
        Plugin.LogInfo(
            $"processor start: npc={NpcName}#{NpcUid} machine={MachineName}#{MachineUid} " +
            $"skill={SkillId}/{NpcSkill} count={Remaining} wasParty={WasPartyMember}");

        try
        {
            // Immediate kick so the NPC leaves whatever idle/work goal they had.
            worker.SetAIImmediate(new AI_NpcProcess
            {
                jobToken = NpcUid ^ MachineUid ^ Remaining
            });
        }
        catch (System.Exception ex)
        {
            Plugin.LogWarn("SetAI AI_NpcProcess failed: " + ex.Message);
            Clear("ai-set-fail", announce: false);
            return false;
        }

        return true;
    }

    internal static int AdjustCostSp(int baseSp)
    {
        if (!Active)
        {
            return baseSp;
        }

        int reduced = baseSp - Mathf.FloorToInt(NpcSkill * NpcSpPerSkill);
        return Mathf.Max(1, reduced);
    }

    internal static void NoteCompleted(int exp)
    {
        if (!Active)
        {
            return;
        }

        Completed++;
        if (Remaining > 0)
        {
            Remaining--;
        }

        if (exp > 0)
        {
            ExpGranted += exp;
        }
    }

    internal static void Clear(string reason, bool announce = true)
    {
        if (!Active && NpcUid == 0)
        {
            ReturnIngredientsToPc();
            Ingredients.Clear();
            return;
        }

        LastClearReason = reason ?? "";
        if (announce && Active)
        {
            AnnounceFinish(reason);
        }

        Plugin.LogDebug(
            $"processor clear ({reason}) npc={NpcUid} done={Completed} left={Remaining} exp={ExpGranted}");

        // Snapshot before zeroing so party restore still finds the worker.
        int restoreUid = NpcUid;
        bool restoreParty = WasPartyMember;

        // Return any leftover claimed ingredients still on map / held by NPC.
        ReturnIngredientsToPc();
        Ingredients.Clear();

        Active = false;
        NpcUid = 0;
        MachineUid = 0;
        CrafterRef = null;
        MachineRef = null;
        SkillId = 0;
        NpcSkill = 0;
        Remaining = 0;
        Completed = 0;
        ExpGranted = 0;
        NpcName = null;
        MachineName = null;
        WasPartyMember = false;

        if (restoreParty && restoreUid > 0)
        {
            TryRestoreParty(restoreUid);
        }
    }

    static void TryRestoreParty(int uid)
    {
        try
        {
            Chara? worker = RefChara.Get(uid);
            if (worker == null || worker.isDead)
            {
                return;
            }

            Party? party = EClass.pc?.party;
            if (party == null)
            {
                // Keep flag so vanilla OnAfterSimulate can still heal later.
                try { worker.c_wasInPcParty = true; } catch { }
                return;
            }

            try
            {
                if (!worker.IsGlobal)
                {
                    worker.SetGlobal();
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogDebug("processor SetGlobal: " + ex.Message);
            }

            bool already = false;
            try
            {
                already = worker.IsPCParty
                    || worker.party == party
                    || (party.uidMembers != null && party.uidMembers.Contains(worker.uid));
            }
            catch
            {
                already = false;
            }

            bool rejoined = already;
            if (!already)
            {
                try
                {
                    try { worker.party = null; } catch { }
                    party.AddMemeber(worker, showMsg: false);
                }
                catch (System.Exception ex)
                {
                    Plugin.LogDebug("processor party rejoin: " + ex.Message);
                    try
                    {
                        if (party.uidMembers != null && !party.uidMembers.Contains(worker.uid))
                        {
                            party.uidMembers.Add(worker.uid);
                        }
                        try { party._members = null; } catch { }
                        try { worker.party = party; } catch { }
                    }
                    catch
                    {
                    }
                }

                try
                {
                    rejoined = worker.IsPCParty
                        || worker.party == party
                        || (party.uidMembers != null && party.uidMembers.Contains(worker.uid));
                }
                catch
                {
                    rejoined = false;
                }
            }

            // Only clear vanilla auto-rejoin flag once membership looks good.
            if (rejoined)
            {
                try { worker.c_wasInPcParty = false; } catch { }
            }
            else
            {
                try { worker.c_wasInPcParty = true; } catch { }
                Plugin.LogDebug("processor party restore incomplete uid=" + uid);
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("processor party restore: " + ex.Message);
        }
    }

    internal static void ReturnIngredientsToPc()
    {
        if (EClass.pc == null)
        {
            return;
        }

        for (int i = 0; i < Ingredients.Count; i++)
        {
            Thing? t = Ingredients[i];
            if (t == null || t.isDestroyed || t.Num <= 0)
            {
                continue;
            }

            try
            {
                // If still sitting on the machine tile (consume-ing path), pick back to PC.
                if (t.ExistsOnMap)
                {
                    t.isHidden = false;
                    EClass.pc.Pick(t);
                    continue;
                }

                // If in NPC inventory, transfer to PC.
                Card? root = t.GetRootCard();
                if (root != null && root.IsPC)
                {
                    continue;
                }

                if (root != null && root.isChara && root != EClass.pc)
                {
                    EClass.pc.Pick(t);
                    continue;
                }

                // Orphaned stack — try pick.
                EClass.pc.Pick(t);
            }
            catch (System.Exception ex)
            {
                Plugin.LogDebug("return ingredient failed: " + ex.Message);
            }
        }
    }

    static void AnnounceFinish(string reason)
    {
        string name = NpcName ?? NpcLabor.LaborText.T("proc.msg.residentFallback");

        if (reason == "reopen" || reason == "plugin-destroy" || reason == "ai-set-fail")
        {
            return;
        }

        if (Completed > 0)
        {
            if (ExpGranted > 0)
            {
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.doneSkilled", name, NpcLabor.LaborTerms.Process));
            }
            else
            {
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.done", name, NpcLabor.LaborTerms.Process));
            }
            return;
        }

        switch (reason)
        {
            case "ai-cancel":
            case "ai-fail":
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.interrupted", name, NpcLabor.LaborTerms.Process));
                break;
            case "pc-death":
            case "zone-change":
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.stopped", name, NpcLabor.LaborTerms.Process));
                break;
            case "no-fuel":
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.noFuel", name));
                break;
            case "no-ings":
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.noMaterial", name));
                break;
            case "machine-gone":
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.noMachine", name));
                break;
            case "stamina":
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.exhausted", name));
                break;
            default:
                Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.ended", name, NpcLabor.LaborTerms.Process));
                break;
        }
    }
}

