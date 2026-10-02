using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace NpcLabor;

/// <summary>
/// Tunable balance numbers loaded from package/labor_config.json.
/// Missing/invalid fields fall back to coded defaults (current balance pass).
/// </summary>
internal sealed class LaborConfigFile
{
    [JsonProperty("features")]
    public FeatureConfigSection Features = new FeatureConfigSection();

    [JsonProperty("townLabor")]
    public TownLaborConfigSection TownLabor = new TownLaborConfigSection();

    [JsonProperty("dispatch")]
    public DispatchConfigSection Dispatch = new DispatchConfigSection();

    [JsonProperty("trade")]
    public TradeConfigSection Trade = new TradeConfigSection();

    [JsonProperty("processFuel")]
    public ProcessFuelConfigSection ProcessFuel = new ProcessFuelConfigSection();

    [JsonProperty("unlock")]
    public UnlockConfigSection Unlock = new UnlockConfigSection();
}

/// <summary>
/// Slice B optional fuel model. Off hands fuel handling back to vanilla (log / branch only,
/// added straight to the machine's charge). On uses burn value instead: one craft costs
/// <see cref="CostPerBurn"/> points, and every stack below pays points per item into a
/// per-machine pool. Setting any value to 0 removes that item from the fuel list.
/// </summary>
internal sealed class ProcessFuelConfigSection
{
    /// <summary>Use burn value instead of vanilla's log/branch refuel.</summary>
    [JsonProperty("customFuel")]
    public bool CustomFuel = true;

    /// <summary>Points one craft consumes.</summary>
    [JsonProperty("costPerBurn")]
    public int CostPerBurn = 5;

    /// <summary>Leaves and grass.</summary>
    [JsonProperty("valueLeaf")]
    public int ValueLeaf = 1;

    /// <summary>Twigs and branches.</summary>
    [JsonProperty("valueBranch")]
    public int ValueBranch = 2;

    /// <summary>Logs.</summary>
    [JsonProperty("valueLog")]
    public int ValueLog = 10;

    /// <summary>Logs made of carbon (charcoal) burn for double.</summary>
    [JsonProperty("valueCarbonLog")]
    public int ValueCarbonLog = 20;

    /// <summary>Branches made of carbon (charcoal).</summary>
    [JsonProperty("valueCarbonBranch")]
    public int ValueCarbonBranch = 5;
}

/// <summary>
/// Per-slice on/off switches. These gate the entry points: with a slice off the
/// board row disappears and no new job can be started, while anything already
/// running keeps settling so disabling a slice never strands cargo or wages.
/// The master <see cref="Enabled"/> switch is the hard off - it also stops the
/// hourly tick, which is what you want when the whole mod is being parked.
/// </summary>
internal sealed class FeatureConfigSection
{
    [JsonProperty("enabled")]
    public bool Enabled = true;

    [JsonProperty("dispatch")]
    public bool Dispatch = true;

    [JsonProperty("townLabor")]
    public bool TownLabor = true;

    [JsonProperty("trade")]
    public bool Trade = true;

    [JsonProperty("craft")]
    public bool Craft = true;
}

/// <summary>
/// Slice F (caravan trade). Distances are overworld grid steps along the walked
/// path, so every knob here is expressed in steps rather than in days.
/// </summary>
internal sealed class TradeConfigSection
{
    /// <summary>Single items heavier than this are cut from a fish pool. 0 disables the cap.</summary>
    [JsonProperty("heavyFishMaxWeight")]
    public int HeavyFishMaxWeight = 50;

    /// <summary>Steps of distance that carry no price bonus at all.</summary>
    [JsonProperty("distFreeSteps")]
    public int DistFreeSteps = 2;

    /// <summary>
    /// 0 = derive the anchor from <see cref="DistMultMaxDays"/> (the normal case).
    /// A positive number pins the anchor to an exact step count instead, which
    /// overrides the day based rule.
    /// </summary>
    [JsonProperty("distMultMaxSteps")]
    public int DistMultMaxSteps = 0;

