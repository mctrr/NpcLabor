using HarmonyLib;
using NpcLabor.CoCraft;
using NpcLabor.Dispatch;
using NpcLabor.TownLabor;
using NpcLabor.Process;

namespace NpcLabor.Patches;

[HarmonyPatch(typeof(AI_UseCrafter))]
internal static class AiUseCrafterPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AI_UseCrafter.OnStart))]
    static void OnStartPrefix(AI_UseCrafter __instance)
    {
        try
        {
            OpenFromAi(__instance);
        }
        catch (System.Exception ex)
        {
            Plugin.LogWarn($"co-craft OnStart failed: {ex}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AI_UseCrafter.OnEnd))]
    static void OnEndPrefix()
    {
        CoCraftSession.Clear("ai-end");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AI_UseCrafter.OnCancel))]
    static void OnCancelPrefix()
    {
        // OnCancel calls OnEnd; clear early so any cancel-side skill reads are vanilla.
        CoCraftSession.Clear("ai-cancel");
    }

    internal static void OpenFromAi(AI_UseCrafter ai)
    {
        if (ai == null || EClass.pc == null)
        {
            return;
        }

        // Only assist when the PC is the craft owner.
        if (ai.owner != null && ai.owner != EClass.pc)
        {
            return;
        }

        int skillId = 0;
        Recipe? recipe = ai.recipe;
        TraitCrafter? crafter = ai.crafter;

        if (recipe?.source != null)
        {
            skillId = recipe.source.GetReqSkill().id;
        }
        else if (crafter != null)
        {
            string alias = crafter.IDReqEle(recipe?.source);
            if (!alias.IsEmpty())
            {
                try
                {
                    skillId = EClass.sources.elements.alias[alias].id;
                }
                catch
                {
                    skillId = 0;
                }
            }
        }

        if (skillId == 0)
        {
            return;
        }

        Chara? assistant = AssistantResolver.ResolveForCraft(skillId);
        if (assistant == null)
        {
            CoCraftSession.Clear("no-assistant");
            return;
        }

        CoCraftSession.Open(assistant, recipe, crafter);
        if (ai.num > 0)
        {
            CoCraftSession.BatchNum = ai.num;
        }
    }
}

[HarmonyPatch(typeof(Chara), nameof(Chara.Die))]
internal static class PcDeathClearPatch
{
    [HarmonyPrefix]
    static void Prefix(Chara __instance)
    {
        if (__instance == null)
        {
            return;
        }

        if (__instance.IsPC)
        {
            if (CoCraftSession.Active)
            {
                CoCraftSession.Clear("pc-death");
            }

            if (ProcessorJobSession.Active)
            {
                ProcessorJobSession.Clear("pc-death");
            }

            return;
        }

        // Worker death aborts their processor job.
        if (ProcessorJobSession.Active && __instance.uid == ProcessorJobSession.NpcUid)
        {
            ProcessorJobSession.Clear("ai-fail");
        }

        // Dispatch worker death drops the mission (no death penalty rewards).
        try
        {
            DungeonDispatchManager.OnWorkerDied(__instance);
        }
        catch (System.Exception __e) { Plugin.LogDebug("LifecyclePatches.cs silent catch: " + __e.Message); }
// Town labor worker death aborts mission (no main prize).
        try
        {
            TownLaborManager.OnWorkerDied(__instance);
        }
        catch (System.Exception __e) { Plugin.LogDebug("LifecyclePatches.cs silent catch: " + __e.Message); }
}
}

[HarmonyPatch(typeof(Player), nameof(Player.MoveZone), typeof(Zone))]
internal static class ZoneChangeClearPatch
{
    [HarmonyPrefix]
    static void Prefix()
    {
        if (CoCraftSession.Active)
        {
            CoCraftSession.Clear("zone-change");
        }

        if (ProcessorJobSession.Active)
        {
            ProcessorJobSession.Clear("zone-change");
        }
    }
}

/// <summary>
/// Confirm before PC leaves a map while town labor is active.
/// Real leave starts at Chara.MoveZone; Player.MoveZone is scene init after the move is already committed.
/// </summary>
[HarmonyPatch(typeof(Chara), nameof(Chara.MoveZone), typeof(Zone), typeof(ZoneTransition))]
internal static class TownLaborLeaveConfirmPatch
{
    static bool _allowNextPcMove;

    [HarmonyPrefix]
    static bool Prefix(Chara __instance, Zone z, ZoneTransition transition)
    {
        try
        {
            if (__instance == null || !__instance.IsPC)
            {
                return true;
            }

            if (_allowNextPcMove)
            {
                _allowNextPcMove = false;
                return true;
            }

            if (z == null)
            {
                return true;
            }

            // Same-zone no-op is already handled in vanilla; still skip confirm.
            try
            {
                if (__instance.currentZone != null && __instance.currentZone.uid == z.uid)
                {
                    return true;
                }
            }
            catch
            {
            }

            if (!TownLaborManager.ShouldConfirmLeave(z, out string? prompt) || string.IsNullOrEmpty(prompt))
            {
                return true;
            }

            Zone dest = z;
            ZoneTransition trans = transition;
            // PC self-work: multi-choice (abort / hand off / cancel). Companion-only: YesNo.
            if (TownLaborManager.HasActivePcSelfLabor())
            {
                // Menu itself aborts / hands off; stay on map so the next leave is free.
                TownLaborUi.OpenPcSelfLeaveMenu();
                return false;
            }

            Dialog.YesNo(prompt, () =>
            {
                try
                {
                    _allowNextPcMove = true;
                    if (EClass.pc != null && dest != null)
                    {
                        if (trans != null)
                        {
                            EClass.pc.MoveZone(dest, trans);
                        }
                        else
                        {
                            EClass.pc.MoveZone(dest);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    _allowNextPcMove = false;
                    Plugin.LogWarn("townlabor leave confirm yes: " + ex.Message);
                }
            });
            return false;
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("townlabor leave confirm: " + ex.Message);
            return true;
        }
    }
}

