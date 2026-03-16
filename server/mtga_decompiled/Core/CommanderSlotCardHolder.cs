using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;

public class CommanderSlotCardHolder : MetaCardHolder
{
	public CustomButton AddButton;

	[SerializeField]
	private StaticColumnMetaCardView _cardViewPrefab;

	[SerializeField]
	private Transform _cardParent;

	[SerializeField]
	private Animator _commanderSlotAnimator;

	[SerializeField]
	private Image _cardSlot;

	[SerializeField]
	private FrameSpriteData _frameSprites;

	[SerializeField]
	private Sprite _defaultFrame;

	[SerializeField]
	private Image _crest;

	[SerializeField]
	private FrameSpriteData _crestSprites;

	[SerializeField]
	private Sprite _defaultCrest;

	private StaticColumnMetaCardView _commanderCardView;

	private bool _populated;

	private bool _addButtonActive;

	public bool IsCompanion;

	public bool IsPartner;

	public void SetCard(ListMetaCardViewDisplayInformation di, bool ignoreErrorStates)
	{
		SetPopulated(populated: true);
		SetAddButtonActive(active: false);
		if (_commanderCardView == null)
		{
			_commanderCardView = Object.Instantiate(_cardViewPrefab, _cardParent);
			_commanderCardView.Init(base.CardDatabase, base.CardViewBuilder);
			_commanderCardView.Holder = this;
		}
		CardData cardData = CardDataExtensions.CreateSkinCard(di.Card.GrpId, base.CardDatabase, di.SkinCode);
		CardData visualData = cardData;
		if (di.VisualCard != null && di.VisualCard.GrpId != di.Card.GrpId)
		{
			visualData = CardDataExtensions.CreateSkinCard(di.VisualCard.GrpId, base.CardDatabase, di.SkinCode);
		}
		_commanderCardView.SetData(cardData, CardHolderType.Deckbuilder, visualData);
		_commanderCardView.Quantity = (int)di.Quantity;
		if (ignoreErrorStates)
		{
			_commanderCardView.SetErrors(banned: false, unowned: false, invalid: false);
		}
		else
		{
			_commanderCardView.SetErrors(di.Banned, di.Unowned, di.Invalid);
			_commanderCardView.HangerSituation = new HangerSituation
			{
				ContextualHangers = di.ContextualHangers
			};
		}
		Sprite frameSprite = ListMetaCardHolder_Expanding.GetFrameSprite(cardData, _frameSprites);
		_cardSlot.sprite = frameSprite;
		Sprite frameSprite2 = ListMetaCardHolder_Expanding.GetFrameSprite(cardData, _crestSprites);
		_crest.sprite = frameSprite2;
		_commanderCardView.UpdateVisuals();
	}

	public override void ClearCards()
	{
		if (_commanderCardView != null)
		{
			Object.Destroy(_commanderCardView.gameObject);
			_commanderCardView = null;
		}
		_cardSlot.sprite = _defaultFrame;
		_crest.sprite = _defaultCrest;
		SetPopulated(populated: false);
		SetAddButtonActive(_addButtonActive);
	}

	private void OnEnable()
	{
		SetPopulated(_populated);
		SetAddButtonActive(_addButtonActive);
	}

	private void SetPopulated(bool populated)
	{
		_populated = populated;
		_commanderSlotAnimator.SetBool(CommanderSlotUtils.POPULATED_HASH, populated);
	}

	public void SetAddButtonActive(bool active)
	{
		_addButtonActive = active;
		_commanderSlotAnimator.SetBool(CommanderSlotUtils.ACTIVE_HASH, active);
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
