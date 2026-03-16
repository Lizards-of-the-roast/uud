using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Browser;

public class BackgroundSFX : IPayload
{
	public readonly SfxData OpenEvent = new SfxData();

	public readonly SfxData CloseEvent = new SfxData();

	public readonly SfxData SelectionEvent = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return null;
	}
}
