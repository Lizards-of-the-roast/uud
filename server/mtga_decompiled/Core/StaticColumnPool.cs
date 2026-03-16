using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

public class StaticColumnPool : MetaCardViewPool<StaticColumnMetaCardView>
{
	protected override void OnCardViewAcquired(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, StaticColumnMetaCardView cardView, ICardCollectionItem item, MetaCardHolder holder, Transform parent, CardData previousCard)
	{
		cardView.Init(cardDatabase, cardViewBuilder);
		if (previousCard == null || cardView.Card.GrpId != previousCard.GrpId)
		{
			cardView.SetData(item.Card);
		}
		cardView.Quantity = item.Quantity;
	}
}
