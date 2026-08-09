public class TraitDyeMaker : TraitCrafter
{
	public override string IdSource => "DyeMaker";

	public override string CrafterTitle => "invBoil";

	public override ToggleType ToggleType => ToggleType.Fire;

	public override AnimeID IdAnimeProgress => AnimeID.Shiver;

	public override AnimeType animeType => AnimeType.Pot;

	public override string idSoundProgress => "cook_pot";

	public override string idSoundBG => "bg_boil";

	public override int numIng => 2;

	public override string IDReqEle(RecipeSource r)
	{
		return "alchemy";
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.1.8388' (yours is '8.2.0.7535-95108c96')
