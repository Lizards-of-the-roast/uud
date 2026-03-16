using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class DefaultSpaceConverterCollectionUtility : ISpaceConverterCollectionUtility
{
	public void AddCardViewToSet(DuelScene_CDC cardView, HashSet<Transform> set)
	{
		set.Add(cardView.EffectsRoot);
	}

	public void AddPlayerViewToSet(IAvatarViewProvider avatarViewProvider, GREPlayerNum playerNum, HashSet<Transform> set)
	{
		if (avatarViewProvider.TryGetAvatarByPlayerSide(playerNum, out var avatar))
		{
			set.Add(avatar.EffectsRoot);
		}
	}

	public void AddZoneToSet(ICardHolderProvider cardHolderController, GREPlayerNum playerNum, CardHolderType holderType, HashSet<Transform> set)
	{
		if (cardHolderController.TryGetCardHolder(playerNum, holderType, out var cardHolder) && cardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			set.Add(zoneCardHolderBase.EffectsRoot);
		}
	}
}
