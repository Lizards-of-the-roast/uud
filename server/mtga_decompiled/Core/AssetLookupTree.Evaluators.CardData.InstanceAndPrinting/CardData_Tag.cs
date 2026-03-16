using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.CardData.InstanceAndPrinting;

public class CardData_Tag : EvaluatorBase_List<MetaDataTag>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<MetaDataTag>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Tags, MinCount, MaxCount);
		}
		return false;
	}
}
