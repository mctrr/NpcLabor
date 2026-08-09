using System;
using UnityEngine;

public class ItemQuestTracker : EMono
{
	private FastString sb = new FastString();

	private FastString lastSb = new FastString();

	public Color colorTitle;

	public Quest quest;

	public UIText text;

	public UIText textTitle;

	public UIButton buttonGoto;

	public void Refresh()
	{
		if (!quest.track || !EMono.game.quests.list.Contains(quest))
		{
			Kill();
			return;
		}
		sb.Clear();
		if (quest.deadline > 0 && !quest.UseInstanceZone)
		{
			sb.Append("- " + "days1".lang() + quest.TextDeadline);
			sb.Append(Environment.NewLine);
		}
		string value = quest.GetTrackerText().TrimEnd(Environment.NewLine.ToCharArray());
		sb.Append(value);
		bool enable = false;
		if (quest is QuestDeliver)
		{
			QuestDeliver questDeliver = quest as QuestDeliver;
			if (!questDeliver.IsDeliver || EMono._zone == questDeliver.DestZone)
			{
				Chara tg = EMono._map.FindChara(questDeliver.IsDeliver ? questDeliver.uidTarget : (questDeliver.person.chara?.uid ?? 0));
				if (tg != null)
				{
					enable = true;
					buttonGoto.SetOnClick(delegate
					{
						if (!EMono.pc.HasNoGoal)
						{
							SE.BeepSmall();
						}
						else
						{
							EMono.pc.SetAIImmediate(new AI_Goto(tg, 1));
						}
					});
				}
			}
		}
		buttonGoto.SetActive(enable);
		if (!sb.Equals(lastSb))
		{
			this.SetActive(enable: true);
			textTitle.SetText(quest.GetTitle().TagColor(colorTitle));
			text.SetText(sb.ToString());
			lastSb.Set(sb);
			this.RebuildLayout();
		}
	}

	public void OnClickClose()
	{
		if ((bool)EMono.ui.GetLayer<LayerJournal>())
		{
			return;
		}
		quest.track = false;
		Kill();
		SE.Trash();
		if (quest is QuestTrackCraft)
		{
			EMono.game.quests.Remove(quest);
			if ((bool)LayerCraft.Instance)
			{
				LayerCraft.Instance.RefreshTrackButton();
			}
		}
	}

	public void Kill()
	{
		if ((bool)WidgetQuestTracker.Instance)
		{
			WidgetQuestTracker.Instance.items.Remove(this);
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}
}
