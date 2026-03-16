using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Resolution;

public abstract class SFX_Base : ILayeredPayload, IPayload
{
	public SfxData SfxData = new SfxData();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
