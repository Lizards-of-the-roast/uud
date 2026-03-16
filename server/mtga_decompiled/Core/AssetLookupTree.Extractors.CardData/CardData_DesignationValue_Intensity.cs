using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_DesignationValue_Intensity : IExtractor<int>
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
		foreach (DesignationData designation in bb.CardData.Instance.Designations)
		{
			if (designation.Type == Designation.Intensity && designation.Value.HasValue)
			{
				value = (int)designation.Value.Value;
				return true;
			}
		}
		return false;
	}
}
