using NpcLabor.CoCraft;
using NpcLabor.Craft;
using NpcLabor.Dispatch;
using NpcLabor.Process;
using NpcLabor.Trade;

namespace NpcLabor.TownLabor;

/// <summary>
/// Shared busy check across A co-craft / B processor / D dispatch / E town labor.
/// </summary>
internal static class LaborBusy
{
    internal static bool IsBusy(int uidChara)
    {
        if (uidChara <= 0)
        {
            return false;
        }

        try
        {
            if (TradeManager.IsBusy(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (TownLaborManager.IsBusy(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (DungeonDispatchManager.IsBusy(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (ProcessorJobSession.IsJobHeld() && ProcessorJobSession.NpcUid == uidChara)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (CoCraftSession.Active && CoCraftSession.NpcUid == uidChara)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (CraftManager.Has(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
return false;
    }

    /// <summary>
    /// Busy check for the production slice itself. It must not count its own assignment as
    /// "busy", or a resident would be permanently blocked by the very job they were given.
    /// So this checks every other slice and skips the craft table.
    /// </summary>
    internal static bool IsBusyElsewhere(int uidChara)
    {
        if (uidChara <= 0)
        {
            return false;
        }

        try
        {
            if (TradeManager.IsBusy(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (TownLaborManager.IsBusy(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (DungeonDispatchManager.IsBusy(uidChara))
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (ProcessorJobSession.IsJobHeld() && ProcessorJobSession.NpcUid == uidChara)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (CoCraftSession.Active && CoCraftSession.NpcUid == uidChara)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
return false;
    }

    internal static bool IsBusy(Chara? c)
    {
        if (c == null)
        {
            return true;
        }

        try
        {
            return IsBusy(c.uid);
        }
        catch
        {
            return true;
        }
    }

    internal static string? BusyReason(Chara? c)
    {
        if (c == null)
        {
            return NpcLabor.LaborText.T("town.busy.invalid");
        }

        int uid = c.uid;
        try
        {
            if (TradeManager.IsBusy(uid))
            {
                return NpcLabor.LaborText.T("town.busy.trade");
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (TownLaborManager.IsBusy(uid))
            {
                return NpcLabor.LaborText.T("town.busy.town", NpcLabor.LaborTerms.TownWork);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (DungeonDispatchManager.IsBusy(uid))
            {
                return NpcLabor.LaborText.T("town.busy.dungeon", NpcLabor.LaborTerms.DungeonExplore);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (ProcessorJobSession.IsJobHeld() && ProcessorJobSession.NpcUid == uid)
            {
                return NpcLabor.LaborText.T("town.busy.process", NpcLabor.LaborTerms.Process);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (CoCraftSession.Active && CoCraftSession.NpcUid == uid)
            {
                return NpcLabor.LaborText.T("town.busy.coCraft");
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
try
        {
            if (CraftManager.Has(uid))
            {
                return NpcLabor.LaborText.T("town.busy.craft");
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("LaborBusy.cs silent catch: " + __e.Message); }
return null;
    }
}
