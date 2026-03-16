using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Browser;

public class Browser_ElementKey : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.CardBrowserElementID))
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardBrowserElementID);
		}
		return false;
	}
}
