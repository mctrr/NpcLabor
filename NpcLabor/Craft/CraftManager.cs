using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NpcLabor.TownLabor;

namespace NpcLabor.Craft;

/// <summary>
/// The cottage industry slice: standing instructions that keep a resident of the colony
/// making something — cooking for the settlement being the case it was written for.
///
/// A job is deliberately coarse. It owns a worker, a recipe and a count, and it advances
/// on the same hourly tick the rest of the mod uses. Each hour the worker puts in a shift's
/// worth of minutes (faster if their skill is above the recipe's requirement), the pantry
/// is drawn down, and finished goods go back to the pantry or onto the tile the player
/// marked with a sales tag.
///
/// Two rules keep this from becoming a second, invisible economy:
///   * Work only happens in a PC faction zone. A resident who is out on a dispatch, on a
///     caravan or simply standing in town does nothing, which is what "not while we are
///     away exploring" means in practice.
///   * Ingredients are taken all-or-nothing, and a craft that cannot be finished hands
///     everything back. The pantry is never drained by a half-run.
/// </summary>
internal static class CraftManager
{
    /// <summary>Vanilla sales tag — the marker a player drops to say "put it here".</summary>
    internal const string SpotThingId = "tag_sell";

    /// <summary>
    /// Stamped on the tags this mod drops, so a tag the player placed for their own shop
    /// is never mistaken for a drop point, and ours is never read as a shop tag.
    /// </summary>
    internal const string SpotMarkerKey = "npclabor_droppoint";

    internal static List<CraftJob> Jobs = new List<CraftJob>();

    static bool _dirty;

    /// <summary>Something changed that the save file should hear about.</summary>
    internal static bool Dirty => _dirty;

    /// <summary>Most items one visible craft act turns out, however fast the cook is.</summary>
    const int MaxBatch = 10;

    // ────────────────────────────────────────────────────────────── the act reports back

    internal static CraftJob? JobOf(int uid)
    {
        for (int i = 0; i < Jobs.Count; i++)
        {
            if (Jobs[i] != null && Jobs[i].uid == uid)
            {
                return Jobs[i];
            }
        }

        return null;
    }

    /// <summary>
    /// An act reporting back: how many items it turned out, how many shifts' worth it had
    /// to hand back, and why, if it was cut short. Runs on the game thread, like the tick.
    /// </summary>
    internal static void NoteMade(int uid, int made, int refundItems, string? note)
    {
        CraftJob? job = JobOf(uid);
        if (job == null)
        {
            return;
        }

        if (made > 0)
        {
            job.produced += made;
            _dirty = true;
        }

        if (refundItems > 0)
        {
            int cost = CraftEngine.MinutesPerItem(job.recipeId);
            if (cost <= 0)
            {
                cost = 30;
            }

            job.minutes += (double)refundItems * cost;
        }

        if (!string.IsNullOrEmpty(note))
        {
            job.stalled = true;
            job.note = note ?? "";
        }
        else
        {
            job.stalled = false;
            job.note = "";
        }
    }

    /// <summary>Put unfinished ingredients back in the pantry, or at the worker's feet.</summary>
    internal static void ReturnToPantry(IList<Thing> things, Chara? worker)
    {
        if (things == null || things.Count == 0)
        {
            return;
        }

        List<Thing> pantry = Pantry();
        for (int i = 0; i < things.Count; i++)
        {
            Thing t = things[i];
            if (t == null || t.isDestroyed)
            {
                continue;
            }

            bool stored = false;
            for (int k = 0; k < pantry.Count && !stored; k++)
            {
                stored = Put(pantry[k], t);
            }

            if (!stored)
            {
                DropAt(worker?.pos, t);
            }
        }
    }

    /// <summary>Put a finished item away — the craft act owns the product, not the tick.</summary>
    internal static void DepositProduct(Thing product, Chara worker)
    {
        Deposit(product, worker, out _);
    }

