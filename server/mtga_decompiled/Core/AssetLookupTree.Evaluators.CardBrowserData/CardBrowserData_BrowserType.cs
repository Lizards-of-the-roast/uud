using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardBrowserData;

public class CardBrowserData_BrowserType : EvaluatorBase_List<DuelSceneBrowserType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardBrowserType != DuelSceneBrowserType.Invalid)
		{
			return EvaluatorBase_List<DuelSceneBrowserType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardBrowserType);
		}
		return false;
	}
}
