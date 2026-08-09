using System;
using HarmonyLib;
using NpcLabor.Dispatch;
using NpcLabor.TownLabor;

namespace NpcLabor.Patches;

/// <summary>
/// Slice E: append town shop labor rows under vanilla quests on tab 0.
/// Keep D dispatch button/tab independent.
/// </summary>
[HarmonyPatch(typeof(LayerQuestBoard), nameof(LayerQuestBoard.RefreshQuest))]
internal static class TownLaborQuestBoardPatch
{
    [HarmonyPostfix]
    static void Postfix(LayerQuestBoard __instance)
    {
        try
        {
            AppendTownLaborRows(__instance);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor RefreshQuest: " + ex.Message);
        }
    }

    static void AppendTownLaborRows(LayerQuestBoard board)
    {
        if (board?.list == null)
        {
            return;
        }

        if (!TownLaborManager.CanHostTownLabor(EClass._zone))
        {
            return;
        }

        var offers = TownLaborManager.ListOffers(EClass._zone);
        if (offers == null || offers.Count == 0)
        {
            return;
        }

        // Vanilla RefreshQuest already set callbacks and refreshed. Append more rows
        // and refresh again so ItemQuest rows appear under vanilla quests.
        int added = 0;
        foreach (TownLaborOffer offer in offers)
        {
            if (offer?.Client == null || offer.Def == null)
            {
                continue;
            }

            QuestNpcLaborTownLaborOffer q = BuildOfferQuest(offer);
            try
            {
                board.list.Add(q);
                added++;
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("townlabor list.Add: " + ex.Message);
            }
        }

        if (added <= 0)
        {
            return;
        }

        try
        {
            board.list.Refresh();
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("townlabor list.Refresh: " + ex.Message);
        }

        // Re-bind click handlers: vanilla onInstantiate may have already run for existing
        // items; after Refresh, ensure labor rows call OnClickQuest (our override) only.
        // ItemQuest already sets button.SetOnClick -> a.OnClickQuest via callbacks.
        Plugin.LogDebug("townlabor board rows +" + added);
    }

    static QuestNpcLaborTownLaborOffer BuildOfferQuest(TownLaborOffer offer)
    {
        var q = new QuestNpcLaborTownLaborOffer
        {
            clientUid = offer.Client.uid,
            jobId = offer.Def.Id,
            isActiveLabor = offer.IsActive,
            laborHours = offer.Hours,
            // Unique non-zero uid so Quest.Equals / UIList.Contains do not collapse rows.
            // Do not bump game.quests.uid (that counter is for real started quests).
            uid = NextOfferUid(offer.Client.uid, offer.Def.Id),
            id = "npclabor_townlabor_offer_" + offer.Client.uid,
            // Keep reward column quiet ("-"); real rewards are flavor/time only on detail.
            rewardMoney = 0,
            isNew = !offer.IsActive,
            isComplete = false,
            phase = 0,
            deadline = 0,
            track = false,
            startDate = 0,
        };

        try
        {
            // Bind portrait/client name without assigning chara.quest (assignQuest: false).
            q.SetClient(offer.Client, assignQuest: false);
        }
        catch
        {
            try
            {
                q.person = new Person(offer.Client);
            }
            catch
            {
                q.EnsureSafePerson();
            }
        }

        // Force person temp cache so Client resolve works for map-local merchants.
        try
        {
            if (q.person == null)
            {
                q.person = new Person(offer.Client);
            }
            else
            {
                q.person.SetChara(offer.Client);
            }
        }
        catch
        {
            q.EnsureSafePerson();
        }

        q.EnsureSafePerson();
        q.isComplete = false;
        q.phase = 0;

        // Fake remaining hours for Date.GetText(q.Hours) display: Hours uses deadline.
        // deadline==0 => Hours=-1 => "dateDayVoid". Prefer a temporary raw deadline.
        try
        {
            int hours = Math.Max(1, offer.Hours);
            if (offer.IsActive)
            {
                TownLaborMission? m = TownLaborManager.FindByClient(offer.Client.uid);
                hours = m != null ? Math.Max(1, m.hoursLeft) : hours;
            }

            q.deadline = EClass.world.date.GetRaw() + hours * 60;
            // Guard against accidental expiry chrome if date math goes weird.
            if (q.Hours < 0)
            {
                q.deadline = EClass.world.date.GetRaw() + Math.Max(1, offer.Hours) * 60;
            }
        }
        catch
        {
            q.deadline = 0;
        }

        q.isComplete = false;
        return q;
    }

