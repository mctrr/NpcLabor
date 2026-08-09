public class AIWork_Chore : AIWork
{
	public override void SetDestPos()
	{
		if (EClass._map.rooms.listRoom.Count > 0 && EClass.rnd(2) == 0)
		{
			Room room = EClass._map.rooms.listRoom.RandomItem();
			if (!room.IsPrivate)
			{
				destPos = room.GetRandomPoint();
			}
			else
			{
				destPos = EClass._map.bounds.GetRandomSurface();
			}
		}
		else
		{
			destPos = EClass._map.bounds.GetRandomSurface();
		}
	}

	public override void OnPerformWork(bool realtime)
	{
	}
}
