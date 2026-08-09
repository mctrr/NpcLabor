using UnityEngine;

public class ELayer : Layer
{
	private PopItem itemTalk;

	public static Core core => Core.Instance;

	public static Game game => core.game;

	public static bool AdvMode => scene.actionMode == ActionMode.Adv;

	public static Player player => core.game.player;

	public static Chara pc => core.game.player.chara;

	public static FactionBranch Branch => core.game.activeZone.branch;

	public static Faction Home => core.game.factions.Home;

	public static UI ui => core.ui;

	public static Map _map => core.game.activeZone.map;

	public static Zone _zone => core.game.activeZone;

	public static Scene scene => core.scene;

	public static BaseGameScreen screen => core.screen;

	public static CoreConfig config => core.config;

	public static GameSetting setting => core.gameSetting;

	public static ColorProfile Colors => core.Colors;

	public static SourceManager sources => core.sources;

	public static World world => core.game.world;

	public static SoundManager Sound => SoundManager.current;

	public static CoreDebug debug => core.debug;

	public bool IsFloat => ui.layerFloat.layers.Contains(this);

	public virtual string IdHelp => GetType().Name;

	public static int rnd(int a)
	{
		return Rand.Range(0, a);
	}

	public override void OnBeforeAddLayer()
	{
		if (core.IsGameStarted && defaultActionMode)
		{
			ActionMode.DefaultMode.Activate();
		}
	}

	public void InitInspector()
	{
		base.Init();
	}

	public sealed override void Init()
	{
		EInput.Consume(0);
		if (closeOthers)
		{
			ui.RemoveLayers();
		}
		base.Init();
	}

	public override void OnAfterAddLayer()
	{
		TryShowHelp();
		WidgetMainText.HideLog();
	}

	public void TryShowHelp()
	{
		bool flag = Resources.Load<TextAsset>(CorePath.Text_DialogHelp + IdHelp);
		if (flag && !core.config.helpFlags.Contains(IdHelp))
		{
			LayerDrama.ActivateNerun(IdHelp);
			core.config.helpFlags.Add(IdHelp);
		}
		if (windows.Count <= 0 || !windows[0].buttonHelp)
		{
			return;
		}
		windows[0].buttonHelp.SetActive(flag);
		if (flag)
		{
			windows[0].buttonHelp.onClick.AddListener(delegate
			{
				LayerDrama.ActivateNerun(IdHelp);
			});
		}
	}

	public void AddLayerToUI(string id)
	{
		ui.AddLayer(id);
	}

	protected sealed override void _Close()
	{
		base._Close();
		EInput.Consume();
		if (!option.consumeInput)
		{
			EInput.rightMouse.consumed = false;
			EInput.rightMouse.ignoreClick = true;
		}
	}

	protected sealed override void Kill()
	{
		if (!isDestroyed && !(base.gameObject == null))
		{
			if (itemTalk != null)
			{
				itemTalk.important = false;
				WidgetFeed.Instance.pop.Kill(itemTalk);
			}
			BaseCore.Instance.eventSystem.SetSelectedGameObject(null);
			EInput.DisableIME();
			base.Kill();
			if ((bool)option.playlist && core.IsGameStarted)
			{
				_zone.RefreshBGM();
			}
			if (debug.alwaysResetWindow)
			{
				Window.dictData.Clear();
			}
			if (option.hideFloatUI)
			{
				ui.ShowFloats();
			}
			if (option.hideWidgets)
			{
				ui.widgets.Show();
			}
		}
	}

	public void TryShowHint(string _langHint = null)
	{
		if (!_langHint.IsEmpty() && Lang.Has(_langHint))
		{
			langHint = _langHint;
		}
		string text = "h_" + GetType().Name;
		if (!langHint.IsEmpty())
		{
			ui.hud.hint.Show(langHint.lang());
		}
		else if (Lang.Has(text))
		{
			ui.hud.hint.Show(text);
		}
		else if (!option.dontRefreshHint)
		{
			ui.hud.hint.Refresh();
		}
	}

	public void TalkHomeMemeber(string id)
	{
		Msg.TalkHomeMemeber(id);
	}

	public void TalkMaid(string id)
	{
		Msg.TalkMaid(id);
	}

	public override void OnRightClick()
	{
		ButtonGrid componentOf = InputModuleEX.GetComponentOf<ButtonGrid>();
		if (!componentOf || componentOf.invOwner == null)
		{
			base.OnRightClick();
		}
	}
}
