using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors;

public class InteractionSource_Card : IIndirector
{
	private ICardDataAdapter _cachedCardData;

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState == null || bb.Interaction == null || bb.CardDataProvider == null)
		{
			yield break;
		}
		MtgCardInstance sourceCardInstance = GetSourceCardInstance(bb.GameState, bb.Interaction);
		if (sourceCardInstance != null)
		{
			ICardDataAdapter cardDataAdapter = CardDataExtensions.CreateWithDatabase(sourceCardInstance, bb.CardDatabase);
			if (cardDataAdapter != null)
			{
				bb.SetCardDataExtensive(cardDataAdapter);
				yield return bb;
			}
		}
	}

	private MtgCardInstance GetSourceCardInstance(MtgGameState gameState, WorkflowBase workflow)
	{
		return gameState.GetCardById(workflow.SourceId) ?? FindSourceFromPrompt(gameState, workflow) ?? gameState.GetTopCardOnStack();
	}

	private MtgCardInstance FindSourceFromPrompt(MtgGameState gameState, WorkflowBase workflow)
	{
		if (workflow.Prompt == null)
		{
			return null;
		}
		if (workflow.Prompt.Parameters.Count <= 0)
		{
			return null;
		}
		if (workflow.Prompt.Parameters[0].Type != ParameterType.Number || workflow.Prompt.Parameters[0].ParameterName != "CardId")
		{
			return null;
		}
		return gameState.GetCardById((uint)workflow.Prompt.Parameters[0].NumberValue);
	}

	public void SetCache(IBlackboard bb)
	{
		_cachedCardData = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cachedCardData);
		_cachedCardData = null;
	}
}
