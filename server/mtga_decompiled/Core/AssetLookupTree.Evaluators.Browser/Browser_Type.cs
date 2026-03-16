using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Browser;

public class Browser_Type : EvaluatorBase_List<DuelSceneBrowserType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<DuelSceneBrowserType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardBrowserType);
	}
}
