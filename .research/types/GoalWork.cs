using System.Collections.Generic;

public class GoalWork : Goal
{
	public virtual bool IsHobby => false;

	public virtual List<Hobby> GetWorks()
	{
		return owner.ListWorks();
	}

	public override IEnumerable<Status> Run()
	{
		if (FindWork(owner))
		{
			yield return Success();
		}
		yield return DoIdle();
	}

	public void ValidateHobby(Chara c)
	{
		owner = c;
		WorkSummary workSummary = owner.GetWorkSummary();
		List<Hobby> list = owner.ListHobbies();
		workSummary.hobbies.Clear();
		Room room = owner.FindRoom();
		for (int i = 0; i < list.Count; i++)
		{
			if (room != null)
			{
				Lot lot = room.lot;
				if (TryWork(room, list[i], setAI: false))
				{
					continue;
				}
				bool flag = false;
				foreach (Room item in EClass._map.rooms.listRoom)
				{
					if (item.lot == lot && item != room && TryWork(item, list[i], setAI: false))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			TryWork(null, list[i], setAI: false);
		}
	}

	public bool FindWork(Chara c, bool setAI = true)
	{
		owner = c;
		WorkSummary workSummary = owner.GetWorkSummary();
		if (IsHobby)
		{
			workSummary.hobbies.Clear();
		}
		else
		{
			workSummary.work = null;
		}
		Room room = owner.FindRoom();
		if (room != null)
		{
			if (TryWork(room, setAI))
			{
				return true;
			}
			Lot lot = room.lot;
			foreach (Room item in EClass._map.rooms.listRoom)
			{
				if (item.lot == lot && item != room && !item.IsPrivate && TryWork(item, setAI))
				{
					return true;
				}
			}
		}
		if (TryWork(null, setAI))
		{
			return true;
		}
		return false;
	}

	public bool TryWork(BaseArea destArea, bool setAI = true)
	{
		List<Hobby> works = GetWorks();
		if (IsHobby)
		{
			Hobby h = works.RandomItem();
			if (TryWork(destArea, h, setAI))
			{
				return true;
			}
			return false;
		}
		foreach (Hobby item in works)
		{
			if (TryWork(destArea, item, setAI))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryWork(BaseArea destArea, Hobby h, bool setAI)
	{
		AIWork aI = h.GetAI(owner);
		aI.destArea = destArea;
		if (aI.SetDestination())
		{
			WorkSummary workSummary = owner.GetWorkSummary();
			if (IsHobby)
			{
				workSummary.hobbies.Add(aI.GetSession());
			}
			else
			{
				workSummary.work = aI.GetSession();
			}
			if (setAI)
			{
				owner.SetAI(aI);
			}
			return true;
		}
		return false;
	}

	public override void OnSimulatePosition()
	{
		Room room = owner.FindRoom();
		if (room != null)
		{
			owner.MoveImmediate(room.GetRandomFreePos());
		}
	}
}
