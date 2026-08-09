public class TraitChef : TraitMerchantFood
{
	public override ShopType ShopType => ShopType.Sweet;

	public override bool CanServeFood => true;
}
