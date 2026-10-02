using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NpcLabor.Process;

/// <summary>
/// Optional "burn value" fuel model for outsourced processing (slice B).
///
/// Vanilla only knows two fuels: <c>log</c> (20) and <c>branch</c> (5), added straight to
/// the machine's charge by <c>Trait.Refuel</c>. This module replaces that with its own
/// points: every craft costs <c>costPerBurn</c>, and the worker feeds leaves/grass, twigs
/// and logs into a per-machine pool kept on the card itself, so unspent burn value stays
/// in the machine across sessions instead of being lost.
///
/// Turned off, everything falls back to the vanilla path (Trait.TryRefuel, log/branch).
/// </summary>
internal static class ProcessorFuel
{
    /// <summary>Card int key holding the machine's leftover burn value.</summary>
    const string PoolKey = "npclabor_fuel";

    /// <summary>Shortest gap between two "added fuel" notes, so away catch-up cannot spam the log.</summary>
    const float NoteGapSeconds = 1.5f;

    static float _lastNoteTime = -99f;

    /// <summary>Master switch for this module. Off = vanilla fuel behaviour.</summary>
    internal static bool Custom => NpcLabor.LaborConfig.ProcessFuel.CustomFuel;

    /// <summary>Burn value one craft eats.</summary>
    internal static int CostPerBurn => Math.Max(0, NpcLabor.LaborConfig.ProcessFuel.CostPerBurn);

    /// <summary>Burn value still stored in the machine.</summary>
    internal static int PoolOf(Card machine)
    {
        if (machine == null || machine.isDestroyed)
        {
            return 0;
        }

        try
        {
            return machine.GetInt(PoolKey);
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel read: " + ex.Message);
            return 0;
        }
    }

    /// <summary>Top the machine up until it can pay for one more craft. True when it can.</summary>
    internal static bool EnsureFuel(Chara worker, Card machine, List<Thing> excludes)
    {
        if (machine == null || machine.isDestroyed)
        {
            return false;
        }

        int need = CostPerBurn;
        if (need <= 0)
        {
            return true;
        }

        if (PoolOf(machine) >= need)
        {
            return true;
        }

        Feed(machine, need - PoolOf(machine), excludes, worker);
        return PoolOf(machine) >= need;
    }

    /// <summary>Pay for one finished craft.</summary>
    internal static void Spend(Card machine)
    {
        if (machine == null || machine.isDestroyed)
        {
            return;
        }

        try
        {
            machine.SetInt(PoolKey, Math.Max(0, PoolOf(machine) - CostPerBurn));
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel spend: " + ex.Message);
        }
    }

    /// <summary>
    /// Burn until <paramref name="missing"/> points were added, then stop - unlike a hoarding
    /// refuel this never strips more from storage than the job actually needs.
    /// </summary>
    static void Feed(Card machine, int missing, List<Thing> excludes, Chara worker)
    {
        if (missing <= 0)
        {
            return;
        }

        List<Thing> sources = CollectFuel(machine, excludes);
        if (sources.Count == 0)
        {
            return;
        }

        int added = 0;
        for (int i = 0; i < sources.Count; i++)
        {
            if (added >= missing)
            {
                break;
            }

            Thing t = sources[i];
            if (t == null || t.isDestroyed || t.Num <= 0)
            {
                continue;
            }

            int unit = ValueOf(t);
            if (unit <= 0)
            {
                continue;
            }

            try
            {
                if (t.Num <= 0)
                {
                    continue;
                }

                int take = Mathf.Min(t.Num, Mathf.CeilToInt((missing - added) / (float)unit));
                if (take <= 0)
                {
                    continue;
                }

                Thing piece = take >= t.Num ? t : t.Split(take);
                if (piece == null || piece.isDestroyed || piece.Num <= 0)
                {
                    continue;
                }

                int num = piece.Num;
                string name = piece.Name ?? t.id ?? "?";
                int gain = unit * num;

                piece.Destroy();

                int pool = PoolOf(machine) + gain;
                added += gain;
                WritePool(machine, pool);
                Note(worker, machine, num, name, gain, pool);
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("processor fuel burn: " + ex.Message);
            }
        }
    }

    /// <summary>How far from the machine a container may stand and still feed the fire.</summary>
    const int StorageRadius = 4;

