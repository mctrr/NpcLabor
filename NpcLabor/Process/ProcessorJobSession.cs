using System.Collections.Generic;
using UnityEngine;

namespace NpcLabor.Process;

/// <summary>
/// Runtime-only processor outsourcing jobs (slice B). Not saved.
/// Ingredients park on the machine tile. PC leave: party worker Clears; resident
/// job is held (Suspended) with SuspendedAtRaw recorded — no hour-tick craft while
/// away. On return to WorkZoneUid: compute finished crafts from elapsed game minutes,
/// spawn products for that count at half away speed, then resume freestanding AI if Remaining > 0.
/// </summary>
internal static class ProcessorJobSession
{
    internal const float NpcSpPerSkill = 0.25f;
    /// <summary>NPC process duration multiplier vs skill-scaled base (half speed-cost = faster).</summary>
    internal const float NpcDurationFactor = 0.5f;
    /// <summary>
    /// Catch-up model: one AI progress step ~= one game minute (GameDate raw unit).
    /// Live craft uses Progress_Custom duration with interval 2; away catch-up is a
    /// coarse estimate (duration minutes per craft), not a full anime sim.
    /// </summary>
    internal const int MinutesPerProgress = 1;
    /// <summary>Away catch-up only: half on-map rate (double minutes per craft).</summary>
    internal const int AwaySpeedDivisor = 2;

    internal static bool Active;
    /// <summary>Resident job held while PC is away from the work map. No live AI / no hour sim.</summary>
    internal static bool Suspended;
    internal static int NpcUid;
    internal static int MachineUid;
    /// <summary>Zone where machine + parked ings live. Catch-up only runs when PC re-enters this zone.</summary>
    internal static int WorkZoneUid;
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
    /// <summary>Last observed live craft duration (after NPC half-cut). Used for return catch-up.</summary>
    internal static int LastDuration;
    /// <summary>Last observed SP cost per craft (after NPC reduction; SP not drained for NPC jobs).</summary>
    internal static int LastCostSp;
    /// <summary>world.date.GetRaw() when the job was suspended (PC left).</summary>
    internal static int SuspendedAtRaw;
    /// <summary>Ignore freestanding AI cancel for a few frames after resume SetAI.</summary>
    internal static int ResumeIgnoreCancelUntilFrame;

    // Strong refs: installed furniture is not always found via map uid lookup alone.
    internal static TraitCrafter? CrafterRef;
    internal static Card? MachineRef;

    // Live ingredient stacks for the job. After zone unload these refs go stale -
    // rebind from IngredientMarks before any catch-up / resume craft.
    internal static readonly List<Thing> Ingredients = new List<Thing>();
    /// <summary>Stable identity for parked ings across map unload/reload.</summary>
    internal static readonly List<IngredientMark> IngredientMarks = new List<IngredientMark>();

    internal struct IngredientMark
    {
        internal int Uid;
        internal string Id;
        internal int MaterialId;

        internal static IngredientMark From(Thing t)
        {
            IngredientMark m = default;
            if (t == null)
            {
                return m;
            }

            try { m.Uid = t.uid; } catch { m.Uid = 0; }
            try { m.Id = t.id ?? ""; } catch { m.Id = ""; }
            try { m.MaterialId = t.material != null ? t.material.id : t.idMaterial; } catch { m.MaterialId = 0; }
            return m;
        }
    }

