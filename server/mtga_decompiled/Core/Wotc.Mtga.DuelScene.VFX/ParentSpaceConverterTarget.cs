using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class ParentSpaceConverterTarget : ISpaceConverter
{
	private readonly ICardViewProvider _cardViewProvider;

	public ParentSpaceConverterTarget(ICardViewProvider cardViewProvider)
	{
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		if (!(contextEntity is MtgCardInstance mtgCardInstance))
		{
			return;
		}
		foreach (MtgEntity target in mtgCardInstance.Targets)
		{
			if (target is MtgCardInstance mtgCardInstance2 && _cardViewProvider.TryGetCardView(mtgCardInstance2.ParentId, out var cardView))
			{
				spaceConverterUtility.AddCardViewToSet(cardView, set);
			}
		}
	}
}
