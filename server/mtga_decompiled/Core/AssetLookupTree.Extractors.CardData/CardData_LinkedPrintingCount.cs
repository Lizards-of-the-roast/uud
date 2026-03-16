using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_LinkedPrintingCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData?.Printing?.LinkedFacePrintings == null)
		{
			return false;
		}
		value = bb.CardData.Printing.LinkedFacePrintings.Count;
		return true;
	}
}
