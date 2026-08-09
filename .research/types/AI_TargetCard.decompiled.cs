using System.Collections.Generic;

public class AI_TargetCard : AIAct
{
	public Card target;

	public virtual bool GotoTarget => false;

	public override bool HasProgress => false;

	public virtual bool CanTargetInventory => false;

	public override bool CanProgress()
	{
		if (!target.ExistsOnMap)
		{
			if (CanTargetInventory)
			{
				return target.GetRootCard() == owner;
			}
			return false;
		}
		return true;
	}

	public override IEnumerable<Status> Run()
	{
		isFail = () => !CanProgress();
		if (target.ExistsOnMap)
		{
			if (target.isThing && !GotoTarget)
			{
				yield return DoGotoInteraction(target.pos);
			}
			else
			{
				yield return DoGoto(target);
			}
			if (target.Dist(owner) > 1)
			{
				yield return Cancel();
			}
			owner.LookAt(target);
		}
		else if (!CanTargetInventory || target.GetRootCard() != owner)
		{
			yield return Cancel();
		}
		if (HasProgress)
		{
			yield return DoProgress();
		}
		else
		{
			OnProgressComplete();
		}
	}
}
