public class TraitMill : TraitCrafter
{
	public override string IdSource => "Mill";

	public override int numIng => 2;

	public override string CrafterTitle => "invMill";

	public override AnimeID IdAnimeProgress => AnimeID.Shiver;

	public override string idSoundProgress => "cook_grind";
}
