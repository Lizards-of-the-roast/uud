using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.Action;

public class Action_Source : IIndirector
{
	private ICardDataAdapter _cacheCardData;

	private MtgPlayer _cachePlayer;

	public void SetCache(IBlackboard bb)
	{
		_cacheCardData = bb.CardData;
		_cachePlayer = bb.Player;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		MtgGameState gameState = bb.GameState;
		Wotc.Mtgo.Gre.External.Messaging.Action greAction = bb.GreAction;
		ICardDatabaseAdapter cardDatabase = bb.CardDatabase;
		if (gameState == null || greAction == null || cardDatabase == null || !gameState.TryGetEntity(greAction.SourceId, out var mtgEntity))
		{
			yield break;
		}
		if (!(mtgEntity is MtgPlayer player))
		{
			if (mtgEntity is MtgCardInstance instance)
			{
				bb.SetCardDataExtensive(CardDataExtensions.CreateWithDatabase(instance, cardDatabase));
				yield return bb;
			}
		}
		else
		{
			bb.Player = player;
			yield return bb;
		}
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cacheCardData);
		bb.Player = _cachePlayer;
		_cacheCardData = null;
		_cachePlayer = null;
	}
}