    /// <summary>
    /// Stable-ish unique synthetic uid for board rows. Stays outside typical quest uid range
    /// collisions by mixing client + job hash with a large base offset.
    /// </summary>
    static int NextOfferUid(int clientUid, string jobId)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + clientUid;
            h = h * 31 + (jobId ?? string.Empty).GetHashCode();
             // Use negative uid to avoid colliding with real quest uids (which are always positive).
             int uid = -(Math.Abs(h) + 1);
             if (uid >= 0) { uid = -1; } // safety: ensure always negative
            if (uid == 0)
            {
                 uid = -1;
            }

            return uid;
        }
    }
}

// Hour / save / load are extended on the existing D patches (same GameDate / Game hooks).
// Zone enter / dialog / death are also extended on D / Lifecycle patches.

[HarmonyPatch(typeof(QuestManager), nameof(QuestManager.OnShowDialog))]
internal static class TownLaborQuestOnShowDialogPatch
{
    // Runs alongside DispatchQuestOnShowDialogPatch; both Prefixes are fine.
    [HarmonyPrefix]
    static void Prefix(QuestManager __instance)
    {
        try
        {
            if (__instance?.list == null)
            {
                return;
            }

            foreach (Quest q in __instance.list)
            {
                if (q is QuestNpcLaborTownLabor tq)
                {
                    tq.EnsureSafePerson();
                    tq.deadline = 0;
                }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("TownLaborPatches.cs silent catch: " + __e.Message); }
}
}

[HarmonyPatch(typeof(Chara), nameof(Chara.ShowDialog))]
[HarmonyPatch(new Type[] { })]
internal static class TownLaborShowDialogPatch
{
    [HarmonyPrefix]
    static bool Prefix(Chara __instance)
    {
        try
        {
            if (__instance == null)
            {
                return true;
            }

            if (!TownLaborManager.IsBusy(__instance.uid))
            {
                return true;
            }

            try
            {
        string who = __instance.NameSimple ?? __instance.Name ?? NpcLabor.LaborText.T("town.msg.companion");
        Msg.Say(NpcLabor.LaborText.T("town.talk.busy", who, NpcLabor.LaborTerms.TownWork));
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborPatches.cs silent catch: " + __e.Message); }
try { SE.Beep(); } catch { }
            return false;
        }
        catch
        {
            return true;
        }
    }
}

[HarmonyPatch(typeof(LayerInteraction), nameof(LayerInteraction.Show), typeof(IInspect))]
internal static class TownLaborLayerInteractionPatch
{
    [HarmonyPrefix]
    static bool Prefix(IInspect newTarget)
    {
        try
        {
            Chara? c = newTarget as Chara;
            if (c == null)
            {
                return true;
            }

            if (!TownLaborManager.IsBusy(c.uid))
            {
                return true;
            }

            try
            {
        string who = c.NameSimple ?? c.Name ?? NpcLabor.LaborText.T("town.msg.companion");
        Msg.Say(NpcLabor.LaborText.T("town.talk.noInteract", who, NpcLabor.LaborTerms.TownWork));
            }
            catch (System.Exception __e) { Plugin.LogDebug("TownLaborPatches.cs silent catch: " + __e.Message); }
try { SE.Beep(); } catch { }
            return false;
        }
        catch
        {
            return true;
        }
    }
}
