using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_LinkedInstanceCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData?.Instance?.LinkedFaceInstances == null)
		{
			return false;
		}
		value = bb.CardData.Instance.LinkedFaceInstances.Count;
		return true;
	}
}
