using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Extensions;

public class SideboardListCardHolder : MetaCardHolder
{
	private enum SideboardType
	{
		AllModes,
		Traditional
	}

	[Header("SideboardListCardHolder Specific Parameters")]
	[SerializeField]
	private Transform _allModesCardTilesParent;

	[SerializeField]
	private Transform _traditionalCardTilesParent;

	public ListCommanderHolder CompanionCardHolder;

	[SerializeField]
	private Button _poolShieldButton;

	[Header("Text Header References")]
	[SerializeField]
	private TMP_Text _traditionalHeaderText;

	[Header("Card Tile Parameters")]
	[SerializeField]
	private ListMetaCardView_Expanding _listCardViewPrefab;

	[SerializeField]
	private ListCardViewAnimationData _animationData;

	[SerializeField]
	private FrameSpriteData[] _frameSpritesData;

	private List<ListMetaCardView_Expanding> _allModesListCardViews = new List<ListMetaCardView_Expanding>();

	private List<ListMetaCardViewDisplayInformation> _allDisplayData = new List<ListMetaCardViewDisplayInformation>();

	private List<ListMetaCardViewDisplayInformation> _allModesDisplayData = new List<ListMetaCardViewDisplayInformation>();

	private SortType[] _sortCriteria;

	private bool _isReadOnlyMode;

	private MetaCardView _selectedCardView;

	public Action<MetaCardView> OnCardAddClicked { get; set; }

	public Action<MetaCardView> OnCardRemoveClicked { get; set; }

	public Action<MetaCardView> OnCompanionCardRemoveClicked { get; set; }

	public Action<MetaCardView, MetaCardHolder> OnCompanionCardDropped { get; set; }

	public Action<IEnumerable<CardAndQuantity>> OnReducedSideboardUpdated { get; set; }

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private void Awake()
	{
		_traditionalHeaderText.gameObject.SetActive(value: false);
		CompanionCardHolder.gameObject.SetActive(value: false);
		ListCommanderHolder companionCardHolder = CompanionCardHolder;
		companionCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(companionCardHolder.OnCardDropped, (Action<MetaCardView, MetaCardHolder>)delegate(MetaCardView cardView, MetaCardHolder cardHolder)
		{
			OnCompanionCardDropped?.Invoke(cardView, cardHolder);
		});
		base.CanDragCards = (MetaCardView cardView) => true;
		base.CanDropCards = (MetaCardView cardView) => true;
		base.CanSingleClickCards = (MetaCardView cardView) => true;
		base.ShowHighlight = (MetaCardView cardView) => false;
		CompanionCardHolder.CanDragCards = (MetaCardView cardView) => true;
		CompanionCardHolder.CanDropCards = (MetaCardView cardView) => true;
		CompanionCardHolder.CanSingleClickCards = (MetaCardView cardView) => true;
		CompanionCardHolder.ShowHighlight = (MetaCardView cardView) => false;
		_poolShieldButton.onClick.AddListener(delegate
		{
			_selectCardView(null);
		});
	}

	private void OnEnable()
	{
		VisualsUpdater.SideboardVisualsUpdated += SetCards;
	}

	private void OnDisable()
	{
		VisualsUpdater.SideboardVisualsUpdated -= SetCards;
	}

	public void SetCards(List<ListMetaCardViewDisplayInformation> displayDataToSet, SortType[] sortCriteria, bool isReadOnly = false)
	{
		_allDisplayData = displayDataToSet;
		_isReadOnlyMode = isReadOnly;
		_sortCriteria = sortCriteria;
		if (_selectedCardView != null)
		{
			_selectCardView(null);
		}
		_traditionalHeaderText.gameObject.SetActive(value: false);
		if (!_allModesDisplayData.IsEqualTo(displayDataToSet))
		{
			_setAllModesCards(displayDataToSet, isReadOnly);
		}
		foreach (ListMetaCardView_Expanding allModesListCardView in _allModesListCardViews)
		{
			SetCardViewTagDisplayType(allModesListCardView, allModesListCardView.transform.parent);
		}
	}

