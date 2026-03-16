using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.LinkInfo;

public class LinkInfo_AbilityIsSourceAbility : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, !bb.LinkedInfoText.Equals(default(LinkedInfoText)) && bb.Ability != null && bb.Ability.Id == bb.LinkInfo.SourceAbilityId);
	}
}
