using System;
using System.Collections.Generic;
using NpcLabor.Dispatch;
using UnityEngine;

namespace NpcLabor.TownLabor;

/// <summary>
/// Rewards for town shop labor. Does NOT call Quest.Complete.
/// Success: silent affinity, proportional cash (skill/shopLv), plat, furniture ticket chance,
/// shop invest only when skill > shopLv and under town invest cap,
/// themed leftover, hours-based skill exp.
/// Fail/recall: no main prize; recall may grant tiny exp.
/// Delivery: same-map Drop near PC; off-map home mailbox.
/// Player settle log stays lean: money / plat / ticket / leftover + soft skill/invest warns.
/// </summary>
internal static class TownLaborRewards
{
    // Back-compat aliases used by console/debug; live values come from LaborConfig.
    internal static int MinWagePerHour => LaborConfig.MinWagePerHour;
    internal static int ExpPerHour => LaborConfig.ExpPerHour;
    internal static int FurnitureTicketChancePercent => LaborConfig.FurnitureTicketChancePercent;

    internal static void Deliver(
        TownLaborMission mission,
        TownLaborSettleKind kind,
        Chara? worker,
        Chara? client)
    {
        if (mission == null)
        {
            return;
        }

        if (!mission.rewardPreviewLocked)
        {
            LockRewardPreview(mission);
        }

        if (mission.rewardLog == null)
        {
            mission.rewardLog = new List<string>();
        }
        else
        {
            mission.rewardLog.Clear();
        }

        bool success = kind == TownLaborSettleKind.Success;
        TownLaborJobDef? def = mission.Def;
        int hours = Math.Max(1, mission.hoursTotal > 0 ? mission.hoursTotal : 8);
        // Money / skill bar / invest-raise skill gate all use 店等级 = c_invest.
        // Vanilla Trait.ShopLv is only used inside town invest cap (CanRaiseShopInvestUnderTownCap).
        int investLv = Math.Max(0, mission.clientInvest);
        if (investLv <= 0 && client != null)
        {
            try { investLv = Math.Max(0, client.c_invest); } catch { investLv = 0; }
        }
        // Skill pay bar = invest tier (c_invest), min 1. Same scale as wages / invest raise.
        int shopLv = Math.Max(1, investLv);
        int workerSkill = mission.workerSkill;
        if (workerSkill <= 0 && worker != null && mission.skillId > 0)
        {
            workerSkill = TownLaborJobs.GetWorkerSkill(worker, mission.skillId);
        }

        int clientInvest = mission.clientInvest;
        if (clientInvest <= 0 && client != null)
        {
            try { clientInvest = Math.Max(0, client.c_invest); } catch { clientInvest = 0; }
        }

        var drops = new List<Thing>();

        // Affinity — silent.
        if (success && client != null && !client.isDead && EClass.pc != null)
        {
            int aff = LaborConfig.AffinityBase;
            try
            {
                client.ModAffinity(EClass.pc, aff, show: false);
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("townlabor affinity: " + ex.Message);
            }
        }

        // Shop invest: skill must beat 店等级 (c_invest), same bar as full pay; town cap still applies.
        // Soft reminder when skill is high enough but town investment cap blocks the raise.
        if (success && client != null && !client.isDead && workerSkill > investLv)
        {
            try
            {
                bool canInvestTrait = false;
                try
                {
                    if (client.trait is TraitCitizen cit)
                    {
                        canInvestTrait = cit.CanInvest;
                    }
                    else if (client.trait is TraitMerchant)
                    {
                        canInvestTrait = true;
                    }
                    else
                    {
                        // Some shop NPCs expose CanInvest without the exact type tests above.
                        try
                        {
                            var tr = client.trait;
                            if (tr != null)
                            {
                                var p = tr.GetType().GetProperty("CanInvest");
                                if (p != null && p.PropertyType == typeof(bool))
                                {
                                    canInvestTrait = (bool)(p.GetValue(tr) ?? false);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("TownLaborRewards.cs silent catch: " + __e.Message); }
                if (canInvestTrait)
                {
                    if (TownLaborJobs.CanRaiseShopInvestUnderTownCap(client))
                    {
                        client.c_invest = client.c_invest + 1;
                        // Keep plat curve aligned with post-invest value when we just raised it.
                        try { clientInvest = Math.Max(clientInvest, client.c_invest); } catch { }

                        string shop = "";
                        try
                        {
                            shop = mission.clientName
                                ?? client.NameSimple
                                ?? client.Name
                                ?? "";
                        }
                        catch
                        {
                            shop = mission.clientName ?? "";
                        }

                        if (string.IsNullOrWhiteSpace(shop))
                        {
                            shop = LaborText.T("town.msg.town");
                        }

                        string investMsg = LaborText.T("reward.investRaised", shop.Trim());
                        mission.rewardLog.Add(investMsg);
                        // Dedicated line so the raise is never buried under money/plat/ticket.
                        try { Msg.Say(investMsg); } catch { }
                    }
                    else
                    {
                        string capMsg = LaborText.T("reward.investTownCap");
                        mission.rewardLog.Add(capMsg);
                        try { Msg.Say(capMsg); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("townlabor invest: " + ex.Message);
            }
        }

        // Cash: locked at accept so the task description matches the payout.
        // factor = clamp(skill/shopLv, 0..1); 0 skill => 0 pay.
        if (success)
        {
            int money = Math.Max(0, mission.rewardMoney);
            if (money > 0)
            {
                Thing? coin = TryCreateMoney(money);
                if (coin != null)
                {
                    drops.Add(coin);
                    mission.rewardLog.Add(LaborText.T("reward.money", money));
                }
            }

            if (CalcPayFactor(workerSkill, shopLv) < 0.999f)
            {
                mission.rewardLog.Add(LaborText.T("reward.skillDiscount"));
            }
        }

        // Platinum (白金币) — locked at accept; scales with shop invest tier.
        if (success)
        {
            int platNum = Math.Max(0, mission.rewardPlat);
            Thing? plat = TryCreatePlat(platNum);
            if (plat != null)
            {
                drops.Add(plat);
                mission.rewardLog.Add(LaborText.T("reward.plat", platNum));
            }
        }

        // Local furniture ticket — roll locked at accept.
        if (success && mission.rewardTicketGranted)
        {
            Thing? ticket = TryCreateFurnitureTicket(mission, client);
            if (ticket != null)
            {
                drops.Add(ticket);
                mission.rewardLog.Add(LaborText.T("reward.furnitureTicket"));
            }
        }

        // Themed leftover / 打工奖励 (success only, optional flavor).
        if (success && def != null)
        {
            Thing? leftover = RollLeftover(def);
            if (leftover != null)
            {
                drops.Add(leftover);
                string name = SafeName(leftover);
                mission.rewardLog.Add(LaborText.T("reward.leftover", name));
            }
        }

        // Skill exp: companion full on settle; PC self already dripped half-rate hourly
        // so success settle skips bulk exp (avoid double). Recall still grants tiny consolation.
        if (worker != null && !worker.isDead && mission.skillId > 0)
        {
            int exp = 0;
            bool pcSelf = false;
            try { pcSelf = mission.isPcSelf; } catch { pcSelf = false; }

            if (success)
            {
                if (!pcSelf)
                {
                    exp = ExpPerHour * hours;
                }
                // PC self: already granted ExpPerHour/2 each worked hour.
            }
            else if (kind == TownLaborSettleKind.Recall)
            {
                exp = Math.Max(6, hours); // tiny consolation
            }

            if (exp > 0)
            {
                try
                {
                    worker.ModExp(mission.skillId, exp);
                }
                catch (Exception ex)
                {
                    Plugin.LogDebug("townlabor exp: " + ex.Message);
                }
            }
        }

        if (drops.Count == 0)
        {
            return;
        }

        DeliverThings(mission, drops);
    }

    /// <summary>
    /// Plat curve: min platBase; +platPerTier roughly every investTierSize shop invest.
    /// </summary>
    internal static int CalcRewardPlat(int clientInvest)
    {
        int invest = Math.Max(0, clientInvest);
        int basePlat = LaborConfig.PlatBase;
        int perTier = LaborConfig.PlatPerInvestTier;
        int tier = LaborConfig.InvestTierSize;
        return Math.Max(basePlat, basePlat + perTier * (invest / tier));
    }

    /// <summary>
    /// Pay scale vs shop bar. skill >= shop => 1; skill 0 => 0.
    /// </summary>
    /// <summary>
    /// Skill pay factor. Bar is invest tier (c_invest), min 1.
    /// Full pay when skill >= invest; 0 skill => 0 pay.
    /// </summary>
    internal static float CalcPayFactor(int workerSkill, int investLv)
    {
        int bar = Math.Max(1, investLv);
        int skill = Math.Max(0, workerSkill);
        if (skill >= bar)
        {
            return 1f;
        }

        return Mathf.Clamp01(skill / (float)bar);
    }

    /// <summary>
    /// Hourly wage before skill factor: (50 + invest).
    /// Matches player-facing formula (minWage + 投资等级).
    /// </summary>
    internal static int CalcFullWagePerHour(int investLv)
        => MinWagePerHour + Math.Max(0, investLv);

    /// <summary>Preview cash after skill factor (no variance).</summary>
    internal static int CalcPreviewMoney(int investLv, int workerSkill, int hours)
    {
        int full = CalcFullWagePerHour(investLv) * Math.Max(1, hours);
        return Mathf.RoundToInt(full * CalcPayFactor(workerSkill, investLv));
    }

    /// <summary>
    /// Lock deterministic reward pins (cash / plat / ticket) at accept so the task
    /// description can show the exact payout. Leftover stays a settle-time roll.
    /// </summary>
    internal static void LockRewardPreview(TownLaborMission mission)
    {
        if (mission == null)
        {
            return;
        }

        int workerSkill = Math.Max(0, mission.workerSkill);
        int clientInvest = Math.Max(0, mission.clientInvest);
        int hours = Math.Max(1, mission.hoursTotal > 0 ? mission.hoursTotal : 8);
        mission.rewardMoney = CalcPreviewMoney(clientInvest, workerSkill, hours);
        mission.rewardPlat = CalcRewardPlat(clientInvest);
        mission.rewardTicketGranted = RollPercent(FurnitureTicketChancePercent);
        mission.rewardPreviewLocked = true;
    }

    /// <summary>
    /// Task-description reward line from the locked pins (money / plat / ticket +
    /// skill warn). Empty when there is nothing to show.
    /// </summary>
    internal static string RewardPreviewLine(TownLaborMission mission)
    {
        if (mission == null || !mission.rewardPreviewLocked)
        {
            return "";
        }

        var bits = new List<string>();
        if (mission.rewardMoney > 0)
        {
            bits.Add(LaborText.T("reward.money", mission.rewardMoney));
        }

        if (mission.rewardPlat > 0)
        {
            bits.Add(LaborText.T("reward.plat", mission.rewardPlat));
        }

        if (mission.rewardTicketGranted)
        {
            bits.Add(LaborText.T("reward.furnitureTicket"));
        }

        if (mission.workerSkill < Math.Max(1, mission.clientInvest))
        {
            bits.Add(LaborText.T("reward.skillDiscount"));
        }

        if (bits.Count == 0)
        {
            return "";
        }

        return LaborText.T("town.reward.line", string.Join(" / ", bits));
    }

    /// <summary>
    /// Board/tracker money-only line. No platinum / ticket / skill wording.
    /// </summary>
    internal static string MoneyOnlyPreviewLine(TownLaborMission mission)
    {
        if (mission == null)
        {
            return "";
        }

        if (!mission.rewardPreviewLocked)
        {
            LockRewardPreview(mission);
        }

        int money = Math.Max(0, mission.rewardMoney);
        if (money <= 0)
        {
            int investLv = Math.Max(0, mission.clientInvest);
            int hours = Math.Max(1, mission.hoursTotal > 0 ? mission.hoursTotal : 8);
            money = CalcFullWagePerHour(investLv) * hours;
        }

        if (money <= 0)
        {
            return "";
        }

        return LaborText.T("town.reward.moneyOnly", money);
    }

    static bool RollPercent(int percent)
    {
        if (percent <= 0)
        {
            return false;
        }

        if (percent >= 100)
        {
            return true;
        }

        try
        {
            return EClass.rnd(100) < percent;
        }
        catch
        {
            return false;
        }
    }

    static void DeliverThings(TownLaborMission mission, List<Thing> things)
    {
        if (things == null || things.Count == 0)
        {
            return;
        }

        bool sameMap = false;
        try
        {
            sameMap = EClass._zone != null && EClass._zone.uid == mission.uidZone
                && EClass.pc != null && EClass.pc.ExistsOnMap;
        }
        catch
        {
            sameMap = false;
        }

        if (sameMap)
        {
            Point? at = null;
            try
            {
                at = EClass.pc?.pos?.Copy() ?? EClass.pc?.pos;
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborRewards.cs silent catch: " + __e.Message); }
foreach (Thing t in things)
            {
                if (t == null || t.isDestroyed)
                {
                    continue;
                }

                try
                {
                    if (at != null && EClass._zone != null)
                    {
                        EClass._zone.AddCard(t, at);
                    }
                    else if (EClass.pc != null)
                    {
                        EClass.pc.Pick(t);
                    }
                }
                catch
                {
                    try { EClass.pc?.Pick(t); } catch { }
                }
            }

            return;
        }

        // Off-map: home mailbox parcel.
        try
        {
            FactionBranch? branch = EClass.BranchOrHomeBranch ?? EClass.Branch;
            if (branch == null)
            {
                foreach (Thing t in things)
                {
                    try { EClass.pc?.Pick(t); } catch { }
                }

                return;
            }

            string title = LaborTerms.TownWork + " · " + (string.IsNullOrEmpty(mission.jobTitle) ? NpcLabor.LaborText.T("town.msg.town") : mission.jobTitle);
            Thing? pack = null;
            try
            {
                pack = ThingGen.CreateParcel(title, things.ToArray());
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("townlabor parcel: " + ex.Message);
            }

            if (pack == null)
            {
                foreach (Thing t in things)
                {
                    try { branch.PutInMailBox(t); } catch { }
                }

                return;
            }

            try
            {
                branch.PutInMailBox(pack);
            }
            catch (Exception ex)
            {
                Plugin.LogWarn("townlabor mailbox: " + ex.Message);
                try { EClass.pc?.Pick(pack); } catch { }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor deliver offmap: " + ex.Message);
        }
    }

    static Thing? TryCreateMoney(int amount)
    {
        if (amount <= 0)
        {
            return null;
        }

        try
        {
            return ThingGen.CreateCurrency(amount, "money");
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborRewards.cs silent catch: " + __e.Message); }
try
        {
            Thing? t = ThingGen.Create("money");
            if (t != null)
            {
                try { t.SetNum(amount); } catch { }
            }

            return t;
        }
        catch
        {
            return null;
        }
    }

    static Thing? TryCreatePlat(int amount)
    {
        if (amount <= 0)
        {
            return null;
        }

        try
        {
            Thing? t = ThingGen.Create("plat");
            if (t != null)
            {
                try { t.SetNum(amount); } catch { }
                return t;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborRewards.cs silent catch: " + __e.Message); }
try
        {
            return ThingGen.Create("plat").SetNum(amount);
        }
        catch
        {
            return null;
        }
    }

    static Thing? TryCreateFurnitureTicket(TownLaborMission mission, Chara? client)
    {
        try
        {
            Thing? t = ThingGen.Create("ticket_furniture");
            if (t == null || t.isDestroyed)
            {
                return null;
            }

            Zone? zone = null;
            try
            {
                zone = mission.GetZone()
                    ?? client?.currentZone
                    ?? client?.homeZone
                    ?? EClass._zone;
                if (zone != null)
                {
                    zone = zone.GetTopZone() ?? zone;
                }
            }
            catch
            {
                zone = EClass._zone;
            }

            if (zone != null)
            {
                try
                {
                    TraitTicketFurniture.SetZone(zone, t);
                }
                catch (Exception ex)
                {
                    Plugin.LogDebug("townlabor ticket zone: " + ex.Message);
                }
            }

            return t;
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor ticket: " + ex.Message);
            return null;
        }
    }

    static Thing? RollLeftover(TownLaborJobDef def)
    {
        if (def?.LeftoverIds == null || def.LeftoverIds.Length == 0)
        {
            return null;
        }

        int tries = Math.Min(4, def.LeftoverIds.Length);
        for (int i = 0; i < tries; i++)
        {
            string id;
            try
            {
                id = def.LeftoverIds[EClass.rnd(def.LeftoverIds.Length)];
            }
            catch
            {
                id = def.LeftoverIds[0];
            }

            Thing? t = TryCreate(id, 1);
            if (t != null)
            {
                return t;
            }
        }

        for (int i = 0; i < def.LeftoverIds.Length; i++)
        {
            Thing? t = TryCreate(def.LeftoverIds[i], 1);
            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    static Thing? TryCreate(string id, int num)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        try
        {
            Thing? t = ThingGen.Create(id);
            if (t == null || t.isDestroyed)
            {
                return null;
            }

            if (num > 1)
            {
                try { t.SetNum(num); } catch { }
            }

            return t;
        }
        catch
        {
            return null;
        }
    }

    static string SafeName(Thing t)
    {
        try
        {
            return t.Name ?? t.id ?? "?";
        }
        catch
        {
            try { return t.id ?? "?"; }
            catch { return "?"; }
        }
    }
}
