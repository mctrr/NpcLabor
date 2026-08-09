using Newtonsoft.Json;

public class QuestMusic : QuestInstance
{
	[JsonProperty]
	public int score;

	[JsonProperty]
	public int destScore = 10;

	[JsonProperty]
	public int sumMoney;

	public override DifficultyType difficultyType => DifficultyType.Music;

	public override string IdZone => "instance_music";

	public override string RewardSuffix => "Music";

	public override string RefDrama2 => destScore.ToString() ?? "";

	public override int KarmaOnFail => 0;

	public override ZoneEventQuest CreateEvent()
	{
		return new ZoneEventMusic();
	}

	public override ZoneInstanceRandomQuest CreateInstance()
	{
		return new ZoneInstanceMusic();
	}

	public override string GetTextProgress()
	{
		return "progressMusic".lang(score.ToString() ?? "", destScore.ToString() ?? "");
	}

	public override int GetRewardPlat(int money)
	{
		return difficulty + EClass.rnd(2);
	}

	public override void OnInit()
	{
		destScore = difficulty * 150;
		destScore += EClass.rnd(destScore / 5);
	}
}
