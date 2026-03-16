using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Browser;

public class Browser_IsModal : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardBrowserType == DuelSceneBrowserType.SelectCards && bb.CardBrowserLayoutID == "Modal");
	}
}
