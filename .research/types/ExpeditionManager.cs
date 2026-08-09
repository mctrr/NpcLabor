using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ExpeditionManager : EClass
{
	[JsonProperty]
	public Dictionary<int, Expedition> dict = new Dictionary<int, Expedition>();

	public FactionBranch branch;

	public void SetOwner(FactionBranch _branch)
	{
		branch = _branch;
		foreach (Expedition value in dict.Values)
		{
			value.SetOwner(branch);
		}
	}

	public void Add(Expedition ex)
	{
		dict[ex.uidChara] = ex;
		ex.Start();
	}

	public void OnSimulateHour()
	{
		dict.Values.ToList().ForeachReverse(delegate(Expedition e)
		{
			e.OnAdvanceHour();
		});
	}
}
