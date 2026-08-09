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
You are not using the latest version of the tool, please update.
Latest version is '10.1.1.8388' (yours is '8.2.0.7535-95108c96')
