using HarmonyLib;

using NpcLabor.Process;

namespace NpcLabor.Patches;

/// <summary>
/// Allow AI_NpcProcess workers to walk through other characters
/// (equivalent to vanilla party-follow behavior).
/// </summary>
[HarmonyPatch(typeof(Chara))]
public static class MovementPatches
{
    [HarmonyPatch(nameof(Chara.CanMoveTo))]
    [HarmonyPrefix]
    public static bool CanMoveTo_Prefix(Chara __instance, Point p, ref bool __result)
    {
        if (__instance == null || p == null)
            return true;

        if (!ProcessorJobSession.Active || ProcessorJobSession.NpcUid == 0)
            return true;

        if (__instance.uid != ProcessorJobSession.NpcUid)
            return true;

        // Allow processor NPC to enter tiles occupied by other characters
        // (same behavior as party members following the PC)
        if (p.HasChara && !__instance.IsMultisize && !__instance.CanReplace(p.FirstChara))
        {
            __result = true;
            return false; // Skip original HasChara block
        }

        return true;
    }
}
