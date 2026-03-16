using System.Linq;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_ActionTypes : EvaluatorBase_List<ActionType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<ActionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Actions.Select((ActionInfo x) => x.Action.ActionType), MinCount, MaxCount);
		}
		return false;
	}
}
