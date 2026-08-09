using System.Collections.Generic;
using UnityEngine.UI;

public class WidgetQuestTracker : Widget
{
	public static WidgetQuestTracker Instance;

	public List<ItemQuestTracker> items;

	public ItemQuestTracker mold;

	public LayoutGroup layout;

	private void OnEnable()
	{
		InvokeRepeating("Refresh", 0.5f, 0.5f);
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	public override void OnActivate()
	{
		Instance = this;
		Refresh();
	}

	public void Refresh()
	{
		if (EMono.game == null || (bool)LayerDrama.Instance)
		{
			return;
		}
		foreach (Quest item in EMono.game.quests.list)
		{
			if (!item.track)
			{
				continue;
			}
			ItemQuestTracker itemQuestTracker = null;
			foreach (ItemQuestTracker item2 in items)
			{
				if (item2.quest == item)
				{
					itemQuestTracker = item2;
					break;
				}
			}
			if (!(itemQuestTracker != null))
			{
				itemQuestTracker = Util.Instantiate(mold, layout);
				itemQuestTracker.SetActive(enable: false);
				itemQuestTracker.quest = item;
				items.Add(itemQuestTracker);
			}
		}
		items.ForeachReverse(delegate(ItemQuestTracker i)
		{
			i.Refresh();
		});
		this.RebuildLayout();
		if (items.Count == 0)
		{
			EMono.ui.widgets.DeactivateWidget(this);
		}
		else
		{
			EMono.player.questTracker = true;
		}
	}

	public static void Show()
	{
		if (!EMono.ui.widgets.GetWidget("QuestTracker"))
		{
			EMono.ui.widgets.Toggle("QuestTracker");
		}
	}

	public static bool TryShow()
	{
		bool flag = false;
		foreach (Quest item in EMono.game.quests.list)
		{
			if (item.track)
			{
				flag = true;
			}
		}
		if (flag || (bool)EMono.ui.widgets.GetWidget("QuestTracker"))
		{
			EMono.ui.widgets.Toggle("QuestTracker")?.SoundActivate();
			return false;
		}
		if (!EMono.ui.GetLayer<LayerJournal>())
		{
			EMono.ui.ToggleLayer<LayerJournal>().SwitchContent(0, 0);
		}
		return true;
	}
}
