using NpcLabor.TownLabor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NpcLabor.Dispatch;

internal sealed class DungeonDispatchLastPreset
{
    [JsonProperty]
    public string key = "";

    [JsonProperty]
    public bool isRegion;

    [JsonProperty]
    public string regionKind = "";

    [JsonProperty]
    public int uidZone;

    [JsonProperty]
    public string zoneId = "";

    [JsonProperty]
    public int exploreWeeks = 1;

    [JsonProperty]
    public List<int> uidMembers = new List<int>();
}

internal sealed class DungeonDispatchSaveData
{
    [JsonProperty]
    public List<DungeonDispatchMission> missions = new List<DungeonDispatchMission>();

    /// <summary>Last started team per target key (region kind or zone id).</summary>
    [JsonProperty]
    public List<DungeonDispatchLastPreset> lastPresets = new List<DungeonDispatchLastPreset>();

    /// <summary>
    /// World date.GetRaw() written on every mod save.
    /// If the world advances past this without a mod save, Load treats it as
    /// "played without NPC Labor" and auto-recalls open missions (reinstall heal).
    /// </summary>
    [JsonProperty]
    public int lastSeenWorldRaw;
}

/// <summary>
/// Runtime + save manager for slice D dungeon dispatch.
/// One mission per dungeon, up to 4 members. Save: npclabor_dispatch.json
/// </summary>
internal static class DungeonDispatchManager
{
    static readonly List<DungeonDispatchMission> Missions = new List<DungeonDispatchMission>();
    static readonly List<DungeonDispatchLastPreset> LastPresets = new List<DungeonDispatchLastPreset>();
    static int _nextMissionId = 1;

    internal static IReadOnlyList<DungeonDispatchMission> All => Missions;

    internal static int Count => Missions.Count;

