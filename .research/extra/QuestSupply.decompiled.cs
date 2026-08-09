public class QuestSupply : QuestDeliver
{
	public override string TextExtra2
	{
		get
		{
			if (!(this is QuestSupplyCat))
			{
				return "";
			}
			return "deliverCategory".lang();
		}
	}

	public override bool ForbidTeleport => false;

	public override bool IsDeliver => false;

	public override string RewardSuffix => "SupplyCost";

	public override DifficultyType difficultyType => DifficultyType.Supply;

	public override int GetBonus(Thing t)
	{
		return (int)((float)t.GetPrice(CurrencyType.Money, sell: true, PriceType.Shipping, EClass.pc) * 1.5f);
	}
}
