using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Counters : EvaluatorBase_Int
{
	public Wotc.Mtgo.Gre.External.Messaging.CounterType CounterType = Wotc.Mtgo.Gre.External.Messaging.CounterType.P1P1;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		int value = 0;
		bb.CardData.Counters.TryGetValue(CounterType, out value);
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, value);
	}
}
