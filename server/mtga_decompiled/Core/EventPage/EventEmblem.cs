using System;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace EventPage;

public class EventEmblem : MonoBehaviour
{
	public enum eCardType
	{
		SkinCard,
		Emblem
	}

	public EventsCardView CardView;

	public EventCardHolder CardHolder;

	[NonSerialized]
	public eCardType CardType;

	public void Show(uint ID, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, ICardRolloverZoom zoomHandler = null)
	{
		base.gameObject.UpdateActive(active: true);
		base.transform.SetAsLastSibling();
		CardPrintingData cardPrintingData = cardDatabase.CardDataProvider.GetCardPrintingById(ID) ?? CardPrintingData.Blank;
		CardData data = null;
		if (CardType == eCardType.Emblem)
		{
			MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Emblem);
			mtgCardInstance.GrpId = cardPrintingData.GrpId;
			mtgCardInstance.ObjectSourceGrpId = cardPrintingData.GrpId;
			mtgCardInstance.Zone = new MtgZone
			{
				Type = ZoneType.Command,
				Visibility = Visibility.Public
			};
			data = new CardData(mtgCardInstance, cardPrintingData);
			CardView.Init(cardDatabase, cardViewBuilder);
			CardView.CardView.ModelOverride = new ModelOverride(null, ZoneType.Command, null, null);
			CardView.CardView.HolderTypeOverride = CardHolderType.Command;
			CardView.CardView.IsMousedOver = false;
		}
		else if (CardType == eCardType.SkinCard)
		{
			data = CardDataExtensions.CreateSkinCard(cardPrintingData.GrpId, cardDatabase);
			CardView.Init(cardDatabase, cardViewBuilder);
		}
		CardView.SetData(data);
		CardView.Holder = CardHolder;
		if (zoomHandler != null)
		{
			CardView.Holder.RolloverZoomView = zoomHandler;
		}
		CardHolder.EnsureInit(cardDatabase, cardViewBuilder);
	}
}
