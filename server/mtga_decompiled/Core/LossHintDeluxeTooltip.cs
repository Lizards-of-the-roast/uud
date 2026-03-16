using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wizards.MDN;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class LossHintDeluxeTooltip : MonoBehaviour
{
	[LocTerm]
	[SerializeField]
	private string _defaultLossHintTitle;

	[LocTerm]
	[SerializeField]
	private string _defualtLossHintDescription;

	[SerializeField]
	private CardInitInfo[] _cardInitInfos;

	public string DefaultLossHintTitle => _defaultLossHintTitle;

	public string DefaultLossHintDescription => _defualtLossHintDescription;

	public void SetCards(EventContext eventContext, ICardRolloverZoom zoomHandler = null)
	{
		CardDatabase cardDatabase = WrapperController.Instance.CardDatabase;
		CardViewBuilder cardViewBuilder = WrapperController.Instance.CardViewBuilder;
		CardInitInfo[] cardInitInfos = _cardInitInfos;
		for (int i = 0; i < cardInitInfos.Length; i++)
		{
			CardInitInfo cardInitInfo = cardInitInfos[i];
			foreach (Transform item in cardInitInfo.Anchor)
			{
				if (item.GetComponent<CDCPart>() != null)
				{
					item.gameObject.SetActive(value: false);
				}
			}
			CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(cardInitInfo.DefaultGrpId);
			MtgZone mtgZone = new MtgZone();
			mtgZone.Type = ZoneType.Battlefield;
			MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance();
			mtgCardInstance.Zone = mtgZone;
			Counters[] counters = cardInitInfo.Counters;
			for (int j = 0; j < counters.Length; j++)
			{
				Counters counters2 = counters[j];
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
			BASE_CDC bASE_CDC = cardViewBuilder.CreateMetaCdc(data, cardInitInfo.Anchor);
			bASE_CDC.HolderTypeOverride = cardInitInfo.CardHolderType;
			bASE_CDC.gameObject.SetLayer(LayerMask.NameToLayer("Default"));
			bASE_CDC.UpdateCounterVisibility(display: true);
			bASE_CDC.UpdateVisuals();
			if (zoomHandler != null)
			{
				CardZoomTrigger cardZoomTrigger = bASE_CDC.gameObject.AddComponent<CardZoomTrigger>();
				cardZoomTrigger.CardView = bASE_CDC;
				cardZoomTrigger.ZoomView = zoomHandler;
				cardZoomTrigger.DisableCardOnZoom = false;
			}
		}
	}
}
