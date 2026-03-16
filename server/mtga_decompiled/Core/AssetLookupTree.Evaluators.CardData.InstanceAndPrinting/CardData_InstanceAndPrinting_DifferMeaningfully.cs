using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData.InstanceAndPrinting;

public class CardData_InstanceAndPrinting_DifferMeaningfully : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null && bb.CardData.Printing != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, CardUtilities.DoesInstanceDifferMeaningfullyFromPrinting(bb.CardData));
		}
		return false;
	}
}