    internal static int BusyMemberCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < Missions.Count; i++)
            {
                Missions[i].EnsureMemberList();
                n += Missions[i].MemberCount;
            }

            return n;
        }
    }

    internal static bool IsBusy(int uidChara)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].HasMember(uidChara))
            {
                return true;
            }
        }

        return false;
    }

    internal static DungeonDispatchMission? FindByChara(int uidChara)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            if (Missions[i].HasMember(uidChara))
            {
                return Missions[i];
            }
        }

        return null;
    }

    internal static DungeonDispatchMission? FindByZone(int uidZone)
    {
        for (int i = 0; i < Missions.Count; i++)
        {
            DungeonDispatchMission m = Missions[i];
            if (m.uidZone == uidZone)
            {
                return m;
            }

            // Also match child floors under the same top dungeon / region field.
            try
            {
                Zone? mz = m.GetZone();
                Zone? z = EClass.game?.spatials?.Find(uidZone);
                if (mz != null && z != null)
                {
                    Zone topM = mz.GetTopZone() ?? mz;
                    Zone topZ = z.GetTopZone() ?? z;
                    if (topM.uid == topZ.uid)
                    {
                        return m;
                    }
                }

                // Region field: match by overworld tile coords.
                if (m.isRegion && z != null)
                {
                    try
                    {
                        if (z.x == m.regionGx && z.y == m.regionGy)
                        {
                            return m;
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

        return null;
    }

    internal static DungeonDispatchMission? FindByMissionId(int id)
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

    internal static bool IsZoneBusy(int uidZone)
    {
        return FindByZone(uidZone) != null;
    }

    internal static DungeonDispatchMission? FindByRegion(string regionKind)
    {
        string want = DungeonDispatchTargets.NormalizeRegionKind(regionKind);
        if (string.IsNullOrEmpty(want))
        {
            return null;
        }

        for (int i = 0; i < Missions.Count; i++)
        {
            DungeonDispatchMission m = Missions[i];
            if (m != null && m.isRegion
                && DungeonDispatchTargets.NormalizeRegionKind(m.regionKind) == want)
            {
                return m;
            }
        }

        return null;
    }

    internal static bool IsRegionBusy(string regionKind)
    {
        return FindByRegion(regionKind) != null;
    }

    internal static bool IsTargetBusy(DungeonDispatchTarget target)
    {
        if (target == null)
        {
            return false;
        }

        if (target.IsRegion)
        {
            return IsRegionBusy(target.RegionKind);
        }

        try
        {
            return target.Zone != null && IsZoneBusy(target.Zone.uid);
        }
        catch
        {
            return false;
        }
    }

    internal static List<Chara> ListCandidates()
    {
        var list = new List<Chara>();
        var seen = new HashSet<int>();

        try
        {
            Party? party = EClass.pc?.party;
            if (party?.members != null)
            {
                foreach (Chara m in party.members)
                {
                    if (!IsCandidate(m) || !seen.Add(m.uid))
                    {
                        continue;
                    }

                    list.Add(m);
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch;
            if (branch?.members != null)
            {
                foreach (Chara m in branch.members)
                {
                    if (!IsCandidate(m) || !seen.Add(m.uid))
                    {
                        continue;
                    }

                    list.Add(m);
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
list.Sort((a, b) =>
        {
            int pa = IsParty(a) ? 0 : 1;
            int pb = IsParty(b) ? 0 : 1;
            int cmp = pa.CompareTo(pb);
            if (cmp != 0)
            {
                return cmp;
            }

            int ca = DungeonDispatchTargets.GetCombatPower(a);
            int cb = DungeonDispatchTargets.GetCombatPower(b);
            cmp = cb.CompareTo(ca);
            return cmp != 0 ? cmp : a.uid.CompareTo(b.uid);
        });
        return list;
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

    internal static bool IsCandidate(Chara? c)
    {
        if (c == null)
        {
            return false;
        }

        try
        {
            if (CoCraft.PersonPickerUi.IsPcLike(c) || c.isDead)
            {
                return false;
            }

            if (CoCraft.PersonPickerUi.IsStayHomeUnique(c))
            {
                return false;
            }

            // Shared busy across A/B/D/E.
            if (LaborBusy.IsBusy(c.uid) || IsBusy(c.uid))
            {
                return false;
            }

            bool ok = false;
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

            return ok;
        }
        catch
        {
            return false;
        }
    }

    static string TargetKey(DungeonDispatchTarget target)
    {
        if (target == null)
        {
            return "";
        }

        if (target.IsRegion)
        {
            return "region:" + DungeonDispatchTargets.NormalizeRegionKind(target.RegionKind);
        }

        try
        {
            if (target.Zone != null && target.Zone.uid > 0)
            {
                return "zone:" + target.Zone.uid;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            if (target.Zone != null && !string.IsNullOrEmpty(target.Zone.id))
            {
                return "zoneid:" + target.Zone.id;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
return "name:" + (target.Name ?? "");
    }

    static string TargetKey(DungeonDispatchMission mission)
    {
        if (mission == null)
        {
            return "";
        }

        if (mission.isRegion)
        {
            return "region:" + DungeonDispatchTargets.NormalizeRegionKind(mission.regionKind);
        }

        if (mission.uidZone > 0)
        {
            return "zone:" + mission.uidZone;
        }

        if (!string.IsNullOrEmpty(mission.zoneId))
        {
            return "zoneid:" + mission.zoneId;
        }

        return "name:" + (mission.zoneName ?? "");
    }

    internal static void RememberLastDispatch(DungeonDispatchMission mission)
    {
        if (mission == null)
        {
            return;
        }

        try
        {
            mission.EnsureMemberList();
            string key = TargetKey(mission);
            if (string.IsNullOrEmpty(key) || mission.uidMembers == null || mission.uidMembers.Count == 0)
            {
                return;
            }

            var preset = new DungeonDispatchLastPreset
            {
                key = key,
                isRegion = mission.isRegion,
                regionKind = mission.regionKind ?? "",
                uidZone = mission.uidZone,
                zoneId = mission.zoneId ?? "",
                exploreWeeks = Mathf.Clamp(mission.exploreWeeks <= 0 ? 1 : mission.exploreWeeks, 1, 4),
                uidMembers = mission.uidMembers.ToList(),
            };

            for (int i = LastPresets.Count - 1; i >= 0; i--)
            {
                if (LastPresets[i] != null
                    && string.Equals(LastPresets[i].key, key, StringComparison.OrdinalIgnoreCase))
                {
                    LastPresets.RemoveAt(i);
                }
            }

            LastPresets.Insert(0, preset);
            while (LastPresets.Count > 24)
            {
                LastPresets.RemoveAt(LastPresets.Count - 1);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch remember last: " + ex.Message);
        }
    }

    internal static DungeonDispatchLastPreset? GetLastPreset(DungeonDispatchTarget target)
    {
        if (target == null)
        {
            return null;
        }

        string key = TargetKey(target);
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        for (int i = 0; i < LastPresets.Count; i++)
        {
            DungeonDispatchLastPreset p = LastPresets[i];
            if (p != null && string.Equals(p.key, key, StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }
        }

        return null;
    }

    internal static List<Chara> ResolveAvailableMembers(IEnumerable<int> uids, int max = DungeonDispatchMission.MaxMembersPerDungeon)
    {
        var list = new List<Chara>();
        if (uids == null)
        {
            return list;
        }

        var seen = new HashSet<int>();
        foreach (int uid in uids)
        {
            if (uid <= 0 || !seen.Add(uid) || list.Count >= max)
            {
                continue;
            }

            if (IsBusy(uid))
            {
                continue;
            }

            Chara? c = null;
            try { c = RefChara.Get(uid); } catch { c = null; }
            if (c == null)
            {
                continue;
            }

            try
            {
                if (!IsCandidate(c))
                {
                    continue;
                }
            }
            catch
            {
                continue;
            }

            list.Add(c);
        }

        return list;
    }

    internal static List<Chara> GetLastDispatchMembers(DungeonDispatchTarget target)
    {
        DungeonDispatchLastPreset? preset = GetLastPreset(target);
        if (preset?.uidMembers == null || preset.uidMembers.Count == 0)
        {
            return new List<Chara>();
        }

        return ResolveAvailableMembers(preset.uidMembers);
    }

    internal static int GetLastExploreWeeks(DungeonDispatchTarget target)
    {
        DungeonDispatchLastPreset? preset = GetLastPreset(target);
        if (preset == null)
        {
            return 1;
        }

        return Mathf.Clamp(preset.exploreWeeks <= 0 ? 1 : preset.exploreWeeks, 1, 4);
    }

    /// <summary>
    /// Greedy pick of up to 4 available members maximizing expected haul score.
    /// Region: explore + gather (+ region gather bias). Dungeon: combat power first, then skills.
    /// </summary>
    internal static List<Chara> PickMaxHarvestMembers(DungeonDispatchTarget target)
    {
        var picked = new List<Chara>();
        if (target == null)
        {
            return picked;
        }

        List<Chara> candidates = ListCandidates();
        if (candidates.Count == 0)
        {
            return picked;
        }

        bool region = target.IsRegion;
        string rk = DungeonDispatchTargets.NormalizeRegionKind(target.RegionKind);
        var scored = new List<(Chara c, int score, int power)>();
        for (int i = 0; i < candidates.Count; i++)
        {
            Chara c = candidates[i];
            int explore = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillExplore);
            int lockpick = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillLockpick);
            int gather = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillGather);
            int specialty = 0;
            if (region)
            {
                // Per-region specialty: beach dig/fish, forest lumber, mountain mine, plain gather.
                if (rk == "forest")
                {
                    specialty = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillLumber);
                    gather = Math.Max(gather, specialty);
                }
                else if (rk == "mountain")
                {
                    specialty = Math.Max(
                        DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillMining),
                        DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillDigging));
                    gather = Math.Max(gather, specialty);
                }
                else if (rk == "beach")
                {
                    int dig = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillDigging);
                    int fish = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillFishing);
                    specialty = Math.Max(dig, fish);
                    gather = Math.Max(gather, specialty);
                }
                else // plain
                {
                    specialty = gather;
                }
            }
            else
            {
                gather = Math.Max(gather, DungeonDispatchTargets.GetRelevantGatherSkill(c, target.Zone));
            }

            int power = DungeonDispatchTargets.GetCombatPower(c);
            int score;
            if (region)
            {
                // Region haul is skill-driven; weight local specialty over flat lockpick.
                // explore materials + specialty food/mats + light lockpick.
                score = explore * 2 + specialty * 3 + gather + lockpick;
            }
            else
            {
                // Dungeon haul is smaller; combat success gates mail, so weight power heavily.
                score = power * 8 + explore + gather + lockpick;
            }

            scored.Add((c, score, power));
        }

        scored.Sort((a, b) =>
        {
            int cmp = b.score.CompareTo(a.score);
            if (cmp != 0) return cmp;
            cmp = b.power.CompareTo(a.power);
            if (cmp != 0) return cmp;
            return string.Compare(a.c.NameSimple ?? a.c.Name, b.c.NameSimple ?? b.c.Name, StringComparison.Ordinal);
        });

        for (int i = 0; i < scored.Count && picked.Count < DungeonDispatchMission.MaxMembersPerDungeon; i++)
        {
            picked.Add(scored[i].c);
        }

        return picked;
    }

    internal static int EstimateTeamHarvestScore(DungeonDispatchTarget target, IList<Chara> members)
    {
        if (target == null || members == null || members.Count == 0)
        {
            return 0;
        }

        int explore = DungeonDispatchTargets.AggregateSkill(members, DungeonDispatchMission.SkillExplore);
        int lockpick = DungeonDispatchTargets.AggregateSkill(members, DungeonDispatchMission.SkillLockpick);
        int gather;
        if (target.IsRegion)
        {
            gather = DungeonDispatchTargets.AggregateRegionGather(members, target.RegionKind);
            return explore + gather + lockpick;
        }

        gather = DungeonDispatchTargets.AggregateGatherSkill(members, target.Zone);
        int power = DungeonDispatchTargets.AggregateCombatPower(members);
        return power * 8 + explore + gather + lockpick;
    }

    internal static string? TryStart(IList<Chara> workers, DungeonDispatchTarget target, int exploreWeeks = 1)
    {
        if (target == null)
        {
            return NpcLabor.LaborText.T("dis.error.invalidTarget");
        }

        if (target.IsRegion)
        {
            return TryStartRegion(workers, target, exploreWeeks);
        }

        if (target.Zone == null)
        {
            return NpcLabor.LaborText.T("dis.error.invalidTarget");
        }

        return TryStart(workers, target.Zone);
    }

    internal static string? TryStartRegion(IList<Chara> workers, DungeonDispatchTarget target, int exploreWeeks = 1)
    {
        try
        {
            if (workers == null || workers.Count == 0 || target == null || !target.IsRegion)
            {
                return NpcLabor.LaborText.T("dis.error.invalidTarget");
            }

            if (!NpcLabor.LaborTerms.CanRegionDispatch(out string? regionDeny))
            {
                return regionDeny ?? NpcLabor.LaborText.T("dis.error.notUnlocked", NpcLabor.LaborTerms.RegionDispatch);
            }

            if (workers.Count > DungeonDispatchMission.MaxMembersPerDungeon)
            {
                return NpcLabor.LaborText.T("dis.error.tooManyTarget", DungeonDispatchMission.MaxMembersPerDungeon);
            }

            var members = new List<Chara>();
            var seen = new HashSet<int>();
            foreach (Chara w in workers)
            {
                if (w == null || !seen.Add(w.uid))
                {
                    continue;
                }

                if (!IsCandidate(w))
                {
                    return NpcLabor.LaborText.T("dis.error.cannotSend", w.NameSimple ?? w.Name ?? ("#" + w.uid));
                }

                members.Add(w);
            }

            if (members.Count == 0)
            {
                return NpcLabor.LaborText.T("dis.error.noOne");
            }

            if (IsRegionBusy(target.RegionKind))
            {
                return NpcLabor.LaborText.T("dis.error.regionBusy", NpcLabor.LaborTerms.RegionDispatch);
            }

            FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch ?? members[0].homeBranch;
            Zone? home = branch?.owner ?? members[0].homeZone ?? EClass.pc?.homeZone;
            if (branch == null || home == null)
            {
                return NpcLabor.LaborText.T("dis.error.needBase");
            }

            foreach (Chara worker in members)
            {
                if (IsParty(worker))
                {
                    try
                    {
                        EClass.pc?.party?.RemoveMember(worker);
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogWarn("dispatch remove party failed: " + ex.Message);
                    }
                }
            }

            int power = DungeonDispatchTargets.AggregateCombatPower(members);
            int explore = DungeonDispatchTargets.AggregateSkill(members, DungeonDispatchMission.SkillExplore);
            int lockpick = DungeonDispatchTargets.AggregateSkill(members, DungeonDispatchMission.SkillLockpick);
            int gather = 0;
            foreach (Chara c in members)
            {
                int g = DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillGather);
                string rk = DungeonDispatchTargets.NormalizeRegionKind(target.RegionKind);
                if (rk == "forest")
                {
                    g = Math.Max(g, DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillLumber));
                }
                else if (rk == "mountain")
                {
                    g = Math.Max(g, Math.Max(
                        DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillMining),
                        DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillDigging)));
                }
                else if (rk == "beach")
                {
                    g = Math.Max(g, Math.Max(
                        DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillDigging),
                        DungeonDispatchTargets.SafeSkill(c, DungeonDispatchMission.SkillFishing)));
                }

                gather += g;
            }

            int distDays = Math.Max(1, target.DistDays);
            int weeks = Mathf.Clamp(exploreWeeks <= 0 ? 1 : exploreWeeks, 1, 4);
            // Travel leg (half distance) + chosen explore weeks. No danger days / success roll.
            int travelHours = DungeonDispatchTargets.GetRegionTravelHours(distDays);
            int exploreHours = weeks * 7 * 24;
            int baseHours = travelHours + exploreHours;
            int hours = baseHours;
            try
            {
                int tMin = Mathf.Max(6, Mathf.FloorToInt(travelHours * 0.8f));
                int tMax = Mathf.Max(tMin + 1, Mathf.FloorToInt(travelHours * 1.2f));
                hours = (tMin + EClass.rnd(Math.Max(1, tMax - tMin + 1))) + exploreHours;
            }
            catch
            {
                hours = baseHours;
            }

            if (target.RegionGx == int.MinValue || target.RegionGy == int.MinValue)
            {
                return NpcLabor.LaborText.T("dis.error.regionTile");
            }

            Zone? field = DungeonDispatchTargets.EnsureRegionFieldZone(target.RegionGx, target.RegionGy);
            if (field == null || !DungeonDispatchTargets.IsReusableFieldZone(field))
            {
                return NpcLabor.LaborText.T("dis.error.regionTile");
            }

            int fieldUid = 0;
            string fieldId = "region:" + DungeonDispatchTargets.NormalizeRegionKind(target.RegionKind);
            try { fieldUid = field.uid; } catch { fieldUid = 0; }
            try
            {
                if (!string.IsNullOrEmpty(field.id))
                {
                    fieldId = field.id;
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }

            var mission = new DungeonDispatchMission
            {
                missionId = _nextMissionId++,
                uidMembers = members.Select(m => m.uid).ToList(),
                uidChara = members[0].uid,
                uidZone = fieldUid,
                zoneId = fieldId,
                zoneName = target.Name,
                dangerLv = 1, // unused for region
                isRandomSite = false,
                isRegion = true,
                regionKind = DungeonDispatchTargets.NormalizeRegionKind(target.RegionKind),
                regionGx = target.RegionGx,
                regionGy = target.RegionGy,
                exploreWeeks = weeks,
                hoursTotal = hours,
                hoursLeft = hours,
                combatPower = power,
                exploreSkill = explore,
                lockpickSkill = lockpick,
                gatherSkill = gather,
                successChance = 100, // region has no combat success roll
                distDays = distDays,
                travelHours = Math.Max(DungeonDispatchTargets.GetRegionTravelHours(distDays), Math.Max(0, hours - exploreHours)),
                dangerDays = 0,
                homeZoneUid = home.uid,
                currentFloorLv = 0,
                deepestFloorLv = 0,
                lootLog = new List<string>(),
            };

            foreach (Chara worker in members)
            {
                try
                {
                    PrepareDispatchedChara(worker);
                    if (field != null)
                    {
                        try
                        {
                            worker.MoveZone(field, ZoneTransition.EnterState.RandomVisit);
                        }
                        catch
                        {
                            worker.MoveZone(field);
                        }
                    }
                    else
                    {
                        // Last resort if field creation failed — invisible but mission still ticks.
                        worker.MoveZone("somewhere");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogWarn("dispatch region MoveZone failed: " + ex.Message);
                    try { PrepareDispatchedChara(worker); } catch { }
                }

                try { PrepareDispatchedChara(worker); } catch { }
            }

            Missions.Add(mission);
            RememberLastDispatch(mission);
            // Lock region haul immediately so quest tracker shows real harvest from hour 0.
            try { DungeonDispatchRewards.TickPartialLoot(branch, mission); } catch { }
            TryStartTrackerQuest(mission);
            RefreshRegionMapMarkers();
            try { Save(); } catch { }

            string names = mission.MemberNames();
            string msg = NpcLabor.LaborText.T(
                "dis.start.region",
                names,
                NpcLabor.LaborTerms.RegionDispatch,
                mission.zoneName,
                members.Count,
                weeks);
            try { Msg.Say(msg); } catch { }
            try { branch.LogRaw(msg); } catch { }

            Plugin.LogInfo("dispatch start region mission=" + mission.missionId
                + " kind=" + mission.regionKind + " members=" + members.Count + " hours=" + hours);
            return null;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch TryStartRegion failed: " + ex);
            return NpcLabor.LaborText.T("dis.error.startFail", ex.Message);
        }
    }

    internal static string? TryStart(IList<Chara> workers, Zone target)
    {
        try
        {
            if (workers == null || workers.Count == 0 || target == null)
            {
                return NpcLabor.LaborText.T("dis.error.invalidTarget");
            }

            if (!NpcLabor.LaborTerms.CanDungeonExplore(out string? dungeonDeny))
            {
                return dungeonDeny ?? NpcLabor.LaborText.T("dis.error.notUnlocked", NpcLabor.LaborTerms.DungeonExplore);
            }

            if (workers.Count > DungeonDispatchMission.MaxMembersPerDungeon)
            {
                return NpcLabor.LaborText.T("dis.error.tooManyDungeon", DungeonDispatchMission.MaxMembersPerDungeon);
            }

            // Dedup + validate.
            var members = new List<Chara>();
            var seen = new HashSet<int>();
            foreach (Chara w in workers)
            {
                if (w == null || !seen.Add(w.uid))
                {
                    continue;
                }

                if (!IsCandidate(w))
                {
                    return NpcLabor.LaborText.T("dis.error.cannotSend", w.NameSimple ?? w.Name ?? ("#" + w.uid));
                }

                members.Add(w);
            }

            if (members.Count == 0)
            {
                return NpcLabor.LaborText.T("dis.error.noOne");
            }

            if (IsZoneBusy(target.uid))
            {
                return NpcLabor.LaborText.T("dis.error.dungeonBusy", NpcLabor.LaborTerms.DungeonExplore);
            }

            // Also block same top dungeon.
            try
            {
                Zone top = target.GetTopZone() ?? target;
                if (FindByZone(top.uid) != null)
                {
                    return NpcLabor.LaborText.T("dis.error.dungeonBusy", NpcLabor.LaborTerms.DungeonExplore);
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch ?? members[0].homeBranch;
            Zone? home = branch?.owner ?? members[0].homeZone ?? EClass.pc?.homeZone;
            if (branch == null || home == null)
            {
                return NpcLabor.LaborText.T("dis.error.needBase");
            }

            if (!DungeonDispatchTargets.IsDispatchable(target, home))
            {
                return NpcLabor.LaborText.T("dis.error.dungeonNo", NpcLabor.LaborTerms.DungeonExplore);
            }

            foreach (Chara worker in members)
            {
                if (IsParty(worker))
                {
                    try
                    {
                        EClass.pc?.party?.RemoveMember(worker);
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogWarn("dispatch remove party failed: " + ex.Message);
                    }
                }
            }

            int power = DungeonDispatchTargets.AggregateCombatPower(members);
            int explore = DungeonDispatchTargets.AggregateSkill(members, DungeonDispatchMission.SkillExplore);
            int lockpick = DungeonDispatchTargets.AggregateSkill(members, DungeonDispatchMission.SkillLockpick);
            int gather = DungeonDispatchTargets.AggregateGatherSkill(members, target);

            int distDays = DungeonDispatchTargets.ZoneDistDays(home, target);
            int dangerDays = DungeonDispatchTargets.GetDangerDays(target.DangerLv, power);
            int baseHours = DungeonDispatchTargets.GetBaseHours(distDays, dangerDays);
            int hours = DungeonDispatchTargets.RollMissionHours(baseHours);
            int chance = DungeonDispatchTargets.GetSuccessChance(power, target.DangerLv, explore, lockpick, gather);

            Zone root = target;
            try
            {
                root = target.GetTopZone() ?? target;
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
int entranceLv = 0;
            try
            {
                entranceLv = root.lv;
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
var mission = new DungeonDispatchMission
            {
                missionId = _nextMissionId++,
                uidMembers = members.Select(m => m.uid).ToList(),
                uidChara = members[0].uid,
                uidZone = root.uid,
                zoneId = root.id ?? target.id ?? "",
                zoneName = DungeonDispatchTargets.SafeZoneName(root),
                dangerLv = Math.Max(1, root.DangerLv > 0 ? root.DangerLv : target.DangerLv),
                isRandomSite = root.isRandomSite || target.isRandomSite,
                hoursTotal = hours,
                hoursLeft = hours,
                combatPower = power,
                exploreSkill = explore,
                lockpickSkill = lockpick,
                gatherSkill = gather,
                successChance = chance,
                distDays = distDays,
                travelHours = Math.Max(DungeonDispatchTargets.GetRegionTravelHours(distDays), Math.Max(6, distDays * 12)),
                dangerDays = dangerDays,
                homeZoneUid = home.uid,
                currentFloorLv = entranceLv,
                deepestFloorLv = entranceLv,
                lootLog = new List<string>(),
            };

            // Move all members into the real target zone (friendly non-interactive stakeout).
            Zone startFloor = DungeonDispatchTargets.ResolveFloorForProgress(root, 0) ?? root;
            mission.currentFloorLv = SafeLv(startFloor);
            foreach (Chara worker in members)
            {
                try
                {
                    PrepareDispatchedChara(worker);
                    worker.MoveZone(startFloor, ZoneTransition.EnterState.RandomVisit);
                }
                catch (Exception ex)
                {
                    Plugin.LogWarn("dispatch MoveZone failed: " + ex.Message);
                    try
                    {
                        worker.MoveZone(startFloor);
                    }
                    catch
                    {
                        try
                        {
                            worker.MoveZone(root);
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}
                }

                try
                {
                    PrepareDispatchedChara(worker);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

            Missions.Add(mission);
            RememberLastDispatch(mission);
            TryStartTrackerQuest(mission);
            try { Save(); } catch { }

            string names = mission.MemberNames();
            string msg = NpcLabor.LaborText.T(
                "dis.start.dungeon",
                names,
                NpcLabor.LaborTerms.DungeonExplore,
                mission.zoneName,
                members.Count,
                Mathf.CeilToInt(hours / 24f));
            try
            {
                Msg.Say(msg);
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
            {
                branch.LogRaw(msg);
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
Plugin.LogInfo("dispatch start mission=" + mission.missionId + " members=" + members.Count
                + " zone=" + root.uid + " hours=" + hours);
            return null;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch TryStart failed: " + ex);
            return NpcLabor.LaborText.T("dis.error.startFail", ex.Message);
        }
    }

    // Back-compat single-worker entry.
    internal static string? TryStart(Chara worker, Zone target)
    {
        if (worker == null)
        {
            return NpcLabor.LaborText.T("dis.error.invalidTarget");
        }

        return TryStart(new List<Chara> { worker }, target);
    }

    static int SafeLv(Zone z)
    {
        try
        {
            return z.lv;
        }
        catch
        {
            return 0;
        }
    }

    static int FloorDepthAbs(int entranceLv, int floorLv)
    {
        return Math.Abs(floorLv - entranceLv);
    }

    internal static void PrepareDispatchedChara(Chara c)
    {
        if (c == null)
        {
            return;
        }

        try
        {
            c.SetHostility(Hostility.Friend);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            c.noMove = true;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            c.enemy = null;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            // Keep them from normal resident talk trees while exploring.
            c.isRestrained = true;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            c.SetAIIdle();
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

    internal static void ClearDispatchedFlags(Chara? c)
    {
        if (c == null)
        {
            return;
        }

        try
        {
            c.noMove = false;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            c.isRestrained = false;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

    internal static bool TryRecall(int missionIdOrCharaUid)
    {
        DungeonDispatchMission? m = FindByMissionId(missionIdOrCharaUid) ?? FindByChara(missionIdOrCharaUid);
        if (m == null)
        {
            return false;
        }

        Settle(m, DispatchSettleKind.Recall, destroyRandom: false);
        return true;
    }

    internal static void OnSimulateHour()
    {
        if (Missions.Count == 0)
        {
            return;
        }

        // Do not RefreshTrackerQuests every hour. That path can QuestManager.Start a pin
        // (journal spam / widget reorder). Tracker text reads live mission state already.
        // Pin recovery is load / mission-start only.
        var snapshot = Missions.ToList();
        foreach (DungeonDispatchMission m in snapshot)
        {
            try
            {
                if (!Missions.Contains(m))
                {
                    continue;
                }

                m.EnsureMemberList();
                var alive = new List<Chara>();
                foreach (int uid in m.uidMembers.ToList())
                {
                    Chara? c = null;
                    try
                    {
                        c = RefChara.Get(uid);
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
if (c == null || c.isDead)
                    {
                        m.uidMembers.Remove(uid);
                        continue;
                    }

                    alive.Add(c);
                }

                if (alive.Count == 0)
                {
                    Plugin.LogInfo("dispatch drop empty mission id=" + m.missionId);
                    Missions.Remove(m);
                    RemoveTrackerQuest(m);
                    continue;
                }

                // Keep first uid for legacy field.
                m.uidChara = m.uidMembers[0];

                // Stakeout: region stays on field tile; dungeon may advance through existing floors.
                try
                {
                    if (m.isRegion)
                    {
                        Zone? field = m.GetZone();
                        if (field == null || field.destryoed)
                        {
                            field = DungeonDispatchTargets.EnsureRegionFieldZone(m.regionGx, m.regionGy);
                            if (field != null)
                            {
                                m.uidZone = field.uid;
                                try
                                {
                                    if (!string.IsNullOrEmpty(field.id))
                                    {
                                        m.zoneId = field.id;
                                    }
                                }
                                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                            }
                        }

                        // Keep the stakeout field alive for the whole mission
                        // (vanilla field default expiry is 7 days; Region.OnActivate
                        // would otherwise reap it and leave the player an empty field).
                        try
                        {
                            if (field != null)
                            {
                                field.dateExpire = 0;
                            }
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }

                        foreach (Chara c in alive)
                        {
                            try
                            {
                                if (field != null && (c.currentZone == null || c.currentZone.uid != field.uid))
                                {
                                    try
                                    {
                                        c.MoveZone(field, ZoneTransition.EnterState.RandomVisit);
                                    }
                                    catch
                                    {
                                        c.MoveZone(field);
                                    }
                                }
                            }
                            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
PrepareDispatchedChara(c);
                        }

                        m.currentFloorLv = 0;
                    }
                    else
                    {
                        Zone? root = m.GetZone();
                        if (root != null)
                        {
                            try { root = root.GetTopZone() ?? root; } catch { }
                            Zone? floor = DungeonDispatchTargets.ResolveFloorForProgress(root, m.ProgressPercent) ?? root;
                            int flv = SafeLv(floor);
                            m.currentFloorLv = flv;
                            if (FloorDepthAbs(SafeLv(root), flv) > FloorDepthAbs(SafeLv(root), m.deepestFloorLv))
                            {
                                m.deepestFloorLv = flv;
                            }

                            foreach (Chara c in alive)
                            {
                                try
                                {
                                    bool needMove = c.currentZone == null;
                                    if (!needMove && floor != null && c.currentZone != null
                                        && c.currentZone.uid != floor.uid)
                                    {
                                        // Only remount when target floor already exists and differs.
                                        needMove = true;
                                    }

                                    if (needMove && floor != null)
                                    {
                                        try
                                        {
                                            c.MoveZone(floor, ZoneTransition.EnterState.RandomVisit);
                                        }
                                        catch
                                        {
                                            c.MoveZone(floor);
                                        }
                                    }
                                }
                                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
PrepareDispatchedChara(c);
                            }
                        }
                        else
                        {
                            foreach (Chara c in alive)
                            {
                                PrepareDispatchedChara(c);
                            }
                        }
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
// Spend the hour first so harvest gate / progress see the new clock.
                m.hoursLeft--;

                // Partial loot log while exploring (only after travelHours).
                try
                {
                    FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch ?? alive[0].homeBranch;
                    DungeonDispatchRewards.TickPartialLoot(branch, m);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
if (m.hoursLeft <= 0)
                {
                    bool success = RollSuccess(m);
                    Settle(
                        m,
                        success ? DispatchSettleKind.Success : DispatchSettleKind.Failure,
                        destroyRandom: success && m.isRandomSite);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("dispatch hour tick failed: " + ex.Message);
            }
        }

        // Keep lastSeenWorldRaw fresh even if the player has not Game.Save'd yet.
        if (Missions.Count > 0)
        {
            try { Save(); } catch { }
        }
    }

    static bool RollSuccess(DungeonDispatchMission m)
    {
        if (m == null)
        {
            return false;
        }

        // Region outing has no combat success gate.
        if (m.isRegion)
        {
            return true;
        }

        try
        {
            int roll = EClass.rnd(100);
            return roll < Mathf.Clamp(m.successChance, 15, 100);
        }
        catch
        {
            return m.successChance >= 50;
        }
    }

    static void Settle(DungeonDispatchMission m, DispatchSettleKind kind, bool destroyRandom)
    {
        if (m == null)
        {
            return;
        }

        m.EnsureMemberList();
        var members = m.GetMembers();

        FactionBranch? branch = null;
        try
        {
            if (members.Count > 0)
            {
                branch = members[0].homeBranch;
            }

            branch ??= EClass.BranchOrHomeBranch ?? EClass.Branch;
        }
        catch
        {
            branch = EClass.BranchOrHomeBranch ?? EClass.Branch;
        }

        Zone? home = m.GetHomeZone();

        // Sync tracker to current harvest gate, then deliver (mail qty == details).
        try
        {
            if (m.isRegion)
            {
                DungeonDispatchRewards.TickPartialLoot(branch, m);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
// Deliver first so lootLog is rewritten from real stacks while mission still exists.
        try
        {
            if (branch != null)
            {
                DungeonDispatchRewards.Deliver(branch, m, kind);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch deliver: " + ex.Message);
        }

        // Then drop mission + tracker pin (force untrack + WidgetQuestTracker.Refresh).
        Missions.Remove(m);
        RemoveTrackerQuest(m);
        RefreshRegionMapMarkers();

        try
        {
            DungeonDispatchRewards.GrantExp(members, kind, m);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            DungeonDispatchRewards.GrantFameOnDungeonSuccess(m, kind);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
// Homecoming for all members.
        foreach (Chara c in members)
        {
            try
            {
                ClearDispatchedFlags(c);
                if (home != null)
                {
                    c.MoveZone(home, ZoneTransition.EnterState.Return);
                }
                else if (c.homeZone != null)
                {
                    c.MoveZone(c.homeZone);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("dispatch home MoveZone: " + ex.Message);
            }
        }

        if (destroyRandom && m.isRandomSite)
        {
            try
            {
                Zone? z = m.GetZone();
                if (z != null)
                {
                    try
                    {
                        z = z.GetTopZone() ?? z;
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

                if (z != null && z.isRandomSite && !z.IsPCFaction && !z.destryoed)
                {
                    if (EClass._zone != null && (EClass._zone.uid == z.uid
                        || ((EClass._zone.GetTopZone()?.uid ?? -1) == z.uid)))
                    {
                        z.dateExpire = 1;
                        Plugin.LogInfo("dispatch expire active random site uid=" + z.uid);
                    }
                    else
                    {
                        z.Destroy();
                        Plugin.LogInfo("dispatch destroy random site uid=" + z.uid);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("dispatch destroy site: " + ex.Message);
            }
        }

        string outcome = kind switch
        {
            DispatchSettleKind.Success => LaborText.T("dispatch.settle.success"),
            DispatchSettleKind.Failure => LaborText.T("dispatch.settle.fail"),
            DispatchSettleKind.Recall => LaborText.T("dispatch.settle.recall"),
            _ => LaborText.T("dispatch.settle.end"),
        };
        string who = m.MemberNames();
        string loot = m.LootSummary(8);
        string feature = m.isRegion ? LaborTerms.RegionDispatch : LaborTerms.DungeonExplore;
        string msg = LaborText.T("dispatch.settle.line", who, feature, m.zoneName, outcome);
        if (!string.IsNullOrEmpty(loot) && loot != NpcLabor.LaborText.T("dis.loot.none"))
        {
            msg += " " + loot;
        }

        // Failure educational beat: what success might have added (boss/fame or fuller haul).
        // Fixed-dungeon reward math stays unchanged; this is flavor only.
        if (kind == DispatchSettleKind.Failure)
        {
            string hint = m.isRegion
                ? LaborText.T("dispatch.fail.hint.region")
                : LaborText.T("dispatch.fail.hint.dungeon");
            if (!string.IsNullOrEmpty(hint))
            {
                msg += " " + hint;
            }
        }
        try
        {
            Msg.Say(msg);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
try
        {
            branch?.LogRaw(msg);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
Plugin.LogInfo("dispatch settle kind=" + kind + " mission=" + m.missionId + " zone=" + m.uidZone
            + " loot=" + loot);
    }

    internal static void OnWorkerDied(Chara c)
    {
        if (c == null)
        {
            return;
        }

        DungeonDispatchMission? m = FindByChara(c.uid);
        if (m == null)
        {
            return;
        }

        m.EnsureMemberList();
        m.uidMembers.Remove(c.uid);
        if (m.uidMembers.Count == 0)
        {
            Missions.Remove(m);
            RemoveTrackerQuest(m);
            Plugin.LogInfo("dispatch cleared on death (empty) uid=" + c.uid);
        }
        else
        {
            m.uidChara = m.uidMembers[0];
            Plugin.LogInfo("dispatch member death uid=" + c.uid + " remaining=" + m.uidMembers.Count);
        }
    }

    internal static void OnZoneEntered(Zone zone)
    {
        if (zone == null || Missions.Count == 0)
        {
            return;
        }

        try
        {
            DungeonDispatchMission? m = FindByZone(zone.uid);
            if (m == null)
            {
                // Region fallback by tile coords.
                for (int i = 0; i < Missions.Count; i++)
                {
                    DungeonDispatchMission cand = Missions[i];
                    if (!cand.isRegion)
                    {
                        continue;
                    }

                    try
                    {
                        if (zone.x == cand.regionGx && zone.y == cand.regionGy)
                        {
                            m = cand;
                            break;
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}
            }

            if (m == null)
            {
                return;
            }

            m.EnsureMemberList();

            Zone? stake = zone;
            if (m.isRegion)
            {
                // Ensure mission points at this field and members stand here with PC.
                m.uidZone = zone.uid;
                try
                {
                    if (!string.IsNullOrEmpty(zone.id))
                    {
                        m.zoneId = zone.id;
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }

                // This is the live stakeout field: keep it non-destructible for the mission.
                try
                {
                    zone.dateExpire = 0;
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
m.currentFloorLv = 0;
            }
            else
            {
                try
                {
                    Zone root = m.GetZone()?.GetTopZone() ?? m.GetZone() ?? zone;
                    stake = DungeonDispatchTargets.ResolveFloorForProgress(root, m.ProgressPercent) ?? zone;
                    m.currentFloorLv = SafeLv(stake);
                }
                catch
                {
                    stake = zone;
                    m.currentFloorLv = SafeLv(zone);
                }
            }

            foreach (Chara c in m.GetMembers())
            {
                try
                {
                    if (stake != null && (c.currentZone == null || c.currentZone.uid != stake.uid))
                    {
                        try
                        {
                            c.MoveZone(stake, ZoneTransition.EnterState.RandomVisit);
                        }
                        catch
                        {
                            try { c.MoveZone(stake); } catch { }
                        }
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
PrepareDispatchedChara(c);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch OnZoneEntered: " + ex.Message);
        }
    }

    internal static string MissionTrackerText(DungeonDispatchMission m)
    {
        m.EnsureMemberList();
        string where;
        if (m.isRegion)
        {
            where = string.IsNullOrEmpty(m.regionKind)
                ? (m.zoneName ?? NpcLabor.LaborText.T("dis.q.area"))
                : DungeonDispatchTargets.RegionDisplayName(m.regionKind);
        }
        else
        {
            where = FormatFloorLabel(m.currentFloorLv);
        }

        string text = m.zoneName
            + "\n" + m.MemberNames()
            + "\n" + NpcLabor.LaborText.T("dis.q.progress", m.ProgressPercent)
            + "  " + NpcLabor.LaborText.T("dis.q.left", m.DaysLeft.ToString("0.0")) + " · " + where;
        // Region outing intentionally omits live haul preview.
        if (!m.isRegion)
        {
            text += "\n" + NpcLabor.LaborText.T("dis.q.harvest", m.LootSummary(8));
        }

        return text;
    }

    internal static string FormatFloorLabel(int lv)
    {
        if (lv == 0)
        {
            return NpcLabor.LaborText.T("dis.q.entrance");
        }

        if (lv < 0)
        {
            return "B" + (-lv);
        }

        return "F" + lv;
    }


    static int CurrentWorldRaw()
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
    /// True when world time moved past the last mod-side save — typically the player
    /// ran the save without this plugin. Normal same-mod load keeps lastSeen in sync.
    /// Legacy files with lastSeen 0 resume missions (no forced recall).
    /// </summary>
    static bool ShouldAutoRecallAfterModGap(int lastSeenWorldRaw)
    {
        int now = CurrentWorldRaw();
        if (lastSeenWorldRaw <= 0 || now <= 0)
        {
            return false;
        }

        return now > lastSeenWorldRaw;
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

            return Path.Combine(root, "npclabor_dispatch.json");
        }
        catch
        {
            return null;
        }
    }

    internal static void Save()
    {
        try
        {
            string? path = SavePath();
            if (path == null)
            {
                return;
            }

            foreach (DungeonDispatchMission m in Missions)
            {
                m.EnsureMemberList();
                if (m.missionId <= 0)
                {
                    m.missionId = _nextMissionId++;
                }

                if (m.uidMembers.Count > 0)
                {
                    m.uidChara = m.uidMembers[0];
                }
            }

            var data = new DungeonDispatchSaveData
            {
                missions = Missions.ToList(),
                lastPresets = LastPresets.ToList(),
                lastSeenWorldRaw = CurrentWorldRaw(),
            };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            Plugin.LogDebug("dispatch saved " + Missions.Count + " -> " + path);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch save failed: " + ex.Message);
        }
    }

    internal static void Load()
    {
        Missions.Clear();
        LastPresets.Clear();
        _nextMissionId = 1;
        try
        {
            string? path = SavePath();
            if (path == null || !File.Exists(path))
            {
                Plugin.LogDebug("dispatch load: no file");
                return;
            }

            string json = File.ReadAllText(path);
            DungeonDispatchSaveData? data = JsonConvert.DeserializeObject<DungeonDispatchSaveData>(json);
            if (data?.missions == null && (data?.lastPresets == null || data.lastPresets.Count == 0))
            {
                return;
            }

            if (data?.lastPresets != null)
            {
                for (int i = 0; i < data.lastPresets.Count; i++)
                {
                    DungeonDispatchLastPreset p = data.lastPresets[i];
                    if (p == null || string.IsNullOrEmpty(p.key) || p.uidMembers == null || p.uidMembers.Count == 0)
                    {
                        continue;
                    }

                    if (p.uidMembers == null)
                    {
                        p.uidMembers = new List<int>();
                    }

                    LastPresets.Add(p);
                }
            }

            if (data?.missions == null)
            {
                return;
            }

            foreach (DungeonDispatchMission m in data.missions)
            {
                if (m == null)
                {
                    continue;
                }

                m.EnsureMemberList();
                if (m.MemberCount <= 0)
                {
                    continue;
                }

                if (m.missionId <= 0)
                {
                    m.missionId = _nextMissionId++;
                }
                else if (m.missionId >= _nextMissionId)
                {
                    _nextMissionId = m.missionId + 1;
                }

                Missions.Add(m);
            }

            foreach (DungeonDispatchMission m in Missions.ToList())
            {
                m.EnsureMemberList();
                var alive = new List<int>();
                foreach (int uid in m.uidMembers)
                {
                    Chara? c = null;
                    try
                    {
                        c = RefChara.Get(uid);
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
if (c == null || c.isDead)
                    {
                        continue;
                    }

                    alive.Add(uid);
                }

                m.uidMembers = alive;
                if (m.uidMembers.Count == 0)
                {
                    Missions.Remove(m);
                    continue;
                }

                m.uidChara = m.uidMembers[0];

                if (m.isRegion)
                {
                    Zone? field = m.GetZone();
                    if (field == null || field.destryoed)
                    {
                        field = DungeonDispatchTargets.EnsureRegionFieldZone(m.regionGx, m.regionGy);
                        if (field != null)
                        {
                            m.uidZone = field.uid;
                        }
                    }

                    foreach (Chara c in m.GetMembers())
                    {
                        try
                        {
                            PrepareDispatchedChara(c);
                            if (field != null
                                && (c.currentZone == null
                                    || (c.homeZone != null && c.currentZone.uid == c.homeZone.uid)
                                    || c.currentZone.uid != field.uid))
                            {
                                try
                                {
                                    c.MoveZone(field, ZoneTransition.EnterState.RandomVisit);
                                }
                                catch
                                {
                                    try { c.MoveZone(field); } catch { }
                                }
                            }

                            PrepareDispatchedChara(c);
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

                    m.currentFloorLv = 0;
                    continue;
                }

                Zone? z = m.GetZone();
                if (z == null || z.destryoed)
                {
                    foreach (Chara c in m.GetMembers())
                    {
                        try
                        {
                            ClearDispatchedFlags(c);
                            Zone? home = m.GetHomeZone();
                            if (home != null)
                            {
                                c.MoveZone(home, ZoneTransition.EnterState.Return);
                            }
                        }
                        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}

                    Missions.Remove(m);
                    continue;
                }

                Zone? floor = DungeonDispatchTargets.ResolveFloorForProgress(z, m.ProgressPercent) ?? z;
                m.currentFloorLv = SafeLv(floor);
                foreach (Chara c in m.GetMembers())
                {
                    try
                    {
                        if (c.currentZone == null
                            || (floor != null && c.currentZone.uid != floor.uid
                                && (c.homeZone == null || c.currentZone.uid == c.homeZone.uid)))
                        {
                            if (floor != null)
                            {
                                c.MoveZone(floor, ZoneTransition.EnterState.RandomVisit);
                            }
                        }

                        PrepareDispatchedChara(c);
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}
            }

            Plugin.LogInfo("dispatch loaded missions=" + Missions.Count);

            int lastSeen = 0;
            try { lastSeen = data != null ? data.lastSeenWorldRaw : 0; } catch { lastSeen = 0; }
            if (ShouldAutoRecallAfterModGap(lastSeen) && Missions.Count > 0)
            {
                int n = Missions.Count;
                Plugin.LogWarn("dispatch reinstall heal: world advanced without mod save (lastSeen="
                    + lastSeen + " now=" + CurrentWorldRaw() + "); auto-recalling " + n + " mission(s)");
                foreach (DungeonDispatchMission m in Missions.ToList())
                {
                    try
                    {
                        if (m != null && Missions.Contains(m))
                        {
                            Settle(m, DispatchSettleKind.Recall, destroyRandom: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogWarn("dispatch reinstall recall: " + ex.Message);
                    }
                }

                try { Save(); } catch { }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch load failed: " + ex.Message);
        }

        try
        {
            // One typed pin per mission via quests.list only. Widget paints itself.
            RefreshTrackerQuests();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch load tracker: " + ex.Message);
        }

        try
        {
            SanitizeRegionMapLights();
            RefreshRegionMapMarkers();
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
}



    internal static bool IsOurDispatchTrackerQuest(Quest? q)
    {
        if (q == null)
        {
            return false;
        }

        if (q is QuestNpcLaborDispatch)
        {
            return true;
        }

        try
        {
            string id = q.id ?? string.Empty;
            if (id.StartsWith("npclabor_dispatch_", StringComparison.Ordinal))
            {
                return true;
            }
        }
        catch { }

        // Legacy Dummy rows may lose idSource / type but keep title crumbs — still strip by type name.
        try
        {
            string tn = q.GetType().Name ?? string.Empty;
            if (tn.IndexOf("NpcLaborDispatch", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        catch { }

        return false;
    }

    /// <summary>
    /// F9/auto-save: keep live typed pins so tracker order stays stable. Only drop
    /// Dummy / missionId=0 / orphan / duplicate rows that would double on next load.
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

            var seen = new HashSet<int>();
            foreach (Quest q in qm.list.ToList())
            {
                if (!IsOurDispatchTrackerQuest(q))
                {
                    continue;
                }

                int mid = 0;
                bool typed = false;
                try
                {
                    if (q is QuestNpcLaborDispatch dq)
                    {
                        typed = true;
                        mid = dq.missionId;
                    }
                    else
                    {
                        string id = q.id ?? string.Empty;
                        const string prefix = "npclabor_dispatch_";
                        if (id.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            int.TryParse(id.Substring(prefix.Length), out mid);
                        }
                    }
                }
                catch { mid = 0; }

                bool orphan = mid <= 0 || FindByMissionId(mid) == null;
                bool dup = mid > 0 && !seen.Add(mid);
                // Always drop non-typed (QuestDummy / legacy) — typed live rows stay.
                bool drop = !typed || orphan || dup;
                if (!drop)
                {
                    try
                    {
                        if (q is QuestNpcLaborDispatch live)
                        {
                            live.track = true;
                            live.deadline = 0;
                            live.EnsureSafePerson();
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
            Plugin.LogDebug("dispatch sanitize broken trackers: " + ex.Message);
        }
    }

    /// <summary>
    /// Ask vanilla WidgetQuestTracker to repaint from quests.list.
    /// Never DestroyImmediate rows here — that caused SetActive NREs.
    /// </summary>
    internal static void RequestQuestTrackerRefresh()
    {
        try
        {
            if (WidgetQuestTracker.Instance != null)
            {
                WidgetQuestTracker.Instance.Refresh();
                return;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch tracker refresh instance: " + ex.Message);
        }

        try
        {
            var w = EClass.ui?.widgets?.GetWidget("QuestTracker") as WidgetQuestTracker;
            w?.Refresh();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch tracker refresh widget: " + ex.Message);
        }
    }


    static void TryStartTrackerQuest(DungeonDispatchMission mission)
    {
        if (mission == null || mission.missionId <= 0)
        {
            return;
        }

        try
        {
            // Guard: never Start if a live pin already exists for this mission.
            // Do not Remove+Start (that races the widget and can leave two rows).
            try
            {
                QuestManager? qm = EClass.game?.quests;
                if (qm?.list != null)
                {
                    foreach (Quest existing in qm.list)
                    {
                        if (existing is QuestNpcLaborDispatch dq && dq.missionId == mission.missionId)
                        {
                            try
                            {
                                dq.track = true;
                                dq.deadline = 0;
                                dq.isNew = false;
                                dq.EnsureSafePerson();
                            }
                            catch { }
                            // Same Quest object already in list — widget row already bound by ref.
                            // Do not Show/Refresh here; that only races concurrent Refresh loops.
                            return;
                        }
                    }

                    // Drop Dummy/orphan rows for this id only (list-level, no widget destroy).
                    string wantId = "npclabor_dispatch_" + mission.missionId;
                    foreach (Quest orphan in qm.list.ToList())
                    {
                        if (orphan is QuestNpcLaborDispatch)
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

            var q = new QuestNpcLaborDispatch
            {
                missionId = mission.missionId,
                id = "npclabor_dispatch_" + mission.missionId,
            };

            // Assign uid + defaults like vanilla Quest.Create path.
            try
            {
                q.Init();
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
            // deadline 0 = never expires (GameDate raw timestamp otherwise).
            // Setting hoursLeft here made multi-day dispatches "expire" in a few hours,
            // calling Quest.Fail → fame loss and recalling members out of the dungeon.
            q.deadline = 0;
            q.track = true;
            // isNew false: we are a synthetic pin, not a freshly accepted journal quest.
            q.isNew = false;

            // Quest.chara / QuestManager.OnShowDialog dereference person — must be non-null
            // and must NOT bind a client NPC (would steal dialogs / NRE on null person).
            try
            {
                q.SetClient(null, assignQuest: false);
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
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
            // List-only safety net in case Start raced another pin for the same id.
            // Do not call WidgetQuestTracker.Refresh here — vanilla Start already Show()s.
            DedupeNpcLaborTrackerQuests();
            Plugin.LogDebug("dispatch tracker quest start id=" + mission.missionId);
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch tracker quest failed: " + ex.Message);
        }
    }

    static void RemoveTrackerQuest(DungeonDispatchMission? mission)
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

            string wantId = "npclabor_dispatch_" + mission.missionId;
            foreach (Quest q in qm.list.ToList())
            {
                bool match = false;
                try
                {
                    if (q is QuestNpcLaborDispatch dq && dq.missionId == mission.missionId)
                    {
                        match = true;
                    }
                    else if (q != null && string.Equals(q.id, wantId, StringComparison.Ordinal))
                    {
                        // Legacy / QuestDummy rows that lost the concrete type or missionId.
                        match = true;
                    }
                }
                catch { }

                if (!match)
                {
                    continue;
                }

                try
                {
                    // Force pin off before remove so WidgetQuestTracker.ItemQuestTracker.Kill runs.
                    if (q != null)
                    {
                        q.track = false;
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                try
                {
                    if (q != null)
                    {
                        qm.Remove(q);
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
            }

            // Vanilla ItemQuestTracker.Refresh kills rows whose quest left the list.
            RequestQuestTrackerRefresh();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch tracker remove: " + ex.Message);
        }
    }

    /// <summary>
    /// QuestManager.Remove does not refresh WidgetQuestTracker. Force pin rows to drop
    /// when a dispatch quest ends (otherwise player must click the X manually).
    /// </summary>
    static void RefreshQuestTrackerWidget() => RequestQuestTrackerRefresh();

    /// <summary>
    /// Drop extra live quests.list entries that share the same NpcLabor missionId
    /// (typed or Dummy with our id prefix). Keeps one row per mission.
    /// List-only — never DestroyImmediate widget rows.
    /// </summary>
    internal static void DedupeNpcLaborTrackerQuests()
    {
        try
        {
            QuestManager? qm = EClass.game?.quests;
            if (qm?.list == null)
            {
                return;
            }

            var seenDispatch = new HashSet<int>();
            var seenTown = new HashSet<int>();

            foreach (Quest q in qm.list.ToList())
            {
                if (q == null)
                {
                    continue;
                }

                bool isDispatch = IsOurDispatchTrackerQuest(q);
                bool isTown = false;
                try { isTown = TownLaborManager.IsOurTownTrackerQuestPublic(q); } catch { isTown = false; }
                if (!isDispatch && !isTown)
                {
                    continue;
                }

                int mid = 0;
                bool typed = false;
                try
                {
                    if (q is QuestNpcLaborDispatch dq)
                    {
                        typed = true;
                        mid = dq.missionId;
                    }
                    else if (q is QuestNpcLaborTownLabor tq)
                    {
                        typed = true;
                        mid = tq.missionId;
                    }
                    else
                    {
                        string id = q.id ?? string.Empty;
                        const string dPrefix = "npclabor_dispatch_";
                        const string tPrefix = "npclabor_townlabor_";
                        if (id.StartsWith(dPrefix, StringComparison.Ordinal))
                        {
                            int.TryParse(id.Substring(dPrefix.Length), out mid);
                        }
                        else if (id.StartsWith(tPrefix, StringComparison.Ordinal))
                        {
                            int.TryParse(id.Substring(tPrefix.Length), out mid);
                        }
                    }
                }
                catch { mid = 0; }

                bool orphan;
                bool dup;
                if (isDispatch)
                {
                    orphan = mid <= 0 || FindByMissionId(mid) == null;
                    dup = mid > 0 && !seenDispatch.Add(mid);
                }
                else
                {
                    orphan = mid <= 0 || TownLaborManager.FindByMissionId(mid) == null;
                    dup = mid > 0 && !seenTown.Add(mid);
                }

                // Prefer typed live rows. Non-typed / orphan / dup always drop.
                bool drop = !typed || orphan || dup;
                if (!drop)
                {
                    try
                    {
                        if (q is QuestNpcLaborDispatch liveD)
                        {
                            liveD.track = true;
                            liveD.deadline = 0;
                            liveD.EnsureSafePerson();
                        }
                        else if (q is QuestNpcLaborTownLabor liveT)
                        {
                            liveT.track = true;
                            liveT.deadline = 0;
                            liveT.EnsureSafePerson();
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
            Plugin.LogDebug("dispatch dedupe trackers: " + ex.Message);
        }
    }


internal static void RefreshTrackerQuests()
    {
        try
        {
            // First pass: drop orphan / legacy / duplicate pins so we never keep two rows.
            try
            {
                QuestManager? qm = EClass.game?.quests;
                if (qm?.list != null)
                {
                    var seen = new HashSet<int>();
                    bool removed = false;
                    foreach (Quest q in qm.list.ToList())
                    {
                        if (!IsOurDispatchTrackerQuest(q))
                        {
                            continue;
                        }

                        int mid = 0;
                        try
                        {
                            if (q is QuestNpcLaborDispatch dq)
                            {
                                mid = dq.missionId;
                            }
                            else
                            {
                                string id = q.id ?? string.Empty;
                                const string prefix = "npclabor_dispatch_";
                                if (id.StartsWith(prefix, StringComparison.Ordinal))
                                {
                                    int.TryParse(id.Substring(prefix.Length), out mid);
                                }
                            }
                        }
                        catch { mid = 0; }

                        bool typed = q is QuestNpcLaborDispatch;
                        bool orphan = mid <= 0 || FindByMissionId(mid) == null;
                        bool dup = mid > 0 && !seen.Add(mid);
                        // Keep only one live typed pin; Dummy/legacy always drop and re-Start.
                        if (typed && !orphan && !dup)
                        {
                            continue;
                        }

                        try { q.track = false; } catch { }
                        try { qm.Remove(q); } catch { }
                        removed = true;
                    }

                    if (removed)
                    {
                        // Defer paint; Start/Show below will refresh once.
                    }
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }

            foreach (DungeonDispatchMission m in Missions)
            {
                bool has = false;
                try
                {
                    QuestManager? qm = EClass.game?.quests;
                    if (qm?.list != null)
                    {
                        foreach (Quest q in qm.list)
                        {
                            if (q is QuestNpcLaborDispatch dq && dq.missionId == m.missionId)
                            {
                                has = true;
                                try
                                {
                                    dq.track = true;
                                    dq.deadline = 0;
                                    dq.EnsureSafePerson();
                                }
                                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                                break;
                            }
                        }
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                if (!has)
                {
                    TryStartTrackerQuest(m);
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    /// <summary>
    /// Debug: dump planned full success harvest for every active mission.
    /// Region missions ensure plan first; non-region notes explore-scale only.
    /// </summary>
    internal static string DebugDumpSuccessHarvests()
    {
        try
        {
            if (Missions.Count == 0)
            {
                string empty = NpcLabor.LaborText.T(
                    "dis.msg.empty",
                    NpcLabor.LaborTerms.DungeonExplore,
                    NpcLabor.LaborTerms.RegionDispatch);
                try { Msg.Say(empty); } catch { }
                Plugin.LogInfo("dispatch debug dump: no missions");
                return empty;
            }

            var lines = new List<string>();
            for (int i = 0; i < Missions.Count; i++)
            {
                DungeonDispatchMission m = Missions[i];
                if (m == null)
                {
                    continue;
                }

                m.EnsureMemberList();
                string head = "#" + m.missionId + " " + (m.zoneName ?? "?")
                    + " [" + m.MemberNames() + "]"
                    + " progress=" + m.ProgressPercent + "%"
                    + " weeks=" + Math.Max(1, m.exploreWeeks)
                    + " skill=E" + m.exploreSkill + "/G" + m.gatherSkill + "/L" + m.lockpickSkill
                    + " haul=" + DungeonDispatchRewards.PlannedHaulTotal(m)
                    + " (ex" + DungeonDispatchRewards.PlannedExploreHaulTotal(m)
                    + "+fd" + DungeonDispatchRewards.PlannedGatherHaulTotal(m)
                    + "+lk" + DungeonDispatchRewards.PlannedLockpickChestCount(m) + ")"
                    + " x" + (m.haulVariance > 0.01f ? m.haulVariance.ToString("0.00") : "1.00");

                string haul;
                if (m.isRegion)
                {
                    haul = DungeonDispatchRewards.FormatPlannedSuccessHarvest(m, 16);
                }
                else
                {
                    haul = NpcLabor.LaborText.T("dis.debug.nonRegion")
                        + m.LootSummary(8);
                }

                string line = head + " => " + haul;
                lines.Add(line);
                Plugin.LogInfo("dispatch debug harvest " + line);
            }

            string msg = NpcLabor.LaborText.T("dis.debug.harvestDump", lines.Count) + string.Join(" | ", lines);
            try { Msg.Say(msg); } catch { }
            return msg;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch debug dump failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Debug: force-complete all active missions as Success and deliver mail.
    /// </summary>
    internal static int DebugCompleteAllSuccess()
    {
        int n = 0;
        try
        {
            // Copy list because Settle removes from Missions.
            var list = Missions.ToList();
            if (list.Count == 0)
            {
                try { Msg.Say(NpcLabor.LaborText.T("dis.debug.noMission")); } catch { }
                return 0;
            }

            for (int i = 0; i < list.Count; i++)
            {
                DungeonDispatchMission m = list[i];
                if (m == null)
                {
                    continue;
                }

                try
                {
                    // Ensure region plan exists so success mail uses locked haul.
                    if (m.isRegion)
                    {
                        DungeonDispatchRewards.EnsureRegionPlanPublic(m);
                    }

                    Settle(m, DispatchSettleKind.Success, destroyRandom: m.isRandomSite);
                    n++;
                }
                catch (Exception ex)
                {
                    Plugin.LogWarn("dispatch debug complete mission " + m.missionId + ": " + ex.Message);
                }
            }

            string msg = NpcLabor.LaborText.T("dis.debug.forceDone", n);
            try { Msg.Say(msg); } catch { }
            Plugin.LogInfo("dispatch debug complete-all n=" + n);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch debug complete-all failed: " + ex.Message);
        }

        return n;
    }

    /// <summary>Debug: force-complete one mission by id or member uid.</summary>
    internal static bool DebugCompleteOne(int missionIdOrCharaUid)
    {
        try
        {
            DungeonDispatchMission? m = FindByMissionId(missionIdOrCharaUid);
            if (m == null)
            {
                m = FindByChara(missionIdOrCharaUid);
            }

            if (m == null)
            {
                try { Msg.Say(NpcLabor.LaborText.T("dis.debug.notFound", missionIdOrCharaUid)); } catch { }
                return false;
            }

            if (m.isRegion)
            {
                DungeonDispatchRewards.EnsureRegionPlanPublic(m);
            }

            Settle(m, DispatchSettleKind.Success, destroyRandom: m.isRandomSite);
            try { Msg.Say(NpcLabor.LaborText.T("dis.debug.forceDoneOne", m.missionId, m.zoneName)); } catch { }
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch debug complete-one failed: " + ex.Message);
            return false;
        }
    }

    internal static void ClearAllRuntime()
    {
        Missions.Clear();
        try { ClearRegionMapMarkers(); } catch { }
    }

    /// <summary>
    /// Pure visual overworld pins for active region dispatches.
    /// Uses the 旅行商人营地 (tinkerCamp) zone icon tile so the outing is easy to
    /// spot on EloMap. Never pass "iconFlag" as an AddLight prefab id — that is a
    /// zone tile tag and leaves null SpriteRenderers that NRE in OnChangeHour.
    /// Old elolight pins are still scrubbed/removed for save/session safety.
    /// </summary>
    internal static void RefreshRegionMapMarkers()
    {
        try
        {
            EloMap? map = TryGetEloMap();
            if (map == null)
            {
                return;
            }

            // Restore previous pins, scrub broken lights, then re-pin as camp icons.
            ClearRegionMapMarkers();
            ScrubBrokenEloMapLights(map);

            foreach (DungeonDispatchMission m in Missions)
            {
                if (m == null || !m.isRegion)
                {
                    continue;
                }

                if (!TryAddRegionMarker(map, m.regionGx, m.regionGy))
                {
                    Plugin.LogDebug("region marker add failed @" + m.regionGx + "," + m.regionGy);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("region markers: " + ex.Message);
        }
    }

    /// <summary>
    /// Defensive repair for saves/sessions that already polluted actor.lights with null sr.
    /// Safe to call every hour before vanilla EloMapActor.OnChangeHour.
    /// </summary>
    internal static void SanitizeRegionMapLights()
    {
        try
        {
            EloMap? map = TryGetEloMap();
            if (map == null)
            {
                return;
            }

            ScrubBrokenEloMapLights(map);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    internal static void ClearRegionMapMarkers()
    {
        try
        {
            if (_pinnedRegionMarks == null)
            {
                _pinnedRegionMarks = new List<long>();
            }

            if (_pinnedRegionPrevObj == null)
            {
                _pinnedRegionPrevObj = new Dictionary<long, int>();
            }

            EloMap? map = TryGetEloMap();
            if (map == null)
            {
                _pinnedRegionMarks.Clear();
                _pinnedRegionPrevObj.Clear();
                return;
            }

            foreach (long key in _pinnedRegionMarks.ToList())
            {
                int gx = (int)(key >> 32);
                int gy = unchecked((int)(key & 0xffffffffL));
                try
                {
                    TryRestoreRegionMarker(map, gx, gy, key);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
            }

            _pinnedRegionMarks.Clear();
            _pinnedRegionPrevObj.Clear();
            ScrubBrokenEloMapLights(map);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    static List<long> _pinnedRegionMarks = new List<long>();

    /// <summary>Previous EloMap cell.obj under each active region pin (restore on clear).</summary>
    static Dictionary<long, int> _pinnedRegionPrevObj = new Dictionary<long, int>();

    /// <summary>Cached tinkerCamp SourceZone icon id; 0 means unresolved.</summary>
    static int _regionDispatchIcon;

    static EloMap? TryGetEloMap()
    {
        try
        {
            return EClass.world?.region?.elomap ?? EClass.scene?.elomap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Icon tile for region-dispatch pins = 旅行商人营地 (tinkerCamp) art.
    /// Runtime-resolved from SourceZone.pos[2]; fallback 334.
    /// </summary>
    static int GetRegionDispatchIcon()
    {
        if (_regionDispatchIcon > 0)
        {
            return _regionDispatchIcon;
        }

        try
        {
            // Elin SourceDataString.map.TryGetValue(id) returns the row (not Dictionary out-bool).
            SourceZone.Row? row = null;
            try
            {
                row = EClass.sources?.zones?.map?.TryGetValue("tinkerCamp");
            }
            catch
            {
                row = null;
            }

            if (row == null)
            {
                try
                {
                    if (EClass.sources?.zones?.map != null
                        && EClass.sources.zones.map.ContainsKey("tinkerCamp"))
                    {
                        row = EClass.sources.zones.map["tinkerCamp"];
                    }
                }
                catch
                {
                    row = null;
                }
            }

            if (row?.pos != null && row.pos.Length >= 3 && row.pos[2] > 0)
            {
                _regionDispatchIcon = row.pos[2];
                return _regionDispatchIcon;
            }
        }
        catch (System.Exception __e)
        {
            Plugin.LogDebug("region icon resolve: " + __e.Message);
        }

        _regionDispatchIcon = 334;
        return _regionDispatchIcon;
    }

    static long RegionPinKey(int gx, int gy)
    {
        return ((long)gx << 32) | (uint)gy;
    }

    static bool TryAddRegionMarker(EloMap map, int gx, int gy)
    {
        if (map == null)
        {
            return false;
        }

        int icon = GetRegionDispatchIcon();
        if (icon <= 0)
        {
            return false;
        }

        EloMap.Cell? cell = null;
        try
        {
            cell = map.GetCell(gx, gy);
        }
        catch
        {
            cell = null;
        }

        if (cell == null)
        {
            return false;
        }

        // Drop any legacy light pin on this tile (pre-icon builds).
        try
        {
            TryRemoveOurLight(map, gx, gy);
        }
        catch
        {
        }

        long key = RegionPinKey(gx, gy);
        int currentObj = 0;
        try { currentObj = cell.obj; } catch { currentObj = 0; }

        // Already showing the camp pin — just track it.
        if (currentObj == icon)
        {
            if (!_pinnedRegionPrevObj.ContainsKey(key))
            {
                // Unknown prior tile; treat as empty so clear erases the pin.
                _pinnedRegionPrevObj[key] = 0;
            }

            RememberRegionPin(gx, gy);
            return true;
        }

        Zone? zone = null;
        try
        {
            zone = cell.zone;
        }
        catch
        {
            zone = null;
        }

        // Do not stomp real town/dungeon icons. Field (and empty) tiles are fair game.
        if (!CanRestyleRegionCell(zone, currentObj, icon))
        {
            Plugin.LogDebug("region marker skip real zone @" + gx + "," + gy
                + " obj=" + currentObj
                + (zone != null ? (" id=" + (zone.id ?? "?")) : ""));
            return false;
        }

        if (!_pinnedRegionPrevObj.ContainsKey(key))
        {
            _pinnedRegionPrevObj[key] = currentObj;
        }

        // Prefer restyling a bound field zone so EloMap.SetZone stays consistent.
        try
        {
            if (zone != null && IsFieldLikeZone(zone))
            {
                try { zone.icon = icon; } catch { }
                map.SetZone(gx, gy, zone, updateMesh: true);
                // SetZone no-ops when cell.obj already equals z.icon; force tile if needed.
                ForceRegionIconTile(map, cell, gx, gy, icon);
                RememberRegionPin(gx, gy);
                return true;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("region marker SetZone: " + ex.Message);
        }

        // Direct objmap overlay when no restylable field zone is bound.
        try
        {
            ForceRegionIconTile(map, cell, gx, gy, icon);
            RememberRegionPin(gx, gy);
            return true;
        }
        catch (Exception ex)
        {
            ScrubBrokenEloMapLights(map);
            Plugin.LogDebug("region marker tile: " + ex.Message);
            return false;
        }
    }

    static void TryRestoreRegionMarker(EloMap map, int gx, int gy, long key)
    {
        if (map == null)
        {
            return;
        }

        // Always drop legacy light pins at this coordinate.
        try { TryRemoveOurLight(map, gx, gy); } catch { }

        int prevObj = 0;
        bool hadPrev = false;
        if (_pinnedRegionPrevObj != null && _pinnedRegionPrevObj.TryGetValue(key, out int stored))
        {
            prevObj = stored;
            hadPrev = true;
        }

        EloMap.Cell? cell = null;
        try { cell = map.GetCell(gx, gy); } catch { cell = null; }
        if (cell == null)
        {
            return;
        }

        int icon = GetRegionDispatchIcon();
        int currentObj = 0;
        try { currentObj = cell.obj; } catch { currentObj = 0; }

        // Only restore tiles we actually restyled (or that still show our pin).
        if (!hadPrev && currentObj != icon)
        {
            return;
        }

        Zone? zone = null;
        try { zone = cell.zone; } catch { zone = null; }

        if (zone != null && IsFieldLikeZone(zone))
        {
            try
            {
                // Put field icon back; 0 erases the overworld tile via SetZone.
                zone.icon = prevObj;
                map.SetZone(gx, gy, zone, updateMesh: true);
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("region marker restore SetZone: " + ex.Message);
            }
        }

        // Force tile back even if SetZone no-op'd (cell.obj already matched).
        try
        {
            if (prevObj <= 0)
            {
                cell.obj = 0;
                if (map.objmap != null)
                {
                    map.objmap.Erase(gx, gy);
                    map.objmap.UpdateMeshImmediate();
                }
            }
            else
            {
                ForceRegionIconTile(map, cell, gx, gy, prevObj);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("region marker restore tile: " + ex.Message);
        }
    }

    static void ForceRegionIconTile(EloMap map, EloMap.Cell cell, int gx, int gy, int icon)
    {
        if (map == null || cell == null)
        {
            return;
        }

        try { cell.obj = icon; } catch { }

        try
        {
            if (map.objmap == null)
            {
                return;
            }

            if (icon <= 0)
            {
                map.objmap.Erase(gx, gy);
            }
            else
            {
                map.objmap.SetTile(gx, gy, icon);
            }

            map.objmap.UpdateMeshImmediate();
        }
        catch
        {
        }
    }

    static bool CanRestyleRegionCell(Zone? zone, int currentObj, int pinIcon)
    {
        if (currentObj == 0 || currentObj == pinIcon)
        {
            return true;
        }

        if (zone == null)
        {
            // Bare tile with some decorative obj — still allow pin so the outing is visible.
            return true;
        }

        return IsFieldLikeZone(zone);
    }

    static bool IsFieldLikeZone(Zone zone)
    {
        if (zone == null)
        {
            return false;
        }

        try
        {
            if (zone is Region)
            {
                return false;
            }
        }
        catch
        {
        }

        string id = "";
        try { id = zone.id ?? ""; } catch { id = ""; }

        if (string.Equals(id, "field", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Transient region outing fields sometimes keep a synthetic id like "region:plain".
        if (id.StartsWith("region:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    static void TryRemoveOurLight(EloMap map, int gx, int gy)
    {
        if (map == null)
        {
            return;
        }

        EloMapActor? actor = null;
        try
        {
            actor = map.actor;
        }
        catch
        {
            return;
        }

        if (actor?.lights == null)
        {
            return;
        }

        try
        {
            for (int i = actor.lights.Count - 1; i >= 0; i--)
            {
                EloMapLight light = actor.lights[i];
                if (light == null)
                {
                    try { actor.lights.RemoveAt(i); } catch { }
                    continue;
                }

                if (light.gx != gx || light.gy != gy)
                {
                    continue;
                }

                // Destroy only entries we own at this pin. If sr is already null, just drop the slot.
                try
                {
                    if (light.sr != null && light.sr.gameObject != null)
                    {
                        UnityEngine.Object.DestroyImmediate(light.sr.gameObject);
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                try
                {
                    actor.lights.RemoveAt(i);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                // One pin per cell for our markers.
                break;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    static void ScrubBrokenEloMapLights(EloMap? map)
    {
        if (map == null)
        {
            return;
        }

        EloMapActor? actor = null;
        try
        {
            actor = map.actor;
        }
        catch
        {
            return;
        }

        if (actor?.lights == null || actor.lights.Count == 0)
        {
            return;
        }

        try
        {
            for (int i = actor.lights.Count - 1; i >= 0; i--)
            {
                EloMapLight? light = null;
                try
                {
                    light = actor.lights[i];
                }
                catch
                {
                    continue;
                }

                bool broken = light == null;
                if (!broken && light != null)
                {
                    try
                    {
                        broken = light.sr == null;
                    }
                    catch
                    {
                        broken = true;
                    }
                }

                if (!broken)
                {
                    continue;
                }

                try
                {
                    actor.lights.RemoveAt(i);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    static bool IsOurRegionPin(int gx, int gy)
    {
        if (_pinnedRegionMarks == null)
        {
            return false;
        }

        long key = RegionPinKey(gx, gy);
        return _pinnedRegionMarks.Contains(key);
    }

    static void RememberRegionPin(int gx, int gy)
    {
        if (_pinnedRegionMarks == null)
        {
            _pinnedRegionMarks = new List<long>();
        }

        long key = RegionPinKey(gx, gy);
        if (!_pinnedRegionMarks.Contains(key))
        {
            _pinnedRegionMarks.Add(key);
        }
    }
}
