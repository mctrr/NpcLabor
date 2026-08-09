public class TraitMerchant : TraitCitizen
{
	public override int GuidePriotiy => 10;

	public override string IDRumor => "shopkeeper";

	public override ShopType ShopType => ShopType.Goods;

	public override bool CanInvest => true;

	public override void OnCreate(int lv)
	{
		base.owner.c_invest = EClass.rnd(10);
	}
}
