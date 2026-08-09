using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

public class WorkSession : EClass
{
	[JsonProperty]
	public List<WorkOutcome> outcomes;

	[JsonProperty]
	public int[] ints = new int[5];

	public BitArray32 _bits;

	public int id
	{
		get
		{
			return ints[1];
		}
		set
		{
			ints[1] = value;
		}
	}

	public int efficiency
	{
		get
		{
			return ints[2];
		}
		set
		{
			ints[2] = value;
		}
	}

	[OnSerializing]
	private void _OnSerializing(StreamingContext context)
	{
		ints[0] = _bits.ToInt();
	}

	[OnDeserialized]
	private void _OnDeserialized(StreamingContext context)
	{
		_bits.SetInt(ints[0]);
	}

	public void AddOutcome(WorkOutcome o)
	{
		if (outcomes == null)
		{
			outcomes = new List<WorkOutcome>();
		}
		outcomes.Add(o);
	}
}
