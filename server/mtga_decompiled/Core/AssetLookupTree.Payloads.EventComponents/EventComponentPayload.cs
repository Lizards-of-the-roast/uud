using System.Collections.Generic;
using EventPage.Components;

namespace AssetLookupTree.Payloads.EventComponents;

public abstract class EventComponentPayload<TComponent> : IPayload where TComponent : EventComponent
{
	public AltAssetReference<TComponent> EventComponent = new AltAssetReference<TComponent>();

	public string PrefabPath => EventComponent?.RelativePath;

	public IEnumerable<string> GetFilePaths()
	{
		if (EventComponent != null)
		{
			yield return EventComponent.RelativePath;
		}
	}
}
