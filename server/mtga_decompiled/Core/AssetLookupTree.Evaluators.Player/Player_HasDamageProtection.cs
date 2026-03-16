using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Player;

public class Player_HasDamageProtection : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			bool inValue = bb.Player.ReplacementEffects.Exists((ReplacementEffectData x) => x.SpawnerType.Contains(ReplacementEffectSpawnerType.PreventDamage) && x.RecipientIds != null && x.RecipientIds.Contains(bb.Player.InstanceId));
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
		}
		return false;
	}
}
