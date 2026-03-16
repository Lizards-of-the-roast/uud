using EventPage.Components;

namespace AssetLookupTree.Payloads.EventComponents;

public static class ComponentPayloadUtils
{
	public static string GetEventComponentPath<TPayload, TComponent>(this AssetLookupSystem assets) where TPayload : EventComponentPayload<TComponent> where TComponent : EventComponent
	{
		assets.Blackboard.Clear();
		return assets.TreeLoader.LoadTree<TPayload>(returnNewTree: false).GetPayload(assets.Blackboard).PrefabPath;
	}
}
