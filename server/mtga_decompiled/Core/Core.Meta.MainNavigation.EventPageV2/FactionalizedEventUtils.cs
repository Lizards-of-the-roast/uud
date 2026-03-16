using AssetLookupTree;
using AssetLookupTree.Payloads.Event;
using Wizards.MDN;

namespace Core.Meta.MainNavigation.EventPageV2;

public static class FactionalizedEventUtils
{
	public static bool TryFetchFactionalizedEvent_BackgroundPayload(EventContext context, string eventFaction, AssetLookupSystem assetLookupSystem, out BackgroundPayload payload)
	{
		payload = null;
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BackgroundPayload> loadedTree))
		{
			return false;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.Event = context;
		assetLookupSystem.Blackboard.Flavor = eventFaction;
		BackgroundPayload payload2 = loadedTree.GetPayload(assetLookupSystem.Blackboard);
		if (payload2 != null)
		{
			payload = payload2;
			return true;
		}
		return false;
	}

	public static bool TryFetchFactionalizedEvent_BannerPayload(EventContext context, string eventFaction, AssetLookupSystem assetLookupSystem, out BannerPayload payload)
	{
		payload = null;
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BannerPayload> loadedTree))
		{
			return false;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.Event = context;
		assetLookupSystem.Blackboard.Flavor = eventFaction;
		BannerPayload payload2 = loadedTree.GetPayload(assetLookupSystem.Blackboard);
		if (payload2 != null)
		{
			payload = payload2;
			return true;
		}
		return false;
	}
}
