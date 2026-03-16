using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_DigitalReleaseSet : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrEmpty(bb.CardData?.Printing?.DigitalReleaseSet))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CardData.Printing?.DigitalReleaseSet);
		}
		return false;
	}
}
