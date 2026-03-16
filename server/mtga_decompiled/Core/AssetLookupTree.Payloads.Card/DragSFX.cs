using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class DragSFX : ILayeredPayload, IPayload
{
	public SfxData SfxData = new SfxData();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
