using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_ArtSize : EvaluatorBase_List<CardArtSize>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<CardArtSize>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.ArtSize);
		}
		return false;
	}
}
