using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NpcLabor.CoCraft;
using NpcLabor.Dispatch;
using NpcLabor.Process;
using UnityEngine;

namespace NpcLabor.TownLabor;

/// <summary>
/// Runtime + save manager for slice E town shop labor.
/// Save: npclabor_townlabor.json. Max 2 concurrent per town; 1 worker per client.
/// </summary>
internal static class TownLaborManager
{
    static readonly List<TownLaborMission> Missions = new List<TownLaborMission>();
    static int _nextMissionId = 1;
    static int _poseTick;


    internal static IReadOnlyList<TownLaborMission> All => Missions;
    internal static int Count => Missions.Count;

    /// <summary>
    /// True when leaving the current map should prompt about active town labor.
    /// Companion labor keeps ticking off-map (YesNo confirm).
    /// PC self-work opens a multi-choice leave menu instead of silent leave.
    /// </summary>
    internal static bool ShouldConfirmLeave(Zone? dest, out string? prompt)
    {
        prompt = null;
        if (Missions.Count == 0)
        {
            return false;
        }

        try
        {
            Zone? cur = EClass._zone;
            if (cur == null)
            {
                return false;
            }

            // Same zone or null dest: no leave.
            if (dest != null && dest.uid == cur.uid)
            {
                return false;
            }

            bool hasPcSelf = false;
            bool hasAny = false;
            bool leavingWorkZone = false;
            for (int i = 0; i < Missions.Count; i++)
            {
                TownLaborMission m = Missions[i];
                if (m == null)
                {
                    continue;
                }

                hasAny = true;
                if (m.isPcSelf)
                {
                    hasPcSelf = true;
                }

                if (m.uidZone == cur.uid)
                {
                    leavingWorkZone = true;
                }
            }

            if (!hasAny)
            {
                return false;
            }

            // Prompt when leaving the town where labor is running, or any leave during PC self-work.
            if (!leavingWorkZone && !hasPcSelf)
            {
                return false;
            }

            prompt = hasPcSelf
                ? NpcLabor.LaborText.T("town.leave.confirmSelf")
                : NpcLabor.LaborText.T("town.leave.confirm");
            return !string.IsNullOrEmpty(prompt);
        }
        catch
        {
            prompt = null;
            return false;
        }
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

    /// <summary>
    /// World advanced past last mod save => played without plugin; auto-recall on Load.
    /// Legacy lastSeen 0 resumes (no forced recall).
    /// </summary>
    static bool ShouldAutoRecallAfterModGap(int lastSeenWorldRaw)
    {
        int now = CurrentRawDate();
        if (lastSeenWorldRaw <= 0 || now <= 0)
        {
            return false;
        }

        return now > lastSeenWorldRaw;
    }



    internal static bool IsBusy(int uidChara)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].uidWorker == uidChara)
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsClientBusy(int uidClient)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].uidClient == uidClient)
            {
                return true;
            }
        }

        return false;
    }

    internal static int CountInZone(int uidZone)
    {
        int n = 0;
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].uidZone == uidZone)
            {
                n++;
            }
        }

        return n;
    }

    internal static TownLaborMission? FindByMissionId(int id)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].missionId == id)
            {
                return Missions[i];
            }
        }

        return null;
    }

    internal static TownLaborMission? FindByWorker(int uid)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].uidWorker == uid)
            {
                return Missions[i];
            }
        }

        return null;
    }

    internal static TownLaborMission? FindByClient(int uid)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].uidClient == uid)
            {
                return Missions[i];
            }
        }

        return null;
    }
    /// <summary>
    /// Resolve a chara by uid for town labor.
    /// Local merchants are often map-only; RefChara.Get only hits globalCharas.
    /// Order: person temp cache → current map → global → branch members.
    /// </summary>
    internal static Chara? ResolveChara(int uid, Person? person = null)
    {
        if (uid <= 0)
        {
            return null;
        }

        // PC self-work missions key uidWorker to EClass.pc.
        try
        {
            if (EClass.pc != null && EClass.pc.uid == uid && !EClass.pc.isDead)
            {
                return EClass.pc;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            if (person != null)
            {
                Chara? fromPerson = person.chara;
                if (fromPerson != null && fromPerson.uid == uid && !fromPerson.isDead)
                {
                    return fromPerson;
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            if (EClass._map?.charas != null)
            {
                foreach (Chara c in EClass._map.charas)
                {
                    if (c != null && c.uid == uid && !c.isDead)
                    {
                        return c;
                    }
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            Chara? global = RefChara.Get(uid);
            if (global != null && !global.isDead)
            {
                return global;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch;
            if (branch?.members != null)
            {
                foreach (Chara m in branch.members)
                {
                    if (m != null && m.uid == uid && !m.isDead)
                    {
                        return m;
                    }
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            if (EClass.pc?.party?.members != null)
            {
                foreach (Chara m in EClass.pc.party.members)
                {
                    if (m != null && m.uid == uid && !m.isDead)
                    {
                        return m;
                    }
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
return null;
    }


    /// <summary>Whether the current zone can host town labor rows.</summary>
    internal static bool CanHostTownLabor(Zone? zone)
    {
        if (zone == null)
        {
            return false;
        }

        try
        {
            // User lock: base/PC-faction has no town labor rows.
            if (zone.IsPCFaction)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        try
        {
            // Prefer towns; still allow non-base settlements that host merchants.
            if (zone.IsTown)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
// Fallback: any non-PCFaction zone that currently has a matching merchant.
        return true;
    }

    internal static List<TownLaborOffer> ListOffers(Zone? zone = null)
    {
        var offers = new List<TownLaborOffer>();
        zone ??= EClass._zone;
        if (!CanHostTownLabor(zone))
        {
            return offers;
        }

        if (EClass._map?.charas == null)
        {
            return offers;
        }

        var seen = new HashSet<int>();
        foreach (Chara c in EClass._map.charas)
        {
            if (c == null || c.isDead || PersonPickerUi.IsPcLike(c))
            {
                continue;
            }

            if (!seen.Add(c.uid))
            {
                continue;
            }

            // Skip own faction residents/party as clients.
            try
            {
                if (c.IsPCFaction || c.IsPCParty)
                {
                    continue;
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
TownLaborJobDef? def = TownLaborJobs.MatchJob(c);
            if (def == null)
            {
                continue;
            }

            bool active = IsClientBusy(c.uid);
            offers.Add(new TownLaborOffer
            {
                Client = c,
                Def = def,
                Hours = TownLaborJobs.RollWorkHours(),
                IsActive = active,
            });
        }

        // Always keep already-active labor rows visible.
        var activeOffers = new List<TownLaborOffer>();
        var free = new List<TownLaborOffer>();
        for (int i = 0; i < offers.Count; i++)
        {
            if (offers[i].IsActive)
            {
                activeOffers.Add(offers[i]);
            }
            else
            {
                free.Add(offers[i]);
            }
        }

        // Board only needs a few free posts, not every shopkeeper.
        int take = SampleBoardCount(zone, free.Count);
        if (free.Count > take)
        {
            free = SampleOffers(free, take, zone);
        }

        free.Sort(CompareOffers);
        activeOffers.Sort(CompareOffers);

        var result = new List<TownLaborOffer>(activeOffers.Count + free.Count);
        result.AddRange(activeOffers);
        result.AddRange(free);
        return result;
    }

    static int CompareOffers(TownLaborOffer a, TownLaborOffer b)
    {
        int ka = (int)a.Def.Kind;
        int kb = (int)b.Def.Kind;
        int cmp = ka.CompareTo(kb);
        if (cmp != 0)
        {
            return cmp;
        }

        return a.Client.uid.CompareTo(b.Client.uid);
    }

    static int SampleBoardCount(Zone? zone, int freeCount)
    {
        if (freeCount <= 0)
        {
            return 0;
        }

        if (freeCount <= TownLaborJobs.MinBoardOffers)
        {
            return freeCount;
        }

        // Stable 2..3 per zone+week so reopening board does not thrash.
        int seed = BoardSeed(zone);
        int span = TownLaborJobs.MaxBoardOffers - TownLaborJobs.MinBoardOffers + 1;
        int n = TownLaborJobs.MinBoardOffers + (seed % span);
        if (n > freeCount)
        {
            n = freeCount;
        }

        if (n > TownLaborJobs.MaxBoardOffers)
        {
            n = TownLaborJobs.MaxBoardOffers;
        }

        return n;
    }

    static int BoardSeed(Zone? zone)
    {
        int z = 0;
        try { z = zone?.uid ?? 0; } catch { z = 0; }

        // One board sample per calendar week (30-day months -> week buckets of 7).
        int week = 0;
        try
        {
            int day = EClass.world?.date?.day ?? 1;
            int month = 1;
            int year = 0;
            try { month = EClass.world?.date?.month ?? 1; } catch { month = 1; }
            try { year = EClass.world?.date?.year ?? 0; } catch { year = 0; }
            // Absolute day index, then /7 so offers refresh weekly rather than daily.
            int dayIndex = year * 360 + Math.Max(0, month - 1) * 30 + Math.Max(1, day);
            week = dayIndex / 7;
        }
        catch
        {
            week = 0;
        }

        unchecked
        {
            return z * 397 ^ week * 31 ^ 0x51a7;
        }
    }

    static List<TownLaborOffer> SampleOffers(List<TownLaborOffer> source, int take, Zone? zone)
    {
        if (source == null || source.Count == 0 || take <= 0)
        {
            return new List<TownLaborOffer>();
        }

        if (take >= source.Count)
        {
            return new List<TownLaborOffer>(source);
        }

        var copy = new List<TownLaborOffer>(source);
        int state = BoardSeed(zone);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            state = unchecked(state * 1103515245 + 12345);
            int j = (state & 0x7fffffff) % (i + 1);
            TownLaborOffer tmp = copy[i];
            copy[i] = copy[j];
            copy[j] = tmp;
        }

        return copy.GetRange(0, take);
    }

    internal static List<Chara> ListCandidates(TownLaborJobDef? def = null)
    {
        var list = new List<Chara>();
        var seen = new HashSet<int>();

        void Consider(Chara? c)
        {
            if (c == null || !seen.Add(c.uid))
            {
                return;
            }

            if (!IsCandidate(c, def))
            {
                return;
            }

            list.Add(c);
        }

        try
        {
            // Prefer currently-loaded eligible pool (party + residents).
            foreach (Chara c in AssistantResolver.EnumerateEligible())
            {
                Consider(c);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
// Also accept home-branch members not currently in zone (dispatch-style).
        try
        {
            FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch;
            if (branch?.members != null)
            {
                foreach (Chara m in branch.members)
                {
                    Consider(m);
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
list.Sort((a, b) =>
        {
            int pa = IsParty(a) ? 0 : 1;
            int pb = IsParty(b) ? 0 : 1;
            int cmp = pa.CompareTo(pb);
            if (cmp != 0)
            {
                return cmp;
            }

            int sa = def != null && def.SkillId > 0 ? SafeSkill(a, def.SkillId) : 0;
            int sb = def != null && def.SkillId > 0 ? SafeSkill(b, def.SkillId) : 0;
            cmp = sb.CompareTo(sa);
            return cmp != 0 ? cmp : a.uid.CompareTo(b.uid);
        });
        return list;
    }

    internal static bool IsCandidate(Chara? c, TownLaborJobDef? def = null)
    {
        if (c == null || PersonPickerUi.IsPcLike(c) || c.isDead)
        {
            return false;
        }

        if (PersonPickerUi.IsStayHomeUnique(c))
        {
            return false;
        }

        if (LaborBusy.IsBusy(c.uid))
        {
            return false;
        }

        bool ok = false;
        try
        {
            if (IsParty(c))
            {
                ok = true;
            }
            else
            {
                FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch;
                if (branch?.members != null && branch.members.Contains(c))
                {
                    ok = true;
                }
                else if (c.IsPCFaction && c.homeBranch != null)
                {
                    ok = true;
                }
            }
        }
        catch
        {
            ok = false;
        }

        return ok;
    }

    static bool IsParty(Chara c)
    {
        try
        {
            return c != null && (c.IsPCParty || (EClass.pc?.party?.members?.Contains(c) ?? false));
        }
        catch
        {
            return false;
        }
    }

    static int SafeSkill(Chara c, int skillId)
    {
        try
        {
            return skillId > 0 ? c.Evalue(skillId) : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// PC self-work: walk to client (no teleport), then same hours/rewards as companion labor.
    /// SP drains each hour only after arrival. No party hop.
    /// </summary>
    internal static string? TryStartSelf(Chara client, TownLaborJobDef def, int offeredHours = 0)
    {
        Chara? pc = null;
        try { pc = EClass.pc; } catch { pc = null; }
        if (pc == null || client == null || def == null)
        {
            return NpcLabor.LaborText.T("town.error.invalidJob");
        }

        Zone? zone = EClass._zone;
        if (!CanHostTownLabor(zone))
        {
            return NpcLabor.LaborText.T("town.error.noTownWork", NpcLabor.LaborTerms.TownWork);
        }

        if (zone == null)
        {
            return NpcLabor.LaborText.T("town.error.invalidPlace");
        }

        if (LaborBusy.IsBusy(pc.uid))
        {
            return LaborBusy.BusyReason(pc) ?? NpcLabor.LaborText.T("town.error.busy");
        }

        // Co-craft / process / dispatch already covered by LaborBusy; double-check process/co-craft sessions.
        try
        {
            if (CoCraftSession.Active)
            {
                return NpcLabor.LaborText.T("town.error.busyCoCraft");
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            if (ProcessorJobSession.IsJobHeld())
            {
                return NpcLabor.LaborText.T("town.error.busyProcess", NpcLabor.LaborTerms.Process);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
        if (IsClientBusy(client.uid))
        {
            return NpcLabor.LaborText.T("town.error.clientBusy", NpcLabor.LaborTerms.TownWork);
        }

        if (CountInZone(zone.uid) >= TownLaborJobs.MaxConcurrentPerTown)
        {
            return NpcLabor.LaborText.T("town.error.maxConcurrent", TownLaborJobs.MaxConcurrentPerTown, NpcLabor.LaborTerms.TownWork);
        }

        if (TownLaborJobs.MatchJob(client) == null)
        {
            return NpcLabor.LaborText.T("town.error.jobGone");
        }

        // Need some stamina to start.
        int spNow = 0;
        int spMax = 1;
        try
        {
            spNow = pc.stamina != null ? pc.stamina.value : 0;
            spMax = pc.stamina != null ? Math.Max(1, pc.stamina.max) : 1;
        }
        catch
        {
            spNow = 0;
            spMax = 1;
        }

        if (spNow <= 0)
        {
            return NpcLabor.LaborText.T("town.error.noSp");
        }

        int hours = TownLaborJobs.ResolveWorkHours(offeredHours);
        int shopLv = TownLaborJobs.GetShopLv(client);
        int workerSkill = TownLaborJobs.GetWorkerSkill(pc, def.SkillId);
        int clientInvest = 0;
        try { clientInvest = Math.Max(0, client.c_invest); } catch { clientInvest = 0; }
        Zone? home = null;
        try
        {
            home = pc.homeZone
                ?? EClass.BranchOrHomeBranch?.owner
                ?? EClass.Branch?.owner;
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
string pcName;
        try { pcName = pc.NameSimple ?? pc.Name ?? NpcLabor.LaborText.T("town.you"); }
        catch { pcName = NpcLabor.LaborText.T("town.you"); }

        var mission = new TownLaborMission
        {
            missionId = _nextMissionId++,
            jobId = def.Id,
            Kind = def.Kind,
            uidWorker = pc.uid,
            uidClient = client.uid,
            uidZone = zone.uid,
            zoneId = zone.id ?? "",
            zoneName = SafeZoneName(zone),
            clientName = client.NameSimple ?? client.Name ?? ("#" + client.uid),
            workerName = pcName,
            jobTitle = TownLaborJobs.ResolveBoardTitle(client, def),
            skillId = def.SkillId,
            hoursTotal = hours,
            hoursLeft = hours,
            wasPartyMember = false,
            isPcSelf = true,
            workStarted = false,
            homeZoneUid = home?.uid ?? 0,
            shopLv = shopLv,
            workerSkill = workerSkill,
            clientInvest = clientInvest,
            rewardLog = new List<string>(),
        };

        TownLaborRewards.LockRewardPreview(mission);
        Missions.Add(mission);
        TryStartTrackerQuest(mission);

        // Walk to client first; hours/SP start only after arrival.
        try
        {
            StartPcSelfApproach(mission, pc);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor self approach AI: " + ex.Message);
        }

        // If already adjacent, StartPcSelfApproach marks workStarted and says "开始".
        if (!mission.workStarted)
        {
            int startHours = Math.Max(1, hours);
            string msg = NpcLabor.LaborText.T("town.msg.youStart", mission.clientName, mission.jobTitle, startHours);
            try { Msg.Say(msg); } catch { }
        }

        Plugin.LogInfo("townlabor start SELF id=" + mission.missionId + " job=" + def.Id
            + " client=" + mission.uidClient + " hours=" + hours
            + " workStarted=" + mission.workStarted);

        try { Save(); } catch { }
        return null;
    }

    /// <summary>Hourly SP drain for PC self-work. Fixed light cost; never lethal.</summary>
    internal static int SelfWorkSpCost(int spMax, bool firstHour = false)
    {
        // Base 2, later hours 3; soft-cap vs max so low-stamina chars still finish.
        int cost = firstHour ? 2 : 3;
        int cap = Math.Max(1, spMax / 8);
        if (cost > cap)
        {
            cost = cap;
        }

        return Math.Max(1, cost);
    }

    internal static string? TryStart(Chara worker, Chara client, TownLaborJobDef def, int offeredHours = 0)
    {
        if (worker == null || client == null || def == null)
        {
            return NpcLabor.LaborText.T("town.error.invalidJob");
        }

        Zone? zone = EClass._zone;
        if (!CanHostTownLabor(zone))
        {
            return NpcLabor.LaborText.T("town.error.noTownWork", NpcLabor.LaborTerms.TownWork);
        }

        if (zone == null)
        {
            return NpcLabor.LaborText.T("town.error.invalidPlace");
        }

        if (LaborBusy.IsBusy(worker))
        {
            return LaborBusy.BusyReason(worker) ?? NpcLabor.LaborText.T("town.error.workerBusy");
        }

        if (IsClientBusy(client.uid))
        {
            return NpcLabor.LaborText.T("town.error.clientBusy", NpcLabor.LaborTerms.TownWork);
        }

        if (CountInZone(zone.uid) >= TownLaborJobs.MaxConcurrentPerTown)
        {
            return NpcLabor.LaborText.T("town.error.maxConcurrent", TownLaborJobs.MaxConcurrentPerTown, NpcLabor.LaborTerms.TownWork);
        }

        if (!IsCandidate(worker, def))
        {
            return NpcLabor.LaborText.T("town.error.cannotAssign", worker.NameSimple ?? worker.Name ?? ("#" + worker.uid));
        }

        if (TownLaborJobs.MatchJob(client)?.Kind != def.Kind)
        {
            // Soft: still allow if client still has a merchant trait, but prefer exact.
            if (TownLaborJobs.MatchJob(client) == null)
            {
                return NpcLabor.LaborText.T("town.error.jobGone");
            }
        }

        bool wasParty = IsParty(worker);
        if (wasParty)
        {
            try
            {
                // Keep global so later AddMemeber can succeed.
                try
                {
                    if (!worker.IsGlobal)
                    {
                        worker.SetGlobal();
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
EClass.pc?.party?.RemoveMember(worker);
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("townlabor remove party failed: " + ex.Message);
            }

            // Vanilla branch hour-tick can auto-rejoin if this stays true;
            // also used as a restore fallback if mission flag is lost.
            try { worker.c_wasInPcParty = true; } catch { }
        }

        int hours = TownLaborJobs.ResolveWorkHours(offeredHours);
        int shopLv = TownLaborJobs.GetShopLv(client);
        int workerSkill = TownLaborJobs.GetWorkerSkill(worker, def.SkillId);
        int clientInvest = 0;
        try { clientInvest = Math.Max(0, client.c_invest); } catch { clientInvest = 0; }
        Zone? home = null;
        try
        {
            home = worker.homeZone
                ?? EClass.BranchOrHomeBranch?.owner
                ?? EClass.Branch?.owner
                ?? EClass.pc?.homeZone;
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
var mission = new TownLaborMission
        {
            missionId = _nextMissionId++,
            jobId = def.Id,
            Kind = def.Kind,
            uidWorker = worker.uid,
            uidClient = client.uid,
            uidZone = zone.uid,
            zoneId = zone.id ?? "",
            zoneName = SafeZoneName(zone),
            clientName = client.NameSimple ?? client.Name ?? ("#" + client.uid),
            workerName = worker.NameSimple ?? worker.Name ?? ("#" + worker.uid),
            jobTitle = TownLaborJobs.ResolveBoardTitle(client, def),
            skillId = def.SkillId,
            hoursTotal = hours,
            hoursLeft = hours,
            wasPartyMember = wasParty,
            isPcSelf = false,
            workStarted = true,
            homeZoneUid = home?.uid ?? 0,
            shopLv = shopLv,
            workerSkill = workerSkill,
            clientInvest = clientInvest,
            rewardLog = new List<string>(),
        };

        // Place worker near client and start cosmetic work AI.
        try
        {
            PlaceWorkerNearClient(worker, client);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor place worker: " + ex.Message);
        }

        try
        {
            PrepareWorkingChara(worker);
            worker.SetAI(new AI_TownLabor { missionId = mission.missionId });
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor set AI: " + ex.Message);
            try { worker.SetAIIdle(); } catch { }
        }

        TownLaborRewards.LockRewardPreview(mission);
        Missions.Add(mission);
        TryStartTrackerQuest(mission);

        int startHours = Math.Max(1, mission.hoursTotal > 0 ? mission.hoursTotal : (int)Math.Ceiling(Math.Max(0f, mission.hoursLeft)));
        string msg = NpcLabor.LaborText.T(
            "town.msg.workerStart",
            mission.workerName,
            mission.clientName,
            mission.jobTitle,
            startHours);
        try { Msg.Say(msg); } catch { }
        Plugin.LogInfo("townlabor start id=" + mission.missionId + " job=" + def.Id
            + " worker=" + mission.uidWorker + " client=" + mission.uidClient
            + " hours=" + hours);

        return null;
    }

    internal static bool TryRecall(int missionIdOrWorkerUid)
    {
        TownLaborMission? m = FindByMissionId(missionIdOrWorkerUid) ?? FindByWorker(missionIdOrWorkerUid);
        if (m == null)
        {
            return false;
        }

        Settle(m, TownLaborSettleKind.Recall);
        return true;
    }

    internal static bool HasActivePcSelfLabor()
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            TownLaborMission m = Missions[i];
            if (m != null && m.isPcSelf)
            {
                return true;
            }
        }

        return false;
    }

    internal static TownLaborMission? FindActivePcSelf()
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            TownLaborMission m = Missions[i];
            if (m != null && m.isPcSelf)
            {
                return m;
            }
        }

        return null;
    }

    /// <summary>
    /// Abort all PC self-work missions as Failed (fame hit via Quest.Fail).
    /// </summary>
    internal static bool AbortAllPcSelfLabor()
    {
        var list = new List<TownLaborMission>();
        for (int i = 0; i < Missions.Count; i++)
        {
            TownLaborMission m = Missions[i];
            if (m != null && m.isPcSelf)
            {
                list.Add(m);
            }
        }

        if (list.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < list.Count; i++)
        {
            try { Settle(list[i], TownLaborSettleKind.Failed); }
            catch (Exception ex) { Plugin.LogWarn("townlabor abort self: " + ex.Message); }
        }

        return true;
    }

    /// <summary>
    /// Hand PC self-work to a companion mid-job. Keeps remaining hours; switches to NPC labor.
    /// Locked reward pins stay (accept contract); workerSkill updates for invest-raise gate.
    /// </summary>
    internal static string? TryHandOffPcSelfTo(int missionId, Chara worker)
    {
        TownLaborMission? m = FindByMissionId(missionId);
        if (m == null || !m.isPcSelf)
        {
            return LaborText.T("town.msg.endedShort", LaborTerms.TownWork);
        }

        if (worker == null || worker.isDead)
        {
            return LaborText.T("town.msg.invalidChoice");
        }

        try
        {
            if (worker.IsPC)
            {
                return LaborText.T("town.msg.invalidChoice");
            }
        }
        catch { }

        if (LaborBusy.IsBusy(worker.uid))
        {
            return LaborBusy.BusyReason(worker) ?? LaborText.T("town.error.busy");
        }

        TownLaborJobDef? def = m.Def ?? TownLaborJobs.GetById(m.jobId);
        if (def == null)
        {
            return LaborText.T("town.error.invalidJob");
        }

        if (!IsCandidate(worker, def))
        {
            return LaborText.T("town.msg.invalidChoice");
        }

        Chara? client = m.GetClient() ?? ResolveChara(m.uidClient);
        if (client == null || client.isDead)
        {
            return LaborText.T("town.msg.clientGone");
        }

        // Clear PC self AI first.
        try { ClearPcSelfApproachAi(); } catch { }

        bool wasParty = false;
        try
        {
            wasParty = worker.IsPCParty
                || (EClass.pc?.party?.members?.Contains(worker) ?? false);
        }
        catch { wasParty = false; }

        if (wasParty)
        {
            try
            {
                try
                {
                    if (!worker.IsGlobal)
                    {
                        worker.SetGlobal();
                    }
                }
                catch { }
                EClass.pc?.party?.RemoveMember(worker);
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("townlabor handoff remove party: " + ex.Message);
            }

            try { worker.c_wasInPcParty = true; } catch { }
        }

        // Convert mission identity to companion labor.
        m.isPcSelf = false;
        m.workStarted = true;
        m.uidWorker = worker.uid;
        try { m.workerName = worker.NameSimple ?? worker.Name ?? ("#" + worker.uid); }
        catch { m.workerName = "#" + worker.uid; }
        m.wasPartyMember = wasParty;
        try { m.workerSkill = TownLaborJobs.GetWorkerSkill(worker, m.skillId); } catch { }
        // Keep rewardMoney/plat/ticket locked at accept so board contract stays honest.

        try
        {
            PlaceWorkerNearClient(worker, client, force: true);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor handoff place: " + ex.Message);
        }

        try
        {
            PrepareWorkingChara(worker);
            worker.SetAI(new AI_TownLabor { missionId = m.missionId });
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor handoff AI: " + ex.Message);
            try { worker.SetAIIdle(); } catch { }
        }

        try { RefreshTrackerQuests(); } catch { }
        try { Save(); } catch { }
        Plugin.LogInfo("townlabor handoff self->npc id=" + m.missionId + " worker=" + m.uidWorker
            + " hoursLeft=" + m.hoursLeft);
        return null;
    }

    internal static void OnSimulateHour()
    {
        if (Missions.Count == 0)
        {
            return;
        }

        // Do not RefreshTrackerQuests every hour (QuestManager.Start / journal spam / dups).
        // Tracker text already reads live mission progress.
        var snapshot = Missions.ToList();
        foreach (TownLaborMission m in snapshot)
        {
            try
            {
                if (!Missions.Contains(m))
                {
                    continue;
                }

                Chara? worker = m.GetWorker();
                bool pcSelf = false;
                try { pcSelf = m.isPcSelf; } catch { pcSelf = false; }

                if (!pcSelf && worker != null && worker.isDead)
                {
                    Plugin.LogInfo("townlabor drop dead worker id=" + m.missionId);
                    Settle(m, TownLaborSettleKind.Failed);
                    continue;
                }

                // Worker may be temporarily unresolved off-map; keep ticking hours.
                // Only fail when we positively see a dead client. Map-local merchants
                // unload when PC leaves town — that must NOT cancel labor.
                Chara? client = m.GetClient();
                if (client != null && client.isDead)
                {
                    Plugin.LogInfo("townlabor drop dead client id=" + m.missionId);
                    Settle(m, TownLaborSettleKind.Failed);
                    continue;
                }

                // Soft presence only for NPC workers while PC is on the work map.
                if (!pcSelf && worker != null && IsWorkZoneActive(m))
                {
                    try
                    {
                        MaintainPresence(m, worker, client);
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

                // PC self-work: wait until arrived at client before hour/SP ticks.
                if (pcSelf && !m.workStarted)
                {
                    try
                    {
                        MaybeResumePcSelfApproach(m);
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogDebug("townlabor self approach resume: " + ex.Message);
                    }

                    continue;
                }

                // PC self-work: drain SP each hour after arrival. Exhaustion = Quest.Fail path.
                if (pcSelf)
                {
                    try
                    {
                        Chara? pc = EClass.pc;
                        if (pc == null || pc.isDead)
                        {
                            Settle(m, TownLaborSettleKind.Failed);
                            continue;
                        }

                        int spMax = 1;
                        try { spMax = pc.stamina != null ? Math.Max(1, pc.stamina.max) : 1; } catch { spMax = 1; }
                        int cost = SelfWorkSpCost(spMax, firstHour: false);
                        try
                        {
                            if (pc.stamina != null && cost > 0)
                            {
                                pc.stamina.Mod(-cost);
                            }
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
int spLeft = 1;
                        try { spLeft = pc.stamina != null ? pc.stamina.value : 0; } catch { spLeft = 0; }
                        if (spLeft <= 0)
                        {
                            try
                            {
                                Msg.Say(NpcLabor.LaborText.T(
                                    "town.msg.youTired",
                                    string.IsNullOrEmpty(m.jobTitle) ? NpcLabor.LaborTerms.TownWork : m.jobTitle));
                            }
                            catch { }
                            Settle(m, TownLaborSettleKind.Failed);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogWarn("townlabor self sp tick: " + ex.Message);
                    }
                }

                // Hourly life-skill exp: PC self gets half the settle rate each hour.
                // Companion workers keep full exp on settle only (no hourly drip).
                if (pcSelf && m.skillId > 0)
                {
                    try
                    {
                        Chara? expTarget = EClass.pc;
                        if (expTarget != null && !expTarget.isDead)
                        {
                            int hourly = Math.Max(0, TownLaborRewards.ExpPerHour / 2);
                            if (hourly > 0)
                            {
                                expTarget.ModExp(m.skillId, hourly);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogDebug("townlabor self exp tick: " + ex.Message);
                    }
                }

                m.hoursLeft = Math.Max(0, m.hoursLeft - 1);
                if (m.hoursLeft <= 0)
                {
                    Settle(m, TownLaborSettleKind.Success);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("townlabor hour tick id=" + m.missionId + ": " + ex.Message);
            }
        }

        // Cosmetic pose refresh occasionally while PC is in a labor zone.
        _poseTick++;
        if (_poseTick % 2 == 0)
        {
            try
            {
                RefreshWorkingPoses();
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

        // Hour ticks can burst (sleep/wait). Mark dirty and flush once after AdvanceHour.
        if (Missions.Count > 0)
        {
            _hourSavePending = true;
        }
    }

    internal static void OnWorkerDied(Chara c)
    {
        if (c == null)
        {
            return;
        }

        TownLaborMission? m = FindByWorker(c.uid);
        if (m != null)
        {
            Settle(m, TownLaborSettleKind.Failed);
        }
    }

    internal static void OnZoneEntered(Zone zone)
    {
        if (zone == null || Missions.Count == 0)
        {
            return;
        }

        foreach (TownLaborMission m in Missions)
        {
            if (m.uidZone != zone.uid)
            {
                continue;
            }

            try
            {
                if (m.isPcSelf)
                {
                    if (!m.workStarted)
                    {
                        MaybeResumePcSelfApproach(m);
                    }

                    continue;
                }

                Chara? worker = m.GetWorker();
                if (worker == null || worker.isDead)
                {
                    continue;
                }

                Chara? client = m.GetClient();
                MaintainPresence(m, worker, client);
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}
    }

    static bool IsWorkZoneActive(TownLaborMission m)
    {
        try
        {
            return m != null && EClass._zone != null && EClass._zone.uid == m.uidZone;
        }
        catch
        {
            return false;
        }
    }

    static void MaintainPresence(TownLaborMission m, Chara worker, Chara? client)
    {
        if (m == null || worker == null || worker.isDead)
        {
            return;
        }

        Zone? workZone = m.GetZone();
        if (workZone == null)
        {
            try { workZone = EClass._zone; } catch { workZone = null; }
        }

        if (workZone == null)
        {
            return;
        }

        // Only act when the work map is the active map. Off-map labor just ticks hours.
        try
        {
            if (EClass._zone == null || EClass._zone.uid != workZone.uid)
            {
                return;
            }
        }
        catch
        {
            return;
        }

        // Bring missing worker onto the loaded work map once.
        // Repeated MoveZone while already present can create invisible clones.
        bool onWorkMap = false;
        try
        {
            onWorkMap = worker.ExistsOnMap
                && worker.currentZone != null
                && worker.currentZone.uid == workZone.uid;
        }
        catch
        {
            onWorkMap = false;
        }

        if (!onWorkMap)
        {
            try
            {
                if (worker.currentZone == null || worker.currentZone.uid != workZone.uid)
                {
                    worker.MoveZone(workZone, ZoneTransition.EnterState.RandomVisit);
                }
            }
            catch
            {
                try { worker.MoveZone(workZone); } catch { }
            }
        }

        // Soft placement only when very far. No tight leash.
        try
        {
            if (client != null
                && client.ExistsOnMap
                && worker.ExistsOnMap
                && worker.Dist(client) > 12)
            {
                PlaceWorkerNearClient(worker, client, force: false);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            if (!(worker.ai is AI_TownLabor labor) || labor.missionId != m.missionId)
            {
                PrepareWorkingChara(worker);
                worker.SetAI(new AI_TownLabor { missionId = m.missionId });
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

    static void RefreshWorkingPoses()
    {
        if (EClass._zone == null)
        {
            return;
        }

        foreach (TownLaborMission m in Missions)
        {
            if (m.uidZone != EClass._zone.uid)
            {
                continue;
            }

            if (m.isPcSelf)
            {
                continue;
            }

            Chara? worker = m.GetWorker();
            if (worker == null || !worker.ExistsOnMap)
            {
                continue;
            }

            TownLaborJobDef? def = m.Def;
            try
            {
                worker.ShowEmo(def?.WorkEmo ?? Emo.happy);
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}
    }

    internal static void PlaceWorkerNearClient(Chara worker, Chara client, bool force = true)
    {
        if (worker == null || client == null)
        {
            return;
        }

        try
        {
            if (worker.ExistsOnMap && client.ExistsOnMap)
            {
                int dist = worker.Dist(client);
                if (!force && dist <= 12)
                {
                    return;
                }

                if (force && dist <= 4)
                {
                    try { worker.LookAt(client); } catch { }
                    return;
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
Point? dest = null;
        try
        {
            if (client.ExistsOnMap && client.pos != null)
            {
                dest = FindNearPoint(client.pos, worker);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
if (dest == null)
        {
            return;
        }

        try
        {
            if (EClass.pc != null && EClass.pc.pos != null && dest.Equals(EClass.pc.pos))
            {
                return;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            if (client.pos != null && dest.Equals(client.pos))
            {
                return;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
bool teleported = false;
        if (force)
        {
            try
            {
                worker.Teleport(dest, silent: true, force: true);
                teleported = true;
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

        if (!teleported)
        {
            try
            {
                worker.MoveImmediate(dest, focus: false, cancelAI: false);
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("townlabor place: " + ex.Message);
            }
        }

        try
        {
            worker.LookAt(client);
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

    static Point? FindNearPoint(Point origin, Chara? worker = null)
    {
        if (origin == null)
        {
            return null;
        }

        int[] dx = { 2, -2, 0, 0, 2, 2, -2, -2, 1, -1, 0, 0, 1, 1, -1, -1, 3, -3, 0, 0 };
        int[] dy = { 0, 0, 2, -2, 2, -2, 2, -2, 0, 0, 1, -1, 1, -1, 1, -1, 0, 0, 3, -3 };
        Point? pcPos = null;
        try { pcPos = EClass.pc?.pos; } catch { pcPos = null; }

        for (int i = 0; i < dx.Length; i++)
        {
            try
            {
                var p = new Point(origin.x + dx[i], origin.z + dy[i]);
                if (!p.IsValid || !p.IsInBounds)
                {
                    continue;
                }

                if (p.IsBlocked)
                {
                    continue;
                }

                if (pcPos != null && p.Equals(pcPos))
                {
                    continue;
                }

                if (p.Equals(origin))
                {
                    continue;
                }

                if (p.HasChara)
                {
                    bool self = false;
                    try
                    {
                        if (worker != null && worker.pos != null && p.Equals(worker.pos))
                        {
                            self = true;
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
if (!self)
                    {
                        continue;
                    }
                }

                return p;
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

        return null;
    }

    internal static void PrepareWorkingChara(Chara c)
    {
        if (c == null)
        {
            return;
        }

        try { c.SetHostility(Hostility.Friend); } catch { }
        try { c.enemy = null; } catch { }
        // Do NOT set isRestrained — town labor should look like free work, not shackles.
        // noMove false so DoGoto can step around the shop.
        try { c.noMove = false; } catch { }
    }

    internal static void ClearWorkingFlags(Chara? c)
    {
        if (c == null)
        {
            return;
        }

        try { c.noMove = false; } catch { }
        try
        {
            if (c.ai is AI_TownLabor)
            {
                c.SetAIIdle();
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}

    static void Settle(TownLaborMission m, TownLaborSettleKind kind)
    {
        if (m == null)
        {
            return;
        }

        if (!Missions.Contains(m))
        {
            return;
        }

        Chara? worker = m.GetWorker() ?? ResolveChara(m.uidWorker);
        Chara? client = m.GetClient() ?? ResolveChara(m.uidClient);

        // Last-chance global lookup for party restore.
        if (worker == null && m.uidWorker > 0)
        {
            try { worker = RefChara.Get(m.uidWorker); } catch { }
        }

        // Deliver first while mission still known.
        try
        {
            TownLaborRewards.Deliver(m, kind, worker, client);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor deliver: " + ex.Message);
        }

        Missions.Remove(m);

        bool pcSelf = false;
        try { pcSelf = m.isPcSelf; } catch { pcSelf = false; }

        if (pcSelf)
        {
            try { ClearPcSelfApproachAi(); } catch { }
        }
        else if (worker != null && !worker.isDead)
        {
            try
            {
                ClearWorkingFlags(worker);
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
RestoreWorkerAfterLabor(m, worker);
        }
        else if (m.wasPartyMember)
        {
            Plugin.LogWarn("townlabor settle: worker unresolved, cannot rejoin party uid=" + m.uidWorker);
        }

        // Hard fail uses vanilla Quest.Fail (sound + fame formula + karma).
        // Success/recall keep quiet tracker remove (custom rewards already delivered).
        if (kind == TownLaborSettleKind.Failed)
        {
            FailTrackerQuest(m);
        }
        else
        {
            RemoveTrackerQuest(m);
        }

        string outcome = kind switch
        {
            TownLaborSettleKind.Success => LaborText.T("town.settle.success"),
            TownLaborSettleKind.Recall => LaborText.T("town.settle.recall"),
            _ => LaborText.T("town.settle.fail"),
        };

        string who;
        if (pcSelf)
        {
            who = LaborText.T("town.you");
        }
        else
        {
            who = string.IsNullOrEmpty(m.workerName) ? ("#" + m.uidWorker) : m.workerName;
        }

        string job = string.IsNullOrEmpty(m.jobTitle) ? LaborTerms.TownWork : m.jobTitle;
        string msg = LaborText.T("town.settle.line", who, job, outcome);
        if (m.rewardLog != null && m.rewardLog.Count > 0)
        {
            // Keep invest / skill warns visible; don't hard-cap at 3 when invest raised.
            int take = Math.Min(m.rewardLog.Count, 5);
            msg += " " + string.Join(" / ", m.rewardLog.Take(take));
        }

        // Quest.Fail already plays questFail + fame; keep settle line lean.
        try { Msg.Say(msg); } catch { }
        Plugin.LogInfo("townlabor settle id=" + m.missionId + " kind=" + kind);
        try { Save(); } catch { }
    }

    static void RestoreWorkerAfterLabor(TownLaborMission m, Chara worker)
    {
        if (worker == null || worker.isDead)
        {
            return;
        }

        bool wantParty = false;
        try { wantParty = m != null && m.wasPartyMember; } catch { wantParty = false; }
        if (!wantParty)
        {
            try { wantParty = worker.c_wasInPcParty; } catch { }
        }

        if (wantParty)
        {
            bool rejoined = false;
            try
            {
                try
                {
                    if (!worker.IsGlobal)
                    {
                        worker.SetGlobal();
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogWarn("townlabor SetGlobal: " + ex.Message);
                }

                // Idle AI before party ops.
                try
                {
                    if (worker.ai is AI_TownLabor)
                    {
                        worker.SetAIIdle();
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
Party? party = EClass.pc?.party;
                if (party != null)
                {
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

                    if (!already)
                    {
                        try
                        {
                            // Force-clear stale party link then add.
                            try { worker.party = null; } catch { }
                            party.AddMemeber(worker, showMsg: true);
                        }
                        catch (Exception ex)
                        {
                            Plugin.LogWarn("townlabor AddMemeber: " + ex.Message);
                        }

                        // Verify / force uid membership.
                        try
                        {
                            if (party.uidMembers != null && !party.uidMembers.Contains(worker.uid))
                            {
                                party.uidMembers.Add(worker.uid);
                            }
                            try { party._members = null; } catch { }
                            try { worker.party = party; } catch { }
                            try { WidgetRoster.SetDirty(); } catch { }
                        }
                        catch (Exception ex2)
                        {
                            Plugin.LogWarn("townlabor party force: " + ex2.Message);
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

                // Only clear the auto-rejoin flag once membership looks good.
                if (rejoined)
                {
                    try { worker.c_wasInPcParty = false; } catch { }
                }
                else
                {
                    try { worker.c_wasInPcParty = true; } catch { }
                    Plugin.LogWarn("townlabor party restore incomplete uid=" + worker.uid);
                }

                // Bring teammate to PC map / nearby.
                try
                {
                    Zone? pcZone = EClass._zone ?? EClass.pc?.currentZone;
                    if (pcZone != null
                        && (worker.currentZone == null || worker.currentZone.uid != pcZone.uid))
                    {
                        worker.MoveZone(pcZone, ZoneTransition.EnterState.Return);
                    }

                    if (EClass.pc != null && EClass.pc.ExistsOnMap)
                    {
                        try
                        {
                            if (!worker.ExistsOnMap || worker.Dist(EClass.pc) > 2)
                            {
                                Point? nearPc = FindNearPoint(EClass.pc.pos, worker);
                                if (nearPc != null)
                                {
                                    try { worker.Teleport(nearPc, silent: true, force: true); }
                                    catch
                                    {
                                        try { worker.MoveImmediate(nearPc, focus: false, cancelAI: true); } catch { }
                                    }
                                }
                            }
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}
                }
                catch (Exception ex)
                {
                    Plugin.LogWarn("townlabor party return: " + ex.Message);
                }

                Plugin.LogInfo("townlabor party restore worker=" + worker.uid + " rejoined=" + rejoined);
                return;
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("townlabor party restore: " + ex.Message);
            }
        }

        try
        {
            Zone? home = m?.GetHomeZone();
            if (home != null && (worker.currentZone == null || worker.currentZone.uid != home.uid))
            {
                worker.MoveZone(home, ZoneTransition.EnterState.Return);
            }
            else if (home == null && worker.homeZone != null
                && (worker.currentZone == null || worker.currentZone.uid != worker.homeZone.uid))
            {
                worker.MoveZone(worker.homeZone);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor home MoveZone: " + ex.Message);
        }
    }

    static string SafeZoneName(Zone zone)
    {
        try
        {
            return zone.Name ?? zone.NameWithLevel ?? zone.id ?? ("#" + zone.uid);
        }
        catch
        {
            try { return zone.id ?? ("#" + zone.uid); }
            catch { return NpcLabor.LaborText.T("town.msg.townName"); }
        }
    }

    static string? SavePath()
    {
        try
        {
            string root = GameIO.pathCurrentSave;
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            return Path.Combine(root, "npclabor_townlabor.json");
        }
        catch
        {
            return null;
        }
    }

    static bool _hourSavePending;

    internal static bool HasPendingHourSave => _hourSavePending;

    internal static void FlushPendingHourSave()
    {
        if (!_hourSavePending)
        {
            return;
        }

        _hourSavePending = false;
        if (Missions.Count > 0)
        {
            Save();
        }
    }

    internal static void Save()
    {
        _hourSavePending = false;
        try
        {
            string? path = SavePath();
            if (path == null)
            {
                return;
            }

            foreach (TownLaborMission m in Missions)
            {
                if (m.missionId <= 0)
                {
                    m.missionId = _nextMissionId++;
                }
            }

            var data = new TownLaborSaveData
            {
                missions = Missions.ToList(),
                lastSeenWorldRaw = CurrentRawDate(),
            };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            Plugin.LogDebug("townlabor saved " + Missions.Count + " -> " + path);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor save failed: " + ex.Message);
        }
    }

    internal static void Load()
    {
        Missions.Clear();
        _nextMissionId = 1;
        try
        {
            string? path = SavePath();
            if (path == null || !File.Exists(path))
            {
                Plugin.LogDebug("townlabor load: no file");
                return;
            }

            string json = File.ReadAllText(path);
            TownLaborSaveData? data = JsonConvert.DeserializeObject<TownLaborSaveData>(json);
            if (data?.missions == null)
            {
                return;
            }

            foreach (TownLaborMission m in data.missions)
            {
                if (m == null || m.uidWorker <= 0)
                {
                    continue;
                }

                if (m.missionId <= 0)
                {
                    m.missionId = _nextMissionId++;
                }

                if (m.missionId >= _nextMissionId)
                {
                    _nextMissionId = m.missionId + 1;
                }

                if (m.rewardLog == null)
                {
                    m.rewardLog = new List<string>();
                }

                Missions.Add(m);
            }


            Plugin.LogInfo("townlabor loaded " + Missions.Count);

            int lastSeen = 0;
            try { lastSeen = data.lastSeenWorldRaw; } catch { lastSeen = 0; }
            if (ShouldAutoRecallAfterModGap(lastSeen) && Missions.Count > 0)
            {
                int n = Missions.Count;
                Plugin.LogWarn("townlabor reinstall heal: world advanced without mod save (lastSeen="
                    + lastSeen + " now=" + CurrentRawDate() + "); auto-recalling " + n + " mission(s)");
                foreach (TownLaborMission m in Missions.ToList())
                {
                    try
                    {
                        if (m != null && Missions.Contains(m))
                        {
                            Settle(m, TownLaborSettleKind.Recall);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogWarn("townlabor reinstall recall: " + ex.Message);
                    }
                }

                try { Save(); } catch { }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor load failed: " + ex.Message);
            Missions.Clear();
        }

        // Old saves lack workStarted: companions must tick; PC self defaults to already-working
        // only if missing was false for isPcSelf false (C# default true handles missing JSON).
        try
        {
            foreach (TownLaborMission m in Missions)
            {
                if (m == null)
                {
                    continue;
                }

                if (!m.isPcSelf)
                {
                    m.workStarted = true;
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            // Migrate old saves: lock reward pins so descriptions match payouts.
            foreach (TownLaborMission m in Missions)
            {
                if (m != null && !m.rewardPreviewLocked)
                {
                    TownLaborRewards.LockRewardPreview(m);
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
        try
        {
            RefreshTrackerQuests();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor load tracker: " + ex.Message);
        }
    }

    internal static void ClearAllRuntime()
    {
        Missions.Clear();
        _nextMissionId = 1;
    }

    /// <summary>Start PC walk-to-client AI. No teleport.</summary>
    static void StartPcSelfApproach(TownLaborMission mission, Chara? pc)
    {
        if (mission == null || pc == null || pc.isDead)
        {
            return;
        }

        try
        {
            // Already next to client: start work immediately.
            Chara? client = mission.GetClient();
            if (client != null
                && client.ExistsOnMap
                && pc.ExistsOnMap
                && EClass._zone != null
                && EClass._zone.uid == mission.uidZone)
            {
                int dist = 99;
                try { dist = pc.Dist(client); } catch { dist = 99; }
                if (dist <= AI_TownLaborPcSelf.ArriveDist)
                {
                    MarkPcSelfWorkStarted(mission, firstHourSp: true);
                    return;
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
try
        {
            pc.SetAIImmediate(new AI_TownLaborPcSelf { missionId = mission.missionId });
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor SetAIImmediate approach: " + ex.Message);
            try
            {
                pc.SetAI(new AI_TownLaborPcSelf { missionId = mission.missionId });
            }
            catch (Exception ex2)
            {
                Plugin.LogWarn("townlabor SetAI approach: " + ex2.Message);
            }
        }
    }

    /// <summary>Called by AI when PC reaches the shop client.</summary>
    internal static void NotifyPcSelfArrived(int missionId)
    {
        TownLaborMission? m = FindByMissionId(missionId);
        if (m == null || !m.isPcSelf || m.workStarted)
        {
            return;
        }

        MarkPcSelfWorkStarted(m, firstHourSp: true);
    }

    /// <summary>Approach cancelled (manual cancel / damage / path fail) => real fail.</summary>
    internal static void NotifyPcSelfApproachCancelled(int missionId)
    {
        TownLaborMission? m = FindByMissionId(missionId);
        if (m == null || !m.isPcSelf)
        {
            return;
        }

        // Work already started: the player stopped working mid-shift -> recall
        // with no main prize (same as the 停下 menu action).
        if (m.workStarted)
        {
            Plugin.LogInfo("townlabor self work cancel id=" + m.missionId);
            Settle(m, TownLaborSettleKind.Recall);
            return;
        }

        Plugin.LogInfo("townlabor self approach cancel id=" + m.missionId);
        Settle(m, TownLaborSettleKind.Failed);
    }

    static void MarkPcSelfWorkStarted(TownLaborMission m, bool firstHourSp)
    {
        if (m == null || m.workStarted)
        {
            return;
        }

        m.workStarted = true;

        Chara? pc = null;
        try { pc = EClass.pc; } catch { pc = null; }

        if (firstHourSp && pc != null && !pc.isDead)
        {
            int spMax = 1;
            try { spMax = pc.stamina != null ? Math.Max(1, pc.stamina.max) : 1; } catch { spMax = 1; }
            int startSp = SelfWorkSpCost(spMax, firstHour: true);
            try
            {
                if (pc.stamina != null && startSp > 0)
                {
                    pc.stamina.Mod(-startSp);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("townlabor self arrive sp: " + ex.Message);
            }
        }

        string job = string.IsNullOrEmpty(m.jobTitle) ? NpcLabor.LaborTerms.TownWork : m.jobTitle;
        string msg = NpcLabor.LaborText.T("town.msg.workStart", m.clientName, job);
        try { Msg.Say(msg); } catch { }
        Plugin.LogInfo("townlabor self work started id=" + m.missionId);

        try { Save(); } catch { }
        try { RefreshTrackerQuests(); } catch { }
    }

    static void MaybeResumePcSelfApproach(TownLaborMission m)
    {
        if (m == null || !m.isPcSelf || m.workStarted)
        {
            return;
        }

        Chara? pc = null;
        try { pc = EClass.pc; } catch { pc = null; }
        if (pc == null || pc.isDead)
        {
            return;
        }

        // Only resume when PC is on the work map.
        try
        {
            if (EClass._zone == null || EClass._zone.uid != m.uidZone)
            {
                return;
            }
        }
        catch
        {
            return;
        }

        Chara? client = m.GetClient();
        if (client != null && client.ExistsOnMap && pc.ExistsOnMap)
        {
            int dist = 99;
            try { dist = pc.Dist(client); } catch { dist = 99; }
            if (dist <= AI_TownLaborPcSelf.ArriveDist)
            {
                MarkPcSelfWorkStarted(m, firstHourSp: true);
                return;
            }
        }

        // Already walking for this mission.
        try
        {
            if (pc.ai is AI_TownLaborPcSelf a && a.missionId == m.missionId && a.IsRunning)
            {
                return;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
StartPcSelfApproach(m, pc);
    }

    static void ClearPcSelfApproachAi()
    {
        Chara? pc = null;
        try { pc = EClass.pc; } catch { pc = null; }
        if (pc == null)
        {
            return;
        }

        try
        {
            if (pc.ai is AI_TownLaborPcSelf)
            {
                // Success path already ends AI; cancel only if still running.
                if (pc.ai.IsRunning)
                {
                    pc.ai.Success();
                }
            }
        }
        catch
        {
            try
            {
                if (pc.ai is AI_TownLaborPcSelf)
                {
                    pc.SetAIIdle();
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
}
    }

    /// <summary>Vanilla Quest.Fail for hard fail (sound + fame + karma). Safe if already removed.</summary>
    static void FailTrackerQuest(TownLaborMission? mission)
    {
        if (mission == null)
        {
            return;
        }

        try
        {
            QuestManager? qm = EClass.game?.quests;
            if (qm?.list == null)
            {
                return;
            }

            foreach (Quest q in qm.list.ToList())
            {
                if (q is QuestNpcLaborTownLabor tq && tq.missionId == mission.missionId)
                {
                    try { tq.track = false; } catch { }
                    try
                    {
                        tq.Fail();
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogDebug("townlabor tracker Fail(): " + ex.Message);
                        try { qm.Remove(q); } catch { }
                    }
                }
            }

                        RefreshQuestTrackerWidget();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor tracker fail remove: " + ex.Message);
            try { RemoveTrackerQuest(mission); } catch { }
        }
    }

    

    internal static bool IsOurTownTrackerQuestPublic(Quest? q) => IsOurTownTrackerQuest(q);

    internal static bool IsOurTownTrackerQuest(Quest? q)
    {
        if (q == null)
        {
            return false;
        }

        if (q is QuestNpcLaborTownLabor)
        {
            return true;
        }

        try
        {
            string id = q.id ?? string.Empty;
            if (id.StartsWith("npclabor_townlabor_", StringComparison.Ordinal))
            {
                return true;
            }
        }
        catch { }

        try
        {
            string tn = q.GetType().Name ?? string.Empty;
            if (tn.IndexOf("NpcLaborTownLabor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// F9/auto-save: keep live typed town pins. Drop Dummy / orphan / dups only.
    /// </summary>
    internal static void SanitizeBrokenTrackerQuestsForSave()
    {
        try
        {
            QuestManager? qm = EClass.game?.quests;
            if (qm?.list == null)
            {
                return;
            }

            var live = new HashSet<int>();
            foreach (TownLaborMission m in Missions)
            {
                if (m != null && m.missionId > 0)
                {
                    live.Add(m.missionId);
                }
            }

            var seen = new HashSet<int>();
            foreach (Quest q in qm.list.ToList())
            {
                if (!IsOurTownTrackerQuest(q))
                {
                    continue;
                }

                int mid = 0;
                bool typed = false;
                try
                {
                    if (q is QuestNpcLaborTownLabor tq)
                    {
                        typed = true;
                        mid = tq.missionId;
                    }
                    else
                    {
                        string id = q.id ?? string.Empty;
                        const string prefix = "npclabor_townlabor_";
                        if (id.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            int.TryParse(id.Substring(prefix.Length), out mid);
                        }
                    }
                }
                catch { mid = 0; }

                bool orphan = mid <= 0 || !live.Contains(mid);
                bool dup = mid > 0 && !seen.Add(mid);
                bool drop = !typed || orphan || dup;
                if (!drop)
                {
                    try
                    {
                        if (q is QuestNpcLaborTownLabor liveQ)
                        {
                            liveQ.track = true;
                            liveQ.deadline = 0;
                            liveQ.EnsureSafePerson();
                        }
                    }
                    catch { }
                    continue;
                }

                try { q.track = false; } catch { }
                try { qm.Remove(q); } catch { }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor sanitize broken trackers: " + ex.Message);
        }
    }

    static void TryStartTrackerQuest(TownLaborMission mission)
    {
        if (mission == null || mission.missionId <= 0)
        {
            return;
        }

        try
        {
            // Guard: never Start if a live pin already exists.
            // Do not Remove+Start (widget race / double pin).
            try
            {
                QuestManager? qm = EClass.game?.quests;
                if (qm?.list != null)
                {
                    foreach (Quest existing in qm.list)
                    {
                        if (existing is QuestNpcLaborTownLabor tq && tq.missionId == mission.missionId)
                        {
                            try
                            {
                                tq.track = true;
                                tq.deadline = 0;
                                tq.isNew = false;
                                tq.EnsureSafePerson();
                            }
                            catch { }
                            // Same Quest object already in list — widget row already bound by ref.
                            return;
                        }
                    }

                    string wantId = "npclabor_townlabor_" + mission.missionId;
                    foreach (Quest orphan in qm.list.ToList())
                    {
                        if (orphan is QuestNpcLaborTownLabor)
                        {
                            continue;
                        }

                        if (orphan == null)
                        {
                            continue;
                        }

                        bool match = false;
                        try
                        {
                            match = string.Equals(orphan.id, wantId, StringComparison.Ordinal);
                        }
                        catch { match = false; }
                        if (!match)
                        {
                            continue;
                        }

                        try { orphan.track = false; } catch { }
                        try { qm.Remove(orphan); } catch { }
                    }
                }
            }
            catch { }

            var q = new QuestNpcLaborTownLabor
            {
                missionId = mission.missionId,
                id = "npclabor_townlabor_" + mission.missionId,
            };

            try { q.Init(); } catch { }
            q.deadline = 0;
            q.track = true;
            q.isNew = false;
            try { q.SetClient(null, assignQuest: false); } catch { }
            q.EnsureSafePerson();
            QuestManager? startQm = EClass.game?.quests;
            if (startQm == null)
            {
                return;
            }

            startQm.Start(q);
            q.EnsureSafePerson();
            q.deadline = 0;
            q.track = true;
            q.isNew = false;
            DungeonDispatchManager.DedupeNpcLaborTrackerQuests();
            // Vanilla Start already Show()s the pin; do not thrash WidgetQuestTracker.
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor tracker start: " + ex.Message);
        }
    }

    static void RemoveTrackerQuest(TownLaborMission? mission)
    {
        if (mission == null)
        {
            return;
        }

        try
        {
            QuestManager? qm = EClass.game?.quests;
            if (qm?.list == null)
            {
                return;
            }

            string wantId = "npclabor_townlabor_" + mission.missionId;
            foreach (Quest q in qm.list.ToList())
            {
                bool match = false;
                try
                {
                    if (q is QuestNpcLaborTownLabor tq && tq.missionId == mission.missionId)
                    {
                        match = true;
                    }
                    else if (q != null && string.Equals(q.id, wantId, StringComparison.Ordinal))
                    {
                        match = true;
                    }
                }
                catch { }

                if (!match || q == null)
                {
                    continue;
                }

                try { q.track = false; } catch { }
                try { qm.Remove(q); } catch { }
            }

            // Vanilla kills widget rows whose quest left the list.
            RefreshQuestTrackerWidget();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor tracker remove: " + ex.Message);
        }
    }

    internal static void RefreshTrackerQuestsPublic()
    {
        RefreshTrackerQuests();
    }

    static void RefreshTrackerQuests()
    {
        // Ensure every mission has exactly one tracker; drop orphans/dups/legacy.
        try
        {
            QuestManager? qm = EClass.game?.quests;
            var live = new HashSet<int>();
            foreach (TownLaborMission m in Missions)
            {
                live.Add(m.missionId);
            }

            if (qm?.list != null)
            {
                var seen = new HashSet<int>();
                foreach (Quest q in qm.list.ToList())
                {
                    if (!IsOurTownTrackerQuest(q))
                    {
                        continue;
                    }

                    int mid = 0;
                    try
                    {
                        if (q is QuestNpcLaborTownLabor tq)
                        {
                            mid = tq.missionId;
                        }
                        else
                        {
                            string id = q.id ?? string.Empty;
                            const string prefix = "npclabor_townlabor_";
                            if (id.StartsWith(prefix, StringComparison.Ordinal))
                            {
                                int.TryParse(id.Substring(prefix.Length), out mid);
                            }
                        }
                    }
                    catch { mid = 0; }

                    bool typed = q is QuestNpcLaborTownLabor;
                    bool orphan = mid <= 0 || !live.Contains(mid);
                    bool dup = mid > 0 && !seen.Add(mid);
                    if (typed && !orphan && !dup)
                    {
                        continue;
                    }

                    try { q.track = false; } catch { }
                    try { qm.Remove(q); } catch { }
                }
            }

            foreach (TownLaborMission m in Missions)
            {
                bool has = false;
                if (qm?.list != null)
                {
                    foreach (Quest q in qm.list)
                    {
                        if (q is QuestNpcLaborTownLabor tq && tq.missionId == m.missionId)
                        {
                            has = true;
                            try
                            {
                                tq.track = true;
                                tq.deadline = 0;
                                tq.EnsureSafePerson();
                            }
                            catch { }
                            break;
                        }
                    }
                }

                if (!has)
                {
                    TryStartTrackerQuest(m);
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborManager.cs silent catch: " + __e.Message); }
    }

    static void RefreshQuestTrackerWidget()
    {
        DungeonDispatchManager.RequestQuestTrackerRefresh();
    }

    /// <summary>Debug: finish all active town labor as success.</summary>
    internal static int DebugCompleteAll()
    {
        int n = 0;
        foreach (TownLaborMission m in Missions.ToList())
        {
            try
            {
                Settle(m, TownLaborSettleKind.Success);
                n++;
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("townlabor debug complete: " + ex.Message);
            }
        }

        return n;
    }

    /// <summary>Debug: finish one town labor by missionId or worker uid.</summary>
    internal static bool DebugCompleteOne(int missionIdOrWorkerUid)
    {
        TownLaborMission? m = FindByMissionId(missionIdOrWorkerUid) ?? FindByWorker(missionIdOrWorkerUid);
        if (m == null)
        {
            return false;
        }

        try
        {
            Settle(m, TownLaborSettleKind.Success);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor debug complete one: " + ex.Message);
            return false;
        }
    }
}

/// <summary>Board row model for an available (or active) town labor offer.</summary>
internal sealed class TownLaborOffer
{
    internal Chara Client = null!;
    internal TownLaborJobDef Def = null!;
    internal int Hours = 18;
    internal bool IsActive;
}
