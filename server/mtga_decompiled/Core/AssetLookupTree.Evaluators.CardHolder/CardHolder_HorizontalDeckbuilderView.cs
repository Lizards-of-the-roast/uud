using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardHolder;

public class CardHolder_HorizontalDeckbuilderView : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		bool inValue = bb.CardHolderType == CardHolderType.Deckbuilder && bb.InHorizontalDeckBuilder;
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
	}
}
