using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Browser;

public class Browser_LayoutKey : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.CardBrowserLayoutID))
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardBrowserLayoutID);
		}
		return false;
	}
}
