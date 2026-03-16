using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CounterType : EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.CounterType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.CounterType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CounterType);
	}
}
