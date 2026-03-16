using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardHolder;

public class CardHolder_Owner : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, zoneCardHolderBase.PlayerNum);
		}
		return false;
	}
}
