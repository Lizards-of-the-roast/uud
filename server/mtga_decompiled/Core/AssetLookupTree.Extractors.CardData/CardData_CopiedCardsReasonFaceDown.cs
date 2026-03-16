using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_CopiedCardsReasonFaceDown : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.Instance == null)
		{
			return false;
		}
		value = (int)bb.CardData.Instance.FaceDownState.CopiedCardsReasonFaceDown;
		return true;
	}
}
