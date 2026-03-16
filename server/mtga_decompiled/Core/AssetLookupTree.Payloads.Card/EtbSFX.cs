using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class EtbSFX : ILayeredPayload, IPayload
{
	public SfxData SfxData = new SfxData();

	public bool ForceSubtype;

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
