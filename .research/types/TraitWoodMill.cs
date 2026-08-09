public class TraitWoodMill : TraitCrafter
{
	public override string IdSource => "WoodMill";

	public override int numIng => 2;

	public override string CrafterTitle => "invWoodMill";

	public override AnimeID IdAnimeProgress => AnimeID.Shiver;

	public override string idSoundProgress => "cook_pound";

	public override ToggleType ToggleType => ToggleType.Custom;

	public override AnimeType animeType => AnimeType.Microwave;

	public override bool AutoTurnOff => true;

	public override bool AutoToggle => false;

	public override void PlayToggleEffect(bool silent)
	{
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.1.8388' (yours is '8.2.0.7535-95108c96')
