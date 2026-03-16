using System.Threading.Tasks;
using AssetLookupTree;
using AssetLookupTree.Payloads.Carousel;
using Wotc.Mtga.Network.ServiceWrappers;

internal class CarouselAssetFilter : ICarouselFilter
{
	public async Task<bool> checkVisible(Client_CarouselItem item)
	{
		if (WrapperController.Instance == null)
		{
			return false;
		}
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CarouselName = item.AssetTreeItem;
		AssetLookupTree<CarouselPayload> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<CarouselPayload>();
		if (assetLookupTree == null)
		{
			return false;
		}
		CarouselPayload payload = assetLookupTree.GetPayload(assetLookupSystem.Blackboard);
		if (payload == null || payload.item == null)
		{
			SimpleLog.LogPreProdError("Asset Lookup Tree is missing \"" + item.AssetTreeItem + "\" required by carousel item: " + item.Name);
			return false;
		}
		return true;
	}
}
