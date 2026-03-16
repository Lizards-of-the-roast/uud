using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.LinkInfo;

public class LinkedInfoText_TypeCategory : EvaluatorBase_List<TypeCategory>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<TypeCategory>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LinkedInfoText.Category);
	}
}