    /// <summary>
    /// Walking days that pay the full distance multiplier. A caravan covers two
    /// overworld tiles a day, so 60 days = 120 steps = x<see cref="DistMultMax"/>.
    /// Read in game days because that is what the player sees in the trip estimate.
    /// </summary>
    [JsonProperty("distMultMaxDays")]
    public int DistMultMaxDays = 60;

    /// <summary>Hard ceiling for the distance multiplier.</summary>
    [JsonProperty("distMultMax")]
    public float DistMultMax = 2f;

    /// <summary>Crew size. Goods are split evenly, so this doubles as the capacity knob.</summary>
    [JsonProperty("crewMin")]
    public int CrewMin = 2;

    [JsonProperty("crewMax")]
    public int CrewMax = 6;

    /// <summary>Level used when generating a town's stock from the vanilla pools.</summary>
    [JsonProperty("stockLevel")]
    public int StockLevel = 20;

    /// <summary>Share of the shipped quantity sold per day, in percent of the total.</summary>
    [JsonProperty("sellPercentPerDay")]
    public int SellPercentPerDay = 20;

    /// <summary>Randomised number of days a full sell-off takes.</summary>
    [JsonProperty("sellDaysMin")]
    public int SellDaysMin = 2;

    [JsonProperty("sellDaysMax")]
    public int SellDaysMax = 5;

    /// <summary>Negotiation used by console previews when no live crew is supplied.</summary>
    [JsonProperty("crewFallbackNegotiation")]
    public int CrewFallbackNegotiation = 0;

    /// <summary>
    /// Working capital a two-town shuttle refuses to leave without. A shuttle earns
    /// its money by buying cheap and selling dear, so it has to be staked by the
    /// player: the chest ledger must already hold this much before the crew may set
    /// off. Nothing is conjured — an underfunded chest is a refusal, not a top-up.
    /// One-way and colony routes ignore this.
    /// </summary>
    [JsonProperty("shuttleMinMoney")]
    public int ShuttleMinMoney = 10000;
}

internal sealed class TownLaborConfigSection
{
    [JsonProperty("minWagePerHour")]
    public int MinWagePerHour = 50;

    [JsonProperty("expPerHour")]
    public int ExpPerHour = 12;

    [JsonProperty("furnitureTicketChancePercent")]
    public int FurnitureTicketChancePercent = 20;

    [JsonProperty("hoursMin")]
    public int HoursMin = 6;

    [JsonProperty("hoursMax")]
    public int HoursMax = 12;

    [JsonProperty("investTownHeadroom")]
    public int InvestTownHeadroom = 5;

    [JsonProperty("platBase")]
    public int PlatBase = 2;

    [JsonProperty("platPerInvestTier")]
    public int PlatPerInvestTier = 2;

    [JsonProperty("investTierSize")]
    public int InvestTierSize = 20;

    [JsonProperty("affinityBase")]
    public int AffinityBase = 10;
}

internal sealed class DispatchConfigSection
{
    [JsonProperty("lockpickPerChest")]
    public int LockpickPerChest = 20;

    [JsonProperty("lockpickMaxChests")]
    public int LockpickMaxChests = 12;

    [JsonProperty("bossRewardMult")]
    public float BossRewardMult = 1.2f;

    [JsonProperty("regionSpecialtyMult")]
    public int RegionSpecialtyMult = 2;

    [JsonProperty("expExploreSuccess")]
    public int ExpExploreSuccess = 80;

    [JsonProperty("expExploreFailure")]
    public int ExpExploreFailure = 40;

    [JsonProperty("expExploreRecall")]
    public int ExpExploreRecall = 20;

    [JsonProperty("expLockpickSuccess")]
    public int ExpLockpickSuccess = 50;

    [JsonProperty("expLockpickFailure")]
    public int ExpLockpickFailure = 25;

    [JsonProperty("expLockpickRecall")]
    public int ExpLockpickRecall = 10;

