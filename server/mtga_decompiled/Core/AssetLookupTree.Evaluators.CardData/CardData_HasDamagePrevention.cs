using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasDamagePrevention : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			bool inValue = bb.CardData.Instance.ReplacementEffects.Exists((ReplacementEffectData x) => x.SpawnerType.Contains(ReplacementEffectSpawnerType.PreventDamage) && x.SourceIds != null && x.SourceIds.Contains(bb.CardData.InstanceId));
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
		}
		return false;
	}
}
