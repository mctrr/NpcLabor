public class RefChara : EClass
{
	public Chara chara;

	public Chara GetAndCache(int uid)
	{
		if (chara != null && (chara.IsGlobal || chara.IsAliveInCurrentZone))
		{
			return chara;
		}
		if (uid == 0)
		{
			return null;
		}
		chara = Core.Instance.game.cards.globalCharas.TryGetValue(uid);
		if (chara == null)
		{
			foreach (Chara chara in EClass._map.charas)
			{
				if (chara.uid == uid)
				{
					this.chara = chara;
					break;
				}
			}
		}
		return this.chara;
	}

	public void Set(ref int val, Chara c)
	{
		chara = c;
		val = c?.uid ?? 0;
	}

	public static Chara Get(int uid)
	{
		return Core.Instance.game.cards.globalCharas.TryGetValue(uid);
	}
}
