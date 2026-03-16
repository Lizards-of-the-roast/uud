using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Ability;

public class PlayWarningHangerEntry : HangerEntry, ILayeredPayload, IPayload
{
	public new IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