    /// <summary>Items the instruction still wants, or 0 when it never stops.</summary>
    static int Remaining(CraftJob job)
    {
        switch (job.mode)
        {
            case CraftMode.Count:
                return Math.Max(0, Math.Max(1, job.target) - job.produced);

            case CraftMode.Keep:
                return Math.Max(0, Math.Max(1, job.target) - CountProduct(job.recipeId));

            default:
                return 0;
        }
    }

    /// <summary>
    /// Recipes with no bench work on the spot. Vanilla marks those "self" / "none" / "x"
    /// in the sheet, and <see cref="CraftEngine.NeedFactory"/> says the same thing.
    /// </summary>
    static bool IsRealFactory(string factory)
    {
        if (string.IsNullOrEmpty(factory))
        {
            return false;
        }

        return !string.Equals(factory, "self", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(factory, "none", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(factory, "x", StringComparison.OrdinalIgnoreCase);
    }

    static bool AtBench(Chara worker, Thing bench)
    {
        try
        {
            return worker.Dist(bench) <= 2;
        }
        catch
        {
            return false;
        }
    }

    // ────────────────────────────────────────────────────────────── queries

    internal static bool Has(int uid)
    {
        for (int i = 0; i < Jobs.Count; i++)
        {
            if (Jobs[i] != null && Jobs[i].uid == uid)
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsRunning => Jobs.Count > 0;

    /// <summary>Residents who could be given an instruction: home, alive, not the PC.</summary>
    internal static List<Chara> Candidates()
    {
        var found = new List<Chara>();

        try
        {
            List<Chara>? charas = EClass._map?.charas;
            if (charas == null)
            {
                return found;
            }

            for (int i = 0; i < charas.Count; i++)
            {
                Chara c = charas[i];
                if (c == null || c.isDead || c.IsPC)
                {
                    continue;
                }

                if (!c.IsPCFactionOrMinion)
                {
                    continue;
                }

                found.Add(c);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft candidates: " + ex.Message);
        }

        return found;
    }

    /// <summary>Assign a recipe to a resident. Replaces whatever they were making before.</summary>
    internal static CraftJob? Assign(Chara who, string recipeId, CraftMode mode, int target)
    {
        if (who == null || string.IsNullOrEmpty(recipeId))
        {
            return null;
        }

        Cancel(who.uid);

        var job = new CraftJob
        {
            uid = who.uid,
            name = SafeName(who),
            recipeId = recipeId,
            mode = mode,
            target = Math.Max(1, target),
        };

        Jobs.Add(job);
        _dirty = true;
        return job;
    }

    /// <summary>Drop every instruction (the "careful, this is not the same as stop" case).</summary>
    internal static void Clear()
    {
        Jobs.Clear();
        _dirty = true;
    }

    internal static void Cancel(int uid)
    {
        for (int i = Jobs.Count - 1; i >= 0; i--)
        {
            if (Jobs[i] != null && Jobs[i].uid == uid)
            {
                Jobs.RemoveAt(i);
                _dirty = true;
            }
        }
    }

    internal static void CancelDone()
    {
        for (int i = Jobs.Count - 1; i >= 0; i--)
        {
            if (Jobs[i] != null && Jobs[i].done)
            {
                Jobs.RemoveAt(i);
                _dirty = true;
            }
        }
    }

    // ────────────────────────────────────────────────────────────── hour tick

    internal static void OnSimulateHour()
    {
        if (Jobs.Count == 0)
        {
            return;
        }

        for (int i = 0; i < Jobs.Count; i++)
        {
            CraftJob? job = Jobs[i];
            if (job == null)
            {
                continue;
            }

            try
            {
                Advance(job);
            }
            catch (Exception ex)
            {
                job.stalled = true;
                job.note = "error";
                Plugin.LogWarn("craft tick: " + ex.Message);
            }
        }
    }

    static void Advance(CraftJob job)
    {
        if (job.done)
        {
            return;
        }

        Chara? worker = null;
        try
        {
            worker = RefChara.Get(job.uid);
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft worker: " + ex.Message);
        }

        if (worker == null || worker.isDead)
        {
            job.stalled = true;
            job.note = LaborText.T("craft.note.missing");
            return;
        }

        Zone? zone = EClass._zone;
        if (zone == null || !zone.IsPCFaction)
        {
            job.stalled = true;
            job.note = LaborText.T("craft.note.notHome");
            return;
        }

        Zone? here = worker.currentZone;
        if (here == null || here.uid != zone.uid)
        {
            job.stalled = true;
            job.note = LaborText.T("craft.note.awayWorker");
            return;
        }

        try
        {
            if (LaborBusy.IsBusyElsewhere(job.uid))
            {
                job.stalled = true;
                job.note = LaborText.T("craft.note.busy");
                return;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft busy: " + ex.Message);
        }

        if (Satisfied(job, out int stock))
        {
            if (job.mode == CraftMode.Count)
            {
                job.done = true;
                job.note = "";
            }
            else
            {
                job.stalled = false;
                job.note = LaborText.T("craft.note.stocked", stock.ToString());
            }

            return;
        }

        // A recipe that needs a bench cannot start until one is found, and the worker
        // should be standing at it — that is the "walk over and do it themselves" part.
        string factory = CraftEngine.Factory(job.recipeId);
        Thing? bench = null;
        if (IsRealFactory(factory))
        {
            bench = FindStation(factory, worker);
            if (bench == null)
            {
                job.stalled = true;
                job.note = LaborText.T("craft.note.noBench", CraftEngine.FactoryName(job.recipeId));
                return;
            }
        }

        // Already at it: the act reports back when it has finished, so leave it alone.
        if (worker.ai is CraftActAi working && working.JobUid == job.uid)
        {
            job.stalled = false;
            job.note = LaborText.T("craft.note.working");
            return;
        }

        if (bench != null && !AtBench(worker, bench))
        {
            job.stalled = false;
            job.note = LaborText.T("craft.note.walking");
            if (worker.ai is not AI_Goto)
            {
                try
                {
                    worker.SetAI(new AI_Goto(bench, 1));
                }
                catch (Exception ex)
                {
                    Plugin.LogDebug("craft goto: " + ex.Message);
                }
            }

            return;
        }

        List<Thing> pantry = Pantry();
        if (!CraftEngine.CanSupply(pantry, job.recipeId, out string missing))
        {
            job.stalled = true;
            job.note = LaborText.T("craft.note.missing", missing);
            return;
        }

        job.stalled = false;
        job.note = "";

        double rate = CraftEngine.WorkRate(job.recipeId, worker) * 60.0;
        job.minutes += rate;

        int cost = CraftEngine.MinutesPerItem(job.recipeId);
        if (cost <= 0)
        {
            cost = 30;
        }

        if (job.minutes < cost)
        {
            return;
        }

        // A shift at the bench is one visible act. A resident whose skill is above the
        // recipe gets through more than one item in that shift, and the batch carries
        // exactly that many — which is what makes a good cook worth assigning.
        int batch = (int)(job.minutes / cost);
        if (batch < 1)
        {
            batch = 1;
        }

        if (batch > MaxBatch)
        {
            batch = MaxBatch;
        }

        int want = Remaining(job);
        if (want > 0 && batch > want)
        {
            batch = want;
        }

        // The shift's minutes are spent now; the act hands them back for anything it
        // could not finish, so an interruption never eats a resident's time.
        job.minutes -= (double)batch * cost;
        job.stalled = false;
        job.note = LaborText.T("craft.note.working");
        _dirty = true;

        try
        {
            worker.SetAI(new CraftActAi
            {
                JobUid = job.uid,
                RecipeId = job.recipeId,
                Count = batch,
                Bench = bench,
            });
        }
        catch (Exception ex)
        {
            job.minutes += (double)batch * cost;
            job.stalled = true;
            job.note = LaborText.T("craft.note.failed");
            Plugin.LogWarn("craft start act: " + ex.Message);
        }
    }

    /// <summary>Has the instruction been met? "Keep" also needs the pantry counted.</summary>
    static bool Satisfied(CraftJob job, out int stock)
    {
        stock = 0;

        switch (job.mode)
        {
            case CraftMode.Count:
                return job.produced >= Math.Max(1, job.target);

            case CraftMode.Keep:
                stock = CountProduct(job.recipeId);
                return stock >= Math.Max(1, job.target);

            default:
                return false;
        }
    }

    // ────────────────────────────────────────────────────────────── pantry

    /// <summary>Shared containers in the current zone — the colony's pantry.</summary>
    internal static List<Thing> Pantry()
    {
        var list = new List<Thing>();

        try
        {
            List<Thing>? things = EClass._map?.things;
            if (things == null)
            {
                return list;
            }

            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || t.isDestroyed)
                {
                    continue;
                }

                bool shared = false;
                try
                {
                    shared = t.IsSharedContainer;
                }
                catch (Exception ex)
                {
                    Plugin.LogDebug("craft shared: " + ex.Message);
                }

                if (shared)
                {
                    list.Add(t);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft pantry: " + ex.Message);
        }

        return list;
    }

    /// <summary>How many finished items of a recipe currently sit in the pantry.</summary>
    internal static int CountProduct(string recipeId)
    {
        string? pid = CraftEngine.ProductId(recipeId);
        if (string.IsNullOrEmpty(pid))
        {
            return 0;
        }

        int n = 0;
        List<Thing> pantry = Pantry();

        for (int i = 0; i < pantry.Count; i++)
        {
            List<Thing> items = CraftEngine.Contents(pantry[i]);
            for (int k = 0; k < items.Count; k++)
            {
                try
                {
                    if (string.Equals(items[k].id, pid, StringComparison.OrdinalIgnoreCase))
                    {
                        n += Math.Max(1, items[k].Num);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogDebug("craft count: " + ex.Message);
                }
            }
        }

        return n;
    }

    // ────────────────────────────────────────────────────────────── output

    /// <summary>
    /// Put a finished item away: onto the marked tile first (a container standing on the
    /// tag, otherwise the tile itself), then back into the pantry, and only as a last
    /// resort at the worker's feet.
    /// </summary>
    static void Deposit(Thing product, Chara worker, out string where)
    {
        where = "";

        if (product == null)
        {
            return;
        }

        Thing? spot = FindSpot();
        if (spot != null)
        {
            Thing? onSpot = ContainerAt(spot.pos);
            if (onSpot != null && Put(onSpot, product))
            {
                where = "spot";
                return;
            }

            if (DropAt(spot.pos, product))
            {
                where = "spot-floor";
                return;
            }
        }

        List<Thing> pantry = Pantry();
        for (int i = 0; i < pantry.Count; i++)
        {
            if (Put(pantry[i], product))
            {
                where = "pantry";
                return;
            }
        }

        if (DropAt(worker?.pos, product))
        {
            where = "floor";
            return;
        }

        // Nowhere to put it: keep it in the worker's hands rather than losing it.
        try
        {
            worker?.AddThing(product);
            where = "carried";
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft deposit: " + ex.Message);
            where = "lost";
        }
    }

    static bool Put(Thing container, Thing product)
    {
        try
        {
            if (container == null || product == null || container.things == null)
            {
                return false;
            }

            container.AddThing(product);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft put: " + ex.Message);
            return false;
        }
    }

    static bool DropAt(Point? pos, Thing product)
    {
        if (pos == null || product == null)
        {
            return false;
        }

        try
        {
            EClass._zone.AddCard(product, pos);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft drop: " + ex.Message);
            return false;
        }
    }

    /// <summary>A container sharing the tile with the marked spot, if any.</summary>
    static Thing? ContainerAt(Point pos)
    {
        try
        {
            List<Thing>? things = EClass._map?.things;
            if (things == null)
            {
                return null;
            }

            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || t.isDestroyed || !t.IsContainer)
                {
                    continue;
                }

                if (t.pos.x == pos.x && t.pos.z == pos.z)
                {
                    return t;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft container at: " + ex.Message);
        }

        return null;
    }

    /// <summary>
    /// The drop point, if the zone has one. A tag this mod placed wins over one the player
    /// dropped by hand, so a sales tag serving the player's own shop keeps its meaning.
    /// </summary>
    internal static Thing? FindSpot()
    {
        try
        {
            List<Thing>? things = EClass._map?.things;
            if (things == null)
            {
                return null;
            }

            Thing? loose = null;

            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || t.isDestroyed || !IsSpotTag(t))
                {
                    continue;
                }

                bool marked = false;
                try
                {
                    marked = t.GetInt(SpotMarkerKey) > 0;
                }
                catch (Exception ex)
                {
                    Plugin.LogDebug("craft spot mark: " + ex.Message);
                }

                if (marked)
                {
                    return t;
                }

                if (loose == null)
                {
                    loose = t;
                }
            }

            return loose;
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft spot: " + ex.Message);
        }

        return null;
    }

    static bool IsSpotTag(Thing t)
    {
        try
        {
            return string.Equals(t.id ?? "", SpotThingId, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Drop a marker where the player is standing, so finished goods have a home. It is
    /// the vanilla sales tag, flipped to its placed look and stamped as ours. Vanilla only
    /// acts on a tag flagged for sale, and ours never is, so it carries no shop meaning —
    /// the player can still remove it from the context menu like any other tag.
    /// </summary>
    internal static bool PlaceSpot(Chara? at)
    {
        try
        {
            Chara? who = at ?? EClass.pc;
            if (who == null)
            {
                return false;
            }

            Thing? tag = ThingGen.Create(SpotThingId);
            if (tag == null)
            {
                return false;
            }

            tag.isOn = true;
            tag.SetInt(SpotMarkerKey, 1);

            Card? card = EClass._zone.AddCard(tag, who.pos);
            card?.Install();

            Msg.Say(LaborText.T("craft.msg.spotPlaced"));
            _dirty = true;
            return true;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft place spot: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// The bench for a factory id — vanilla's own rule, since a station's IdSource is the
    /// factory the recipe asks for. When the base has several of the same kind the nearest
    /// to the worker wins, so two cooks do not queue for one pot.
    /// </summary>
    static Thing? FindStation(string factory, Chara? near)
    {
        try
        {
            List<Thing>? things = EClass._map?.things;
            if (things == null)
            {
                return null;
            }

            Thing? best = null;
            int bestDist = int.MaxValue;

            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || t.isDestroyed || t.trait is not TraitCrafter crafter)
                {
                    continue;
                }

                string id = "";
                try
                {
                    id = crafter.IdSource ?? "";
                }
                catch
                {
                    id = "";
                }

                if (!string.Equals(id, factory, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (near == null)
                {
                    return t;
                }

                int dist;
                try
                {
                    dist = near.Dist(t);
                }
                catch
                {
                    dist = int.MaxValue;
                }

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            return best;
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft station: " + ex.Message);
        }

        return null;
    }

    static string SafeName(Chara c)
    {
        try
        {
            return c.Name ?? "";
        }
        catch
        {
            return "";
        }
    }

    // ────────────────────────────────────────────────────────────── save / load

    static string? SavePath()
    {
        try
        {
            string root = GameIO.pathCurrentSave;
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            return Path.Combine(root, "npclabor_craft.json");
        }
        catch
        {
            return null;
        }
    }

    internal static void Save()
    {
        _dirty = false;

        try
        {
            string? path = SavePath();
            if (path == null)
            {
                return;
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(Jobs, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft save failed: " + ex.Message);
        }
    }

    internal static void Load()
    {
        Jobs = new List<CraftJob>();
        CraftEngine.Invalidate();

        try
        {
            string? path = SavePath();
            if (path == null || !File.Exists(path))
            {
                return;
            }

            List<CraftJob>? jobs = JsonConvert.DeserializeObject<List<CraftJob>>(File.ReadAllText(path));
            if (jobs != null)
            {
                Jobs = jobs;
            }

            Plugin.LogDebug("craft loaded " + Jobs.Count + " job(s)");
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft load failed: " + ex.Message);
        }
    }

    internal static string Describe()
    {
        if (Jobs.Count == 0)
        {
            return LaborText.T("craft.state.none");
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Jobs.Count; i++)
        {
            sb.AppendLine("  " + Jobs[i].Describe());
        }

        return sb.ToString().TrimEnd();
    }
}
