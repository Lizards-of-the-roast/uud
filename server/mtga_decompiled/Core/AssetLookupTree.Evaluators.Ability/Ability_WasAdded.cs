using System.Linq;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_WasAdded : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.AddedAbilities != null && bb.Ability != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.AddedAbilities.Contains(bb.Ability));
		}
		return false;
	}
}
