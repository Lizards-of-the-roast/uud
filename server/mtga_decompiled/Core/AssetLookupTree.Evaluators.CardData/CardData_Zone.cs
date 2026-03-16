using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Zone : EvaluatorBase_List<ZoneType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<ZoneType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.ZoneType);
		}
		return false;
	}
}
