public class QuestHunt : QuestRandom
{
	public override int KarmaOnFail => -3;

	public override string RewardSuffix => "Hunt";

	public override int RangeDeadLine => 20;

	public override void OnInit()
	{
		SetTask(new QuestTaskHunt());
	}
}
