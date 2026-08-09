using System.Collections.Generic;

public class GoalHobby : GoalWork
{
	public override bool IsHobby => true;

	public override List<Hobby> GetWorks()
	{
		return owner.ListHobbies();
	}
}
