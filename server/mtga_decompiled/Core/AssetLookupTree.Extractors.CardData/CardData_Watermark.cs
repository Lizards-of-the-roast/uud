using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_Watermark : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CardData?.Printing == null)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(bb.CardData.Printing.Watermark))
		{
			return false;
		}
		value = bb.CardData.Printing.Watermark;
		return true;
	}
}
