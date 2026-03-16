using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_SplitCard : EvaluatorBase_Boolean
{
	public bool IgnoreInstance = true;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.IsSplitCard(ignoreInstances: true));
		}
		return false;
	}
}
