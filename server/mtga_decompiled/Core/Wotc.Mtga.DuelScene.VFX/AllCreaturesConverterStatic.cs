using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.VFX;

public class AllCreaturesConverterStatic : ISpaceConverter
{
	private readonly ICardViewProvider _cardProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly GREPlayerNum _playerNum;

	public AllCreaturesConverterStatic(ICardViewProvider cardProvider, IGameStateProvider gameStateProvider, GREPlayerNum playerNum)
	{
		_cardProvider = cardProvider ?? NullCardViewProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_playerNum = playerNum;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null)
		{
			return;
		}
		IReadOnlyCollection<MtgCardInstance> readOnlyCollection = (IReadOnlyCollection<MtgCardInstance>)(object)Array.Empty<MtgCardInstance>();
		switch (_playerNum)
		{
		case GREPlayerNum.LocalPlayer:
			readOnlyCollection = mtgGameState.LocalPlayerBattlefieldCards;
			break;
		case GREPlayerNum.Opponent:
			readOnlyCollection = mtgGameState.OpponentBattlefieldCards;
			break;
		}
		foreach (MtgCardInstance item in readOnlyCollection)
		{
			if (item.CardTypes.Contains(CardType.Creature) && _cardProvider.TryGetCardView(item.InstanceId, out var cardView))
			{
				spaceConverterUtility.AddCardViewToSet(cardView, set);
			}
		}
	}
}
