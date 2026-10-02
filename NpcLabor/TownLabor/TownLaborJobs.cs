using System;
using System.Collections.Generic;
using System.Reflection;

namespace NpcLabor.TownLabor;

/// <summary>Fixed v1 town shop labor catalog (shop + book/scholar kinds).</summary>
internal enum TownLaborJobKind
{
    InnChore = 1,
    KitchenHelp = 2,
    GeneralClerk = 3,
    SmithAssist = 4,
    FoodClerk = 5,
    BookClerk = 6,
    ScholarAssist = 7,
}

/// <summary>
/// Job definition + client matching. PC self-work (time+SP) is implemented via TownLaborManager.TryStartSelf.
/// </summary>
internal sealed class TownLaborJobDef
{
    internal TownLaborJobKind Kind;
    internal string Id = "";
    internal string Title = "";
    /// <summary>Short board-line reasons; pick one by seed for variety.</summary>
    internal string[] DetailVariants = Array.Empty<string>();
    internal int SkillId;
    internal string SkillLabel = "";
    /// <summary>Primary leftover card ids tried in order.</summary>
    internal string[] LeftoverIds = Array.Empty<string>();
    internal Emo WorkEmo = Emo.happy;

    /// <summary>Stable pick of a short board detail for this offer.</summary>
    internal string PickDetail(int seed)
    {
        if (DetailVariants == null || DetailVariants.Length == 0)
        {
            return NpcLabor.LaborText.T("town.ui.needHelp");
        }

        int idx = seed;
        if (idx < 0)
        {
            idx = -idx;
        }

        return DetailVariants[idx % DetailVariants.Length];
    }
}

internal static class TownLaborJobs
{
    static string J(string key) => NpcLabor.LaborText.T("job." + key);

    static string J(string key, params object[] args) => NpcLabor.LaborText.T("job." + key, args);

    // Life skill ids from SKILL.
    internal const int SkillCooking = 287;
    internal const int SkillNegotiation = 291;
    internal const int SkillBlacksmith = 256;
    internal const int SkillHandicraft = 261;
    internal const int SkillWeightlifting = 207;
    internal const int SkillCarpentry = 255;
    internal const int SkillFarming = 286;
    internal const int SkillReading = 285;
    internal const int SkillMemorization = 307;
    internal const int SkillAppraising = 289;

    internal const int MaxConcurrentPerTown = 2;
    /// <summary>Board shows a random sample of free offers, not every shop.</summary>
    internal const int MinBoardOffers = 2;
    internal const int MaxBoardOffers = 3;

    /// <summary>Work duration is random hoursMin-hoursMax (no difficulty axis).</summary>
    internal static int RollWorkHours()
    {
        int min = LaborConfig.HoursMin;
        int max = LaborConfig.HoursMax;
        int span = Math.Max(0, max - min);
        try
        {
            return min + EClass.rnd(span + 1);
        }
        catch
        {
            return Math.Max(min, (min + max) / 2);
        }
    }

    /// <summary>Prefer board-offered hours when already in configured range; else roll.</summary>
    internal static int ResolveWorkHours(int offeredHours)
    {
        int min = LaborConfig.HoursMin;
        int max = LaborConfig.HoursMax;
        if (offeredHours >= min && offeredHours <= max)
        {
            return offeredHours;
        }

        return RollWorkHours();
    }

    /// <summary>Client shop level from TraitMerchant.ShopLv (or formula fallback).</summary>
    internal static int GetShopLv(Chara? client)
    {
        if (client == null)
        {
            return 1;
        }

        try
        {
            if (client.trait is TraitMerchant m)
            {
                int lv = m.ShopLv;
                if (lv > 0)
                {
                    return Math.Max(1, lv);
                }
            }
        }
        catch
        {
        }

        int development = 0;
        int invest = 0;
        int bonus = 0;
        try
        {
            Zone? z = client.currentZone ?? EClass._zone;
            if (z != null)
            {
                development = Math.Max(0, z.development);
            }
        }
        catch
        {
        }

        try { invest = Math.Max(0, client.c_invest); } catch { invest = 0; }
        try { bonus = Guild.Merchant.InvestBonus(); } catch { bonus = 0; }
        int shop = development / 10 + invest * (100 + bonus) / 100 + 1;
        return Math.Max(1, shop);
    }

