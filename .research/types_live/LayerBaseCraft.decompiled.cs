using System.Collections.Generic;

public class LayerBaseCraft : ELayer
{
	public virtual bool CanCancelAI => false;

	public virtual bool RepeatAI => false;

	public virtual List<Thing> GetTargets()
	{
		return null;
	}

	public virtual int GetReqIngredient(int index)
	{
		return 1;
	}

	public virtual void RefreshCurrentGrid()
	{
	}

	public virtual void ClearButtons()
	{
	}

	public virtual void OnEndCraft()
	{
	}
}
