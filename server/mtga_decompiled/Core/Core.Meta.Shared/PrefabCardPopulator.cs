using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.Shared;

public class PrefabCardPopulator : PrefabPopulator
{
	[SerializeField]
	private List<StoreCardData> _cardData;

	[SerializeField]
	private List<StoreCardView> _cardViews;

	public override void Populate()
	{
		if (GetComponent<StoreCardView>() != null)
		{
			SimpleLog.LogError("Can't use the PrefabCardPopulator component on a StoreCardView.");
		}
		else
		{
			StoreDisplayCardViewBundle.ApplyCardViews(_cardViews, _cardData);
		}
	}
}
