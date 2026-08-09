public class TraitBarrelMaker : TraitCrafter
{
	public override string IdSource => "BarrelMaker";

	public override string CrafterTitle => "invMod";

	public override AnimeID IdAnimeProgress => AnimeID.Shiver;

	public override string idSoundProgress => "grind";

	public override string idSoundComplete => "grind_finish";

	public override int numIng => 2;

	public override bool StopSoundProgress => true;

	public override bool IsConsumeIng => false;

	public override bool ShouldConsumeIng(SourceRecipe.Row item, int index)
	{
		return false;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.1.8388' (yours is '8.2.0.7535-95108c96')
