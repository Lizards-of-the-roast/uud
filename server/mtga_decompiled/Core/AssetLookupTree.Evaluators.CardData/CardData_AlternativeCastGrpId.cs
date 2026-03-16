using System.Linq;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AlternativeCastGrpId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in bb.CardData.Instance.Actions
				where x.Action != null && x.Action.AlternativeGrpId != 0 && x.Action.ActionType == ActionType.Cast
				select (int)x.Action.AlternativeGrpId, MinCount, MaxCount);
		}
		return false;
	}
}
