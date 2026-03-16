using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_DigitalReleaseSetList : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrEmpty(bb.CardData?.Printing?.DigitalReleaseSet))
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing?.DigitalReleaseSet);
		}
		return false;
	}
}
