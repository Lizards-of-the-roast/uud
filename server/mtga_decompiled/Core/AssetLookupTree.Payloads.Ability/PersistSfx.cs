using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Ability;

public class PersistSfx : ILayeredPayload, IPayload
{
	public SfxData SfxData = new SfxData();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
