using System.Collections.Generic;
using AssetLookupTree.Payloads.Helpers;

namespace AssetLookupTree.Payloads.DuelScene.Interactions.ModalBrowser;

public abstract class ModalBrowserHeaderLocPayload : IPayload
{
	public bool UseActionTypeHeader;

	public readonly ClientOrGreLocKey HeaderLocKey = new ClientOrGreLocKey();

	public readonly ClientOrGreLocKey SubheaderLocKey = new ClientOrGreLocKey();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
