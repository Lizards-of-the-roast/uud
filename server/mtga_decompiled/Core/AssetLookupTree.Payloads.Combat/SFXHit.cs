using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Combat;

public class SFXHit : IPayload
{
	public List<AudioEvent> AudioEvents = new List<AudioEvent>(1);

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
