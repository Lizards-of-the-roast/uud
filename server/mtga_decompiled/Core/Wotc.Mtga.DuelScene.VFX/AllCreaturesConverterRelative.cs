using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.VFX;

public class AllCreaturesConverterRelative : ISpaceConverter
{
	public enum RelativeContext
	{
		Local,
		Opponent
	}

	private readonly ICardViewProvider _cardProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly RelativeContext _relativeContext;

	public AllCreaturesConverterRelative(ICardViewProvider cardProvider, IGameStateProvider gameStateProvider, RelativeContext relativeContext)
	{
		_cardProvider = cardProvider ?? NullCardViewProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_relativeContext = relativeContext;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null)
		{
			return;
		}
		GREPlayerNum gREPlayerNum = GREPlayerNum.Invalid;
		if (!(contextEntity is MtgCardInstance mtgCardInstance))
		{
			if (contextEntity is MtgPlayer mtgPlayer)
			{
				gREPlayerNum = mtgPlayer.ClientPlayerEnum;
			}
		}
		else
		{
			gREPlayerNum = mtgCardInstance.Controller?.ClientPlayerEnum ?? GREPlayerNum.Invalid;
		}
		if (gREPlayerNum != GREPlayerNum.Invalid && _relativeContext == RelativeContext.Opponent)
		{
			switch (gREPlayerNum)
			{
			case GREPlayerNum.LocalPlayer:
				gREPlayerNum = GREPlayerNum.Opponent;
				break;
			case GREPlayerNum.Opponent:
				gREPlayerNum = GREPlayerNum.LocalPlayer;
				break;
			}
		}
		IReadOnlyCollection<MtgCardInstance> readOnlyCollection = (IReadOnlyCollection<MtgCardInstance>)(object)Array.Empty<MtgCardInstance>();
		switch (gREPlayerNum)
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
