using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class CombatDeluxeTooltipController : MonoBehaviour
{
	[SerializeField]
	private Animator _textAnimator;

	[SerializeField]
	private Transform _attackerContainer;

	[SerializeField]
	private Transform _blockerContainer;

	public void TriggerShowTextForDismissAnimation()
	{
		_textAnimator.SetTrigger("ShowTextForDismiss");
	}

	private void Start()
	{
		GameManager gameManager = Object.FindObjectOfType<GameManager>();
		if ((bool)gameManager)
		{
			IContext context = gameManager.Context;
			ICardDatabaseAdapter cardDatabaseAdapter = context.Get<ICardDatabaseAdapter>() ?? NullCardDatabaseAdapter.Default;
			ICardBuilder<DuelScene_CDC> obj = context.Get<ICardBuilder<DuelScene_CDC>>() ?? NullCardBuilder<DuelScene_CDC>.Default;
			MtgZone zone = new MtgZone
			{
				Type = ZoneType.Battlefield
			};
			CardPrintingData cardPrintingById = cardDatabaseAdapter.CardDataProvider.GetCardPrintingById(69111u);
			CardPrintingData cardPrintingById2 = cardDatabaseAdapter.CardDataProvider.GetCardPrintingById(69119u);
			MtgCardInstance mtgCardInstance = cardPrintingById.CreateInstance();
			mtgCardInstance.Zone = zone;
			MtgCardInstance mtgCardInstance2 = cardPrintingById2.CreateInstance();
			mtgCardInstance2.Zone = zone;
			CardData cardData = new CardData(mtgCardInstance, cardPrintingById);
			CardData cardData2 = new CardData(mtgCardInstance2, cardPrintingById2);
			DuelScene_CDC duelScene_CDC = obj.CreateCDC(cardData, isVisible: true);
			DuelScene_CDC duelScene_CDC2 = obj.CreateCDC(cardData2, isVisible: true);
			duelScene_CDC.HolderTypeOverride = CardHolderType.Battlefield;
			duelScene_CDC2.HolderTypeOverride = CardHolderType.Battlefield;
			duelScene_CDC.UpdateVisuals();
			duelScene_CDC.Root.SetParent(_attackerContainer.transform, worldPositionStays: true);
			duelScene_CDC.transform.ZeroOut();
			duelScene_CDC.Root.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
			duelScene_CDC2.UpdateVisuals();
			duelScene_CDC2.Root.SetParent(_blockerContainer.transform, worldPositionStays: true);
			duelScene_CDC2.transform.ZeroOut();
			duelScene_CDC2.Root.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
		}
	}
}
