using HarmonyLib;
using NpcLabor.Process;

namespace NpcLabor.Patches;

/// <summary>
/// When the player picks up a parked ingredient belonging to the active processor
/// job, immediately abort the NPC job. Finished products on the same cell are
/// ignored. Uses a prefix so the ingredient is still on the map when we match it.
/// </summary>
[HarmonyPatch(typeof(Chara), nameof(Chara.Pick))]
internal static class ProcessorPickPatch
{
    [HarmonyPrefix]
    static bool Prefix(Chara __instance, Thing t)
    {
        try
        {
            if (!ProcessorJobSession.Active || __instance == null || t == null || t.isDestroyed)
            {
                return true;
            }

            if (!__instance.IsPC)
            {
                return true;
            }

            // Guard against reentrancy while Clear/ReturnIngredientsToPc picks leftovers.
            if (ProcessorJobSession.IsReturningIngredients)
            {
                return true;
            }

            // Abort when the picked item matches any of the job's claimed ingredients.
            bool isIngredient = false;
            var ings = ProcessorJobSession.Ingredients;
            for (int i = 0; i < ings.Count; i++)
            {
                Thing? ing = ings[i];
                if (ing != null && !ing.isDestroyed && ing.uid == t.uid)
                {
                    isIngredient = true;
                    break;
                }
            }

            if (isIngredient)
            {
                ProcessorJobSession.Clear("pc-pick");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("processor pick abort: " + ex.Message);
        }

        return true;
    }
}
