using UnityEngine.UI;

public class ItemQuest : EMono
{
	public Portrait portrait;

	public UIButton button;

	public UIText textDetail;

	public UIText textReward;

	public UIText textDate;

	public UIText textDifficulty;

	public UIText textExtra;

	public UIText textExtra2;

	public Image imageNew;

	public void SetQuest(Quest q)
	{
		portrait.SetPerson(q.person);
		button.mainText.text = (EMono.game.quests.list.Contains(q) ? "questInProgress".lang().TagColor(FontColor.Good) : "") + q.TitlePrefix + q.GetTitle();
		button.subText.text = q.person.NameBraced;
		textDate.SetText(Date.GetText(q.Hours));
		textDate.transform.parent.RebuildLayout();
		textReward.SetText(q.GetRewardText());
		string text = q.GetDetail();
		if (q is QuestSupply questSupply && questSupply.GetDestThing() != null)
		{
			text = "questSupplyOwned".lang().TagColor(FontColor.Good) + text;
		}
		textDetail.SetText(text);
		imageNew.SetActive(q.isNew);
		q.isNew = false;
		textDifficulty.SetText("★".Repeat(q.difficulty));
		textDifficulty.SetActive(q.IsRandomQuest);
		string text2 = q.TextExtra;
		textExtra.SetActive(!text2.IsEmpty());
		textExtra.SetText(text2);
		text2 = q.TextExtra2;
		textExtra2.SetActive(!text2.IsEmpty());
		textExtra2.SetText(text2);
	}
}
