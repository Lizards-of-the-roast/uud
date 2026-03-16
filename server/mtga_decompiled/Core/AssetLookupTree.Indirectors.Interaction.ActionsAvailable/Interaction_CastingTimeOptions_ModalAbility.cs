using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.Interaction.ActionsAvailable;

public class Interaction_CastingTimeOptions_ModalAbility : ActionIndirector
{
	public override IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.Request is CastingTimeOption_ModalRequest castingTimeOption_ModalRequest && bb.GameState.TryGetCard(castingTimeOption_ModalRequest.SourceId, out var card))
		{
			bb.Ability = card.Abilities.GetById(castingTimeOption_ModalRequest.AbilityGrpId);
			yield return bb;
		}
	}
}
