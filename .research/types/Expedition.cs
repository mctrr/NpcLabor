using Newtonsoft.Json;

public class Expedition : EClass
{
	[JsonProperty]
	public int hours;

	[JsonProperty]
	public int uidChara;

	[JsonProperty]
	public ExpeditionType type;

	public HomeResource.CostList costs = new HomeResource.CostList();

	public FactionBranch branch;

	public Chara chara => RefChara.Get(uidChara);

	public int MinHour => (int)((float)hours * 0.8f);

	public int MaxHour => (int)((float)hours * 1.2f);

	public string strType => ("ex" + type).lang();

	public void SetOwner(FactionBranch _branch)
	{
		branch = _branch;
	}

	public void OnCreate()
	{
		_ = branch.resources;
		switch (type)
		{
		case ExpeditionType.Search:
			hours = 24;
			break;
		case ExpeditionType.Training:
			hours = 120;
			break;
		case ExpeditionType.Explore:
			hours = 10;
			break;
		case ExpeditionType.Hunt:
			hours = 10;
			break;
		default:
			hours = 24;
			break;
		}
	}

	public void Start()
	{
		hours = Rand.Range(MinHour, MaxHour);
		costs.Pay();
		WidgetPopText.Say("expeditionStart".lang(chara.Name, strType));
		EClass.Branch.Log("bExpeditionStart", chara, strType);
		if (chara.IsInActiveZone)
		{
			Msg.Say("bExpeditionStart", chara, strType);
		}
		chara.MoveZone("somewhere");
	}

	public void OnAdvanceHour()
	{
		hours--;
		if (hours <= 0)
		{
			End();
		}
	}

	public void End()
	{
		branch.expeditions.dict.Remove(uidChara);
		chara.MoveZone(chara.homeZone);
		WidgetPopText.Say("expeditionEnd".lang(chara.Name, strType));
		branch.Log("bExpeditionEnd", chara, strType);
		if (chara.IsInActiveZone)
		{
			Msg.Say("bExpeditionEnd", chara, strType);
		}
		switch (type)
		{
		case ExpeditionType.Search:
		{
			foreach (Zone item in EClass.world.region.ListZonesInRadius(branch.owner))
			{
				if (!item.isKnown)
				{
					item.isKnown = true;
					WidgetPopText.Say("discoverZone".lang(item.Name), FontColor.Great);
				}
			}
			Zone zone = EClass.world.region.CreateRandomSite(chara.homeZone);
			if (zone != null)
			{
				zone.isKnown = true;
				WidgetPopText.Say("discoverZone".lang(zone.Name), FontColor.Great);
			}
			break;
		}
		case ExpeditionType.Explore:
			chara.GetWorkSummary().progress = 100;
			break;
		}
	}

	public static Expedition Create(Chara c, ExpeditionType type)
	{
		Expedition expedition = new Expedition();
		expedition.uidChara = c.uid;
		expedition.type = type;
		expedition.SetOwner(c.homeBranch);
		expedition.OnCreate();
		return expedition;
	}
}
