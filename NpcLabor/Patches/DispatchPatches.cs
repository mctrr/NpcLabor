using System;
using HarmonyLib;
using UnityEngine;
using NpcLabor.Dispatch;
using NpcLabor.TownLabor;

namespace NpcLabor.Patches;

// Tick once per real game hour everywhere (home, dungeon, region travel).
// Do NOT hook FactionBranch.OnSimulateHour: that only runs on the active PC-faction
// zone, and offline home re-entry Simulate would fast-forward all missed hours again.
[HarmonyPatch(typeof(GameDate), nameof(GameDate.AdvanceHour))]
internal static class DispatchGameDateHourPatch
{
    // Prefix: scrub null EloMap lights BEFORE vanilla Scene.OnChangeHour runs inside AdvanceHour.
    // Broken lights come from older builds that called AddLight(..., "iconFlag").
    [HarmonyPrefix]
    static void Prefix()
    {
        try
        {
            DungeonDispatchManager.SanitizeRegionMapLights();
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}

    [HarmonyPostfix]
    static void Postfix()
    {
        try
        {
            // Always sanitize; cheap and keeps overworld hour ticks safe even with 0 missions.
            DungeonDispatchManager.SanitizeRegionMapLights();

            if (DungeonDispatchManager.Count > 0)
            {
                DungeonDispatchManager.OnSimulateHour();
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch gamedate hour: " + ex.Message);
        }

        try
        {
            if (TownLaborManager.Count > 0)
            {
                TownLaborManager.OnSimulateHour();
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("townlabor gamedate hour: " + ex.Message);
        }
    }
}
// Final safety net: vanilla EloMapActor.OnChangeHour does light.sr.color with no null check.
// Older NPC Labor builds could insert null-sr lights via AddLight("iconFlag").
[HarmonyPatch(typeof(EloMapActor), nameof(EloMapActor.OnChangeHour))]
internal static class DispatchEloMapActorHourPatch
{
    [HarmonyPrefix]
    static void Prefix()
    {
        try
        {
            DungeonDispatchManager.SanitizeRegionMapLights();
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}
}

[HarmonyPatch(typeof(Game), nameof(Game.OnBeforeSave))]
internal static class DispatchSavePatch
{
    [HarmonyPrefix]
    static void Prefix()
    {
        try
        {
            DungeonDispatchManager.Save();
            TownLaborManager.Save();
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch save patch: " + ex.Message);
        }
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.OnLoad))]
internal static class DispatchLoadPatch
{
    [HarmonyPostfix]
    static void Postfix()
    {
        try
        {
            SpecialRewardConfig.Reload();
            DungeonDispatchManager.Load();
            TownLaborManager.Load();
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("dispatch load patch: " + ex.Message);
        }
    }
}

[HarmonyPatch(typeof(LayerQuestBoard))]
internal static class DispatchQuestBoardPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LayerQuestBoard.OnInit))]
    static void OnInitPostfix(LayerQuestBoard __instance)
    {
        try
        {
            EnsureDispatchButton(__instance);
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch OnInit button: " + ex.Message);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LayerQuestBoard.OnSwitchContent))]
    static void OnSwitchContentPostfix(LayerQuestBoard __instance, Window window)
    {
        try
        {
            EnsureDispatchButton(__instance);

            if (window != null && window.idTab == DungeonDispatchUi.TabDispatch)
            {
                if (!DungeonDispatchUi.IsAtPcFactionHome())
                {
                Msg.Say(NpcLabor.LaborText.T(
                    "dis.msg.baseOnly",
                    NpcLabor.LaborTerms.Dispatch));
                    return;
                }

                DungeonDispatchUi.OpenBoard(__instance);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch OnSwitchContent: " + ex.Message);
        }
    }

    static void EnsureDispatchButton(LayerQuestBoard board)
    {
        if (board == null)
        {
            return;
        }

        bool atHome = DungeonDispatchUi.IsAtPcFactionHome();
        Window? host = null;
        try
        {
            if (board.windows != null && board.windows.Count > 0)
            {
                host = board.windows[0];
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
if (host == null)
        {
            return;
        }

        WindowMenu? menu = null;
        try
        {
            menu = host.menuRight;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
if (menu == null)
        {
            try
            {
                menu = host.menuBottom;
            }
            catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}

        if (menu == null)
        {
            Plugin.LogDebug("dispatch: no window menu for button");
            return;
        }

        try
        {
            if (TryRefreshDispatchButton(menu))
            {
                return;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
if (!atHome)
        {
            return;
        }

        try
        {
            // Show active mission count only; concurrency is per-dungeon (max 4 members).
            int n = DungeonDispatchManager.Count;
            string label = n > 0 ? (NpcLabor.LaborTerms.Dispatch + " (" + n + ")") : NpcLabor.LaborTerms.Dispatch;
            UIButton? btn = menu.AddButton(
                "continue",
                _ =>
                {
                    DungeonDispatchUi.OpenBoard(board);
                },
                sprite: null,
                idButton: "Default");

            if (btn != null)
            {
                ForceDispatchButtonWhite(btn, label);
                Plugin.LogDebug("dispatch button ready: " + label);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch AddButton failed: " + ex.Message);
        }
    }

    static bool TryRefreshDispatchButton(WindowMenu menu)
    {
        try
        {
            UIButton[] buttons = Array.Empty<UIButton>();
            try
            {
                if (menu.layout != null)
                {
                    buttons = menu.layout.GetComponentsInChildren<UIButton>(true);
                }
            }
            catch
            {
                return false;
            }

            int n = DungeonDispatchManager.Count;
            string label = n > 0 ? (NpcLabor.LaborTerms.Dispatch + " (" + n + ")") : NpcLabor.LaborTerms.Dispatch;
            foreach (UIButton b in buttons)
            {
                try
                {
                    string t = b?.mainText?.text ?? "";
                    if (t.IndexOf(NpcLabor.LaborTerms.Dispatch, StringComparison.Ordinal) >= 0
                        || t.IndexOf(NpcLabor.LaborTerms.DungeonExplore, StringComparison.Ordinal) >= 0
                        || t.IndexOf("派遣", StringComparison.Ordinal) >= 0)
                    {
                        ForceDispatchButtonWhite(b, label);
                        return true;
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
return false;
    }

    /// <summary>
    /// Quest board side buttons use a dark skin; ButtonGeneral can read as black.
    /// Force plain white so dispatch label stays readable like other tab labels the player expects.
    /// </summary>
    static void ForceDispatchButtonWhite(UIButton? btn, string label)
    {
        if (btn?.mainText == null)
        {
            return;
        }

        try
        {
            // Skin colors first (Button is typically light on quest board chrome).
            btn.mainText.SetText(label, FontColor.Button);
        }
        catch
        {
            try
            {
                btn.mainText.SetText(label);
            }
            catch
            {
                try { btn.mainText.text = label; } catch { }
            }
        }

        try
        {
            // Absolute override — player asked for white text specifically.
            btn.mainText.color = UnityEngine.Color.white;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
try
        {
            btn.mainText.SetColor(FontColor.Button);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
try
        {
            btn.mainText.color = UnityEngine.Color.white;
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}
}

// Zone enter: keep dispatched NPCs on the progress floor as friendly stakeouts.
[HarmonyPatch(typeof(Player), nameof(Player.MoveZone), typeof(Zone))]
internal static class DispatchZoneEnterPatch
{
    [HarmonyPostfix]
    static void Postfix(Zone __0)
    {
        try
        {
            if (__0 != null)
            {
                DungeonDispatchManager.OnZoneEntered(__0);
                TownLaborManager.OnZoneEntered(__0);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("dispatch zone enter: " + ex.Message);
        }
    }
}

// Block talking / menus with dispatched explorers (isRestrained still opens "strain" dialog).
[HarmonyPatch(typeof(Chara), nameof(Chara.ShowDialog))]
[HarmonyPatch(new Type[] { })]
internal static class DispatchShowDialogPatch
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

            if (!DungeonDispatchManager.IsBusy(__instance.uid))
            {
                return true;
            }

            try
            {
        string who = __instance.NameSimple ?? __instance.Name ?? NpcLabor.LaborText.T("town.msg.companion");
                Msg.Say(NpcLabor.LaborText.T("dis.msg.busyTalk", who));
            }
            catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
try
            {
                SE.Beep();
            }
            catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
return false;
        }
        catch
        {
            return true;
        }
    }
}

[HarmonyPatch(typeof(LayerInteraction), nameof(LayerInteraction.Show), typeof(IInspect))]
internal static class DispatchLayerInteractionPatch
{
    [HarmonyPrefix]
    // Vanilla signature: Show(IInspect newTarget). Param name must match for Harmony.
    static bool Prefix(IInspect newTarget)
    {
        try
        {
            Chara? c = newTarget as Chara;
            if (c == null)
            {
                return true;
            }

            if (!DungeonDispatchManager.IsBusy(c.uid))
            {
                return true;
            }

            try
            {
        string who = c.NameSimple ?? c.Name ?? NpcLabor.LaborText.T("town.msg.companion");
                Msg.Say(NpcLabor.LaborText.T("dis.msg.busyInteract", who));
            }
            catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
try
            {
                SE.Beep();
            }
            catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
return false;
        }
        catch
        {
            return true;
        }
    }
}

// Defensive: any quest with null person would NRE vanilla OnShowDialog and break all talk.
[HarmonyPatch(typeof(QuestManager), nameof(QuestManager.OnShowDialog))]
internal static class DispatchQuestOnShowDialogPatch
{
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
                if (q == null)
                {
                    continue;
                }

                try
                {
                    if (q is QuestNpcLaborDispatch dq)
                    {
                        dq.EnsureSafePerson();
                        dq.deadline = 0;
                    }
                    else if (q is QuestNpcLaborTownLabor tq)
                    {
                        tq.EnsureSafePerson();
                        tq.deadline = 0;
                    }
                    else if (q.person == null)
                    {
                        q.person = new Person();
                    }

                    // Touch once so a bad person state fails here, not in vanilla.
                    _ = q.person;
                    _ = q.person?.chara;
                }
                catch
                {
                    try
                    {
                        q.person = new Person();
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}
}

// Load/remove can leave destroyed ItemQuestTracker rows in WidgetQuestTracker.items.
// Vanilla Kill does DestroyImmediate(gameObject) without guarding already-dead rows.
[HarmonyPatch(typeof(ItemQuestTracker), nameof(ItemQuestTracker.Kill))]
internal static class DispatchItemQuestTrackerKillPatch
{
    [HarmonyPrefix]
    static bool Prefix(ItemQuestTracker __instance)
    {
        try
        {
            if (__instance == null)
            {
                return false;
            }

            try
            {
                // Unity fake-null for destroyed components.
                if (__instance.Equals(null))
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
                _ = __instance.gameObject;
            }
            catch
            {
                try
                {
                    if (WidgetQuestTracker.Instance?.items != null)
                    {
                        WidgetQuestTracker.Instance.items.Remove(__instance);
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

[HarmonyPatch(typeof(WidgetQuestTracker), nameof(WidgetQuestTracker.Refresh))]
internal static class DispatchWidgetQuestTrackerRefreshPatch
{
    [HarmonyPrefix]
    static void Prefix()
    {
        try
        {
            DungeonDispatchManager.SanitizeQuestTrackerItems();
        }
        catch (System.Exception __e) { Plugin.LogDebug("DispatchPatches.cs silent catch: " + __e.Message); }
}
}

