public class QuestMeal : QuestSupplySpecific
{
	public override bool UseWeight => false;

	public override DifficultyType difficultyType => DifficultyType.Meal;
}