    [JsonProperty("expGatherSuccess")]
    public int ExpGatherSuccess = 60;

    [JsonProperty("expGatherFailure")]
    public int ExpGatherFailure = 30;

    [JsonProperty("expGatherRecall")]
    public int ExpGatherRecall = 15;

    [JsonProperty("fameBase")]
    public int FameBase = 30;

    [JsonProperty("famePerDanger")]
    public int FamePerDanger = 2;
}

internal sealed class UnlockConfigSection
{
    [JsonProperty("regionBranchLv")]
    public int RegionBranchLv = 3;

    [JsonProperty("dungeonBranchLv")]
    public int DungeonBranchLv = 5;
}

internal static class LaborConfig
{
    static LaborConfigFile _data = CreateDefaults();
    static bool _loaded;
    static string? _configPath;
    static float _lastWrite = -99f;
    static bool _writePending;

    internal static LaborConfigFile Data
    {
        get
        {
            EnsureLoaded();
            return _data;
        }
    }

    internal static TownLaborConfigSection Town
    {
        get
        {
            EnsureLoaded();
            return _data.TownLabor ?? (_data.TownLabor = new TownLaborConfigSection());
        }
    }

    internal static DispatchConfigSection Dispatch
    {
        get
        {
            EnsureLoaded();
            return _data.Dispatch ?? (_data.Dispatch = new DispatchConfigSection());
        }
    }

    internal static UnlockConfigSection Unlock
    {
        get
        {
            EnsureLoaded();
            return _data.Unlock ?? (_data.Unlock = new UnlockConfigSection());
        }
    }

    internal static ProcessFuelConfigSection ProcessFuel
    {
        get
        {
            EnsureLoaded();
            return _data.ProcessFuel ?? (_data.ProcessFuel = new ProcessFuelConfigSection());
        }
    }

    internal static TradeConfigSection Trade
    {
        get
        {
            EnsureLoaded();
            return _data.Trade ?? (_data.Trade = new TradeConfigSection());
        }
    }

    internal static FeatureConfigSection Features
    {
        get
        {
            EnsureLoaded();
            return _data.Features ?? (_data.Features = new FeatureConfigSection());
        }
    }

    /// <summary>Master switch. Off parks every slice and stops the hourly tick.</summary>
    internal static bool FeatureEnabled => Features.Enabled;

    /// <summary>
    /// Slice switches, already folded with the master switch. These gate entry
    /// points only (can I start a new job?) so a slice turned off mid-flight
    /// still lets the running job settle instead of stranding it.
    /// </summary>
    internal static bool FeatureDispatch => FeatureEnabled && Features.Dispatch;
    internal static bool FeatureTownLabor => FeatureEnabled && Features.TownLabor;
    internal static bool FeatureTrade => FeatureEnabled && Features.Trade;
    internal static bool FeatureCraft => FeatureEnabled && Features.Craft;

    internal static int TradeHeavyFishMaxWeight => Math.Max(0, Trade.HeavyFishMaxWeight);
    internal static int TradeDistFreeSteps => Math.Max(0, Trade.DistFreeSteps);

    /// <summary>Steps that reach the cap; 0 means "derive from the walking-day anchor".</summary>
    internal static int TradeDistMultMaxSteps => Math.Max(0, Trade.DistMultMaxSteps);

    /// <summary>Walking days that reach the cap; 0 falls back to the overworld length.</summary>
    internal static int TradeDistMultMaxDays => Math.Max(0, Trade.DistMultMaxDays);

    internal static double TradeDistMultMax => Math.Max(1.0, Trade.DistMultMax);

    internal static int TradeCrewMin => Math.Max(1, Trade.CrewMin);
    internal static int TradeCrewMax => Math.Max(TradeCrewMin, Trade.CrewMax);
    internal static int TradeStockLevel => Math.Max(1, Trade.StockLevel);
    internal static int TradeSellPercentPerDay => Math.Clamp(Trade.SellPercentPerDay, 1, 100);
    internal static int TradeSellDaysMin => Math.Max(1, Trade.SellDaysMin);
    internal static int TradeSellDaysMax => Math.Max(TradeSellDaysMin, Trade.SellDaysMax);
    internal static int TradeCrewFallbackNegotiation => Math.Max(0, Trade.CrewFallbackNegotiation);
    internal static int TradeShuttleMinMoney => Math.Max(0, Trade.ShuttleMinMoney);

