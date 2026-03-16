using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CatalogId : EvaluatorBase_List<WellKnownCatalogId>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_List<WellKnownCatalogId>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Instance.CatalogId);
		}
		return false;
	}
}
