using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasDamageProtection : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			bool inValue = bb.CardData.Instance.ReplacementEffects.Exists((ReplacementEffectData x) => x.SpawnerType.Contains(ReplacementEffectSpawnerType.PreventDamage) && x.RecipientIds != null && x.RecipientIds.Contains(bb.CardData.InstanceId));
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
		}
		return false;
	}
}
