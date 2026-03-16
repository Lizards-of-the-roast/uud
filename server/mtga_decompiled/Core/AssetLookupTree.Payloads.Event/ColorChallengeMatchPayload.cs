using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Event;

public class ColorChallengeMatchPayload : IPayload
{
	public AltAssetReference<ColorChallengeMatch> DataRef = new AltAssetReference<ColorChallengeMatch>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return DataRef.RelativePath;
	}
}
