public class TraitScratchMachine : TraitCrafter
{
	public override string IdSource => "Scratch";

	public override string CrafterTitle => "invScratch";

	public override AnimeID IdAnimeProgress => AnimeID.Shiver;

	public override string idSoundProgress => "craft_scratch";

	public override int GetDuration(AI_UseCrafter ai, int costSp)
	{
		return GetSource(ai).time;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.1.8388' (yours is '8.2.0.7535-95108c96')
