using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public interface ISpaceConverterCollectionUtility
{
	void AddCardViewToSet(DuelScene_CDC cardView, HashSet<Transform> set);

	void AddPlayerViewToSet(IAvatarViewProvider avatarViewProvider, GREPlayerNum playerNum, HashSet<Transform> set);

	void AddZoneToSet(ICardHolderProvider cardHolderController, GREPlayerNum playerNum, CardHolderType holderType, HashSet<Transform> set);
}