	public void ResetCards()
	{
		if (_allDisplayData != null)
		{
			SetCards(_allDisplayData, _sortCriteria, _isReadOnlyMode);
		}
	}

	public override void ClearCards()
	{
		_allDisplayData.Clear();
		_allModesDisplayData.Clear();
		CompanionCardHolder.ClearCards();
		_poolShieldButton.gameObject.SetActive(value: false);
		foreach (ListMetaCardView_Expanding allModesListCardView in _allModesListCardViews)
		{
			UnityEngine.Object.Destroy(allModesListCardView.gameObject);
		}
		_allModesListCardViews.Clear();
	}

	public void SetCompanionCards(ListMetaCardViewDisplayInformation di)
	{
		CompanionCardHolder.gameObject.SetActive(value: true);
		if (di.Card != null)
		{
			CardData cardData = CardDataExtensions.CreateSkinCard(di.Card.GrpId, base.CardDatabase, di.SkinCode);
			FrameSpriteData frameSpriteData = (string.IsNullOrEmpty(di.SkinCode) ? _frameSpritesData[0] : _frameSpritesData[1]);
			Sprite frameSprite = ListMetaCardHolder_Expanding.GetFrameSprite(cardData, frameSpriteData);
			Color titleTextColor = frameSpriteData.TitleTextColor;
			ListCommanderHolder companionCardHolder = CompanionCardHolder;
			companionCardHolder.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(companionCardHolder.OnCardClicked, new Action<MetaCardView>(_onCardTileClicked));
			CompanionCardHolder.SetCard(cardData, frameSprite, titleTextColor, di, base.CardDatabase.GreLocProvider);
		}
		else
		{
			CompanionCardHolder.ClearCards();
		}
	}

	public void ClearCompanionCards()
	{
		CompanionCardHolder.gameObject.SetActive(value: false);
		CompanionCardHolder.ClearCards();
	}

	private void _setCards(ListCardUtility.FlattenedDisplayInfoList displayDataList, List<ListMetaCardView_Expanding> cardTiles, Transform cardTilesParent, bool isReadOnly = false)
	{
		(List<ListMetaCardViewDisplayInformation> Added, List<ListMetaCardViewDisplayInformation> Removed) addedRemoved = ListCardUtility.GetAddedRemoved(cardTiles.Select((ListMetaCardView_Expanding tile) => tile.DisplayInformation), displayDataList, (ListMetaCardViewDisplayInformation x, ListMetaCardViewDisplayInformation y) => x.Equals(y));
		var (list, _) = addedRemoved;
		foreach (ListMetaCardViewDisplayInformation removedDisplayData in addedRemoved.Removed)
		{
			int num = cardTiles.FindIndex((ListMetaCardView_Expanding tile) => tile.DisplayInformation.Equals(removedDisplayData));
			if (num != -1)
			{
				UnityEngine.Object.Destroy(cardTiles[num].gameObject);
				cardTiles.RemoveAt(num);
			}
			else
			{
				SimpleLog.LogError("Failed to remove card tile for removed display info!");
			}
		}
		foreach (ListMetaCardViewDisplayInformation item in list)
		{
			CardData cardData = CardDataExtensions.CreateSkinCard(item.Card.GrpId, base.CardDatabase, item.SkinCode);
			ListMetaCardView_Expanding listMetaCardView_Expanding = _createCardView(cardData, item, cardTilesParent, isReadOnly);
			cardTiles.Add(listMetaCardView_Expanding);
			SetCardViewTagDisplayType(listMetaCardView_Expanding, cardTilesParent);
		}
		foreach (ListMetaCardView_Expanding item2 in CardSorter.SortInternal(cardTiles, (ListMetaCardView_Expanding cardTile) => cardTile.DisplayInformation.Card, base.CardDatabase.GreLocProvider, cardsSortedFromDatabase: false, _sortCriteria))
		{
			item2.transform.SetAsLastSibling();
		}
	}

