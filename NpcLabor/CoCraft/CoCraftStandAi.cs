using System.Collections.Generic;

namespace NpcLabor.CoCraft;

/// <summary>
/// Co-craft stand-in-place AI. The assistant is teleported beside the PC when the
/// craft starts (Open), then held here with noMove = true so they stay put for the
/// whole craft instead of wandering off mid-progress. Replaced with NoGoal on Clear.
/// If the player commands the NPC mid-craft the new AI replaces this one and
/// OnCancel frees movement, so the NPC can obey.
/// </summary>
internal class CoCraftStandAi : AIAct
{
    public override bool CancelWhenDamaged => false;

    public override bool CanManualCancel() => false;

    public override void OnCancel()
    {
        // Player command or Clear replaced us — stop pinning the NPC in place.
        try
        {
            if (owner != null)
            {
                owner.noMove = false;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("CoCraftStandAi.cs silent catch: " + __e.Message); }
}

    public override IEnumerable<Status> Run()
    {
        // Stand idle forever until replaced; noMove keeps the body still.
        while (true)
        {
            yield return Status.Running;
        }
    }
}
