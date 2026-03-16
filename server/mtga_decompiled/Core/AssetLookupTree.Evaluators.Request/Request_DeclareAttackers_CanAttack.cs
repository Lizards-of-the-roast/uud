using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Request;

public class Request_DeclareAttackers_CanAttack : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.Request is DeclareAttackerRequest declareAttackerRequest)
		{
			return declareAttackerRequest.QualifiedAttackers.Exists(bb.CardData.InstanceId, (Attacker attacker, uint instanceId) => attacker.AttackerInstanceId == instanceId);
		}
		return false;
	}
}
