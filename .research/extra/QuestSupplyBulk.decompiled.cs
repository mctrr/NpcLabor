public class QuestSupplyBulk : QuestSupplySpecific
{
	public override string RefDrama4 => "_unit2".lang(num.ToString() ?? "", base.sourceThing.GetText("unit"));

	public override DifficultyType difficultyType => DifficultyType.Bulk;

	public override int GetDestNum()
	{
		return 1 + difficulty * 3 + EClass.rnd(5);
	}
}
