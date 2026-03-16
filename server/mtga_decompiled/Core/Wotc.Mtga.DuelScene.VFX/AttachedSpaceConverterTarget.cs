using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class AttachedSpaceConverterTarget : ISpaceConverter
{
	private readonly ICardViewProvider _cardViewProvider;

	public AttachedSpaceConverterTarget(ICardViewProvider cardViewProvider)
	{
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		if (contextEntity is MtgCardInstance mtgCardInstance && _cardViewProvider.TryGetCardView(mtgCardInstance.AttachedToId, out var cardView))
		{
			spaceConverterUtility.AddCardViewToSet(cardView, set);
		}
	}
}
