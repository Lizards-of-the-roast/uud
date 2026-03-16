using System.Collections.Generic;

namespace AssetLookupTree.Payloads;

public abstract class SfxPayload : ILayeredPayload, IPayload
{
	public SfxData SfxData = new SfxData();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