    /// <summary>
    /// Everything burnable within reach: the machine's own spot (vanilla's fuel spots),
    /// containers standing next to it, then the player's pack (recursively) — never the
    /// ingredient stacks parked on the bench.
    ///
    /// Ordered by burn value per item, highest first, so one carbon log is spent before a
    /// pile of leaves: the keeper of an idle furnace wants the big stuff in the fire, not
    /// twenty trips back to the grass pile.
    /// </summary>
    static List<Thing> CollectFuel(Card machine, List<Thing> excludes)
    {
        List<Thing> list = new List<Thing>();

        try
        {
            List<Thing>? spot = EClass._zone.TryListThingsInSpot<TraitSpotFuel>(t => ValueOf(t) > 0);
            if (spot != null)
            {
                for (int i = 0; i < spot.Count; i++)
                {
                    Thing? t = spot[i];
                    if (t != null && !IsExcluded(excludes, t))
                    {
                        list.Add(t);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel spot scan: " + ex.Message);
        }

        try
        {
            List<Thing>? things = EClass._map?.things;
            if (things != null && machine.pos != null)
            {
                for (int i = 0; i < things.Count; i++)
                {
                    Thing c = things[i];
                    if (c == null || c.isDestroyed || c.things == null || ReferenceEquals(c, machine))
                    {
                        continue;
                    }

                    if (!c.IsInstalled || !Nearby(c, machine))
                    {
                        continue;
                    }

                    c.things.Foreach(t =>
                    {
                        if (t == null || t.isDestroyed || IsExcluded(excludes, t) || ValueOf(t) <= 0)
                        {
                            return;
                        }

                        list.Add(t);
                    }, false);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel chest scan: " + ex.Message);
        }

        try
        {
            ThingContainer? pack = EClass.pc?.things;
            if (pack != null)
            {
                pack.Foreach(t =>
                {
                    if (t == null || t.isDestroyed || t.tier != 0)
                    {
                        return;
                    }

                    if (IsExcluded(excludes, t) || ValueOf(t) <= 0)
                    {
                        return;
                    }

                    list.Add(t);
                }, true);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel pack scan: " + ex.Message);
        }

        return SortByValue(list);
    }

    /// <summary>Biggest burn value first; ties keep the discovery order (LINQ sorts stably).</summary>
    static List<Thing> SortByValue(List<Thing> list)
        => list.OrderByDescending(t => ValueOf(t)).ToList();

    static bool Nearby(Thing container, Card machine)
    {
        if (container.pos == null || machine.pos == null)
        {
            return false;
        }

        int dx = container.pos.x - machine.pos.x;
        int dz = container.pos.z - machine.pos.z;
        if (dx < 0)
        {
            dx = -dx;
        }

        if (dz < 0)
        {
            dz = -dz;
        }

        return Math.Max(dx, dz) <= StorageRadius;
    }

    static bool IsExcluded(List<Thing> excludes, Thing t)
    {
        if (excludes == null)
        {
            return false;
        }

        for (int i = 0; i < excludes.Count; i++)
        {
            if (ReferenceEquals(excludes[i], t))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Burn value per item, 0 when the thing is not fuel.</summary>
    internal static int ValueOf(Thing t)
    {
        if (t == null || t.isDestroyed || t.Num <= 0)
        {
            return 0;
        }

        string id;
        bool carbon;
        try
        {
            if (t.IsImportant)
            {
                return 0;
            }

            id = t.id ?? "";
            carbon = IsCarbon(t);
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel read thing: " + ex.Message);
            return 0;
        }

        if (id.Length == 0)
        {
            return 0;
        }

        NpcLabor.ProcessFuelConfigSection cfg = NpcLabor.LaborConfig.ProcessFuel;

        if (IsLog(id))
        {
            return carbon ? Math.Max(0, cfg.ValueCarbonLog) : Math.Max(0, cfg.ValueLog);
        }

        if (IsBranch(id))
        {
            return carbon ? Math.Max(0, cfg.ValueCarbonBranch) : Math.Max(0, cfg.ValueBranch);
        }

        if (IsLeafy(id))
        {
            return Math.Max(0, cfg.ValueLeaf);
        }

        return 0;
    }

    // Id families deliberately kept narrow: anything furniture shaped (grass door, wood table,
    // wooden chest…) must never qualify as fuel even though its id mentions the same word.
    static bool IsLog(string id) => id == "log" || id.EndsWith("_log");

    static bool IsBranch(string id) => id == "branch" || id.EndsWith("_branch") || id.Contains("twig");

    static bool IsLeafy(string id)
        => id.Contains("leaf") || id == "grass" || id == "pasture" || id.StartsWith("weed");

    /// <summary>True for things whose material is carbon (charcoal), which burn hotter.</summary>
    static bool IsCarbon(Thing t)
    {
        try
        {
            string alias = t.material?.alias ?? "";
            if (alias.Length == 0)
            {
                return false;
            }

            return alias == "carbone" || alias.Contains("carbon") || alias.Contains("charcoal");
        }
        catch
        {
            return false;
        }
    }

    static void WritePool(Card machine, int value)
    {
        try
        {
            machine.SetInt(PoolKey, Math.Max(0, value));
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel write: " + ex.Message);
        }
    }

    /// <summary>Tell the player what actually went into the fire.</summary>
    static void Note(Chara worker, Card machine, int num, string itemName, int gain, int pool)
    {
        string who;
        string what;
        try
        {
            who = (worker != null && !worker.isDestroyed ? worker.Name : null)
                  ?? ProcessorJobSession.NpcName
                  ?? NpcLabor.LaborText.T("proc.msg.residentFallback");
            what = ProcessorJobSession.MachineName ?? machine.Name ?? NpcLabor.LaborText.T("proc.msg.machineFallback");
        }
        catch
        {
            who = "?";
            what = "?";
        }

        Plugin.LogInfo($"processor refuel: {who} -> {what}: {num}x {itemName} (+{gain}, pool {pool})");

        try
        {
            float now = Time.realtimeSinceStartup;
            if (now - _lastNoteTime < NoteGapSeconds)
            {
                return;
            }

            _lastNoteTime = now;
            Msg.SayRaw(NpcLabor.LaborText.T("proc.msg.refuel", who, what, num, itemName, gain, pool));
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("processor fuel note: " + ex.Message);
        }
    }
}
