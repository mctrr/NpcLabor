using UnityEngine;

public class EMono : MonoBehaviour
{
	public static Core core;

	public static Game game => core.game;

	public static bool AdvMode => ActionMode.IsAdv;

	public static Player player => core.game.player;

	public static Chara pc => core.game.player.chara;

	public static UI ui => core.ui;

	public static Map _map => core.game.activeZone.map;

	public static Zone _zone => core.game.activeZone;

	public static FactionBranch Branch => core.game.activeZone.branch;

	public static FactionBranch BranchOrHomeBranch => Branch ?? pc.homeBranch;

	public static Faction Home => core.game.factions.Home;

	public static Scene scene => core.scene;

	public static BaseGameScreen screen => core.screen;

	public static GameSetting setting => core.gameSetting;

	public static GameData gamedata => core.gamedata;

	public static ColorProfile Colors => core.Colors;

	public static World world => core.game.world;

	public static SoundManager Sound => SoundManager.current;

	public static SourceManager sources => core.sources;

	public static SourceManager editorSources => Core.SetCurrent().sources;

	public static CoreDebug debug => core.debug;

	public static int rnd(int a)
	{
		return Rand.rnd(a);
	}
}
