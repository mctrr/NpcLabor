using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using NpcLabor.Dispatch;
using NpcLabor.TownLabor;
using NpcLabor.Process;
using ReflexCLI;

namespace NpcLabor;

public static class ModInfo
{
    public const string Guid = "com.elin.npclabor";
    public const string Name = "NPC Labor";
    public const string Version = "1.14.514";
}

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
internal class Plugin : BaseUnityPlugin
{
    internal static Plugin? Instance;
    internal static Harmony? Harmony;
    static bool _reflexRegistered;

    private void Awake()
    {
        Instance = this;
        Harmony = new Harmony(ModInfo.Guid);
        // Register Reflex BEFORE PatchAll so a Harmony failure cannot skip console cmds.
        // BCS only does assemblies.Add; we also Rebuild for late/early Init order.
        RegisterReflexCommands("Awake-pre");
        try
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (System.Exception ex)
        {
            LogError("Harmony.PatchAll failed (console still registered): " + ex.Message);
        }

        RegisterReflexCommands("Awake-post");
        LogInfo($"NPC Labor {ModInfo.Version}: slice A co-craft + B processor + D dungeon dispatch + E town labor ready.");
        LaborConfig.EnsureLoaded();
        SpecialRewardConfig.EnsureLoaded();
    }

    private void Start()
    {
        // After EModdingKit.Start -> CommandRegistry.Init(), force one more scan.
        // Init rebuilds from assemblies list; if we only Add'ed earlier we are fine,
        // but if Init order was weird, this guarantees bare NpcLabor* commands exist.
        _reflexRegistered = false;
        RegisterReflexCommands("Start");
    }

    private void OnDestroy()
    {
        Harmony?.UnpatchSelf();
        Harmony = null;
        CoCraft.CoCraftSession.Clear("plugin-destroy");
        ProcessorJobSession.Clear("plugin-destroy", announce: false);
        DungeonDispatchManager.ClearAllRuntime();
        TownLaborManager.ClearAllRuntime();
    }

    internal static void RegisterReflexCommands(string phase)
    {
        try
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (!CommandRegistry.assemblies.Contains(asm))
            {
                CommandRegistry.assemblies.Add(asm);
                _reflexRegistered = false;
            }

            // Always rebuild once after we are sure our assembly is listed.
            // Safe to call multiple times; BCS only Adds, but Init may have already run.
            if (!_reflexRegistered)
            {
                CommandRegistry.Rebuild();
                _reflexRegistered = true;
                LogInfo("Reflex commands registered (" + phase + "): NpcLaborHarvest / NpcLaborComplete / NpcLaborCompleteOne");
            }
        }
        catch (System.Exception ex)
        {
            _reflexRegistered = false;
            LogWarn("Reflex CommandRegistry register failed (" + phase + "): " + ex.Message);
        }
    }

    internal static void LogDebug(object message, [CallerMemberName] string caller = "")
        => Instance?.Logger.LogDebug($"[{caller}] {message}");

    internal static void LogInfo(object message)
        => Instance?.Logger.LogInfo(message);

    internal static void LogWarn(object message)
        => Instance?.Logger.LogWarning(message);

    internal static void LogError(object message)
        => Instance?.Logger.LogError(message);
}
