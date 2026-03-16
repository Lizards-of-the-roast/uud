using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Evaluators.CardText;

public class CardText_EntryType : EvaluatorBase_List<EntryType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardTextEntry != null)
		{
			return EvaluatorBase_List<EntryType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardTextEntry.GetEntryType());
		}
		return false;
	}
}
