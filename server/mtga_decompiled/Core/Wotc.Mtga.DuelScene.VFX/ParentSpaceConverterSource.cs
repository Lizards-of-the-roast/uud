using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class ParentSpaceConverterSource : ISpaceConverter
{
	private readonly ICardViewProvider _cardViewProvider;

	public ParentSpaceConverterSource(ICardViewProvider cardViewProvider)
	{
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
	}

	public void PopulateSet(Transform localTransform, MtgEntity contextEntity, HashSet<Transform> set, ISpaceConverterCollectionUtility spaceConverterUtility)
	{
		if (contextEntity is MtgCardInstance { ParentId: not 0u } mtgCardInstance && _cardViewProvider.TryGetCardView(mtgCardInstance.ParentId, out var cardView))
		{
			spaceConverterUtility.AddCardViewToSet(cardView, set);
		}
	}
}
