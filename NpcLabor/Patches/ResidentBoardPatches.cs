using HarmonyLib;
using UnityEngine;

namespace NpcLabor.Patches;

/// <summary>
/// Vanilla BaseListPeople.OnInstantiate fades off-map residents with:
///   gameObject.AddComponent&lt;CanvasGroup&gt;().alpha = 0.6f
/// Dispatch moves workers off the home zone, so they hit this path.
/// DynamicScrollView recycles ItemGeneralPeople rows; the leftover
/// CanvasGroup makes AddComponent return null and NRE on .alpha.
/// Recycled home rows would also stay faded. Drop the leftover first.
/// </summary>
[HarmonyPatch(typeof(BaseListPeople), nameof(BaseListPeople.OnInstantiate))]
internal static class ResidentBoardCanvasGroupPatch
{
    [HarmonyPrefix]
    static void Prefix(ItemGeneral b)
    {
        if (b == null)
        {
            return;
        }

        try
        {
            CanvasGroup existing = b.GetComponent<CanvasGroup>();
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("resident board CanvasGroup: " + ex.Message);
        }
    }
}
