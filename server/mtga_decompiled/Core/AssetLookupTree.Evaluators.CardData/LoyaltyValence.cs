using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Evaluators.CardData;

public class LoyaltyValence : EvaluatorBase_List<Wotc.Mtga.Cards.Text.LoyaltyValence>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<Wotc.Mtga.Cards.Text.LoyaltyValence>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LoyaltyValence);
	}
}