    /// <summary>
    /// Soft town investment ceiling for shop invest grants.
    /// Allow c_invest++ only while resulting ShopLv stays near town development base.
    /// </summary>
    internal static bool CanRaiseShopInvestUnderTownCap(Chara? client)
    {
        if (client == null)
        {
            return false;
        }

        int development = 0;
        int invest = 0;
        int bonus = 0;
        try
        {
            Zone? z = client.currentZone ?? client.homeZone ?? EClass._zone;
            if (z != null)
            {
                development = Math.Max(0, z.development);
            }
        }
        catch
        {
        }

        try { invest = Math.Max(0, client.c_invest); } catch { invest = 0; }
        try { bonus = Guild.Merchant.InvestBonus(); } catch { bonus = 0; }

        int baseFromTown = development / 10 + 1;
        int nextShop = development / 10 + (invest + 1) * (100 + bonus) / 100 + 1;
        return nextShop <= baseFromTown + LaborConfig.InvestTownHeadroom;
    }

    internal static int GetWorkerSkill(Chara? worker, int skillId)
    {
        if (worker == null || skillId <= 0)
        {
            return 0;
        }

        try
        {
            return Math.Max(0, worker.Evalue(skillId));
        }
        catch
        {
            return 0;
        }
    }

    internal static IReadOnlyList<TownLaborJobDef> All { get; } = new List<TownLaborJobDef>
    {
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.InnChore,
            Id = "inn_chore",
            Title = J("inn.title"),
            DetailVariants = new[]
            {
                J("inn.r1"),
                J("inn.r2"),
                J("inn.r3"),
                J("inn.r4"),
            },
            SkillId = SkillWeightlifting,
            SkillLabel = J("inn.skill"),
            LeftoverIds = new[] { "ration", "bread", "beer", "wine", "junk" },
            WorkEmo = Emo.water,
        },
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.KitchenHelp,
            Id = "kitchen_help",
            Title = J("kitchen.title"),
            DetailVariants = new[]
            {
                J("kitchen.r1"),
                J("kitchen.r2"),
                J("kitchen.r3"),
                J("kitchen.r4"),
            },
            SkillId = SkillCooking,
            SkillLabel = J("kitchen.skill"),
            LeftoverIds = new[] { "flour", "egg", "meat", "vegetable", "bread", "ration" },
            WorkEmo = Emo.cook,
        },
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.GeneralClerk,
            Id = "general_clerk",
            Title = J("general.title"),
            DetailVariants = new[]
            {
                J("general.r1"),
                J("general.r2"),
                J("general.r3"),
                J("general.r4"),
            },
            SkillId = SkillNegotiation,
            SkillLabel = J("general.skill"),
            LeftoverIds = new[] { "junk", "rope", "torch", "bandage", "pottery" },
            WorkEmo = Emo.idea,
        },
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.SmithAssist,
            Id = "smith_assist",
            Title = J("smith.title"),
            DetailVariants = new[]
            {
                J("smith.r1"),
                J("smith.r2"),
                J("smith.r3"),
                J("smith.r4"),
            },
            SkillId = SkillBlacksmith,
            SkillLabel = J("smith.skill"),
            LeftoverIds = new[] { "scrap", "ingot", "nail", "ore", "chunk" },
            WorkEmo = Emo.build,
        },
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.FoodClerk,
            Id = "food_clerk",
            Title = J("food.title"),
            DetailVariants = new[]
            {
                J("food.r1"),
                J("food.r2"),
                J("food.r3"),
                J("food.r4"),
            },
            SkillId = SkillCooking,
            SkillLabel = J("food.skill"),
            LeftoverIds = new[] { "fruit", "vegetable", "egg", "meat", "flour", "ration" },
            WorkEmo = Emo.hungry,
        },
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.BookClerk,
            Id = "book_clerk",
            Title = J("book.title"),
            DetailVariants = new[]
            {
                J("book.r1"),
                J("book.r2"),
                J("book.r3"),
                J("book.r4"),
            },
            SkillId = SkillReading,
            SkillLabel = J("book.skill"),
            LeftoverIds = new[] { "book", "book_exp", "scroll", "paper", "junk" },
            WorkEmo = Emo.idea,
        },
        new TownLaborJobDef
        {
            Kind = TownLaborJobKind.ScholarAssist,
            Id = "scholar_assist",
            Title = J("scholar.title"),
            DetailVariants = new[]
            {
                J("scholar.r1"),
                J("scholar.r2"),
                J("scholar.r3"),
                J("scholar.r4"),
            },
            SkillId = SkillMemorization,
            SkillLabel = J("scholar.skill"),
            LeftoverIds = new[] { "book", "book_exp", "paper", "bandage", "junk" },
            WorkEmo = Emo.idea,
        },
    };

    internal static TownLaborJobDef? Get(TownLaborJobKind kind)
    {
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i].Kind == kind)
            {
                return All[i];
            }
        }

        return null;
    }

    internal static TownLaborJobDef? GetById(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        for (int i = 0; i < All.Count; i++)
        {
            if (string.Equals(All[i].Id, id, StringComparison.OrdinalIgnoreCase))
            {
                return All[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Player-facing board title for a client. Prefer shop identity over generic clerk labels.
    /// Example: fishmonger -> "鱼店需要人手" instead of "食品店员".
    /// </summary>
    internal static string ResolveBoardTitle(Chara? client, TownLaborJobDef? def)
    {
        string fallback = def?.Title
            ?? (def != null ? def.Id : NpcLabor.LaborTerms.TownWork);
        if (client == null)
        {
            return fallback;
        }

        Trait? trait = null;
        try { trait = client.trait; } catch { trait = null; }

        try
        {
            if (trait is TraitInnkeeper)
            {
                return J("shop.inn.need");
            }

            if (trait is TraitChef)
            {
                return J("shop.kitchen.need");
            }

            if (trait is TraitMerchantBook
                || (trait is TraitMerchant mb && (mb.ShopType == ShopType.Book || mb.ShopType == ShopType.RedBook)))
            {
                return J("shop.book.need");
            }

            if (trait is TraitGuildClerk
                || trait is TraitClerk_Mage
                || trait is TraitClerk_Merchant
                || trait is TraitClerk_Fighter
                || trait is TraitClerk_Thief
                || trait is TraitGuildPersonnel
                || trait is TraitHealer
                || trait is TraitMerchantPlat
                || trait is TraitMerchantMagic)
            {
                if (trait is TraitGuildDoorman)
                {
                    return fallback;
                }

                return J("shop.scholar.need");
            }

            if (trait is TraitMerchantWeapon
                || string.Equals(SafeAmbience(trait), "blacksmith", StringComparison.OrdinalIgnoreCase)
                || (trait is TraitMerchant mw && mw.ShopType == ShopType.Weapon))
            {
                return J("shop.smith.need");
            }

            if (trait is TraitMerchantFish)
            {
                return J("shop.fish.need");
            }

            if (trait is TraitMerchantMeat)
            {
                return J("shop.meat.need");
            }

            if (trait is TraitMerchantFruit)
            {
                return J("shop.fruit.need");
            }

            if (trait is TraitMerchantBread)
            {
                return J("shop.bread.need");
            }

            if (trait is TraitMerchantMilk)
            {
                return J("shop.milk.need");
            }

            if (trait is TraitMerchantBooze)
            {
                return J("shop.booze.need");
            }

            if (trait is TraitMerchantFood
                || (trait is TraitMerchant mf && mf.ShopType == ShopType.Food))
            {
                // Serve-capable food shop still kitchen; else generic food shop.
                try
                {
                    if (trait is TraitCitizen c2 && c2.CanServeFood)
                    {
                        return J("shop.kitchen.need");
                    }
                }
                catch
                {
                }

                return J("shop.food.need");
            }

            if (trait is TraitMerchantGeneral || trait is TraitMerchantGeneralExotic
                || trait is TraitMerchantJunk || trait is TraitMerchantSouvenir
                || (trait is TraitMerchant mg
                    && (mg.ShopType == ShopType.General
                        || mg.ShopType == ShopType.GeneralExotic
                        || mg.ShopType == ShopType.Goods
                        || mg.ShopType == ShopType.Junk
                        || mg.ShopType == ShopType.Souvenir)))
            {
                if (trait is TraitMerchantJunk || (trait is TraitMerchant mj && mj.ShopType == ShopType.Junk))
                {
                    return J("shop.junk.need");
                }

                if (trait is TraitMerchantSouvenir || (trait is TraitMerchant ms && ms.ShopType == ShopType.Souvenir))
                {
                    return J("shop.souvenir.need");
                }

                return J("shop.general.need");
            }
        }
        catch
        {
        }

        // Fallback: "<client job/name> needs help" when we only know the generic def.
        try
        {
            string who = client.NameSimple ?? client.Name ?? "";
            string job = "";
            try { job = client.job?.GetName() ?? ""; } catch { job = ""; }
            if (string.IsNullOrEmpty(job))
            {
                try { job = client.trait?.Name ?? ""; } catch { job = ""; }
            }

            if (!string.IsNullOrEmpty(job))
            {
                return J("shop.generic.need", job);
            }

            if (!string.IsNullOrEmpty(who))
            {
                return J("shop.generic.need", who);
            }
        }
        catch
        {
        }

        return fallback;
    }

    /// <summary>
    /// Match the best single job for a town NPC. One client maps to at most one job kind.
    /// Priority: inn -> kitchen -> book -> scholar -> smith -> food -> general.
    /// </summary>
    internal static TownLaborJobDef? MatchJob(Chara? client)
    {
        if (client == null || client.isDead || client.IsPC)
        {
            return null;
        }

        Trait? trait = null;
        try
        {
            trait = client.trait;
        }
        catch
        {
            return null;
        }

        if (trait == null)
        {
            return null;
        }

        // Inn first (also is TraitMerchantFood).
        if (trait is TraitInnkeeper)
        {
            return Get(TownLaborJobKind.InnChore);
        }

        // Kitchen / serve food (chef).
        if (trait is TraitChef)
        {
            return Get(TownLaborJobKind.KitchenHelp);
        }

        // Bookstore / red-book clerk - before broad merchant fallback.
        if (trait is TraitMerchantBook)
        {
            return Get(TownLaborJobKind.BookClerk);
        }

        try
        {
            if (trait is TraitMerchant mb && (mb.ShopType == ShopType.Book || mb.ShopType == ShopType.RedBook))
            {
                return Get(TownLaborJobKind.BookClerk);
            }
        }
        catch
        {
        }

        // Scholar / guild clerk / healer desk / plat library - civil-servant-like desk work.
        if (trait is TraitGuildClerk
            || trait is TraitClerk_Mage
            || trait is TraitClerk_Merchant
            || trait is TraitClerk_Fighter
            || trait is TraitClerk_Thief
            || trait is TraitGuildPersonnel
            || trait is TraitHealer
            || trait is TraitMerchantPlat
            || trait is TraitMerchantMagic)
        {
            // Doorman is guild personnel but not desk work.
            if (trait is TraitGuildDoorman)
            {
                return null;
            }

            return Get(TownLaborJobKind.ScholarAssist);
        }

        try
        {
            if (trait is TraitMerchant ms
                && (ms.ShopType == ShopType.Guild
                    || ms.ShopType == ShopType.Plat
                    || ms.ShopType == ShopType.Magic
                    || ms.ShopType == ShopType.Healer))
            {
                return Get(TownLaborJobKind.ScholarAssist);
            }

            if (trait is TraitHealer)
            {
                return Get(TownLaborJobKind.ScholarAssist);
            }
        }
        catch
        {
        }

        // Blacksmith / weapon merchant.
        if (trait is TraitMerchantWeapon
            || string.Equals(SafeAmbience(trait), "blacksmith", StringComparison.OrdinalIgnoreCase))
        {
            return Get(TownLaborJobKind.SmithAssist);
        }

        try
        {
            if (trait is TraitMerchant m && m.ShopType == ShopType.Weapon)
            {
                return Get(TownLaborJobKind.SmithAssist);
            }
        }
        catch
        {
        }

        // Food merchant (non-inn). Serve-capable -> kitchen help, else food clerk.
        if (trait is TraitMerchantFood || trait is TraitMerchantMeat || trait is TraitMerchantFish
            || trait is TraitMerchantFruit || trait is TraitMerchantBread || trait is TraitMerchantMilk
            || trait is TraitMerchantBooze)
        {
            try
            {
                if (trait is TraitCitizen c2 && c2.CanServeFood)
                {
                    return Get(TownLaborJobKind.KitchenHelp);
                }
            }
            catch
            {
            }

            return Get(TownLaborJobKind.FoodClerk);
        }

        try
        {
            if (trait is TraitMerchant m2 && m2.ShopType == ShopType.Food)
            {
                return Get(TownLaborJobKind.FoodClerk);
            }
        }
        catch
        {
        }

        // True general goods only - junk/souvenir/general shelves.
        // Do NOT collapse book/scholar/magic/etc. into 杂货店员.
        if (trait is TraitMerchantGeneral || trait is TraitMerchantGeneralExotic
            || trait is TraitMerchantJunk || trait is TraitMerchantSouvenir)
        {
            return Get(TownLaborJobKind.GeneralClerk);
        }

        try
        {
            if (trait is TraitMerchant m3
                && (m3.ShopType == ShopType.General
                    || m3.ShopType == ShopType.GeneralExotic
                    || m3.ShopType == ShopType.Goods
                    || m3.ShopType == ShopType.Junk
                    || m3.ShopType == ShopType.Souvenir))
            {
                return Get(TownLaborJobKind.GeneralClerk);
            }
        }
        catch
        {
        }

        // No broad "any investable merchant -> general clerk" fallback.
        return null;
    }

    static string? SafeAmbience(Trait trait)
    {
        if (trait == null)
        {
            return null;
        }

        // IdAmbience is not always on base Trait; read via reflection when present.
        try
        {
            var prop = trait.GetType().GetProperty("IdAmbience");
            if (prop != null)
            {
                return prop.GetValue(trait) as string;
            }
        }
        catch
        {
        }

        try
        {
            var field = trait.GetType().GetField("IdAmbience");
            if (field != null)
            {
                return field.GetValue(trait) as string;
            }
        }
        catch
        {
        }

        return null;
    }

}
