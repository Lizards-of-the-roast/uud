using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.VFX;

public class AllCreaturesConverterTarget : ISpaceConverter
{
	private readonly ICardViewProvider _evm;

	private readonly IGameStateProvider _gameStateProvider;

	public AllCreaturesConverterTarget(ICardViewProvider evm, IGameStateProvider gameStateProvider)
	{
		_evm = evm;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null || !(contextEntity is MtgCardInstance mtgCardInstance))
		{
			return;
		}
		foreach (MtgEntity target in mtgCardInstance.Targets)
		{
			GREPlayerNum gREPlayerNum = GREPlayerNum.Invalid;
			if (!(target is MtgCardInstance mtgCardInstance2))
			{
				if (target is MtgPlayer mtgPlayer)
				{
					gREPlayerNum = mtgPlayer.ClientPlayerEnum;
				}
			}
			else
			{
				gREPlayerNum = mtgCardInstance2.Controller?.ClientPlayerEnum ?? GREPlayerNum.Invalid;
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
				if (item.CardTypes.Contains(CardType.Creature) && _evm.TryGetCardView(item.InstanceId, out var cardView))
				{
					spaceConverterUtility.AddCardViewToSet(cardView, set);
				}
			}
		}
	}
}