    internal static int MinWagePerHour => Math.Max(0, Town.MinWagePerHour);
    internal static int ExpPerHour => Math.Max(0, Town.ExpPerHour);
    internal static int FurnitureTicketChancePercent => Mathf.Clamp(Town.FurnitureTicketChancePercent, 0, 100);
    internal static int HoursMin => Math.Max(1, Town.HoursMin);
    internal static int HoursMax => Math.Max(HoursMin, Town.HoursMax);
    internal static int InvestTownHeadroom => Math.Max(0, Town.InvestTownHeadroom);
    internal static int PlatBase => Math.Max(0, Town.PlatBase);
    internal static int PlatPerInvestTier => Math.Max(0, Town.PlatPerInvestTier);
    internal static int InvestTierSize => Math.Max(1, Town.InvestTierSize);
    internal static int AffinityBase => Math.Max(0, Town.AffinityBase);

    internal static int LockpickPerChest => Math.Max(1, Dispatch.LockpickPerChest);
    internal static int LockpickMaxChests => Math.Max(0, Dispatch.LockpickMaxChests);
    internal static float BossRewardMult => Mathf.Max(0.1f, Dispatch.BossRewardMult);
    internal static int RegionSpecialtyMult => Math.Max(1, Dispatch.RegionSpecialtyMult);
    internal static int FameBase => Math.Max(0, Dispatch.FameBase);
    internal static int FamePerDanger => Math.Max(0, Dispatch.FamePerDanger);

    internal static int RegionUnlockBranchLv => Math.Max(0, Unlock.RegionBranchLv);
    internal static int DungeonUnlockBranchLv => Math.Max(0, Unlock.DungeonBranchLv);

    /// <summary>Absolute path of labor_config.json, or "" when the mod dir is unknown.</summary>
    internal static string ConfigPath
    {
        get
        {
            if (_configPath == null)
            {
                _configPath = ResolveConfigPath();
            }

            return _configPath;
        }
    }

