// ---- around 18 ----
		Door,
		Illumination,
		DefaultNoAnime,
		SignalAnime,
		FakeBlock,
		FakeObj
	}

	public enum CopyShopType
	{
		None,
		Item,
		Spellbook
	}

	public static TraitSelfFactory SelfFactory = new TraitSelfFactory();

	public Card owner;

	protected static List<Point> listRadiusPoints = new List<Point>();

	public string[] Params
	{
		get
		{
			if (!owner.c_editorTraitVal.IsEmpty())
			{
				return ("," + owner.c_editorTraitVal).Split(',');
			}
			return owner.sourceCard.trait;
		}
	}

	public virtual byte WeightMod => 0;
// ---- around 524 ----
					return "switch_off";
				}
				return "torch_unlit";
			}
			return "switch_off_electricity";
		}
	}

	public virtual int ShopLv => Mathf.Max(1, EClass._zone.development / 10 + owner.c_invest * (100 + Guild.Merchant.InvestBonus()) / 100 + 1);

	public virtual CopyShopType CopyShop => CopyShopType.None;

	public virtual int NumCopyItem => 2 + Mathf.Min(owner.c_invest / 10, 3);

	public virtual ShopType ShopType
	{
		get
		{
			if (!owner.GetStr("merchant_override").IsEmpty())
			{
				return ShopType.CustomContent;
			}
			return ShopType.None;
		}
	}

	public virtual CurrencyType CurrencyType => CurrencyType.Money;

	public virtual PriceType PriceType => PriceType.Default;

	public virtual bool AllowSell
	{
		get
		{
// ---- around 526 ----
				return "torch_unlit";
			}
			return "switch_off_electricity";
		}
	}

	public virtual int ShopLv => Mathf.Max(1, EClass._zone.development / 10 + owner.c_invest * (100 + Guild.Merchant.InvestBonus()) / 100 + 1);

	public virtual CopyShopType CopyShop => CopyShopType.None;

	public virtual int NumCopyItem => 2 + Mathf.Min(owner.c_invest / 10, 3);

	public virtual ShopType ShopType
	{
		get
		{
			if (!owner.GetStr("merchant_override").IsEmpty())
			{
				return ShopType.CustomContent;
			}
			return ShopType.None;
		}
	}

	public virtual CurrencyType CurrencyType => CurrencyType.Money;

	public virtual PriceType PriceType => PriceType.Default;

	public virtual bool AllowSell
	{
		get
		{
			if (CurrencyType != CurrencyType.Money)
			{
// ---- around 528 ----
			return "switch_off_electricity";
		}
	}

	public virtual int ShopLv => Mathf.Max(1, EClass._zone.development / 10 + owner.c_invest * (100 + Guild.Merchant.InvestBonus()) / 100 + 1);

	public virtual CopyShopType CopyShop => CopyShopType.None;

	public virtual int NumCopyItem => 2 + Mathf.Min(owner.c_invest / 10, 3);

	public virtual ShopType ShopType
	{
		get
		{
			if (!owner.GetStr("merchant_override").IsEmpty())
			{
				return ShopType.CustomContent;
			}
			return ShopType.None;
		}
	}

	public virtual CurrencyType CurrencyType => CurrencyType.Money;

	public virtual PriceType PriceType => PriceType.Default;

	public virtual bool AllowSell
	{
		get
		{
			if (CurrencyType != CurrencyType.Money)
			{
				return CurrencyType == CurrencyType.None;
			}
// ---- around 530 ----
	}

	public virtual int ShopLv => Mathf.Max(1, EClass._zone.development / 10 + owner.c_invest * (100 + Guild.Merchant.InvestBonus()) / 100 + 1);

	public virtual CopyShopType CopyShop => CopyShopType.None;

	public virtual int NumCopyItem => 2 + Mathf.Min(owner.c_invest / 10, 3);

	public virtual ShopType ShopType
	{
		get
		{
			if (!owner.GetStr("merchant_override").IsEmpty())
			{
				return ShopType.CustomContent;
			}
			return ShopType.None;
		}
	}

	public virtual CurrencyType CurrencyType => CurrencyType.Money;

	public virtual PriceType PriceType => PriceType.Default;

	public virtual bool AllowSell
	{
		get
		{
			if (CurrencyType != CurrencyType.Money)
			{
				return CurrencyType == CurrencyType.None;
			}
			return true;
		}
// ---- around 536 ----
	public virtual int NumCopyItem => 2 + Mathf.Min(owner.c_invest / 10, 3);

	public virtual ShopType ShopType
	{
		get
		{
			if (!owner.GetStr("merchant_override").IsEmpty())
			{
				return ShopType.CustomContent;
			}
			return ShopType.None;
		}
	}

	public virtual CurrencyType CurrencyType => CurrencyType.Money;

	public virtual PriceType PriceType => PriceType.Default;

	public virtual bool AllowSell
	{
		get
		{
			if (CurrencyType != CurrencyType.Money)
			{
				return CurrencyType == CurrencyType.None;
			}
			return true;
		}
	}

	public virtual int CostRerollShop
	{
		get
		{
// ---- around 538 ----
	public virtual ShopType ShopType
	{
		get
		{
			if (!owner.GetStr("merchant_override").IsEmpty())
			{
				return ShopType.CustomContent;
			}
			return ShopType.None;
		}
	}

	public virtual CurrencyType CurrencyType => CurrencyType.Money;

	public virtual PriceType PriceType => PriceType.Default;

	public virtual bool AllowSell
	{
		get
		{
			if (CurrencyType != CurrencyType.Money)
			{
				return CurrencyType == CurrencyType.None;
			}
			return true;
		}
	}

	public virtual int CostRerollShop
	{
		get
		{
			if (CurrencyType == CurrencyType.Money || CurrencyType == CurrencyType.Influence)
			{
// ---- around 1688 ----
			if (obj != null && EClass.world.date.IsExpired(obj.dateRefresh))
			{
				return Emo2.restock;
			}
		}
		int c_dateStockExpire = owner.c_dateStockExpire;
		if (c_dateStockExpire != 0 && EClass.world.date.IsExpired(c_dateStockExpire))
		{
			if (ShopType == ShopType.None)
			{
				return Emo2.blessing;
			}
			return Emo2.restock;
		}
		return Emo2.none;
	}

	public virtual void OnBarter(bool reroll = false)
	{
		Thing t = owner.things.Find("chest_merchant");
		if (t == null)
		{
			t = ThingGen.Create("chest_merchant");
			owner.AddThing(t);
		}
		t.c_lockLv = 0;
		if (!EClass.world.date.IsExpired(owner.c_dateStockExpire) || (RestockDay < 0 && owner.isRestocking))
		{
			return;
		}
		owner.c_dateStockExpire = EClass.world.date.GetRaw(24 * RestockDay);
		owner.isRestocking = true;
		t.things.DestroyAll((Thing _t) => _t.GetInt(101) != 0);
		foreach (Thing thing11 in t.things)
// ---- around 1717 ----
		}
		owner.c_dateStockExpire = EClass.world.date.GetRaw(24 * RestockDay);
		owner.isRestocking = true;
		t.things.DestroyAll((Thing _t) => _t.GetInt(101) != 0);
		foreach (Thing thing11 in t.things)
		{
			thing11.invX = -1;
		}
		switch (ShopType)
		{
		case ShopType.Plat:
			NoRestock(ThingGen.Create("lucky_coin").SetNum(10));
			NoRestock(ThingGen.CreateSkillbook(6662));
			NoRestock(ThingGen.CreateSkillbook(6664));
			Add("book_exp", 10, 0);
			break;
		case ShopType.Copy:
		{
			Thing c_copyContainer = owner.c_copyContainer;
			if (c_copyContainer == null)
			{
				break;
			}
			int num7 = 0;
			foreach (Thing thing12 in c_copyContainer.things)
			{
				if (!owner.trait.CanCopy(thing12))
				{
					continue;
				}
				Thing thing6 = thing12.Duplicate(1);
				thing6.isStolen = false;
				thing6.isCopy = true;
				thing6.c_priceFix = 0;
// ---- around 1719 ----
		owner.isRestocking = true;
		t.things.DestroyAll((Thing _t) => _t.GetInt(101) != 0);
		foreach (Thing thing11 in t.things)
		{
			thing11.invX = -1;
		}
		switch (ShopType)
		{
		case ShopType.Plat:
			NoRestock(ThingGen.Create("lucky_coin").SetNum(10));
			NoRestock(ThingGen.CreateSkillbook(6662));
			NoRestock(ThingGen.CreateSkillbook(6664));
			Add("book_exp", 10, 0);
			break;
		case ShopType.Copy:
		{
			Thing c_copyContainer = owner.c_copyContainer;
			if (c_copyContainer == null)
			{
				break;
			}
			int num7 = 0;
			foreach (Thing thing12 in c_copyContainer.things)
			{
				if (!owner.trait.CanCopy(thing12))
				{
					continue;
				}
				Thing thing6 = thing12.Duplicate(1);
				thing6.isStolen = false;
				thing6.isCopy = true;
				thing6.c_priceFix = 0;
				foreach (Element item in thing6.elements.dict.Values.Where((Element e) => e.HasTag("noInherit")).ToList())
				{
// ---- around 1725 ----
		switch (ShopType)
		{
		case ShopType.Plat:
			NoRestock(ThingGen.Create("lucky_coin").SetNum(10));
			NoRestock(ThingGen.CreateSkillbook(6662));
			NoRestock(ThingGen.CreateSkillbook(6664));
			Add("book_exp", 10, 0);
			break;
		case ShopType.Copy:
		{
			Thing c_copyContainer = owner.c_copyContainer;
			if (c_copyContainer == null)
			{
				break;
			}
			int num7 = 0;
			foreach (Thing thing12 in c_copyContainer.things)
			{
				if (!owner.trait.CanCopy(thing12))
				{
					continue;
				}
				Thing thing6 = thing12.Duplicate(1);
				thing6.isStolen = false;
				thing6.isCopy = true;
				thing6.c_priceFix = 0;
				foreach (Element item in thing6.elements.dict.Values.Where((Element e) => e.HasTag("noInherit")).ToList())
				{
					thing6.elements.Remove(item.id);
				}
				int num8 = 1;
				switch (owner.trait.CopyShop)
				{
				case CopyShopType.Item:
// ---- around 1750 ----
				thing6.c_priceFix = 0;
				foreach (Element item in thing6.elements.dict.Values.Where((Element e) => e.HasTag("noInherit")).ToList())
				{
					thing6.elements.Remove(item.id);
				}
				int num8 = 1;
				switch (owner.trait.CopyShop)
				{
				case CopyShopType.Item:
				{
					num8 = (1000 + owner.c_invest * 100) / (thing6.GetPrice(CurrencyType.Money, sell: false, PriceType.CopyShop) + 50);
					int[] array = new int[3] { 704, 703, 702 };
					foreach (int ele in array)
					{
						if (thing6.HasElement(ele))
						{
							num8 = 1;
						}
					}
					break;
				}
				case CopyShopType.Spellbook:
					thing6.c_charges = thing12.c_charges;
					break;
				}
				if (num8 > 1 && thing6.trait.CanStack)
				{
					thing6.SetNum(num8);
				}
				AddThing(thing6);
				num7++;
				if (num7 > owner.trait.NumCopyItem)
				{
					break;
// ---- around 1752 ----
				{
					thing6.elements.Remove(item.id);
				}
				int num8 = 1;
				switch (owner.trait.CopyShop)
				{
				case CopyShopType.Item:
				{
					num8 = (1000 + owner.c_invest * 100) / (thing6.GetPrice(CurrencyType.Money, sell: false, PriceType.CopyShop) + 50);
					int[] array = new int[3] { 704, 703, 702 };
					foreach (int ele in array)
					{
						if (thing6.HasElement(ele))
						{
							num8 = 1;
						}
					}
					break;
				}
				case CopyShopType.Spellbook:
					thing6.c_charges = thing12.c_charges;
					break;
				}
				if (num8 > 1 && thing6.trait.CanStack)
				{
					thing6.SetNum(num8);
				}
				AddThing(thing6);
				num7++;
				if (num7 > owner.trait.NumCopyItem)
				{
					break;
				}
			}
// ---- around 1763 ----
					{
						if (thing6.HasElement(ele))
						{
							num8 = 1;
						}
					}
					break;
				}
				case CopyShopType.Spellbook:
					thing6.c_charges = thing12.c_charges;
					break;
				}
				if (num8 > 1 && thing6.trait.CanStack)
				{
					thing6.SetNum(num8);
				}
				AddThing(thing6);
				num7++;
				if (num7 > owner.trait.NumCopyItem)
				{
					break;
				}
			}
			Steam.GetAchievement((owner.trait is TraitKettle) ? ID_Achievement.KETTLE : ID_Achievement.DEMITAS);
			break;
		}
		case ShopType.Specific:
			switch (owner.id)
			{
			case "mogu":
				AddThing(ThingGen.Create("casino_coin").SetNum(5000));
				break;
			case "felmera":
				foreach (Thing item2 in new DramaOutcome().ListFelmeraBarter())
// ---- around 1781 ----
				if (num7 > owner.trait.NumCopyItem)
				{
					break;
				}
			}
			Steam.GetAchievement((owner.trait is TraitKettle) ? ID_Achievement.KETTLE : ID_Achievement.DEMITAS);
			break;
		}
		case ShopType.Specific:
			switch (owner.id)
			{
			case "mogu":
				AddThing(ThingGen.Create("casino_coin").SetNum(5000));
				break;
			case "felmera":
				foreach (Thing item2 in new DramaOutcome().ListFelmeraBarter())
				{
					AddThing(item2);
				}
				AddThing(ThingGen.Create("crimale2"));
				break;
			case "mimu":
				AddCassette(10, null, 999);
				AddCassette(15, null, 999);
				AddCassette(17, null, 999);
				AddCassette(29, null, 999);
				AddCassette(40, null, 999);
				AddCassette(46, null, 999);
				AddCassette(47, null, 999);
				AddCassette(52, null, 999);
				AddCassette(54, null, 999);
				AddCassette(59, null, 999);
				AddCassette(65, null, 999);
				AddCassette(109, "debt", 0);
// ---- around 1816 ----
				if (EClass.player.stats.married > 0)
				{
					AddCassette(122, null, 999);
					AddCassette(123, null, 999);
				}
				break;
			}
			break;
		case ShopType.Deed:
			Add("deed", 1, 0);
			Add("deed_move", 2 + EClass.rnd(5), 0);
			Add("deed_wedding", 1, 0);
			Add("deed_divorce", 1, 0);
			Add("deed_lostring", 1, 0);
			Add("license_illumination", 1, 0);
			Add("license_void", 1, 0);
			Add("license_adv", 1, 0);
			break;
		case ShopType.RedBook:
		{
			for (int k = 0; k < 30; k++)
			{
				AddThing(ThingGen.CreateFromFilter("shop_seeker"));
			}
			break;
		}
		case ShopType.TravelMerchant2:
		{
			int seed = EClass.game.seed + EClass._zone.uid + EClass.world.date.year * 12 + EClass.world.date.month;
			Add("tool_talisman", 1, 0);
			Add("camera", 1, 0);
			Add("dreambug", EClass.rndHalf(10), 0);
			Add("mathammer", 1, 0).ChangeMaterial(MATERIAL.GetRandomMaterial(80));
			Add("unicorn_horn", 1, 0);
// ---- around 1826 ----
			Add("deed_move", 2 + EClass.rnd(5), 0);
			Add("deed_wedding", 1, 0);
			Add("deed_divorce", 1, 0);
			Add("deed_lostring", 1, 0);
			Add("license_illumination", 1, 0);
			Add("license_void", 1, 0);
			Add("license_adv", 1, 0);
			break;
		case ShopType.RedBook:
		{
			for (int k = 0; k < 30; k++)
			{
				AddThing(ThingGen.CreateFromFilter("shop_seeker"));
			}
			break;
		}
		case ShopType.TravelMerchant2:
		{
			int seed = EClass.game.seed + EClass._zone.uid + EClass.world.date.year * 12 + EClass.world.date.month;
			Add("tool_talisman", 1, 0);
			Add("camera", 1, 0);
			Add("dreambug", EClass.rndHalf(10), 0);
			Add("mathammer", 1, 0).ChangeMaterial(MATERIAL.GetRandomMaterial(80));
			Add("unicorn_horn", 1, 0);
			Add("core_user", 1, 0);
			Add("generator_hamster1", 1, 0);
			Add("generator_hamster2", 1, 0);
			Add("generator_solar", 1, 0);
			Add("generator_wind", 1, 0);
			Add("curtainL", EClass.rndHalf(5), 0);
			Add("1337", EClass.rndHalf(5), 0);
			Add("1338", EClass.rndHalf(5), 0);
			Add("1339", EClass.rndHalf(5), 0);
			Add("1340", EClass.rndHalf(5), 0);
// ---- around 1834 ----
		case ShopType.RedBook:
		{
			for (int k = 0; k < 30; k++)
			{
				AddThing(ThingGen.CreateFromFilter("shop_seeker"));
			}
			break;
		}
		case ShopType.TravelMerchant2:
		{
			int seed = EClass.game.seed + EClass._zone.uid + EClass.world.date.year * 12 + EClass.world.date.month;
			Add("tool_talisman", 1, 0);
			Add("camera", 1, 0);
			Add("dreambug", EClass.rndHalf(10), 0);
			Add("mathammer", 1, 0).ChangeMaterial(MATERIAL.GetRandomMaterial(80));
			Add("unicorn_horn", 1, 0);
			Add("core_user", 1, 0);
			Add("generator_hamster1", 1, 0);
			Add("generator_hamster2", 1, 0);
			Add("generator_solar", 1, 0);
			Add("generator_wind", 1, 0);
			Add("curtainL", EClass.rndHalf(5), 0);
			Add("1337", EClass.rndHalf(5), 0);
			Add("1338", EClass.rndHalf(5), 0);
			Add("1339", EClass.rndHalf(5), 0);
			Add("1340", EClass.rndHalf(5), 0);
			Add("1301", EClass.rndHalf(5), 0);
			Add("1302", EClass.rndHalf(5), 0);
			Add("censored_item", 1, 0);
			Add("block_strawberry", EClass.rndHalf(6), 0);
			Add("block_cream", EClass.rndHalf(6), 0);
			Add("block_mango", EClass.rndHalf(6), 0);
			Add("block_cherry", EClass.rndHalf(6), 0);
			Add("block_chocoplate", EClass.rndHalf(6), 0);
// ---- around 1882 ----
				Add("hammer_garokk", 1, 0);
			}
			if (EClass.rndSeed(EClass.debug.enable ? 1 : 100, seed) == 0)
			{
				Add("water_jure", 1, 0);
			}
			break;
		}
		case ShopType.KeeperOfGarden:
		{
			string[] array2 = new string[11]
			{
				"stone_defense", "1325", "1326", "1327", "1328", "1330", "1331", "1332", "1333", "1283",
				"1268"
			};
			foreach (string id2 in array2)
			{
				AddThing(ThingGen.Create(id2, MATERIAL.GetRandomMaterialFromCategory(50, "rock", EClass.sources.materials.alias["granite"]).id).SetNum(99));
			}
			Add("cloud", 99, 0);
			Add("scroll_alias", 99, 0);
			Add("scroll_biography", 99, 0);
			Add("1329", 1, 0);
			Add("statue_lulu", 1, 0);
			Add("statue_jure", 1, 0);
			Add("statue_ehe", 1, 0);
			break;
		}
		case ShopType.Seed:
		{
			AddThing(TraitSeed.MakeSeed("rice")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("cabbage")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("carrot")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("potato")).SetNum(4 + EClass.rnd(4));
// ---- around 1902 ----
			Add("scroll_alias", 99, 0);
			Add("scroll_biography", 99, 0);
			Add("1329", 1, 0);
			Add("statue_lulu", 1, 0);
			Add("statue_jure", 1, 0);
			Add("statue_ehe", 1, 0);
			break;
		}
		case ShopType.Seed:
		{
			AddThing(TraitSeed.MakeSeed("rice")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("cabbage")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("carrot")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("potato")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("corn")).SetNum(4 + EClass.rnd(4));
			AddThing(TraitSeed.MakeSeed("chanoki")).SetNum(4 + EClass.rnd(4));
			for (int num10 = 0; num10 < EClass.rnd(3) + 1; num10++)
			{
				Add("462", 1, 0);
			}
			for (int num11 = 0; num11 < EClass.rnd(3) + 1; num11++)
			{
				Add("1167", 1, 0);
			}
			break;
		}
		case ShopType.Loytel:
			Add("board_map", 1, 0);
			Add("board_build", 1, 0);
			Add("book_resident", 1, 0);
			Add("board_party", 1, 0);
			Add("board_party2", 1, 0);
			Add("book_roster", 1, 0);
			Add("3", 1, 0);
// ---- around 1920 ----
				Add("462", 1, 0);
			}
			for (int num11 = 0; num11 < EClass.rnd(3) + 1; num11++)
			{
				Add("1167", 1, 0);
			}
			break;
		}
		case ShopType.Loytel:
			Add("board_map", 1, 0);
			Add("board_build", 1, 0);
			Add("book_resident", 1, 0);
			Add("board_party", 1, 0);
			Add("board_party2", 1, 0);
			Add("book_roster", 1, 0);
			Add("3", 1, 0);
			Add("4", 1, 0);
			Add("5", 1, 0);
			AddThing(ThingGen.CreatePlan(2512));
			AddThing(ThingGen.CreatePlan(2810));
			NoRestock(ThingGen.Create("rp_block").SetLv(1).SetNum(10));
			if (EClass.game.quests.GetPhase<QuestVernis>() >= 3)
			{
				NoRestock(ThingGen.CreateRecipe("explosive"));
			}
			break;
		case ShopType.Starter:
		case ShopType.StarterEx:
			Add("board_home", 1, 0);
			Add("board_resident", 1, 0);
			Add("1", 1, 0);
			Add("2", 1, 0);
			if (ShopType == ShopType.StarterEx)
			{
// ---- around 1938 ----
			AddThing(ThingGen.CreatePlan(2512));
			AddThing(ThingGen.CreatePlan(2810));
			NoRestock(ThingGen.Create("rp_block").SetLv(1).SetNum(10));
			if (EClass.game.quests.GetPhase<QuestVernis>() >= 3)
			{
				NoRestock(ThingGen.CreateRecipe("explosive"));
			}
			break;
		case ShopType.Starter:
		case ShopType.StarterEx:
			Add("board_home", 1, 0);
			Add("board_resident", 1, 0);
			Add("1", 1, 0);
			Add("2", 1, 0);
			if (ShopType == ShopType.StarterEx)
			{
				Add("board_expedition", 1, 0);
				Add("mailpost", 1, 0);
				Add("record", 1, 0);
				Add("tent2", 1, 0);
				Add("tent1", 1, 0);
				Add("wagon1", 1, 0);
				Add("wagon_big", 1, 0);
				Add("wagon_big2", 1, 0);
				Add("wagon_big3", 1, 0);
				Add("wagon_big4", 1, 0);
				Add("wagon_big5", 1, 0);
				Add("teleporter", 1, 0);
				Add("teleporter2", 1, 0);
				Add("recharger", 1, 0);
				Add("machine_gene2", 1, 0);
				NoRestock(ThingGen.CreateRecipe("torch_wall"));
				NoRestock(ThingGen.CreateRecipe("factory_sign"));
				NoRestock(ThingGen.CreateRecipe("beehive"));
// ---- around 1939 ----
			AddThing(ThingGen.CreatePlan(2810));
			NoRestock(ThingGen.Create("rp_block").SetLv(1).SetNum(10));
			if (EClass.game.quests.GetPhase<QuestVernis>() >= 3)
			{
				NoRestock(ThingGen.CreateRecipe("explosive"));
			}
			break;
		case ShopType.Starter:
		case ShopType.StarterEx:
			Add("board_home", 1, 0);
			Add("board_resident", 1, 0);
			Add("1", 1, 0);
			Add("2", 1, 0);
			if (ShopType == ShopType.StarterEx)
			{
				Add("board_expedition", 1, 0);
				Add("mailpost", 1, 0);
				Add("record", 1, 0);
				Add("tent2", 1, 0);
				Add("tent1", 1, 0);
				Add("wagon1", 1, 0);
				Add("wagon_big", 1, 0);
				Add("wagon_big2", 1, 0);
				Add("wagon_big3", 1, 0);
				Add("wagon_big4", 1, 0);
				Add("wagon_big5", 1, 0);
				Add("teleporter", 1, 0);
				Add("teleporter2", 1, 0);
				Add("recharger", 1, 0);
				Add("machine_gene2", 1, 0);
				NoRestock(ThingGen.CreateRecipe("torch_wall"));
				NoRestock(ThingGen.CreateRecipe("factory_sign"));
				NoRestock(ThingGen.CreateRecipe("beehive"));
				NoRestock(ThingGen.Create("rp_food").SetNum(5).SetLv(10)
// ---- around 1944 ----
			}
			break;
		case ShopType.Starter:
		case ShopType.StarterEx:
			Add("board_home", 1, 0);
			Add("board_resident", 1, 0);
			Add("1", 1, 0);
			Add("2", 1, 0);
			if (ShopType == ShopType.StarterEx)
			{
				Add("board_expedition", 1, 0);
				Add("mailpost", 1, 0);
				Add("record", 1, 0);
				Add("tent2", 1, 0);
				Add("tent1", 1, 0);
				Add("wagon1", 1, 0);
				Add("wagon_big", 1, 0);
				Add("wagon_big2", 1, 0);
				Add("wagon_big3", 1, 0);
				Add("wagon_big4", 1, 0);
				Add("wagon_big5", 1, 0);
				Add("teleporter", 1, 0);
				Add("teleporter2", 1, 0);
				Add("recharger", 1, 0);
				Add("machine_gene2", 1, 0);
				NoRestock(ThingGen.CreateRecipe("torch_wall"));
				NoRestock(ThingGen.CreateRecipe("factory_sign"));
				NoRestock(ThingGen.CreateRecipe("beehive"));
				NoRestock(ThingGen.Create("rp_food").SetNum(5).SetLv(10)
					.Thing);
				}
				else
				{
					AddThing(ThingGen.CreatePlan(2119));
// ---- around 1974 ----
				}
				else
				{
					AddThing(ThingGen.CreatePlan(2119));
					NoRestock(ThingGen.Create("rp_food").SetNum(5).SetLv(5)
						.Thing);
					}
					break;
				case ShopType.Farris:
					AddThing(ThingGen.CreateScroll(8220, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8221, 4 + EClass.rnd(6)));
					Add("drawing_paper", 10, 0);
					Add("drawing_paper2", 10, 0);
					Add("stethoscope", 1, 0);
					Add("whip_love", 1, 0);
					Add("whip_interest", 1, 0);
					Add("syringe_blood", 20, 0);
					if (EClass.game.IsSurvival)
					{
						Add("chest_tax", 1, 0);
					}
					break;
				case ShopType.Guild:
					if (this is TraitClerk_Merchant)
					{
						Add("flyer", 1, 0).SetNum(99);
					}
					break;
				case ShopType.Influence:
				{
					bool num5 = owner.id == "big_sister";
					TraitTicketFurniture.SetZone(num5 ? EClass.game.spatials.Find("little_garden") : EClass._zone, Add("ticket_furniture", 1, 0).SetNum(99));
					if (num5)
					{
// ---- around 1988 ----
					Add("whip_love", 1, 0);
					Add("whip_interest", 1, 0);
					Add("syringe_blood", 20, 0);
					if (EClass.game.IsSurvival)
					{
						Add("chest_tax", 1, 0);
					}
					break;
				case ShopType.Guild:
					if (this is TraitClerk_Merchant)
					{
						Add("flyer", 1, 0).SetNum(99);
					}
					break;
				case ShopType.Influence:
				{
					bool num5 = owner.id == "big_sister";
					TraitTicketFurniture.SetZone(num5 ? EClass.game.spatials.Find("little_garden") : EClass._zone, Add("ticket_furniture", 1, 0).SetNum(99));
					if (num5)
					{
						Add("littleball", 10, 0);
						if (!owner.Chara.affinity.CanGiveCard())
						{
							break;
						}
						if (!owner.Chara.elements.HasBase(287))
						{
							owner.Chara.elements.SetBase(287, (!EClass.debug.enable) ? 1 : 50);
						}
						if (!reroll)
						{
							for (int m = 0; m < 20; m++)
							{
								owner.Chara.ModExp(287, 1000);
// ---- around 1994 ----
					}
					break;
				case ShopType.Guild:
					if (this is TraitClerk_Merchant)
					{
						Add("flyer", 1, 0).SetNum(99);
					}
					break;
				case ShopType.Influence:
				{
					bool num5 = owner.id == "big_sister";
					TraitTicketFurniture.SetZone(num5 ? EClass.game.spatials.Find("little_garden") : EClass._zone, Add("ticket_furniture", 1, 0).SetNum(99));
					if (num5)
					{
						Add("littleball", 10, 0);
						if (!owner.Chara.affinity.CanGiveCard())
						{
							break;
						}
						if (!owner.Chara.elements.HasBase(287))
						{
							owner.Chara.elements.SetBase(287, (!EClass.debug.enable) ? 1 : 50);
						}
						if (!reroll)
						{
							for (int m = 0; m < 20; m++)
							{
								owner.Chara.ModExp(287, 1000);
							}
						}
						Thing thing3 = CraftUtil.MakeLoveLunch(owner.Chara);
						thing3.elements.SetBase(1229, 1);
						AddThing(thing3);
						break;
// ---- around 2045 ----
							Add("candle9", 1, -1);
							Add("candle8", 1, 0);
							Add("candle8b", 1, 0);
							Add("candle8c", 1, 0);
						}
					}
					break;
				}
				case ShopType.Casino:
				{
					Add("chest_tax", 1, 0);
					Add("1165", 1, 0);
					Add("monsterball", 1, 0).SetNum(3).SetLv(10);
					Add("1175", 1, 0);
					Add("1176", 1, 0);
					Add("pillow_ehekatl", 1, 0);
					Add("grave_dagger1", 1, 0);
					Add("grave_dagger2", 1, 0);
					Add("434", 1, 0);
					Add("433", 1, 0);
					Add("714", 1, 0);
					Add("1017", 1, 0);
					Add("1313", 1, 0);
					Add("1155", 1, 0);
					Add("1287", 1, 0);
					Add("1288", 1, 0);
					Add("1289", 1, 0);
					Add("1290", 1, 0);
					Add("1011", 1, 0);
					AddThing(ThingGen.CreatePerfume(9500, 5));
					AddThing(ThingGen.CreatePerfume(9501, 5));
					AddThing(ThingGen.CreatePerfume(9502, 5));
					AddThing(ThingGen.CreatePerfume(9503, 5));
					for (int l = 0; l < 5; l++)
// ---- around 2079 ----
					{
						Thing thing2 = ThingGen.CreateFromCategory("seasoning").SetNum(10);
						thing2.elements.SetBase(2, 40);
						thing2.c_priceFix = 1000;
						AddThing(thing2);
					}
					break;
				}
				case ShopType.Medal:
					NoRestockId("hammer_garokk", 3, 0);
					NoRestockId("sword_dragon", 1, 0);
					Add("sword_dragon", 1, 0).SetReplica(on: true);
					NoRestockId("point_stick", 1, 0);
					Add("point_stick", 1, 0).SetReplica(on: true);
					NoRestockId("blunt_bonehammer", 1, 0);
					Add("blunt_bonehammer", 1, 0).SetReplica(on: true);
					NoRestockId("pole_gunlance", 1, 0);
					Add("pole_gunlance", 1, 0).SetReplica(on: true);
					NoRestockId("sword_muramasa", 1, 0);
					Add("sword_muramasa", 1, 0).SetReplica(on: true);
					NoRestockId("sword_forgetmenot", 1, 0);
					Add("sword_forgetmenot", 1, 0).SetReplica(on: true);
					NoRestockId("dagger_fish", 1, 0);
					Add("dagger_fish", 1, 0).SetReplica(on: true);
					NoRestockId("sword_zephir", 1, 0);
					Add("sword_zephir", 1, 0).SetReplica(on: true);
					Add("bed_shiawase", 1, 0).SetReplica(on: true);
					Add("ribbon", 1, 0);
					Add("helm_sage", 1, 0);
					NoRestockId("wear_swim_danger", 1, 0);
					NoRestockId("wear_swim_danger", 1, 1);
					Add("diary_sister", 1, 0);
					Add("diary_catsister", 1, 0);
					Add("diary_lady", 1, 0);
// ---- around 2134 ----
					AddThing(ThingGen.CreateSpellbook(9155, 1, 3));
					break;
				default:
				{
					if (!owner.GetStr("merchant_override").IsEmpty())
					{
						break;
					}
					float num2 = (float)(3 + Mathf.Min(ShopLv / 5, 10)) + Mathf.Sqrt(ShopLv);
					int num3 = 300;
					switch (ShopType)
					{
					case ShopType.Ecopo:
						num3 = 30;
						break;
					case ShopType.StrangeGirl:
						num3 = 50;
						break;
					case ShopType.TravelMerchant:
					case ShopType.TravelMerchant2:
						num2 /= 3f;
						if (num2 < 12f)
						{
							num2 = 12f;
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
// ---- around 2136 ----
				default:
				{
					if (!owner.GetStr("merchant_override").IsEmpty())
					{
						break;
					}
					float num2 = (float)(3 + Mathf.Min(ShopLv / 5, 10)) + Mathf.Sqrt(ShopLv);
					int num3 = 300;
					switch (ShopType)
					{
					case ShopType.Ecopo:
						num3 = 30;
						break;
					case ShopType.StrangeGirl:
						num3 = 50;
						break;
					case ShopType.TravelMerchant:
					case ShopType.TravelMerchant2:
						num2 /= 3f;
						if (num2 < 12f)
						{
							num2 = 12f;
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
// ---- around 2138 ----
					if (!owner.GetStr("merchant_override").IsEmpty())
					{
						break;
					}
					float num2 = (float)(3 + Mathf.Min(ShopLv / 5, 10)) + Mathf.Sqrt(ShopLv);
					int num3 = 300;
					switch (ShopType)
					{
					case ShopType.Ecopo:
						num3 = 30;
						break;
					case ShopType.StrangeGirl:
						num3 = 50;
						break;
					case ShopType.TravelMerchant:
					case ShopType.TravelMerchant2:
						num2 /= 3f;
						if (num2 < 12f)
						{
							num2 = 12f;
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
// ---- around 2141 ----
					}
					float num2 = (float)(3 + Mathf.Min(ShopLv / 5, 10)) + Mathf.Sqrt(ShopLv);
					int num3 = 300;
					switch (ShopType)
					{
					case ShopType.Ecopo:
						num3 = 30;
						break;
					case ShopType.StrangeGirl:
						num3 = 50;
						break;
					case ShopType.TravelMerchant:
					case ShopType.TravelMerchant2:
						num2 /= 3f;
						if (num2 < 12f)
						{
							num2 = 12f;
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
// ---- around 2144 ----
					switch (ShopType)
					{
					case ShopType.Ecopo:
						num3 = 30;
						break;
					case ShopType.StrangeGirl:
						num3 = 50;
						break;
					case ShopType.TravelMerchant:
					case ShopType.TravelMerchant2:
						num2 /= 3f;
						if (num2 < 12f)
						{
							num2 = 12f;
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
						if ((!thing.trait.IsNoShop || (ShopType == ShopType.LoytelMart && (EClass.debug.enable || EClass.player.flags.loytelMartLv >= 2))) && (!(thing.trait is TraitRod) || thing.c_charges != 0) && thing.GetPrice() > 0)
						{
							bool tryStack = true;
// ---- around 2145 ----
					{
					case ShopType.Ecopo:
						num3 = 30;
						break;
					case ShopType.StrangeGirl:
						num3 = 50;
						break;
					case ShopType.TravelMerchant:
					case ShopType.TravelMerchant2:
						num2 /= 3f;
						if (num2 < 12f)
						{
							num2 = 12f;
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
						if ((!thing.trait.IsNoShop || (ShopType == ShopType.LoytelMart && (EClass.debug.enable || EClass.player.flags.loytelMartLv >= 2))) && (!(thing.trait is TraitRod) || thing.c_charges != 0) && thing.GetPrice() > 0)
						{
							bool tryStack = true;
							if (ShopType == ShopType.Curry)
// ---- around 2158 ----
						}
						num3 = 30;
						break;
					}
					num2 = num2 * (float)(100 + EClass.pc.Evalue(1406) * 5) / 100f;
					num2 = Mathf.Min(num2, num3);
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
						if ((!thing.trait.IsNoShop || (ShopType == ShopType.LoytelMart && (EClass.debug.enable || EClass.player.flags.loytelMartLv >= 2))) && (!(thing.trait is TraitRod) || thing.c_charges != 0) && thing.GetPrice() > 0)
						{
							bool tryStack = true;
							if (ShopType == ShopType.Curry)
							{
								tryStack = false;
							}
							t.AddThing(thing, tryStack);
						}
					}
					break;
				}
				}
				string str = owner.GetStr("merchant_override");
				if (!str.IsEmpty())
				{
					foreach (Thing item3 in ModUtil.GenerateMerchantStock(owner, str))
// ---- around 2164 ----
					for (int j = 0; (float)j < num2; j++)
					{
						if (ShopType == ShopType.TravelMerchant)
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
						if ((!thing.trait.IsNoShop || (ShopType == ShopType.LoytelMart && (EClass.debug.enable || EClass.player.flags.loytelMartLv >= 2))) && (!(thing.trait is TraitRod) || thing.c_charges != 0) && thing.GetPrice() > 0)
						{
							bool tryStack = true;
							if (ShopType == ShopType.Curry)
							{
								tryStack = false;
							}
							t.AddThing(thing, tryStack);
						}
					}
					break;
				}
				}
				string str = owner.GetStr("merchant_override");
				if (!str.IsEmpty())
				{
					foreach (Thing item3 in ModUtil.GenerateMerchantStock(owner, str))
					{
						AddThing(item3);
					}
				}
				string text = ShopType.ToString();
				foreach (RecipeSource item4 in RecipeManager.list)
// ---- around 2167 ----
						{
							int num4 = EClass.game.seed + (EClass.world.date.year * 12 + EClass.world.date.month) * 30 + EClass._zone.uid;
							Rand.SetBaseSeed(num4 + j);
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
						if ((!thing.trait.IsNoShop || (ShopType == ShopType.LoytelMart && (EClass.debug.enable || EClass.player.flags.loytelMartLv >= 2))) && (!(thing.trait is TraitRod) || thing.c_charges != 0) && thing.GetPrice() > 0)
						{
							bool tryStack = true;
							if (ShopType == ShopType.Curry)
							{
								tryStack = false;
							}
							t.AddThing(thing, tryStack);
						}
					}
					break;
				}
				}
				string str = owner.GetStr("merchant_override");
				if (!str.IsEmpty())
				{
					foreach (Thing item3 in ModUtil.GenerateMerchantStock(owner, str))
					{
						AddThing(item3);
					}
				}
				string text = ShopType.ToString();
				foreach (RecipeSource item4 in RecipeManager.list)
				{
					if (item4.row.recipeKey.IsEmpty())
					{
// ---- around 2170 ----
							Rand.SetSeed(num4 + j);
						}
						Thing thing = CreateStock();
						Rand.SetBaseSeed();
						Rand.SetSeed();
						if ((!thing.trait.IsNoShop || (ShopType == ShopType.LoytelMart && (EClass.debug.enable || EClass.player.flags.loytelMartLv >= 2))) && (!(thing.trait is TraitRod) || thing.c_charges != 0) && thing.GetPrice() > 0)
						{
							bool tryStack = true;
							if (ShopType == ShopType.Curry)
							{
								tryStack = false;
							}
							t.AddThing(thing, tryStack);
						}
					}
					break;
				}
				}
				string str = owner.GetStr("merchant_override");
				if (!str.IsEmpty())
				{
					foreach (Thing item3 in ModUtil.GenerateMerchantStock(owner, str))
					{
						AddThing(item3);
					}
				}
				string text = ShopType.ToString();
				foreach (RecipeSource item4 in RecipeManager.list)
				{
					if (item4.row.recipeKey.IsEmpty())
					{
						continue;
					}
					string[] array2 = item4.row.recipeKey;
// ---- around 2188 ----
				string str = owner.GetStr("merchant_override");
				if (!str.IsEmpty())
				{
					foreach (Thing item3 in ModUtil.GenerateMerchantStock(owner, str))
					{
						AddThing(item3);
					}
				}
				string text = ShopType.ToString();
				foreach (RecipeSource item4 in RecipeManager.list)
				{
					if (item4.row.recipeKey.IsEmpty())
					{
						continue;
					}
					string[] array2 = item4.row.recipeKey;
					for (int num9 = 0; num9 < array2.Length; num9++)
					{
						if (array2[num9] == text)
						{
							NoRestock(ThingGen.CreateRecipe(item4.id));
							break;
						}
					}
				}
				switch (ShopType)
				{
				case ShopType.Curry:
					if (EClass.game.quests.IsCompleted("curry"))
					{
						AddThing(TraitSeed.MakeSeed("redpepper").SetNum(5));
					}
					break;
				case ShopType.Moyer:
// ---- around 2205 ----
					{
						if (array2[num9] == text)
						{
							NoRestock(ThingGen.CreateRecipe(item4.id));
							break;
						}
					}
				}
				switch (ShopType)
				{
				case ShopType.Curry:
					if (EClass.game.quests.IsCompleted("curry"))
					{
						AddThing(TraitSeed.MakeSeed("redpepper").SetNum(5));
					}
					break;
				case ShopType.Moyer:
				{
					for (int num14 = 1; num14 <= 25; num14++)
					{
						AddAdvWeek(num14);
					}
					break;
				}
				case ShopType.StrangeGirl:
				{
					int num15 = (EClass.debug.enable ? 20 : (EClass._zone.development / 10));
					if (num15 > 0)
					{
						Add("syringe_gene", num15, 0);
						Add("diary_little", 1, 0);
					}
					if (num15 > 10)
					{
// ---- around 2207 ----
						{
							NoRestock(ThingGen.CreateRecipe(item4.id));
							break;
						}
					}
				}
				switch (ShopType)
				{
				case ShopType.Curry:
					if (EClass.game.quests.IsCompleted("curry"))
					{
						AddThing(TraitSeed.MakeSeed("redpepper").SetNum(5));
					}
					break;
				case ShopType.Moyer:
				{
					for (int num14 = 1; num14 <= 25; num14++)
					{
						AddAdvWeek(num14);
					}
					break;
				}
				case ShopType.StrangeGirl:
				{
					int num15 = (EClass.debug.enable ? 20 : (EClass._zone.development / 10));
					if (num15 > 0)
					{
						Add("syringe_gene", num15, 0);
						Add("diary_little", 1, 0);
					}
					if (num15 > 10)
					{
						Add("syringe_heaven", num15 / 5, 0);
						Add("1276", 1, 0);
// ---- around 2213 ----
				switch (ShopType)
				{
				case ShopType.Curry:
					if (EClass.game.quests.IsCompleted("curry"))
					{
						AddThing(TraitSeed.MakeSeed("redpepper").SetNum(5));
					}
					break;
				case ShopType.Moyer:
				{
					for (int num14 = 1; num14 <= 25; num14++)
					{
						AddAdvWeek(num14);
					}
					break;
				}
				case ShopType.StrangeGirl:
				{
					int num15 = (EClass.debug.enable ? 20 : (EClass._zone.development / 10));
					if (num15 > 0)
					{
						Add("syringe_gene", num15, 0);
						Add("diary_little", 1, 0);
					}
					if (num15 > 10)
					{
						Add("syringe_heaven", num15 / 5, 0);
						Add("1276", 1, 0);
					}
					Add("medal", 10, 0);
					Add("ticket_fortune", 10, 0);
					break;
				}
				case ShopType.GeneralExotic:
// ---- around 2221 ----
				case ShopType.Moyer:
				{
					for (int num14 = 1; num14 <= 25; num14++)
					{
						AddAdvWeek(num14);
					}
					break;
				}
				case ShopType.StrangeGirl:
				{
					int num15 = (EClass.debug.enable ? 20 : (EClass._zone.development / 10));
					if (num15 > 0)
					{
						Add("syringe_gene", num15, 0);
						Add("diary_little", 1, 0);
					}
					if (num15 > 10)
					{
						Add("syringe_heaven", num15 / 5, 0);
						Add("1276", 1, 0);
					}
					Add("medal", 10, 0);
					Add("ticket_fortune", 10, 0);
					break;
				}
				case ShopType.GeneralExotic:
					Add("tool_talisman", 1, 0);
					break;
				case ShopType.Healer:
					AddThing(ThingGen.CreatePotion(8400).SetNum(4 + EClass.rnd(6)));
					AddThing(ThingGen.CreatePotion(8401).SetNum(2 + EClass.rnd(4)));
					AddThing(ThingGen.CreatePotion(8402).SetNum(1 + EClass.rnd(3)));
					break;
				case ShopType.Food:
// ---- around 2238 ----
					{
						Add("syringe_heaven", num15 / 5, 0);
						Add("1276", 1, 0);
					}
					Add("medal", 10, 0);
					Add("ticket_fortune", 10, 0);
					break;
				}
				case ShopType.GeneralExotic:
					Add("tool_talisman", 1, 0);
					break;
				case ShopType.Healer:
					AddThing(ThingGen.CreatePotion(8400).SetNum(4 + EClass.rnd(6)));
					AddThing(ThingGen.CreatePotion(8401).SetNum(2 + EClass.rnd(4)));
					AddThing(ThingGen.CreatePotion(8402).SetNum(1 + EClass.rnd(3)));
					break;
				case ShopType.Food:
					Add("ration", 2 + EClass.rnd(4), 0);
					break;
				case ShopType.Ecopo:
					Add("ecomark", 5, 0);
					Add("whip_egg", 1, 0);
					Add("helm_chef", 1, 0);
					Add("hammer_strip", 1, 0);
					Add("brush_strip", 1, 0);
					Add("1165", 1, 0);
					Add("plat", 100, 0);
					AddThing(ThingGen.CreateScroll(9160).SetNum(5));
					AddThing(ThingGen.CreateRune(450, 1, free: true));
					break;
				case ShopType.Gun:
					Add("bullet", 1, 0).SetNum(300 + EClass.rnd(100)).ChangeMaterial("iron");
					Add("bullet_energy", 1, 0).SetNum(100 + EClass.rnd(100)).ChangeMaterial("iron");
					break;
// ---- around 2241 ----
					}
					Add("medal", 10, 0);
					Add("ticket_fortune", 10, 0);
					break;
				}
				case ShopType.GeneralExotic:
					Add("tool_talisman", 1, 0);
					break;
				case ShopType.Healer:
					AddThing(ThingGen.CreatePotion(8400).SetNum(4 + EClass.rnd(6)));
					AddThing(ThingGen.CreatePotion(8401).SetNum(2 + EClass.rnd(4)));
					AddThing(ThingGen.CreatePotion(8402).SetNum(1 + EClass.rnd(3)));
					break;
				case ShopType.Food:
					Add("ration", 2 + EClass.rnd(4), 0);
					break;
				case ShopType.Ecopo:
					Add("ecomark", 5, 0);
					Add("whip_egg", 1, 0);
					Add("helm_chef", 1, 0);
					Add("hammer_strip", 1, 0);
					Add("brush_strip", 1, 0);
					Add("1165", 1, 0);
					Add("plat", 100, 0);
					AddThing(ThingGen.CreateScroll(9160).SetNum(5));
					AddThing(ThingGen.CreateRune(450, 1, free: true));
					break;
				case ShopType.Gun:
					Add("bullet", 1, 0).SetNum(300 + EClass.rnd(100)).ChangeMaterial("iron");
					Add("bullet_energy", 1, 0).SetNum(100 + EClass.rnd(100)).ChangeMaterial("iron");
					break;
				case ShopType.Magic:
					if (!Guild.Mage.IsMember && ((EClass._zone.id == "lumiest" && EClass._zone.lv == 0) || (EClass._zone.id != "lumiest" && EClass.rnd(4) == 0)))
					{
// ---- around 2246 ----
				case ShopType.GeneralExotic:
					Add("tool_talisman", 1, 0);
					break;
				case ShopType.Healer:
					AddThing(ThingGen.CreatePotion(8400).SetNum(4 + EClass.rnd(6)));
					AddThing(ThingGen.CreatePotion(8401).SetNum(2 + EClass.rnd(4)));
					AddThing(ThingGen.CreatePotion(8402).SetNum(1 + EClass.rnd(3)));
					break;
				case ShopType.Food:
					Add("ration", 2 + EClass.rnd(4), 0);
					break;
				case ShopType.Ecopo:
					Add("ecomark", 5, 0);
					Add("whip_egg", 1, 0);
					Add("helm_chef", 1, 0);
					Add("hammer_strip", 1, 0);
					Add("brush_strip", 1, 0);
					Add("1165", 1, 0);
					Add("plat", 100, 0);
					AddThing(ThingGen.CreateScroll(9160).SetNum(5));
					AddThing(ThingGen.CreateRune(450, 1, free: true));
					break;
				case ShopType.Gun:
					Add("bullet", 1, 0).SetNum(300 + EClass.rnd(100)).ChangeMaterial("iron");
					Add("bullet_energy", 1, 0).SetNum(100 + EClass.rnd(100)).ChangeMaterial("iron");
					break;
				case ShopType.Magic:
					if (!Guild.Mage.IsMember && ((EClass._zone.id == "lumiest" && EClass._zone.lv == 0) || (EClass._zone.id != "lumiest" && EClass.rnd(4) == 0)))
					{
						t.AddThing(ThingGen.Create("letter_trial"));
					}
					AddThing(ThingGen.CreateScroll(8220, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8221, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8200, 4 + EClass.rnd(6)));
// ---- around 2249 ----
				case ShopType.Healer:
					AddThing(ThingGen.CreatePotion(8400).SetNum(4 + EClass.rnd(6)));
					AddThing(ThingGen.CreatePotion(8401).SetNum(2 + EClass.rnd(4)));
					AddThing(ThingGen.CreatePotion(8402).SetNum(1 + EClass.rnd(3)));
					break;
				case ShopType.Food:
					Add("ration", 2 + EClass.rnd(4), 0);
					break;
				case ShopType.Ecopo:
					Add("ecomark", 5, 0);
					Add("whip_egg", 1, 0);
					Add("helm_chef", 1, 0);
					Add("hammer_strip", 1, 0);
					Add("brush_strip", 1, 0);
					Add("1165", 1, 0);
					Add("plat", 100, 0);
					AddThing(ThingGen.CreateScroll(9160).SetNum(5));
					AddThing(ThingGen.CreateRune(450, 1, free: true));
					break;
				case ShopType.Gun:
					Add("bullet", 1, 0).SetNum(300 + EClass.rnd(100)).ChangeMaterial("iron");
					Add("bullet_energy", 1, 0).SetNum(100 + EClass.rnd(100)).ChangeMaterial("iron");
					break;
				case ShopType.Magic:
					if (!Guild.Mage.IsMember && ((EClass._zone.id == "lumiest" && EClass._zone.lv == 0) || (EClass._zone.id != "lumiest" && EClass.rnd(4) == 0)))
					{
						t.AddThing(ThingGen.Create("letter_trial"));
					}
					AddThing(ThingGen.CreateScroll(8220, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8221, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8200, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8201, 4 + EClass.rnd(6)));
					break;
				case ShopType.Festival:
// ---- around 2260 ----
					Add("helm_chef", 1, 0);
					Add("hammer_strip", 1, 0);
					Add("brush_strip", 1, 0);
					Add("1165", 1, 0);
					Add("plat", 100, 0);
					AddThing(ThingGen.CreateScroll(9160).SetNum(5));
					AddThing(ThingGen.CreateRune(450, 1, free: true));
					break;
				case ShopType.Gun:
					Add("bullet", 1, 0).SetNum(300 + EClass.rnd(100)).ChangeMaterial("iron");
					Add("bullet_energy", 1, 0).SetNum(100 + EClass.rnd(100)).ChangeMaterial("iron");
					break;
				case ShopType.Magic:
					if (!Guild.Mage.IsMember && ((EClass._zone.id == "lumiest" && EClass._zone.lv == 0) || (EClass._zone.id != "lumiest" && EClass.rnd(4) == 0)))
					{
						t.AddThing(ThingGen.Create("letter_trial"));
					}
					AddThing(ThingGen.CreateScroll(8220, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8221, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8200, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8201, 4 + EClass.rnd(6)));
					break;
				case ShopType.Festival:
					if (EClass._zone.IsFestival)
					{
						Add("1085", 1, 0);
						if (EClass._zone.id == "noyel")
						{
							Add("holyFeather", 1, 0);
						}
					}
					break;
				case ShopType.Junk:
				case ShopType.LoytelMart:
// ---- around 2264 ----
					Add("plat", 100, 0);
					AddThing(ThingGen.CreateScroll(9160).SetNum(5));
					AddThing(ThingGen.CreateRune(450, 1, free: true));
					break;
				case ShopType.Gun:
					Add("bullet", 1, 0).SetNum(300 + EClass.rnd(100)).ChangeMaterial("iron");
					Add("bullet_energy", 1, 0).SetNum(100 + EClass.rnd(100)).ChangeMaterial("iron");
					break;
				case ShopType.Magic:
					if (!Guild.Mage.IsMember && ((EClass._zone.id == "lumiest" && EClass._zone.lv == 0) || (EClass._zone.id != "lumiest" && EClass.rnd(4) == 0)))
					{
						t.AddThing(ThingGen.Create("letter_trial"));
					}
					AddThing(ThingGen.CreateScroll(8220, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8221, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8200, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8201, 4 + EClass.rnd(6)));
					break;
				case ShopType.Festival:
					if (EClass._zone.IsFestival)
					{
						Add("1085", 1, 0);
						if (EClass._zone.id == "noyel")
						{
							Add("holyFeather", 1, 0);
						}
					}
					break;
				case ShopType.Junk:
				case ShopType.LoytelMart:
				{
					if (ShopType == ShopType.LoytelMart)
					{
						Add("ticket_massage", 1, 0);
// ---- around 2274 ----
					{
						t.AddThing(ThingGen.Create("letter_trial"));
					}
					AddThing(ThingGen.CreateScroll(8220, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8221, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8200, 4 + EClass.rnd(6)));
					AddThing(ThingGen.CreateScroll(8201, 4 + EClass.rnd(6)));
					break;
				case ShopType.Festival:
					if (EClass._zone.IsFestival)
					{
						Add("1085", 1, 0);
						if (EClass._zone.id == "noyel")
						{
							Add("holyFeather", 1, 0);
						}
					}
					break;
				case ShopType.Junk:
				case ShopType.LoytelMart:
				{
					if (ShopType == ShopType.LoytelMart)
					{
						Add("ticket_massage", 1, 0);
						Add("ticket_armpillow", 1, 0);
						Add("ticket_champagne", 1, 0);
					}
					for (int num12 = 0; num12 < 3; num12++)
					{
						if (EClass.rnd(5) == 0)
						{
							TreasureType treasureType = ((EClass.rnd(10) == 0) ? TreasureType.BossNefia : ((EClass.rnd(10) == 0) ? TreasureType.Map : TreasureType.RandomChest));
							int num13 = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing thing7 = ThingGen.Create(treasureType switch
// ---- around 2284 ----
					{
						Add("1085", 1, 0);
						if (EClass._zone.id == "noyel")
						{
							Add("holyFeather", 1, 0);
						}
					}
					break;
				case ShopType.Junk:
				case ShopType.LoytelMart:
				{
					if (ShopType == ShopType.LoytelMart)
					{
						Add("ticket_massage", 1, 0);
						Add("ticket_armpillow", 1, 0);
						Add("ticket_champagne", 1, 0);
					}
					for (int num12 = 0; num12 < 3; num12++)
					{
						if (EClass.rnd(5) == 0)
						{
							TreasureType treasureType = ((EClass.rnd(10) == 0) ? TreasureType.BossNefia : ((EClass.rnd(10) == 0) ? TreasureType.Map : TreasureType.RandomChest));
							int num13 = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing thing7 = ThingGen.Create(treasureType switch
							{
								TreasureType.Map => "chest_treasure", 
								TreasureType.BossNefia => "chest_boss", 
								_ => "chest3", 
							});
							thing7.c_lockedHard = true;
							thing7.c_lockLv = num13;
							thing7.c_priceAdd = 2000 + num13 * 250 * ((treasureType == TreasureType.RandomChest) ? 1 : 5);
							thing7.c_revealLock = true;
							ThingGen.CreateTreasureContent(thing7, num13, treasureType, clearContent: true);
// ---- around 2285 ----
						Add("1085", 1, 0);
						if (EClass._zone.id == "noyel")
						{
							Add("holyFeather", 1, 0);
						}
					}
					break;
				case ShopType.Junk:
				case ShopType.LoytelMart:
				{
					if (ShopType == ShopType.LoytelMart)
					{
						Add("ticket_massage", 1, 0);
						Add("ticket_armpillow", 1, 0);
						Add("ticket_champagne", 1, 0);
					}
					for (int num12 = 0; num12 < 3; num12++)
					{
						if (EClass.rnd(5) == 0)
						{
							TreasureType treasureType = ((EClass.rnd(10) == 0) ? TreasureType.BossNefia : ((EClass.rnd(10) == 0) ? TreasureType.Map : TreasureType.RandomChest));
							int num13 = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing thing7 = ThingGen.Create(treasureType switch
							{
								TreasureType.Map => "chest_treasure", 
								TreasureType.BossNefia => "chest_boss", 
								_ => "chest3", 
							});
							thing7.c_lockedHard = true;
							thing7.c_lockLv = num13;
							thing7.c_priceAdd = 2000 + num13 * 250 * ((treasureType == TreasureType.RandomChest) ? 1 : 5);
							thing7.c_revealLock = true;
							ThingGen.CreateTreasureContent(thing7, num13, treasureType, clearContent: true);
							AddThing(thing7);
// ---- around 2287 ----
						{
							Add("holyFeather", 1, 0);
						}
					}
					break;
				case ShopType.Junk:
				case ShopType.LoytelMart:
				{
					if (ShopType == ShopType.LoytelMart)
					{
						Add("ticket_massage", 1, 0);
						Add("ticket_armpillow", 1, 0);
						Add("ticket_champagne", 1, 0);
					}
					for (int num12 = 0; num12 < 3; num12++)
					{
						if (EClass.rnd(5) == 0)
						{
							TreasureType treasureType = ((EClass.rnd(10) == 0) ? TreasureType.BossNefia : ((EClass.rnd(10) == 0) ? TreasureType.Map : TreasureType.RandomChest));
							int num13 = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing thing7 = ThingGen.Create(treasureType switch
							{
								TreasureType.Map => "chest_treasure", 
								TreasureType.BossNefia => "chest_boss", 
								_ => "chest3", 
							});
							thing7.c_lockedHard = true;
							thing7.c_lockLv = num13;
							thing7.c_priceAdd = 2000 + num13 * 250 * ((treasureType == TreasureType.RandomChest) ? 1 : 5);
							thing7.c_revealLock = true;
							ThingGen.CreateTreasureContent(thing7, num13, treasureType, clearContent: true);
							AddThing(thing7);
						}
					}
// ---- around 2298 ----
						Add("ticket_armpillow", 1, 0);
						Add("ticket_champagne", 1, 0);
					}
					for (int num12 = 0; num12 < 3; num12++)
					{
						if (EClass.rnd(5) == 0)
						{
							TreasureType treasureType = ((EClass.rnd(10) == 0) ? TreasureType.BossNefia : ((EClass.rnd(10) == 0) ? TreasureType.Map : TreasureType.RandomChest));
							int num13 = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing thing7 = ThingGen.Create(treasureType switch
							{
								TreasureType.Map => "chest_treasure", 
								TreasureType.BossNefia => "chest_boss", 
								_ => "chest3", 
							});
							thing7.c_lockedHard = true;
							thing7.c_lockLv = num13;
							thing7.c_priceAdd = 2000 + num13 * 250 * ((treasureType == TreasureType.RandomChest) ? 1 : 5);
							thing7.c_revealLock = true;
							ThingGen.CreateTreasureContent(thing7, num13, treasureType, clearContent: true);
							AddThing(thing7);
						}
					}
					break;
				}
				}
				switch (ShopType)
				{
				case ShopType.General:
				case ShopType.Food:
				{
					for (int num16 = 0; num16 < (EClass.debug.enable ? 3 : 3); num16++)
					{
						if (EClass.rnd(3) == 0)
// ---- around 2316 ----
							thing7.c_revealLock = true;
							ThingGen.CreateTreasureContent(thing7, num13, treasureType, clearContent: true);
							AddThing(thing7);
						}
					}
					break;
				}
				}
				switch (ShopType)
				{
				case ShopType.General:
				case ShopType.Food:
				{
					for (int num16 = 0; num16 < (EClass.debug.enable ? 3 : 3); num16++)
					{
						if (EClass.rnd(3) == 0)
						{
							int lv = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing t2 = ThingGen.Create("chest_gamble", -1, lv).SetNum(1 + EClass.rnd(20));
							AddThing(t2);
						}
					}
					break;
				}
				case ShopType.Booze:
					if (EClass._zone is Zone_Yowyn && EClass._zone.lv == -1)
					{
						Add("churyu", EClass.rndHalf(10), 0);
					}
					break;
				}
				switch (owner.id)
				{
				case "rodwyn":
// ---- around 2318 ----
							AddThing(thing7);
						}
					}
					break;
				}
				}
				switch (ShopType)
				{
				case ShopType.General:
				case ShopType.Food:
				{
					for (int num16 = 0; num16 < (EClass.debug.enable ? 3 : 3); num16++)
					{
						if (EClass.rnd(3) == 0)
						{
							int lv = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing t2 = ThingGen.Create("chest_gamble", -1, lv).SetNum(1 + EClass.rnd(20));
							AddThing(t2);
						}
					}
					break;
				}
				case ShopType.Booze:
					if (EClass._zone is Zone_Yowyn && EClass._zone.lv == -1)
					{
						Add("churyu", EClass.rndHalf(10), 0);
					}
					break;
				}
				switch (owner.id)
				{
				case "rodwyn":
					AddThing(ThingGen.CreateSpellbook(8790));
					AddThing(ThingGen.CreatePotion(8791).SetNum(3 + EClass.rnd(3)));
// ---- around 2319 ----
						}
					}
					break;
				}
				}
				switch (ShopType)
				{
				case ShopType.General:
				case ShopType.Food:
				{
					for (int num16 = 0; num16 < (EClass.debug.enable ? 3 : 3); num16++)
					{
						if (EClass.rnd(3) == 0)
						{
							int lv = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing t2 = ThingGen.Create("chest_gamble", -1, lv).SetNum(1 + EClass.rnd(20));
							AddThing(t2);
						}
					}
					break;
				}
				case ShopType.Booze:
					if (EClass._zone is Zone_Yowyn && EClass._zone.lv == -1)
					{
						Add("churyu", EClass.rndHalf(10), 0);
					}
					break;
				}
				switch (owner.id)
				{
				case "rodwyn":
					AddThing(ThingGen.CreateSpellbook(8790));
					AddThing(ThingGen.CreatePotion(8791).SetNum(3 + EClass.rnd(3)));
					AddThing(ThingGen.CreatePotion(8792).SetNum(3 + EClass.rnd(3)));
// ---- around 2325 ----
				{
				case ShopType.General:
				case ShopType.Food:
				{
					for (int num16 = 0; num16 < (EClass.debug.enable ? 3 : 3); num16++)
					{
						if (EClass.rnd(3) == 0)
						{
							int lv = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing t2 = ThingGen.Create("chest_gamble", -1, lv).SetNum(1 + EClass.rnd(20));
							AddThing(t2);
						}
					}
					break;
				}
				case ShopType.Booze:
					if (EClass._zone is Zone_Yowyn && EClass._zone.lv == -1)
					{
						Add("churyu", EClass.rndHalf(10), 0);
					}
					break;
				}
				switch (owner.id)
				{
				case "rodwyn":
					AddThing(ThingGen.CreateSpellbook(8790));
					AddThing(ThingGen.CreatePotion(8791).SetNum(3 + EClass.rnd(3)));
					AddThing(ThingGen.CreatePotion(8792).SetNum(3 + EClass.rnd(3)));
					AddThing(ThingGen.CreatePotion(8794).SetNum(3 + EClass.rnd(3)));
					Add("1341", EClass.rndHalf(5), 0);
					break;
				case "girl_blue":
					Add("779", 1 + EClass.rnd(3), 0);
					break;
// ---- around 2332 ----
						{
							int lv = EClass.rnd(EClass.rnd(ShopLv + (EClass.debug.enable ? 200 : 50)) + 1) + 1;
							Thing t2 = ThingGen.Create("chest_gamble", -1, lv).SetNum(1 + EClass.rnd(20));
							AddThing(t2);
						}
					}
					break;
				}
				case ShopType.Booze:
					if (EClass._zone is Zone_Yowyn && EClass._zone.lv == -1)
					{
						Add("churyu", EClass.rndHalf(10), 0);
					}
					break;
				}
				switch (owner.id)
				{
				case "rodwyn":
					AddThing(ThingGen.CreateSpellbook(8790));
					AddThing(ThingGen.CreatePotion(8791).SetNum(3 + EClass.rnd(3)));
					AddThing(ThingGen.CreatePotion(8792).SetNum(3 + EClass.rnd(3)));
					AddThing(ThingGen.CreatePotion(8794).SetNum(3 + EClass.rnd(3)));
					Add("1341", EClass.rndHalf(5), 0);
					break;
				case "girl_blue":
					Add("779", 1 + EClass.rnd(3), 0);
					break;
				case "nola":
					AddThing(ThingGen.CreateRecipe("ic").SetPriceFix(400));
					AddThing(ThingGen.CreateRecipe("bullet").SetPriceFix(300));
					AddThing(ThingGen.CreateRecipe("break_powder").SetPriceFix(1000));
					AddThing(ThingGen.CreateRecipe("quarrel").SetPriceFix(100));
					AddThing(ThingGen.CreateRecipe("1099").SetPriceFix(400));
					AddThing(ThingGen.CreateRecipe("detector").SetPriceFix(700));
// ---- around 2375 ----
					{
						Add("lockpick", 1, 0);
					}
					AddThing(ThingGen.CreateScroll(8780, EClass.rndHalf(5)));
				}
				foreach (Thing thing13 in t.things)
				{
					thing13.c_idBacker = 0;
					if (ShopType != ShopType.Copy)
					{
						thing13.TryMakeRandomItem(ShopLv);
						if (thing13.Num == 1)
						{
							thing13.SetNum(thing13.trait.DefaultStock);
						}
						if (thing13.trait is TraitFoodMeal)
						{
							CraftUtil.MakeDish(thing13, ShopLv, owner.Chara);
						}
						if (thing13.IsFood && owner.id == "rodwyn")
						{
							SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row e) => !e.foodEffect.IsEmpty() && !e.tag.Contains("noRodwyn") && e.category != "feat" && e.chance > 0).RandomItem();
							thing13.elements.SetBase(row.id, 10 + EClass.rnd(10));
						}
					}
					if (CurrencyType == CurrencyType.Casino_coin)
					{
						thing13.noSell = true;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						thing13.isStolen = true;
					}
					if (!(thing13.trait is TraitErohon))
// ---- around 2377 ----
					}
					AddThing(ThingGen.CreateScroll(8780, EClass.rndHalf(5)));
				}
				foreach (Thing thing13 in t.things)
				{
					thing13.c_idBacker = 0;
					if (ShopType != ShopType.Copy)
					{
						thing13.TryMakeRandomItem(ShopLv);
						if (thing13.Num == 1)
						{
							thing13.SetNum(thing13.trait.DefaultStock);
						}
						if (thing13.trait is TraitFoodMeal)
						{
							CraftUtil.MakeDish(thing13, ShopLv, owner.Chara);
						}
						if (thing13.IsFood && owner.id == "rodwyn")
						{
							SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row e) => !e.foodEffect.IsEmpty() && !e.tag.Contains("noRodwyn") && e.category != "feat" && e.chance > 0).RandomItem();
							thing13.elements.SetBase(row.id, 10 + EClass.rnd(10));
						}
					}
					if (CurrencyType == CurrencyType.Casino_coin)
					{
						thing13.noSell = true;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						thing13.isStolen = true;
					}
					if (!(thing13.trait is TraitErohon))
					{
						thing13.c_IDTState = 0;
// ---- around 2384 ----
					{
						thing13.TryMakeRandomItem(ShopLv);
						if (thing13.Num == 1)
						{
							thing13.SetNum(thing13.trait.DefaultStock);
						}
						if (thing13.trait is TraitFoodMeal)
						{
							CraftUtil.MakeDish(thing13, ShopLv, owner.Chara);
						}
						if (thing13.IsFood && owner.id == "rodwyn")
						{
							SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row e) => !e.foodEffect.IsEmpty() && !e.tag.Contains("noRodwyn") && e.category != "feat" && e.chance > 0).RandomItem();
							thing13.elements.SetBase(row.id, 10 + EClass.rnd(10));
						}
					}
					if (CurrencyType == CurrencyType.Casino_coin)
					{
						thing13.noSell = true;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						thing13.isStolen = true;
					}
					if (!(thing13.trait is TraitErohon))
					{
						thing13.c_IDTState = 0;
					}
					if (CurrencyType == CurrencyType.Money && (thing13.category.IsChildOf("meal") || thing13.category.IsChildOf("preserved")) && thing13.id != "ration" && !thing13.IsUnique)
					{
						thing13.c_priceFix = -70;
					}
					if (ShopType == ShopType.TravelMerchant)
					{
// ---- around 2408 ----
					if (!(thing13.trait is TraitErohon))
					{
						thing13.c_IDTState = 0;
					}
					if (CurrencyType == CurrencyType.Money && (thing13.category.IsChildOf("meal") || thing13.category.IsChildOf("preserved")) && thing13.id != "ration" && !thing13.IsUnique)
					{
						thing13.c_priceFix = -70;
					}
					if (ShopType == ShopType.TravelMerchant)
					{
						thing13.c_priceFix = 200;
					}
					if (thing13.trait is TraitErohon)
					{
						thing13.c_IDTState = 5;
					}
					if (thing13.IsContainer && !thing13.c_revealLock)
					{
						thing13.RemoveThings();
						t.c_lockLv = 0;
					}
				}
				if (t.things.Count <= t.things.GridSize)
				{
					return;
				}
				int num17 = t.things.width * 10;
				if (t.things.Count > num17)
				{
					int num18 = t.things.Count - num17;
					for (int num19 = 0; num19 < num18; num19++)
					{
						t.things.LastItem().Destroy();
					}
// ---- around 2439 ----
					{
						t.things.LastItem().Destroy();
					}
				}
				t.things.ChangeSize(t.things.width, Mathf.Min(t.things.Count / t.things.width + 1, 10));
				Thing Add(string id, int a, int idSkin)
				{
					CardBlueprint.SetNormalRarity();
					Thing thing10 = ThingGen.Create(id, -1, ShopLv).SetNum(a);
					thing10.idSkin = ((idSkin == -1) ? EClass.rnd(thing10.source.skins.Length + 1) : idSkin);
					return t.AddThing(thing10);
				}
				void AddAdvWeek(int i)
				{
					Thing thing8 = ThingGen.CreateRedBook("advweek_" + i);
					thing8.c_priceFix = -90;
					AddThing(thing8);
				}
				void AddCassette(int idCas, string idQuest, int phase)
				{
					if (idQuest == null || EClass.game.quests.GetPhase(idQuest) >= phase)
					{
						AddThing(ThingGen.CreateCassette(idCas));
					}
				}
				Thing AddThing(Thing _t)
				{
					return t.AddThing(_t);
				}
				void NoRestock(Thing _t)
				{
					string text2 = owner.id;
					if (_t.idSkin != 0)
					{
// ---- around 2488 ----
				void NoRestockId(string _id, int num, int idSkin)
				{
					Thing thing9 = ThingGen.Create(_id).SetNum(num);
					thing9.idSkin = idSkin;
					NoRestock(thing9);
				}
			}

			public Thing CreateStock()
			{
				switch (ShopType)
				{
				case ShopType.Dye:
				{
					Thing thing4 = ThingGen.Create("dye").SetNum(15 + EClass.rnd(30));
					thing4.ChangeMaterial(EClass.sources.materials.rows.Where((SourceMaterial.Row r) => r.tier <= 4).RandomItem().alias);
					return thing4;
				}
				case ShopType.GeneralExotic:
					return FromFilter("shop_generalExotic");
				case ShopType.VMachine:
					if (EClass.rnd(10) == 0)
					{
						return Create("wear_swim");
					}
					if (EClass.rnd(10) == 0)
					{
						return Create("panty");
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
// ---- around 2490 ----
					Thing thing9 = ThingGen.Create(_id).SetNum(num);
					thing9.idSkin = idSkin;
					NoRestock(thing9);
				}
			}

			public Thing CreateStock()
			{
				switch (ShopType)
				{
				case ShopType.Dye:
				{
					Thing thing4 = ThingGen.Create("dye").SetNum(15 + EClass.rnd(30));
					thing4.ChangeMaterial(EClass.sources.materials.rows.Where((SourceMaterial.Row r) => r.tier <= 4).RandomItem().alias);
					return thing4;
				}
				case ShopType.GeneralExotic:
					return FromFilter("shop_generalExotic");
				case ShopType.VMachine:
					if (EClass.rnd(10) == 0)
					{
						return Create("wear_swim");
					}
					if (EClass.rnd(10) == 0)
					{
						return Create("panty");
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
// ---- around 2492 ----
					NoRestock(thing9);
				}
			}

			public Thing CreateStock()
			{
				switch (ShopType)
				{
				case ShopType.Dye:
				{
					Thing thing4 = ThingGen.Create("dye").SetNum(15 + EClass.rnd(30));
					thing4.ChangeMaterial(EClass.sources.materials.rows.Where((SourceMaterial.Row r) => r.tier <= 4).RandomItem().alias);
					return thing4;
				}
				case ShopType.GeneralExotic:
					return FromFilter("shop_generalExotic");
				case ShopType.VMachine:
					if (EClass.rnd(10) == 0)
					{
						return Create("wear_swim");
					}
					if (EClass.rnd(10) == 0)
					{
						return Create("panty");
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
// ---- around 2498 ----
				switch (ShopType)
				{
				case ShopType.Dye:
				{
					Thing thing4 = ThingGen.Create("dye").SetNum(15 + EClass.rnd(30));
					thing4.ChangeMaterial(EClass.sources.materials.rows.Where((SourceMaterial.Row r) => r.tier <= 4).RandomItem().alias);
					return thing4;
				}
				case ShopType.GeneralExotic:
					return FromFilter("shop_generalExotic");
				case ShopType.VMachine:
					if (EClass.rnd(10) == 0)
					{
						return Create("wear_swim");
					}
					if (EClass.rnd(10) == 0)
					{
						return Create("panty");
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
				case ShopType.Magic:
					return FromFilter("shop_magic");
				case ShopType.Ecopo:
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
// ---- around 2500 ----
				case ShopType.Dye:
				{
					Thing thing4 = ThingGen.Create("dye").SetNum(15 + EClass.rnd(30));
					thing4.ChangeMaterial(EClass.sources.materials.rows.Where((SourceMaterial.Row r) => r.tier <= 4).RandomItem().alias);
					return thing4;
				}
				case ShopType.GeneralExotic:
					return FromFilter("shop_generalExotic");
				case ShopType.VMachine:
					if (EClass.rnd(10) == 0)
					{
						return Create("wear_swim");
					}
					if (EClass.rnd(10) == 0)
					{
						return Create("panty");
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
				case ShopType.Magic:
					return FromFilter("shop_magic");
				case ShopType.Ecopo:
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
					{
						TraitSeed.LevelSeed(thing3, (thing3.trait as TraitSeed).row, 1);
// ---- around 2514 ----
					{
						return Create("panty");
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
				case ShopType.Magic:
					return FromFilter("shop_magic");
				case ShopType.Ecopo:
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
					{
						TraitSeed.LevelSeed(thing3, (thing3.trait as TraitSeed).row, 1);
					}
					return thing3;
				}
				case ShopType.AnimalGoods:
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle_waist");
					}
					return Create("pasture").SetNum(EClass.rndHalf(8));
				case ShopType.Healer:
// ---- around 2516 ----
					}
					if (EClass.rnd(5) == 0)
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
				case ShopType.Magic:
					return FromFilter("shop_magic");
				case ShopType.Ecopo:
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
					{
						TraitSeed.LevelSeed(thing3, (thing3.trait as TraitSeed).row, 1);
					}
					return thing3;
				}
				case ShopType.AnimalGoods:
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle_waist");
					}
					return Create("pasture").SetNum(EClass.rndHalf(8));
				case ShopType.Healer:
				{
					Thing thing2 = null;
// ---- around 2518 ----
					{
						return Create("234");
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
				case ShopType.Magic:
					return FromFilter("shop_magic");
				case ShopType.Ecopo:
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
					{
						TraitSeed.LevelSeed(thing3, (thing3.trait as TraitSeed).row, 1);
					}
					return thing3;
				}
				case ShopType.AnimalGoods:
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle_waist");
					}
					return Create("pasture").SetNum(EClass.rndHalf(8));
				case ShopType.Healer:
				{
					Thing thing2 = null;
					for (int i = 0; i < 1000; i++)
					{
// ---- around 2520 ----
					}
					return FromFilter("shop_drink");
				case ShopType.Furniture:
					return FromFilter("shop_furniture");
				case ShopType.Book:
					return FromFilter("shop_book");
				case ShopType.Magic:
					return FromFilter("shop_magic");
				case ShopType.Ecopo:
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
					{
						TraitSeed.LevelSeed(thing3, (thing3.trait as TraitSeed).row, 1);
					}
					return thing3;
				}
				case ShopType.AnimalGoods:
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle_waist");
					}
					return Create("pasture").SetNum(EClass.rndHalf(8));
				case ShopType.Healer:
				{
					Thing thing2 = null;
					for (int i = 0; i < 1000; i++)
					{
						thing2 = FromFilter("shop_healer");
						if (thing2.trait is TraitScroll { source: not null } traitScroll)
// ---- around 2529 ----
				{
					Thing thing3 = TraitSeed.MakeRandomSeed(enc: true);
					if (EClass.rnd(2) == 0)
					{
						TraitSeed.LevelSeed(thing3, (thing3.trait as TraitSeed).row, 1);
					}
					return thing3;
				}
				case ShopType.AnimalGoods:
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle_waist");
					}
					return Create("pasture").SetNum(EClass.rndHalf(8));
				case ShopType.Healer:
				{
					Thing thing2 = null;
					for (int i = 0; i < 1000; i++)
					{
						thing2 = FromFilter("shop_healer");
						if (thing2.trait is TraitScroll { source: not null } traitScroll)
						{
							if (!(traitScroll.source.aliasParent != "WIL") && !(traitScroll.source.categorySub == "attack"))
							{
								break;
							}
						}
						else if (thing2.trait is TraitPotionRandom { source: not null } traitPotionRandom)
						{
							if (!(traitPotionRandom.source.aliasParent != "WIL") && !(traitPotionRandom.source.categorySub == "attack"))
// ---- around 2539 ----
					{
						return Create("saddle");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("saddle_waist");
					}
					return Create("pasture").SetNum(EClass.rndHalf(8));
				case ShopType.Healer:
				{
					Thing thing2 = null;
					for (int i = 0; i < 1000; i++)
					{
						thing2 = FromFilter("shop_healer");
						if (thing2.trait is TraitScroll { source: not null } traitScroll)
						{
							if (!(traitScroll.source.aliasParent != "WIL") && !(traitScroll.source.categorySub == "attack"))
							{
								break;
							}
						}
						else if (thing2.trait is TraitPotionRandom { source: not null } traitPotionRandom)
						{
							if (!(traitPotionRandom.source.aliasParent != "WIL") && !(traitPotionRandom.source.categorySub == "attack"))
							{
								thing2.SetNum(EClass.rnd(5) + 1);
								break;
							}
						}
						else if (thing2.trait is TraitRodRandom { source: not null } traitRodRandom && !(traitRodRandom.source.aliasParent != "WIL") && !(traitRodRandom.source.categorySub == "attack"))
						{
							break;
						}
					}
// ---- around 2567 ----
						}
						else if (thing2.trait is TraitRodRandom { source: not null } traitRodRandom && !(traitRodRandom.source.aliasParent != "WIL") && !(traitRodRandom.source.categorySub == "attack"))
						{
							break;
						}
					}
					return thing2;
				}
				case ShopType.Milk:
					if (EClass._zone is Zone_Nefu && EClass.rnd(2) == 0)
					{
						Thing thing = ThingGen.Create("_milk");
						thing.MakeRefFrom(EClass.sources.charas.rows.Where((SourceChara.Row r) => r.race == "mifu" || r.race == "nefu").RandomItem().model);
						Debug.Log(thing);
						return thing;
					}
					return Create("_milk");
				case ShopType.Map:
					return ThingGen.CreateMap();
				case ShopType.Plan:
					return Create("book_plan");
				case ShopType.Weapon:
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
// ---- around 2576 ----
					if (EClass._zone is Zone_Nefu && EClass.rnd(2) == 0)
					{
						Thing thing = ThingGen.Create("_milk");
						thing.MakeRefFrom(EClass.sources.charas.rows.Where((SourceChara.Row r) => r.race == "mifu" || r.race == "nefu").RandomItem().model);
						Debug.Log(thing);
						return thing;
					}
					return Create("_milk");
				case ShopType.Map:
					return ThingGen.CreateMap();
				case ShopType.Plan:
					return Create("book_plan");
				case ShopType.Weapon:
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
// ---- around 2578 ----
						Thing thing = ThingGen.Create("_milk");
						thing.MakeRefFrom(EClass.sources.charas.rows.Where((SourceChara.Row r) => r.race == "mifu" || r.race == "nefu").RandomItem().model);
						Debug.Log(thing);
						return thing;
					}
					return Create("_milk");
				case ShopType.Map:
					return ThingGen.CreateMap();
				case ShopType.Plan:
					return Create("book_plan");
				case ShopType.Weapon:
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
// ---- around 2580 ----
						Debug.Log(thing);
						return thing;
					}
					return Create("_milk");
				case ShopType.Map:
					return ThingGen.CreateMap();
				case ShopType.Plan:
					return Create("book_plan");
				case ShopType.Weapon:
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
// ---- around 2582 ----
					}
					return Create("_milk");
				case ShopType.Map:
					return ThingGen.CreateMap();
				case ShopType.Plan:
					return Create("book_plan");
				case ShopType.Weapon:
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
// ---- around 2588 ----
				case ShopType.Weapon:
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
// ---- around 2589 ----
					return FromFilter("shop_weapon");
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
// ---- around 2590 ----
				case ShopType.Gun:
					if (EClass.rnd(8) == 0)
					{
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
					if (EClass.rnd(2) == 0)
// ---- around 2593 ----
						return Create("mod_ranged");
					}
					return FromFilter("shop_gun");
				case ShopType.Blackmarket:
				case ShopType.Exotic:
				case ShopType.TravelMerchant:
				{
					int num = 30;
					if (ShopType == ShopType.TravelMerchant)
					{
						num = 5;
					}
					if (Guild.Thief.IsCurrentZone)
					{
						num = 25;
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
					if (EClass.rnd(2) == 0)
					{
						return Create("bait");
					}
// ---- around 2608 ----
					}
					if (Guild.Merchant.IsCurrentZone)
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
					if (EClass.rnd(2) == 0)
					{
						return Create("bait");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("fishingRod");
					}
					return FromFilter("shop_fish");
				case ShopType.Meat:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
// ---- around 2610 ----
					{
						num = 15;
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
					if (EClass.rnd(2) == 0)
					{
						return Create("bait");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("fishingRod");
					}
					return FromFilter("shop_fish");
				case ShopType.Meat:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
					}
					return FromFilter("shop_bread");
// ---- around 2612 ----
					}
					CardBlueprint.SetRarity((EClass.rnd(num * 5) == 0) ? Rarity.Mythical : ((EClass.rnd(num) == 0) ? Rarity.Legendary : ((EClass.rnd(5) == 0) ? Rarity.Superior : Rarity.Normal)));
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
					if (EClass.rnd(2) == 0)
					{
						return Create("bait");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("fishingRod");
					}
					return FromFilter("shop_fish");
				case ShopType.Meat:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
					}
					return FromFilter("shop_bread");
				case ShopType.Sweet:
					if (EClass.rnd(6) == 0)
// ---- around 2614 ----
					return FromFilter("shop_blackmarket");
				}
				case ShopType.Drink:
					return FromFilter("shop_drink");
				case ShopType.Booze:
					return FromFilter("shop_booze");
				case ShopType.Fruit:
					return FromFilter("shop_fruit");
				case ShopType.Fish:
					if (EClass.rnd(2) == 0)
					{
						return Create("bait");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("fishingRod");
					}
					return FromFilter("shop_fish");
				case ShopType.Meat:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
					}
					return FromFilter("shop_bread");
				case ShopType.Sweet:
					if (EClass.rnd(6) == 0)
					{
						return Create("dough");
// ---- around 2624 ----
					{
						return Create("bait");
					}
					if (EClass.rnd(3) == 0)
					{
						return Create("fishingRod");
					}
					return FromFilter("shop_fish");
				case ShopType.Meat:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
					}
					return FromFilter("shop_bread");
				case ShopType.Sweet:
					if (EClass.rnd(6) == 0)
					{
						return Create("dough");
					}
					if (EClass.rnd(4) == 0)
					{
						return Create("cream");
					}
					return FromFilter("shop_sweet");
				case ShopType.Curry:
					if (EClass.rnd(3) == 0)
					{
						return Create("seasoning");
// ---- around 2630 ----
					}
					return FromFilter("shop_fish");
				case ShopType.Meat:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
					}
					return FromFilter("shop_bread");
				case ShopType.Sweet:
					if (EClass.rnd(6) == 0)
					{
						return Create("dough");
					}
					if (EClass.rnd(4) == 0)
					{
						return Create("cream");
					}
					return FromFilter("shop_sweet");
				case ShopType.Curry:
					if (EClass.rnd(3) == 0)
					{
						return Create("seasoning");
					}
					return Create("693");
				case ShopType.Food:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
// ---- around 2636 ----
					}
					return FromFilter("shop_meat");
				case ShopType.Bread:
					if (EClass.rnd(3) == 0)
					{
						return Create("dough");
					}
					return FromFilter("shop_bread");
				case ShopType.Sweet:
					if (EClass.rnd(6) == 0)
					{
						return Create("dough");
					}
					if (EClass.rnd(4) == 0)
					{
						return Create("cream");
					}
					return FromFilter("shop_sweet");
				case ShopType.Curry:
					if (EClass.rnd(3) == 0)
					{
						return Create("seasoning");
					}
					return Create("693");
				case ShopType.Food:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_food");
				case ShopType.Drug:
					return FromFilter("shop_drug");
				case ShopType.LoytelMart:
				{
// ---- around 2646 ----
					{
						return Create("dough");
					}
					if (EClass.rnd(4) == 0)
					{
						return Create("cream");
					}
					return FromFilter("shop_sweet");
				case ShopType.Curry:
					if (EClass.rnd(3) == 0)
					{
						return Create("seasoning");
					}
					return Create("693");
				case ShopType.Food:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_food");
				case ShopType.Drug:
					return FromFilter("shop_drug");
				case ShopType.LoytelMart:
				{
					int loytelMartLv = EClass.player.flags.loytelMartLv;
					if (loytelMartLv >= 1)
					{
						if (EClass.rnd(10) == 0)
						{
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
// ---- around 2652 ----
					}
					return FromFilter("shop_sweet");
				case ShopType.Curry:
					if (EClass.rnd(3) == 0)
					{
						return Create("seasoning");
					}
					return Create("693");
				case ShopType.Food:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_food");
				case ShopType.Drug:
					return FromFilter("shop_drug");
				case ShopType.LoytelMart:
				{
					int loytelMartLv = EClass.player.flags.loytelMartLv;
					if (loytelMartLv >= 1)
					{
						if (EClass.rnd(10) == 0)
						{
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
						}
						if (EClass.rnd(100) == 0)
						{
							return ThingGen.Create("map_treasure", -1, EClass.rndHalf(ShopLv));
						}
						if (EClass.rnd(40) == 0)
// ---- around 2658 ----
					}
					return Create("693");
				case ShopType.Food:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_food");
				case ShopType.Drug:
					return FromFilter("shop_drug");
				case ShopType.LoytelMart:
				{
					int loytelMartLv = EClass.player.flags.loytelMartLv;
					if (loytelMartLv >= 1)
					{
						if (EClass.rnd(10) == 0)
						{
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
						}
						if (EClass.rnd(100) == 0)
						{
							return ThingGen.Create("map_treasure", -1, EClass.rndHalf(ShopLv));
						}
						if (EClass.rnd(40) == 0)
						{
							return Create("water").SetPriceFix(1000);
						}
						if (EClass.rnd(1000) == 0)
						{
							return Create("1165");
// ---- around 2660 ----
				case ShopType.Food:
					if (EClass.rnd(5) == 0)
					{
						return Create("seasoning");
					}
					return FromFilter("shop_food");
				case ShopType.Drug:
					return FromFilter("shop_drug");
				case ShopType.LoytelMart:
				{
					int loytelMartLv = EClass.player.flags.loytelMartLv;
					if (loytelMartLv >= 1)
					{
						if (EClass.rnd(10) == 0)
						{
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
						}
						if (EClass.rnd(100) == 0)
						{
							return ThingGen.Create("map_treasure", -1, EClass.rndHalf(ShopLv));
						}
						if (EClass.rnd(40) == 0)
						{
							return Create("water").SetPriceFix(1000);
						}
						if (EClass.rnd(1000) == 0)
						{
							return Create("1165");
						}
					}
// ---- around 2667 ----
					return FromFilter("shop_drug");
				case ShopType.LoytelMart:
				{
					int loytelMartLv = EClass.player.flags.loytelMartLv;
					if (loytelMartLv >= 1)
					{
						if (EClass.rnd(10) == 0)
						{
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
						}
						if (EClass.rnd(100) == 0)
						{
							return ThingGen.Create("map_treasure", -1, EClass.rndHalf(ShopLv));
						}
						if (EClass.rnd(40) == 0)
						{
							return Create("water").SetPriceFix(1000);
						}
						if (EClass.rnd(1000) == 0)
						{
							return Create("1165");
						}
					}
					if ((loytelMartLv >= 2 || EClass.debug.enable) && EClass.rnd(10) == 0)
					{
						SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row r) => r.tag.Contains("loytelMart") && ShopLv + 10 >= r.LV).ToList().RandomItemWeighted((SourceElement.Row r) => r.chance);
						switch ((from _s in row.thing.ToCharArray()
							where _s != ' '
							select _s).RandomItem())
						{
// ---- around 2671 ----
					if (loytelMartLv >= 1)
					{
						if (EClass.rnd(10) == 0)
						{
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
						}
						if (EClass.rnd(100) == 0)
						{
							return ThingGen.Create("map_treasure", -1, EClass.rndHalf(ShopLv));
						}
						if (EClass.rnd(40) == 0)
						{
							return Create("water").SetPriceFix(1000);
						}
						if (EClass.rnd(1000) == 0)
						{
							return Create("1165");
						}
					}
					if ((loytelMartLv >= 2 || EClass.debug.enable) && EClass.rnd(10) == 0)
					{
						SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row r) => r.tag.Contains("loytelMart") && ShopLv + 10 >= r.LV).ToList().RandomItemWeighted((SourceElement.Row r) => r.chance);
						switch ((from _s in row.thing.ToCharArray()
							where _s != ' '
							select _s).RandomItem())
						{
						case 'B':
							return ThingGen.CreateSpellbook(row.id);
						case 'P':
							return ThingGen.CreatePotion(row.id);
// ---- around 2675 ----
							return Create("monsterball").SetLv(40 + EClass.rnd(ShopLv)).Thing;
						}
						if (EClass.rnd(30) == 0)
						{
							return ThingGen.Create("rp_random", -1, ShopLv + 10);
						}
						if (EClass.rnd(100) == 0)
						{
							return ThingGen.Create("map_treasure", -1, EClass.rndHalf(ShopLv));
						}
						if (EClass.rnd(40) == 0)
						{
							return Create("water").SetPriceFix(1000);
						}
						if (EClass.rnd(1000) == 0)
						{
							return Create("1165");
						}
					}
					if ((loytelMartLv >= 2 || EClass.debug.enable) && EClass.rnd(10) == 0)
					{
						SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row r) => r.tag.Contains("loytelMart") && ShopLv + 10 >= r.LV).ToList().RandomItemWeighted((SourceElement.Row r) => r.chance);
						switch ((from _s in row.thing.ToCharArray()
							where _s != ' '
							select _s).RandomItem())
						{
						case 'B':
							return ThingGen.CreateSpellbook(row.id);
						case 'P':
							return ThingGen.CreatePotion(row.id);
						case 'R':
							return ThingGen.CreateRod(row.id);
						case 'S':
							return ThingGen.CreateScroll(row.id);
// ---- around 2688 ----
						}
						if (EClass.rnd(1000) == 0)
						{
							return Create("1165");
						}
					}
					if ((loytelMartLv >= 2 || EClass.debug.enable) && EClass.rnd(10) == 0)
					{
						SourceElement.Row row = EClass.sources.elements.rows.Where((SourceElement.Row r) => r.tag.Contains("loytelMart") && ShopLv + 10 >= r.LV).ToList().RandomItemWeighted((SourceElement.Row r) => r.chance);
						switch ((from _s in row.thing.ToCharArray()
							where _s != ' '
							select _s).RandomItem())
						{
						case 'B':
							return ThingGen.CreateSpellbook(row.id);
						case 'P':
							return ThingGen.CreatePotion(row.id);
						case 'R':
							return ThingGen.CreateRod(row.id);
						case 'S':
							return ThingGen.CreateScroll(row.id);
						}
					}
					return FromFilter("shop_junk");
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
// ---- around 2705 ----
						case 'R':
							return ThingGen.CreateRod(row.id);
						case 'S':
							return ThingGen.CreateScroll(row.id);
						}
					}
					return FromFilter("shop_junk");
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
// ---- around 2706 ----
							return ThingGen.CreateRod(row.id);
						case 'S':
							return ThingGen.CreateScroll(row.id);
						}
					}
					return FromFilter("shop_junk");
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[13]
// ---- around 2708 ----
							return ThingGen.CreateScroll(row.id);
						}
					}
					return FromFilter("shop_junk");
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[13]
							{
								"1127", "1128", "xmas_sled", "xmas_bigbag", "xmas_bigbox", "xmas_blackcat", "xmas_blackcat", "xmas_jure", "xmas_crown", "xmas_ball",
// ---- around 2710 ----
					}
					return FromFilter("shop_junk");
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[13]
							{
								"1127", "1128", "xmas_sled", "xmas_bigbag", "xmas_bigbox", "xmas_blackcat", "xmas_blackcat", "xmas_jure", "xmas_crown", "xmas_ball",
								"xmas_ball", "xmas_ball", "xmas_string"
							}.RandomItem());
// ---- around 2711 ----
					return FromFilter("shop_junk");
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[13]
							{
								"1127", "1128", "xmas_sled", "xmas_bigbag", "xmas_bigbox", "xmas_blackcat", "xmas_blackcat", "xmas_jure", "xmas_crown", "xmas_ball",
								"xmas_ball", "xmas_ball", "xmas_string"
							}.RandomItem());
						}
// ---- around 2712 ----
				}
				case ShopType.Junk:
				case ShopType.Moyer:
					return FromFilter("shop_junk");
				case ShopType.Souvenir:
					return FromFilter("shop_souvenir");
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[13]
							{
								"1127", "1128", "xmas_sled", "xmas_bigbag", "xmas_bigbox", "xmas_blackcat", "xmas_blackcat", "xmas_jure", "xmas_crown", "xmas_ball",
								"xmas_ball", "xmas_ball", "xmas_string"
							}.RandomItem());
						}
					}
// ---- around 2718 ----
				case ShopType.StrangeGirl:
					return DNA.GenerateGene(SpawnList.Get("chara").Select(ShopLv + 10), DNA.Type.Brain);
				case ShopType.Fireworks:
					if (EClass.rnd(3) == 0)
					{
						return Create("firework_launcher");
					}
					return Create("firework");
				case ShopType.Festival:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("olvina"))
						{
							return Create(new string[4] { "1125", "1126", "pillow_truth", "1230" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[3] { "hat_mushroom", "hat_witch", "hat_kumiromi" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[13]
							{
								"1127", "1128", "xmas_sled", "xmas_bigbag", "xmas_bigbox", "xmas_blackcat", "xmas_blackcat", "xmas_jure", "xmas_crown", "xmas_ball",
								"xmas_ball", "xmas_ball", "xmas_string"
							}.RandomItem());
						}
					}
					if (EClass.rnd(2) == 0)
					{
						return Create(new string[4] { "1081", "1082", "1083", "1084" }.RandomItem());
					}
					if (EClass.rnd(3) == 0)
					{
// ---- around 2747 ----
					{
						return Create(new string[4] { "1081", "1082", "1083", "1084" }.RandomItem());
					}
					if (EClass.rnd(3) == 0)
					{
						return FromFilter("shop_junk");
					}
					return FromFilter("shop_souvenir");
				case ShopType.Lamp:
					if (EClass.rnd(3) != 0)
					{
						if (IsFestival("kapul"))
						{
							return Create(new string[6] { "999", "1000", "1001", "1002", "1003", "1004" }.RandomItem());
						}
						if (IsFestival("yowyn"))
						{
							return Create(new string[2] { "1072", "1073" }.RandomItem());
						}
						if (IsFestival("noyel"))
						{
							return Create(new string[1] { "1069" }.RandomItem());
						}
						if (IsFestival("olvina"))
						{
							return Create(new string[2] { "1070", "1071" }.RandomItem());
						}
					}
					if (EClass._zone.IsFestival && EClass.rnd(2) == 0)
					{
						return Create(new string[4] { "953", "954", "955", "956" }.RandomItem());
					}
					return FromFilter("shop_lamp");
				default:
// ---- around 2781 ----
					if (EClass.rnd(100) == 0)
					{
						return Create("lockpick");
					}
					return FromFilter("shop_general");
				}
				Thing Create(string s)
				{
					return ThingGen.Create(s, -1, ShopLv);
				}
				Thing FromFilter(string s)
				{
					return ThingGen.CreateFromFilter(s, ShopLv);
				}
				static bool IsFestival(string id)
				{
					if (EClass._zone.id == id)
					{
						return EClass._zone.IsFestival;
					}
					return false;
				}
			}
		}

// ---- around 2785 ----
					return FromFilter("shop_general");
				}
				Thing Create(string s)
				{
					return ThingGen.Create(s, -1, ShopLv);
				}
				Thing FromFilter(string s)
				{
					return ThingGen.CreateFromFilter(s, ShopLv);
				}
				static bool IsFestival(string id)
				{
					if (EClass._zone.id == id)
					{
						return EClass._zone.IsFestival;
					}
					return false;
				}
			}
		}

