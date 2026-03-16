using System.Collections.Generic;
using Wotc.Mtga.CardParts.FieldFillers;

namespace AssetLookupTree.Payloads.ManaCostOverride;

public class ManaCostOverridePayload : IPayload
{
	public ManaCostFillerUtils.ManaCostOverride ManaCostOverride;

	public ManaCostFillerUtils.ModifiedComparer ModifiedComparer;

	public string CostFormat = string.Empty;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
