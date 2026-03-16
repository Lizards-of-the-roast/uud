using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class DisplayCardHolder : MetaCardHolder
{
	[SerializeField]
	private CardInitInfo[] cardInitInfos;

	[NonSerialized]
	private List<BASE_CDC> _cardInstances;

	private static ICardRolloverZoom ZoomHandler => Pantry.Get<ICardRolloverZoom>();

	private static CardDatabase GetCardDatabase()
	{
		return WrapperController.Instance.CardDatabase;
	}

	private static CardViewBuilder GetCardViewBuilder()
	{
		return Pantry.Get<CardViewBuilder>();
	}

	public void OnEnable()
	{
		EnsureInit(GetCardDatabase(), GetCardViewBuilder());
		ClearCards();
		if (_cardInstances == null || _cardInstances.Count == 0)
		{
			_cardInstances = SetCards(base.CardDatabase, base.CardViewBuilder, ZoomHandler).ToList();
		}
		Languages.LanguageChangedSignal.Listeners += UpdateVisualsForLanguageChanges;
	}

	public void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= UpdateVisualsForLanguageChanges;
		ClearCards();
	}

	public void UpdateVisualsForLanguageChanges()
	{
		foreach (BASE_CDC cardInstance in _cardInstances)
		{
			cardInstance.UpdateVisuals();
		}
	}

	private IEnumerable<BASE_CDC> SetCards(ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, ICardRolloverZoom zoomHandler)
	{
		CardInitInfo[] array = cardInitInfos;
		foreach (CardInitInfo initInfo in array)
		{
			yield return SetCard(cardDatabase, cardViewBuilder, zoomHandler, initInfo);
		}
	}

	private static BASE_CDC SetCard(ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, ICardRolloverZoom zoomHandler, CardInitInfo initInfo)
	{
		foreach (Transform item in initInfo.Anchor)
		{
			if (item.GetComponent<CDCPart>() != null)
			{
				item.gameObject.SetActive(value: false);
			}
		}
		CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(initInfo.DefaultGrpId);
		MtgZone zone = new MtgZone
		{
			Type = ZoneType.Battlefield
		};
		MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance();
		mtgCardInstance.Zone = zone;
		Counters[] counters = initInfo.Counters;
		for (int i = 0; i < counters.Length; i++)
		{
			Counters counters2 = counters[i];
			mtgCardInstance.Counters.Add(counters2.CounterType, (int)counters2.NumberOfCounters);
			switch (counters2.CounterType)
			{
			case CounterType.P1P1:
				mtgCardInstance.Power = new StringBackedInt(mtgCardInstance.Power.Value + (int)counters2.NumberOfCounters);
				mtgCardInstance.Toughness = new StringBackedInt(mtgCardInstance.Toughness.Value + (int)counters2.NumberOfCounters);
				break;
			case CounterType.M1M1:
				mtgCardInstance.Power = new StringBackedInt(mtgCardInstance.Power.Value - (int)counters2.NumberOfCounters);
				mtgCardInstance.Toughness = new StringBackedInt(mtgCardInstance.Toughness.Value - (int)counters2.NumberOfCounters);
				break;
			}
		}
		CardData data = new CardData(mtgCardInstance, cardPrintingById);
		BASE_CDC bASE_CDC = cardViewBuilder.CreateMetaCdc(data, initInfo.Anchor);
		bASE_CDC.HolderTypeOverride = initInfo.CardHolderType;
		bASE_CDC.gameObject.SetLayer(LayerMask.NameToLayer("Default"));
		bASE_CDC.UpdateCounterVisibility(display: true);
		bASE_CDC.UpdateVisuals();
		if (zoomHandler != null && !initInfo.DisableRolloverZoom)
		{
			CardZoomTrigger cardZoomTrigger = bASE_CDC.gameObject.AddComponent<CardZoomTrigger>();
			cardZoomTrigger.CardView = bASE_CDC;
			cardZoomTrigger.ZoomView = zoomHandler;
			cardZoomTrigger.DisableCardOnZoom = false;
		}
		return bASE_CDC;
	}

	public override void ClearCards()
	{
		if (_cardInstances == null)
		{
			return;
		}
		foreach (BASE_CDC cardInstance in _cardInstances)
		{
			base.CardViewBuilder.DestroyCDC(cardInstance);
		}
		_cardInstances.Clear();
	}
}
