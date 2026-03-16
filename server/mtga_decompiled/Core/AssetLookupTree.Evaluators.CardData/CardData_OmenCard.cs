using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_OmenCard : EvaluatorBase_Boolean
{
	public bool IgnoreInstances;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.IsOmenCard(IgnoreInstances));
		}
		return false;
	}
}