	private void _setAllModesCards(List<ListMetaCardViewDisplayInformation> newAllModesDisplayData, bool isReadOnly)
	{
		_allModesDisplayData = newAllModesDisplayData;
		ListCardUtility.FlattenedDisplayInfoList displayDataList = ListCardUtility.Flatten(_allModesDisplayData);
		_setCards(displayDataList, _allModesListCardViews, _allModesCardTilesParent, isReadOnly);
	}

	public void _onCartTagClicked(MetaCardView cardView)
	{
		OnCardAddClicked?.Invoke(cardView);
	}

	public void _onCardTileClicked(MetaCardView cardView)
	{
		if (cardView is ListCommanderView)
		{
			OnCompanionCardRemoveClicked?.Invoke(cardView);
		}
		else if (cardView is ListMetaCardView_Expanding)
		{
			if (_selectedCardView == null)
			{
				OnCardRemoveClicked?.Invoke(cardView);
			}
			else if (_selectedCardView == cardView)
			{
				_selectCardView(null);
			}
			else if (cardView.Holder == this)
			{
				_selectCardView(null);
			}
		}
		else
		{
			OnCardRemoveClicked?.Invoke(cardView);
		}
	}

	private ListMetaCardView_Expanding _createCardView(CardData cardData, ListMetaCardViewDisplayInformation displayInformation, Transform cardParent, bool ignoreIsUnowned)
	{
		ListMetaCardView_Expanding listMetaCardView_Expanding = ListCardUtility.CreateListCardView(_listCardViewPrefab, cardParent, cardData, base.CardDatabase);
		if (ignoreIsUnowned)
		{
			displayInformation.Unowned = false;
		}
		listMetaCardView_Expanding.SetDisplayInformation(displayInformation);
		listMetaCardView_Expanding.SetButtonClickCallbacks(_onCartTagClicked, _onCardTileClicked);
		listMetaCardView_Expanding.SetButtonEnabled(isAddButtonEnabled: true, isRemoveButtonEnabled: true);
		listMetaCardView_Expanding.SetFrameSprite(displayInformation.SkinCode, cardData, _frameSpritesData);
		listMetaCardView_Expanding.SetManaCost(cardData);
		listMetaCardView_Expanding.Holder = this;
		return listMetaCardView_Expanding;
	}

	private void _selectCardView(MetaCardView cardViewToSelect)
	{
		if (_selectedCardView != null)
		{
			_selectedCardView.SetSelected(isSelected: false);
		}
		_selectedCardView = cardViewToSelect;
		if (_selectedCardView != null)
		{
			_selectedCardView.SetSelected(isSelected: true);
		}
		if (_allModesListCardViews.Contains(_selectedCardView))
		{
			foreach (ListMetaCardView_Expanding allModesListCardView in _allModesListCardViews)
			{
				if (allModesListCardView != _selectedCardView)
				{
					allModesListCardView.SetDisabled(isDisabled: true);
				}
			}
		}
		else
		{
			foreach (ListMetaCardView_Expanding allModesListCardView2 in _allModesListCardViews)
			{
				allModesListCardView2.SetDisabled(isDisabled: false);
				allModesListCardView2.SetTagHighlight(isHighlighted: false);
			}
		}
		_poolShieldButton.gameObject.SetActive(_selectedCardView != null);
	}

	private void SetCardViewTagDisplayType(ListMetaCardView_Expanding cardView, Transform cardTileParent)
	{
		cardView.SetTagDisplayType(ListMetaCardView_Expanding.TagDisplayType.Default);
	}

	public override DeckBuilderPile? GetParentDeckBuilderPile()
	{
		return DeckBuilderPile.Sideboard;
	}
}
