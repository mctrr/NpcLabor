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
    [JsonProperty("townLabor")]
    public TownLaborConfigSection TownLabor = new TownLaborConfigSection();

    [JsonProperty("dispatch")]
    public DispatchConfigSection Dispatch = new DispatchConfigSection();

    [JsonProperty("unlock")]
    public UnlockConfigSection Unlock = new UnlockConfigSection();
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
            string? path = FindConfigPath();
            if (path == null || !File.Exists(path))
            {
                Plugin.LogDebug("labor_config.json not found; using defaults.");
                return;
            }

            string json = File.ReadAllText(path);
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
            Unlock = new UnlockConfigSection(),
        };

    static LaborConfigFile MergeWithDefaults(LaborConfigFile file)
    {
        LaborConfigFile d = CreateDefaults();
        if (file.TownLabor != null)
        {
            d.TownLabor = file.TownLabor;
        }

        if (file.Dispatch != null)
        {
            d.Dispatch = file.Dispatch;
        }

        if (file.Unlock != null)
        {
            d.Unlock = file.Unlock;
        }

        return d;
    }

    static string? FindConfigPath()
    {
        try
        {
            string? asm = typeof(LaborConfig).Assembly.Location;
            if (!string.IsNullOrEmpty(asm))
            {
                string dir = Path.GetDirectoryName(asm) ?? "";
                string p = Path.Combine(dir, "labor_config.json");
                if (File.Exists(p))
                {
                    return p;
                }
            }
        }
        catch
        {
        }


         // No hardcoded fallback path — rely on Assembly.Location relative lookup only.
         return null;
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
