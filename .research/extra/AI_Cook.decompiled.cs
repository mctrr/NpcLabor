using System.Collections.Generic;

public class AI_Cook : AIAct
{
	public Card factory;

	public override int LeftHand => 1001;

	public override int RightHand => 1002;

	public bool IsValidTarget(Card c)
	{
		if (factory != null)
		{
			return factory.trait.CanCook(c);
		}
		return false;
	}

	public override IEnumerable<Status> Run()
	{
		factory = EClass._map.Installed.traits.GetRandomThing<TraitCooker>();
		if (!IsValidTarget(owner.held))
		{
			yield return Cancel();
		}
		yield return DoGoto(factory);
		if (!IsValidTarget(owner.held))
		{
			yield return Cancel();
		}
		Card target = owner.DropHeld(factory.pos);
		target.TryReserve(this);
		Progress_Custom seq = new Progress_Custom
		{
			canProgress = () => factory.ExistsOnMap && target.ExistsOnMap,
			onProgress = delegate
			{
				owner.LookAt(factory);
				factory.trait.CookProgress();
			},
			onProgressComplete = delegate
			{
			}
		}.SetDuration(25, 5);
		owner.SetTempHand(-1, -1);
		yield return Do(seq);
		yield return Status.Running;
		if (!owner.CanPick(target))
		{
			yield return Cancel();
		}
		owner.HoldCard(target);
	}
}
