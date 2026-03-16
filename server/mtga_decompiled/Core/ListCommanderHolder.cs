using System;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

public class ListCommanderHolder : MetaCardHolder
{
	public CustomButton AddButton;

	[SerializeField]
	private ListCommanderView _cardPrefab;

	[SerializeField]
	private Transform _cardParent;

	[SerializeField]
	private FrameSpriteData _crestFrameSprites;

	[SerializeField]
	private GameObject _emptySlot;

	[SerializeField]
	private Animator _commanderSlotAnimator;

	private ListCommanderView _cardInstance;

	private bool _addButtonActive;

	public bool IsCompanion;

	public bool IsPartner;

	public Action<MetaCardView> OnCardAddClicked { get; set; }

	public bool IsEmpty { get; private set; } = true;

	public void SetCard(CardData cardData, Sprite frameSprite, Color nameColor, ListMetaCardViewDisplayInformation di, IGreLocProvider locManager)
	{
		if (_cardInstance != null && _cardInstance.Card.Printing != cardData.Printing)
		{
			UnityEngine.Object.Destroy(_cardInstance.gameObject);
			_cardInstance = null;
		}
		IsEmpty = false;
		_emptySlot.SetActive(value: false);
		if (_cardInstance == null)
		{
			_cardInstance = UnityEngine.Object.Instantiate(_cardPrefab, _cardParent);
		}
		CardData cardData2 = cardData;
		if (di.VisualCard != null && di.VisualCard.GrpId != di.Card.GrpId)
		{
			cardData2 = CardDataExtensions.CreateSkinCard(di.VisualCard.GrpId, base.CardDatabase, di.SkinCode);
		}
		_cardInstance.Holder = this;
		_cardInstance.Card = cardData;
		_cardInstance.VisualCard = cardData2 ?? cardData;
		_cardInstance.ShowUnCollectedTreatment = di.Unowned;
		_cardInstance.ShowBannedTreatment = di.Banned;
		_cardInstance.ShowInvalidTreatment = di.Invalid;
		_cardInstance.HangerSituation = new HangerSituation
		{
			ContextualHangers = di.ContextualHangers
		};
		_cardInstance.SetName(locManager.GetLocalizedText(_cardInstance.VisualCard.TitleId));
		_cardInstance.SetQuantity((int)di.Quantity);
		_cardInstance.SetFrameSprite(frameSprite);
		_cardInstance.SetNameColor(nameColor);
		ListMetaCardHolder_Expanding.SetManaCost(_cardInstance);
		_cardInstance.UpdateTreatment();
		RegisterClickedCallbacks();
		Sprite frameSprite2 = ListMetaCardHolder_Expanding.GetFrameSprite(cardData, _crestFrameSprites);
		_cardInstance.CrestImage.sprite = frameSprite2;
		SetAddButtonActive(active: false);
	}

	private void RegisterClickedCallbacks()
	{
		ListCommanderView cardInstance = _cardInstance;
		cardInstance.OnAddClicked = (Action<MetaCardView>)Delegate.Remove(cardInstance.OnAddClicked, new Action<MetaCardView>(CardAddClicked));
		ListCommanderView cardInstance2 = _cardInstance;
		cardInstance2.OnAddClicked = (Action<MetaCardView>)Delegate.Combine(cardInstance2.OnAddClicked, new Action<MetaCardView>(CardAddClicked));
		ListCommanderView cardInstance3 = _cardInstance;
		cardInstance3.OnRemoveClicked = (Action<MetaCardView>)Delegate.Remove(cardInstance3.OnRemoveClicked, new Action<MetaCardView>(CardClicked));
		ListCommanderView cardInstance4 = _cardInstance;
		cardInstance4.OnRemoveClicked = (Action<MetaCardView>)Delegate.Combine(cardInstance4.OnRemoveClicked, new Action<MetaCardView>(CardClicked));
	}

	private void CardClicked(MetaCardView cv)
	{
		base.OnCardClicked?.Invoke(cv);
	}

	private void CardAddClicked(MetaCardView cv)
	{
		OnCardAddClicked?.Invoke(cv);
	}

	public override void ClearCards()
	{
		if (_cardInstance != null)
		{
			UnityEngine.Object.Destroy(_cardInstance.gameObject);
			_cardInstance = null;
		}
		IsEmpty = true;
		_emptySlot.SetActive(value: true);
		SetAddButtonActive(_addButtonActive);
	}

	public void SetAddButtonActive(bool active)
	{
		_addButtonActive = active;
		if (_commanderSlotAnimator.isActiveAndEnabled)
		{
			_commanderSlotAnimator.SetBool(CommanderSlotUtils.ACTIVE_HASH, active);
		}
	}

	private void OnEnable()
	{
		SetAddButtonActive(_addButtonActive);
	}

	public override DeckBuilderPile? GetParentDeckBuilderPile()
	{
		if (IsCompanion)
		{
			return DeckBuilderPile.Companion;
		}
		return IsPartner ? DeckBuilderPile.Partner : DeckBuilderPile.Commander;
	}
}