    /// <summary>
    /// Write the live values back to labor_config.json. Called straight from the
    /// config screen, so a slider drag is throttled down to one write every few
    /// frames and <see cref="TickAutoSave"/> flushes the tail once the drag stops.
    /// </summary>
    internal static void Save(bool immediate = false)
    {
        string path = ConfigPath;
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (!immediate && Time.realtimeSinceStartup - _lastWrite < SaveThrottleSeconds)
        {
            _writePending = true;
            return;
        }

        try
        {
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(_data, Formatting.Indented));
            _lastWrite = Time.realtimeSinceStartup;
            _writePending = false;
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("labor_config save failed: " + ex.Message);
        }
    }

    /// <summary>Cheap per-frame flush for a throttled save. No-op when nothing is pending.</summary>
    internal static void TickAutoSave()
    {
        if (!_writePending)
        {
            return;
        }

        if (Time.realtimeSinceStartup - _lastWrite >= SaveThrottleSeconds)
        {
            Save(immediate: true);
        }
    }

    /// <summary>Throw the whole table back to the coded defaults and persist that.</summary>
    internal static void ResetToDefaults()
    {
        _data = CreateDefaults();
        _loaded = true;
        Save(immediate: true);
        Plugin.LogInfo("labor config reset to defaults.");
    }

    internal static void Reload()
    {
        _loaded = false;
        _data = CreateDefaults();
        EnsureLoaded();
    }

    internal static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _data = CreateDefaults();

        try
        {
            string path = ConfigPath;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                Plugin.LogDebug("labor_config.json not found; using defaults.");
                return;
            }

            string json = System.IO.File.ReadAllText(path);
            LaborConfigFile? file = JsonConvert.DeserializeObject<LaborConfigFile>(json);
            if (file == null)
            {
                Plugin.LogWarn("labor_config.json empty; using defaults.");
                return;
            }

            _data = MergeWithDefaults(file);
            Plugin.LogInfo("labor config loaded from " + path);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("labor_config load failed: " + ex.Message);
            _data = CreateDefaults();
        }
    }

    static LaborConfigFile CreateDefaults()
        => new LaborConfigFile
        {
            TownLabor = new TownLaborConfigSection(),
            Dispatch = new DispatchConfigSection(),
            Trade = new TradeConfigSection(),
            Unlock = new UnlockConfigSection(),
            ProcessFuel = new ProcessFuelConfigSection(),
        };

    static LaborConfigFile MergeWithDefaults(LaborConfigFile file)
    {
        LaborConfigFile d = CreateDefaults();
        if (file.Features != null)
        {
            d.Features = file.Features;
        }

        if (file.TownLabor != null)
        {
            d.TownLabor = file.TownLabor;
        }

        if (file.Dispatch != null)
        {
            d.Dispatch = file.Dispatch;
        }

        if (file.Trade != null)
        {
            d.Trade = file.Trade;
        }

        if (file.Unlock != null)
        {
            d.Unlock = file.Unlock;
        }

        if (file.ProcessFuel != null)
        {
            d.ProcessFuel = file.ProcessFuel;
        }

        return d;
    }

    /// <summary>How long a slider must sit still before the throttled write lands.</summary>
    const float SaveThrottleSeconds = 0.4f;

    static string ResolveConfigPath()
    {
        try
        {
            string? asm = typeof(LaborConfig).Assembly.Location;
            if (!string.IsNullOrEmpty(asm))
            {
                string dir = Path.GetDirectoryName(asm) ?? "";
                if (!string.IsNullOrEmpty(dir))
                {
                    return Path.Combine(dir, "labor_config.json");
                }
            }
        }
        catch
        {
        }


         // No hardcoded fallback path — rely on Assembly.Location relative lookup only.
         return "";
    }

    internal static int DispatchExpExplore(global::NpcLabor.Dispatch.DispatchSettleKind kind)
    {
        EnsureLoaded();
        return kind switch
        {
            global::NpcLabor.Dispatch.DispatchSettleKind.Success => Math.Max(0, Dispatch.ExpExploreSuccess),
            global::NpcLabor.Dispatch.DispatchSettleKind.Failure => Math.Max(0, Dispatch.ExpExploreFailure),
            global::NpcLabor.Dispatch.DispatchSettleKind.Recall => Math.Max(0, Dispatch.ExpExploreRecall),
            _ => 10,
        };
    }

    internal static int DispatchExpLockpick(global::NpcLabor.Dispatch.DispatchSettleKind kind)
    {
        EnsureLoaded();
        return kind switch
        {
            global::NpcLabor.Dispatch.DispatchSettleKind.Success => Math.Max(0, Dispatch.ExpLockpickSuccess),
            global::NpcLabor.Dispatch.DispatchSettleKind.Failure => Math.Max(0, Dispatch.ExpLockpickFailure),
            global::NpcLabor.Dispatch.DispatchSettleKind.Recall => Math.Max(0, Dispatch.ExpLockpickRecall),
            _ => 5,
        };
    }

    internal static int DispatchExpGather(global::NpcLabor.Dispatch.DispatchSettleKind kind)
    {
        EnsureLoaded();
        return kind switch
        {
            global::NpcLabor.Dispatch.DispatchSettleKind.Success => Math.Max(0, Dispatch.ExpGatherSuccess),
            global::NpcLabor.Dispatch.DispatchSettleKind.Failure => Math.Max(0, Dispatch.ExpGatherFailure),
            global::NpcLabor.Dispatch.DispatchSettleKind.Recall => Math.Max(0, Dispatch.ExpGatherRecall),
            _ => 8,
        };
    }
}
