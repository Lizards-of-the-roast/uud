using System.Collections.Generic;
using AssetLookupTree;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftListMetaCardHolder : ListMetaCardHolder_Expanding, IDraftMetaCardHolder
{
	public void EnsureInit(ICardRolloverZoom cardZoom, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.RolloverZoomView = cardZoom;
		EnsureInit(cardDatabase, cardViewBuilder);
	}

	public List<MetaCardHolder> GetMetaCardHolderList()
	{
		return new List<MetaCardHolder> { this };
	}

	public List<MetaCardView> GetMetaCardViews()
	{
		return new List<MetaCardView>(base.CardViews);
	}

	public void ResetLanguage()
	{
		foreach (ListMetaCardView_Expanding cardView in base.CardViews)
		{
			cardView.SetName(base.CardDatabase.CardTitleProvider.GetCardTitle(cardView.Card.GrpId));
		}
	}
}
