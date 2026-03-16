using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class Deckbuilder_ColumnStackOffsetOverrides : IPayload
{
	public float VerticalCardSpacing;

	public float VerticalCardSpacingMultiple;

	public float VerticalCardSpacingQuantityAdjust;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
