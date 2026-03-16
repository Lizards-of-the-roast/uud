using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class ManaDeluxeTooltipController : MonoBehaviour
{
	[SerializeField]
	private Animator _textAnimator;

	[SerializeField]
	private Transform _landContainer1;

	[SerializeField]
	private Transform _landContainer2;

	[SerializeField]
	private Transform _twodropContainer;

	private MtgCardInstance _plainsInstance1;

	private MtgCardInstance _plainsInstance2;

	private DuelScene_CDC _plainsView1;

	private DuelScene_CDC _plainsView2;

	private void Start()
	{
		GameManager gameManager = Object.FindObjectOfType<GameManager>();
		if ((bool)gameManager)
		{
			IContext context = gameManager.Context;
			ICardDataProvider cardDataProvider = context.Get<ICardDataProvider>() ?? NullCardDataProvider.Default;
			ICardBuilder<DuelScene_CDC> cardBuilder = context.Get<ICardBuilder<DuelScene_CDC>>() ?? NullCardBuilder<DuelScene_CDC>.Default;
			MtgZone mtgZone = new MtgZone();
			mtgZone.Type = ZoneType.Battlefield;
			MtgZone mtgZone2 = new MtgZone();
			mtgZone2.Type = ZoneType.Library;
			CardPrintingData cardPrintingById = cardDataProvider.GetCardPrintingById(69443u);
			CardPrintingData cardPrintingById2 = cardDataProvider.GetCardPrintingById(69111u);
			_plainsInstance1 = cardPrintingById.CreateInstance();
			_plainsInstance1.Zone = mtgZone;
			_plainsInstance2 = cardPrintingById.CreateInstance();
			_plainsInstance2.Zone = mtgZone;
			MtgCardInstance mtgCardInstance = cardPrintingById2.CreateInstance();
			mtgCardInstance.Zone = mtgZone2;
			CardData cardData = new CardData(_plainsInstance1, cardPrintingById);
			CardData cardData2 = new CardData(_plainsInstance2, cardPrintingById);
			CardData cardData3 = new CardData(mtgCardInstance, cardPrintingById2);
			_plainsView1 = cardBuilder.CreateCDC(cardData, isVisible: true);
			_plainsView2 = cardBuilder.CreateCDC(cardData2, isVisible: true);
			DuelScene_CDC duelScene_CDC = cardBuilder.CreateCDC(cardData3, isVisible: true);
			_plainsView1.HolderTypeOverride = CardHolderType.Battlefield;
			_plainsView2.HolderTypeOverride = CardHolderType.Battlefield;
			duelScene_CDC.HolderTypeOverride = CardHolderType.Library;
			_plainsView1.UpdateVisuals();
			_plainsView1.Root.SetParent(_landContainer1.transform, worldPositionStays: true);
			_plainsView1.transform.ZeroOut();
			_plainsView1.Root.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
			_plainsView2.UpdateVisuals();
			_plainsView2.Root.SetParent(_landContainer2.transform, worldPositionStays: true);
			_plainsView2.transform.ZeroOut();
			_plainsView2.Root.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
			duelScene_CDC.UpdateVisuals();
			duelScene_CDC.Root.SetParent(_twodropContainer.transform, worldPositionStays: true);
			duelScene_CDC.transform.ZeroOut();
			duelScene_CDC.Root.gameObject.SetLayer(LayerMask.NameToLayer("CardsExamine"));
		}
	}

	private void TapPlains1()
	{
		AudioManager.SetSwitch("color", "white", _plainsView1.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_tap, _plainsView1.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_mana_swish, _plainsView1.gameObject);
		_plainsInstance1.IsTapped = true;
		_plainsView1.UpdateVisuals();
	}

	private void TapPlains2()
	{
		AudioManager.SetSwitch("color", "white", _plainsView2.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_tap, _plainsView2.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_mana_swish, _plainsView2.gameObject);
		_plainsInstance2.IsTapped = true;
		_plainsView2.UpdateVisuals();
	}

	private void Hitplains1()
	{
		AudioManager.SetSwitch("color", "white", _plainsView1.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_mana_hit, _plainsView1.gameObject);
	}

	private void Hitplains2()
	{
		AudioManager.SetSwitch("color", "white", _plainsView2.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_mana_hit, _plainsView2.gameObject);
	}

	private void UntapBothPlains()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_untap, _plainsView1.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_untap, _plainsView2.gameObject);
		_plainsInstance1.IsTapped = false;
		_plainsInstance2.IsTapped = false;
		_plainsView1.UpdateVisuals();
		_plainsView2.UpdateVisuals();
	}

	public void TriggerShowTextForDismissAnimation()
	{
		_textAnimator.SetTrigger("ShowTextForDismiss");
	}
}
