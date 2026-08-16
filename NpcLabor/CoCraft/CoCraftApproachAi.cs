using System.Collections.Generic;
using UnityEngine;

namespace NpcLabor.CoCraft;

/// <summary>
/// Co-craft approach AI: the assistant walks to the PC (no teleport), then stands
/// in place beside them for the whole craft. If the path is blocked or they cannot
/// arrive in time, SnapAssistantToPc is used as a last-resort fallback. Replaced
/// with NoGoal on Clear; a player command replaces it too and OnCancel frees movement.
/// </summary>
internal class CoCraftApproachAi : AIAct
{
    /// <summary>Ticks without closing distance before snapping beside the PC.</summary>
    const int StuckTicks = 90;

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
        catch (System.Exception __e) { Plugin.LogDebug("CoCraftApproachAi.cs silent catch: " + __e.Message); }
}

    public override IEnumerable<Status> Run()
    {
        int lastDist = 99;
        int stuck = 0;
        while (CoCraftSession.Active && !CoCraftSession.ApproachExpired())
        {
            Chara? pc = EClass.pc;
            if (owner == null || owner.isDead || pc == null || !pc.ExistsOnMap)
            {
                yield break;
            }

            if (owner.ExistsOnMap && owner.Dist(pc) <= 1)
            {
                break; // already beside the PC
            }

            int dist = owner.ExistsOnMap ? owner.Dist(pc) : 99;
            if (dist < lastDist)
            {
                lastDist = dist;
                stuck = 0;
            }
            else if (++stuck >= StuckTicks)
            {
                break; // path blocked — snap fallback below
            }

            yield return DoGoto(pc, 1);
        }

        if (CoCraftSession.Active && owner != null && !owner.isDead)
        {
            if (!(owner.ExistsOnMap && EClass.pc != null && owner.Dist(EClass.pc) <= 1))
            {
                // Blocked / too slow: snap beside the PC as a last resort.
                if (!CoCraftSession.SnapAssistantToPc(owner))
                {
                    yield break; // give up; the craft continues without the assistant
                }
            }

            // Stand in place until the craft ends.
            try { owner.noMove = true; } catch { }
            while (CoCraftSession.Active)
            {
                yield return Status.Running;
            }

            try { if (owner != null) owner.noMove = false; } catch { }
        }

        yield break;
    }
}
