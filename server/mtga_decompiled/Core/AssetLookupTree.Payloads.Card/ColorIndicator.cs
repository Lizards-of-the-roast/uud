using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class ColorIndicator : IPayload
{
	public enum DisplayType
	{
		None,
		PrintedIndicatorColors,
		CurrentColor
	}

	public DisplayType Type;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