    internal static Chara? GetWorker()
    {
        if (!Active || NpcUid == 0)
        {
            return null;
        }

        Chara? c = RefChara.Get(NpcUid);
        if (c == null || c.isDead)
        {
            return null;
        }

        // Suspended jobs may keep the worker on an unloaded home map; only require
        // live-in-current-zone when freestanding AI should actually run.
        if (!Suspended)
        {
            try
            {
                if (!c.IsAliveInCurrentZone)
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
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
        IngredientMarks.Clear();
        for (int i = 0; i < ings.Count; i++)
        {
            Thing? src = ings[i];
            if (src == null || src.isDestroyed || src.Num <= 0)
            {
                ReturnIngredientsToPc();
                Ingredients.Clear();
                IngredientMarks.Clear();
                return false;
            }

            // Park on the machine tile for the whole multi-batch job.
            // Do not hold bulk materials in PC inventory (overweight).
            if (!ParkIngredientOnMachine(src, crafter))
            {
                ReturnIngredientsToPc();
                Ingredients.Clear();
                IngredientMarks.Clear();
                return false;
            }

            Ingredients.Add(src);
            IngredientMarks.Add(IngredientMark.From(src));
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
        LastDuration = 0;
        LastCostSp = 0;
        Suspended = false;
        SuspendedAtRaw = 0;
        ResumeIgnoreCancelUntilFrame = 0;
        try
        {
            WorkZoneUid = EClass._zone != null ? EClass._zone.uid : 0;
        }
        catch
        {
            WorkZoneUid = 0;
        }

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

    /// <summary>Half NPC process duration vs the skill-scaled vanilla-style base.</summary>
    internal static int ApplyNpcDurationCut(int baseDuration)
    {
        return Mathf.Max(1, Mathf.FloorToInt(Mathf.Max(1, baseDuration) * NpcDurationFactor));
    }

    /// <summary>Cache live craft timing for estimates / debug.</summary>
    internal static void RememberCraftTiming(int duration, int costSp)
    {
        if (!Active)
        {
            return;
        }

        if (duration > 0)
        {
            LastDuration = duration;
        }

        if (costSp > 0)
        {
            LastCostSp = costSp;
        }
    }

    internal static int EstimateDuration()
    {
        if (LastDuration > 0)
        {
            return LastDuration;
        }

        int skill = Mathf.Max(0, NpcSkill);
        int raw = 10 * 100 / Mathf.Max(1, 80 + skill * 5);
        return ApplyNpcDurationCut(raw);
    }

    static int CurrentRawDate()
    {
        try
        {
            return EClass.world?.date?.GetRaw() ?? 0;
        }
        catch
        {
            return 0;
        }
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
            if (Ingredients.Count == 0 && IngredientMarks.Count > 0)
            {
                try { TryRebindIngredients(GetMachineCard()); } catch { }
            }
            ReturnIngredientsToPc();
            Ingredients.Clear();
            IngredientMarks.Clear();
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

        // After suspend, Ingredients were cleared; rebind from marks before refund.
        if (Ingredients.Count == 0 && IngredientMarks.Count > 0)
        {
            try { TryRebindIngredients(GetMachineCard()); } catch { }
        }

        // Return any leftover claimed ingredients still on map / held by NPC.
        ReturnIngredientsToPc();
        Ingredients.Clear();
        IngredientMarks.Clear();

        Active = false;
        Suspended = false;
        NpcUid = 0;
        MachineUid = 0;
        WorkZoneUid = 0;
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
        LastDuration = 0;
        LastCostSp = 0;
        SuspendedAtRaw = 0;
        ResumeIgnoreCancelUntilFrame = 0;

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
                ReturnOneIngredientToPc(t);
            }
            catch (System.Exception ex)
            {
                Plugin.LogDebug("return ingredient failed: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Park a claimed ingredient stack on the machine tile for the job lifetime.
    /// Visible on the machine; ignoreAutoPick so bulk is not auto-looted mid-job.
    /// </summary>
    internal static bool ParkIngredientOnMachine(Thing piece, TraitCrafter crafter)
    {
        if (piece == null || piece.isDestroyed || piece.Num <= 0)
        {
            return false;
        }
        Card? machine = crafter?.owner;
        if (machine == null || machine.isDestroyed || EClass._zone == null)
        {
            return false;
        }
        try
        {
            // Already sitting on the machine cell — just re-mark parking flags.
            if (piece.ExistsOnMap
                && machine.ExistsOnMap
                && piece.pos != null
                && machine.pos != null
                && piece.pos.Equals(machine.pos))
            {
                ApplyParkFlags(piece);
                return true;
            }
            Point? place = machine.ExistsOnMap
                ? machine.pos
                : (EClass.pc != null ? EClass.pc.pos : null);
            if (place == null)
            {
                return false;
            }
            Card card = EClass._zone.AddCard(piece, place);
            if (card == null || card.isDestroyed)
            {
                return false;
            }
            try
            {
                card.altitude = machine.ExistsOnMap ? 0 : 1;
            }
            catch
            {
            }
            ApplyParkFlags(card as Thing ?? piece);
            return true;
        }
        catch (System.Exception ex)
        {
            Plugin.LogWarn("park ingredient on machine failed: " + ex.Message);
            return false;
        }
    }
    internal static void ApplyParkFlags(Thing t)
    {
        if (t == null || t.isDestroyed)
        {
            return;
        }
        // Never hide parked bulk. isHidden makes stacks look deleted and confuses
        // map/inventory scans; ignoreAutoPick alone is enough to stop auto-loot.
        try { t.isHidden = false; } catch { } // heal older builds that hid bulk
        try { t.ignoreAutoPick = true; } catch { }
    }
    internal static void ClearParkFlags(Thing t)
    {
        if (t == null || t.isDestroyed)
        {
            return;
        }
        try { t.isHidden = false; } catch { } // heal older builds that hid bulk
        try { t.ignoreAutoPick = false; } catch { }
    }
    /// <summary>
    /// After Split for one craft cycle: keep microwave pieces hidden; show others for anime.
    /// </summary>
    internal static void PrepareCraftPiece(Thing piece, TraitCrafter crafter)
    {
        if (piece == null || piece.isDestroyed)
        {
            return;
        }
        try
        {
            // Duplicate() copies bit flags from the parked bulk stack.
            bool hide = crafter != null && crafter.animeType == TraitCrafter.AnimeType.Microwave;
            piece.isHidden = hide;
        }
        catch
        {
        }
        try { piece.ignoreAutoPick = true; } catch { }
    }
    internal static void ReturnOneIngredientToPc(Thing t)
    {
        if (t == null || t.isDestroyed || t.Num <= 0 || EClass.pc == null)
        {
            return;
        }
        ClearParkFlags(t);
        Card? root = null;
        try
        {
            root = t.GetRootCard();
        }
        catch
        {
            root = null;
        }
        if (root != null && root.IsPC)
        {
            return;
        }
        EClass.pc.Pick(t);
    }
    /// <summary>True when a process job occupies the worker (running or held away).</summary>
    internal static bool IsJobHeld()
    {
        return Active && NpcUid != 0;
    }

    /// <summary>
    /// PC is leaving the current map. Party workers abort; residents hold the job
    /// (ings stay on machine) until PC returns to WorkZoneUid.
    /// </summary>
    internal static void OnPcLeavingZone()
    {
        if (!Active || Suspended)
        {
            return;
        }

        if (WasPartyMember)
        {
            Clear("zone-change");
            return;
        }

        SuspendForZoneLeave();
    }

    /// <summary>
    /// Freestanding AI cancelled. If PC is leaving / job already held, do not wipe resident work.
    /// </summary>
    internal static void HandleAiInterrupted(string reason)
    {
        if (!Active)
        {
            return;
        }

        if (Suspended)
        {
            return;
        }

        if (WasPartyMember)
        {
            Clear(string.IsNullOrEmpty(reason) ? "ai-cancel" : reason);
            return;
        }

        if ((reason == "ai-cancel" || reason == "ai-fail")
            && ResumeIgnoreCancelUntilFrame > 0
            && Time.frameCount <= ResumeIgnoreCancelUntilFrame)
        {
            return;
        }

        bool leaving = false;
        try
        {
            if (EClass.pc != null && EClass.player != null)
            {
                leaving = EClass.player.nextZone != null
                    && (EClass._zone == null || EClass.player.nextZone.uid != EClass._zone.uid);
            }
        }
        catch { leaving = false; }

        bool offWork = false;
        try
        {
            if (WorkZoneUid != 0 && EClass._zone != null && EClass._zone.uid != WorkZoneUid)
            {
                offWork = true;
            }
        }
        catch { offWork = false; }

        if (leaving || offWork)
        {
            SuspendForZoneLeave();
            return;
        }

        Clear(string.IsNullOrEmpty(reason) ? "ai-cancel" : reason);
    }

    static void SuspendForZoneLeave()
    {
        if (!Active || Suspended)
        {
            return;
        }

        Suspended = true;
        LastClearReason = "zone-suspend";
        SuspendedAtRaw = CurrentRawDate();

        // Drop live Thing refs while the work map is unloading. Marks keep identity
        // so return catch-up rebinds the real parked stacks (avoids ghost Split/dupe).
        RefreshIngredientMarksFromLive();
        Ingredients.Clear();
        CrafterRef = null;
        MachineRef = null;

        // Drop freestanding AI; OnCancel will no-op while Suspended.
        try
        {
            Chara? worker = RefChara.Get(NpcUid);
            if (worker != null && !worker.isDead)
            {
                try
                {
                    // Stop freestanding process AI; Suspended makes OnCancel a no-op.
                    worker.SetAI(new NoGoal());
                }
                catch
                {
                    try { worker.SetNoGoal(); } catch { }
                }
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("processor suspend stop ai: " + ex.Message);
        }

        string name = NpcName ?? NpcLabor.LaborText.T("proc.msg.residentFallback");
        try
        {
            Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.zoneContinue", name, NpcLabor.LaborTerms.Process));
        }
        catch { }

        Plugin.LogInfo(
            $"processor suspend leave npc={NpcUid} left={Remaining} workZone={WorkZoneUid} raw={SuspendedAtRaw}");
    }

    /// <summary>PC entered a zone — if it is the work map, catch up then resume leftover work.</summary>
    internal static void OnZoneEntered(Zone? zone)
    {
        if (!Active || !Suspended || zone == null)
        {
            return;
        }

        try
        {
            if (WorkZoneUid != 0 && zone.uid != WorkZoneUid)
            {
                return;
            }
        }
        catch
        {
            return;
        }

        ApplyReturnCatchUpAndResume();
    }

    static void ApplyReturnCatchUpAndResume()
    {
        if (!Active || !Suspended)
        {
            return;
        }

        // Rebind machine on the now-loaded map.
        CrafterRef = null;
        MachineRef = null;
        TraitCrafter? crafter = GetCrafter();
        Card? machine = crafter?.owner;
        Chara? worker = GetWorker();

        if (worker == null || worker.isDead)
        {
            Clear("ai-fail");
            return;
        }

        if (crafter == null || machine == null || machine.isDestroyed)
        {
            Clear("machine-gone");
            return;
        }

        // Map reload replaces Card instances - rebind parked bulk before any Split/Craft.
        if (!TryRebindIngredients(machine))
        {
            Plugin.LogWarn(
                $"processor return rebind failed npc={NpcUid} marks={IngredientMarks.Count} - clear without free product");
            Clear("no-ings");
            return;
        }

        // GetRaw() is game minutes. Live duration is AI progress steps; treat each
        // step as one minute so leave/return settle without simulating ticks off-map.
        // Away rate is half on-map speed (user lock).
        int now = CurrentRawDate();
        int elapsedMins = SuspendedAtRaw > 0 && now > SuspendedAtRaw ? now - SuspendedAtRaw : 0;
        int minsPerCraft = Mathf.Max(1, EstimateDuration() * MinutesPerProgress * AwaySpeedDivisor);
        int canDo = minsPerCraft > 0 ? elapsedMins / minsPerCraft : 0;
        if (canDo > Remaining)
        {
            canDo = Remaining;
        }

        int did = 0;
        for (int i = 0; i < canDo; i++)
        {
            if (!Active || Remaining <= 0)
            {
                break;
            }

            if (!AI_NpcProcess.TryCatchUpOne(worker, crafter, machine))
            {
                break;
            }

            did++;
        }

        Plugin.LogInfo(
            $"processor return catch-up npc={NpcUid} elapsedMins={elapsedMins} minsPerCraft={minsPerCraft} (away x1/{AwaySpeedDivisor}) did={did} left={Remaining}");

        if (!Active)
        {
            return;
        }

        if (Remaining <= 0)
        {
            // Catch-up finished the whole job while PC was away.
            Clear("ai-end");
            return;
        }

        // Still work left — resume freestanding AI on this map.
        Suspended = false;
        SuspendedAtRaw = 0;
        try { ResumeIgnoreCancelUntilFrame = Time.frameCount + 3; } catch { ResumeIgnoreCancelUntilFrame = 0; }

        string name = NpcName ?? NpcLabor.LaborText.T("proc.msg.residentFallback");
        try
        {
            Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.zoneResume", name, Remaining));
        }
        catch { }

        try
        {
            worker.SetAIImmediate(new AI_NpcProcess
            {
                jobToken = NpcUid ^ MachineUid ^ Remaining
            });
        }
        catch (System.Exception ex)
        {
            Plugin.LogWarn("processor resume SetAI failed: " + ex.Message);
            Clear("ai-set-fail", announce: false);
        }
    }


    static void RefreshIngredientMarksFromLive()
    {
        if (Ingredients.Count == 0)
        {
            return;
        }

        if (IngredientMarks.Count != Ingredients.Count)
        {
            IngredientMarks.Clear();
            for (int i = 0; i < Ingredients.Count; i++)
            {
                IngredientMarks.Add(IngredientMark.From(Ingredients[i]));
            }
            return;
        }

        for (int i = 0; i < Ingredients.Count; i++)
        {
            Thing? t = Ingredients[i];
            if (t == null || t.isDestroyed)
            {
                continue;
            }

            IngredientMarks[i] = IngredientMark.From(t);
        }
    }

    /// <summary>
    /// Rebind Ingredients to live map Things after zone reload.
    /// Prefer uid; fallback to machine-cell stacks matching id/material.
    /// Returns false if any mark cannot be resolved to a live usable stack.
    /// </summary>
    internal static bool TryRebindIngredients(Card? machine)
    {
        Ingredients.Clear();

        if (IngredientMarks.Count == 0)
        {
            return false;
        }

        HashSet<int> used = new HashSet<int>();
        for (int i = 0; i < IngredientMarks.Count; i++)
        {
            IngredientMark mark = IngredientMarks[i];
            Thing? live = ResolveIngredientMark(mark, machine, used);
            if (live == null || live.isDestroyed || live.Num <= 0)
            {
                Ingredients.Clear();
                return false;
            }

            try { used.Add(live.uid); } catch { }
            ApplyParkFlags(live);
            Ingredients.Add(live);
            IngredientMarks[i] = IngredientMark.From(live);
        }

        return Ingredients.Count == IngredientMarks.Count && Ingredients.Count > 0;
    }

    static Thing? ResolveIngredientMark(IngredientMark mark, Card? machine, HashSet<int> used)
    {
        // 1) Exact uid on current map.
        if (mark.Uid != 0)
        {
            Thing? byUid = null;
            try
            {
                Card? c = FindCardByUid(mark.Uid);
                byUid = c as Thing;
            }
            catch
            {
                byUid = null;
            }

            if (byUid != null && !byUid.isDestroyed && byUid.Num > 0)
            {
                int uid = 0;
                try { uid = byUid.uid; } catch { }
                if (uid == 0 || !used.Contains(uid))
                {
                    return byUid;
                }
            }
        }

        // 2) Machine cell stacks matching id + material (parked bulk).
        if (machine != null && !machine.isDestroyed && machine.ExistsOnMap && machine.pos != null)
        {
            Thing? onCell = FindMatchingThingOnPoint(machine.pos, mark, used);
            if (onCell != null)
            {
                return onCell;
            }
        }

        // 3) Any map thing matching id + material, prefer ignoreAutoPick parked.
        try
        {
            if (EClass._map?.things != null)
            {
                Thing? best = null;
                foreach (Thing t in EClass._map.things)
                {
                    if (!MatchesMark(t, mark, used))
                    {
                        continue;
                    }

                    bool parked = false;
                    try { parked = t.ignoreAutoPick; } catch { parked = false; }
                    if (parked)
                    {
                        return t;
                    }

                    if (best == null)
                    {
                        best = t;
                    }
                }

                if (best != null)
                {
                    return best;
                }
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("resolve ingredient map scan: " + ex.Message);
        }

        return null;
    }

    static Thing? FindMatchingThingOnPoint(Point pos, IngredientMark mark, HashSet<int> used)
    {
        if (pos == null)
        {
            return null;
        }

        try
        {
            List<Thing>? list = pos.Things;
            if (list == null)
            {
                return null;
            }

            for (int i = 0; i < list.Count; i++)
            {
                Thing? t = list[i];
                if (MatchesMark(t, mark, used))
                {
                    return t;
                }
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("resolve ingredient cell: " + ex.Message);
        }

        return null;
    }

    static bool MatchesMark(Thing? t, IngredientMark mark, HashSet<int> used)
    {
        if (t == null || t.isDestroyed || t.Num <= 0)
        {
            return false;
        }

        try
        {
            if (used != null && used.Contains(t.uid))
            {
                return false;
            }
        }
        catch
        {
        }

        try
        {
            if (!string.IsNullOrEmpty(mark.Id) && t.id != mark.Id)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        if (mark.MaterialId != 0)
        {
            int mat = 0;
            try { mat = t.material != null ? t.material.id : t.idMaterial; } catch { mat = 0; }
            if (mat != 0 && mat != mark.MaterialId)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>After live Split/consume, keep marks in sync with current bulk uids.</summary>
    internal static void SyncIngredientMarksAfterCraft()
    {
        if (Ingredients.Count == 0)
        {
            return;
        }

        if (IngredientMarks.Count != Ingredients.Count)
        {
            IngredientMarks.Clear();
            for (int i = 0; i < Ingredients.Count; i++)
            {
                IngredientMarks.Add(IngredientMark.From(Ingredients[i]));
            }
            return;
        }

        for (int i = 0; i < Ingredients.Count; i++)
        {
            Thing? t = Ingredients[i];
            if (t == null || t.isDestroyed || t.Num <= 0)
            {
                continue;
            }

            IngredientMarks[i] = IngredientMark.From(t);
        }
    }

    static void AnnounceFinish(string reason)
    {
        string name = NpcName ?? NpcLabor.LaborText.T("proc.msg.residentFallback");

        if (reason == "reopen" || reason == "plugin-destroy" || reason == "ai-set-fail" || reason == "zone-suspend")
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
