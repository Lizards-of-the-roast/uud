using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;

namespace AssetLookupTree.Indirectors.Interaction.ActionsAvailable;

public class Interaction_OrderWorkflow_RequestIds : IIndirector
{
	private ICardDataAdapter _cachedCard;

	public void SetCache(IBlackboard bb)
	{
		_cachedCard = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cachedCard);
		_cachedCard = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState == null || bb.CardDatabase == null || !(bb.Interaction is OrderWorkflow { BaseRequest: OrderRequest baseRequest }))
		{
			yield break;
		}
		foreach (uint id in baseRequest.Ids)
		{
			if (bb.GameState.TryGetCard(id, out var card))
			{
				bb.SetCardDataRaw(CardDataExtensions.CreateWithDatabase(card, bb.CardDatabase));
				yield return bb;
			}
		}
	}
}
