using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.InstanceAndPrinting;

public class CardData_IsDimmed : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.IsDimmedCard);
		}
		return false;
	}
}
