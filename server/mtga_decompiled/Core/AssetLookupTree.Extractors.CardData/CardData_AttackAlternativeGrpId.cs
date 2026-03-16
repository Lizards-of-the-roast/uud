using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_AttackAlternativeGrpId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData?.Instance == null)
		{
			return false;
		}
		if (bb.GameState != null && bb.GameState.AttackInfo.TryGetValue(bb.CardData.InstanceId, out var value2))
		{
			value = (int)value2.AlternativeGrpId;
		}
		return true;
	}
}
