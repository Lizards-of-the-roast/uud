using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards.Text;

namespace AssetLookupTree.Evaluators.CardText.Loyalty;

public class CardText_Loyalty_Valence : EvaluatorBase_List<LoyaltyValence>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardTextEntry is LoyaltyTextEntry loyaltyTextEntry)
		{
			return EvaluatorBase_List<LoyaltyValence>.GetResult(ExpectedValues, Operation, ExpectedResult, loyaltyTextEntry.GetValence());
		}
		return false;
	}
}
