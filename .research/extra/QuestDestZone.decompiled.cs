using Newtonsoft.Json;

public class QuestDestZone : QuestRandom
{
	[JsonProperty]
	public int uidDest;

	[JsonProperty]
	public int uidTarget;

	public virtual bool IsDeliver => true;

	public Zone DestZone => RefZone.Get(uidDest);

	public override Chara DestChara
	{
		get
		{
			if (IsDeliver && DestZone == EClass._zone)
			{
				Chara obj = EClass._zone.FindChara(uidTarget);
				if (obj != null && obj.IsAliveInCurrentZone)
				{
					return EClass._zone.FindChara(uidTarget);
				}
			}
			return base.chara;
		}
	}

	public override string RefDrama3
	{
		get
		{
			if (DestZone != null)
			{
				return DestZone.Name;
			}
			return base.ClientZone?.Name ?? "???";
		}
	}

	public override string RefDrama4
	{
		get
		{
			object obj;
			if (uidTarget != 0)
			{
				obj = DestZone.dictCitizen.TryGetValue(uidTarget);
				if (obj == null)
				{
					return "???";
				}
			}
			else
			{
				obj = "";
			}
			return (string)obj;
		}
	}

	public void SetDest(Zone z, int target)
	{
		uidDest = z.uid;
		uidTarget = target;
	}
}
