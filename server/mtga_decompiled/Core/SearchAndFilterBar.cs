using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class SearchAndFilterBar : MonoBehaviour
{
	public class UpdateActiveIconsSettings
	{
		public bool hideAll;

		public bool hideColors;

		public bool hideLand;

		public bool hideAdvanced;

		public bool hideLargeCards;

		public bool forceRightOverflowActive;
	}

	public DeckbuilderSearchInput SearchInput;

	public Button ClearSearchButton;

	public Toggle LandsToggle;

	public GameObject LandsPreview;

	public Button AdvancedFiltersButton;

	public Toggle CompanionFilterToggle;

	public CustomButton NewDeckButton;

	public Toggle LargeCardsToggle;

	public Toggle CraftingModeToggle;

	public Toggle FiltersUsedToggle;

	public TMP_Text EventCardPoolTitleText;

	public Toggle DeckFilterToggle;

	[SerializeField]
	private GameObject _advancedSearchTips;

	[SerializeField]
	private List<GameObject> _colorFiltersParent;

	[SerializeField]
	private GameObject _dropDownContainer;

	[SerializeField]
	private GameObject _rightOverflow;

	[SerializeField]
	private Localize _readOnlyHeader;

	private bool _hideHeaderWhenExpanded;

	private ICardRolloverZoom _zoomHandler;

	private CardFilterView[] _allViews;

	private readonly List<CardFilterType> _resetFilters = new List<CardFilterType>();

	private string _searchText;

	private bool _suppressValueChangeEvents;

	private ICardRolloverZoom ZoomHandler
	{
		get
		{
			if (_zoomHandler == null)
			{
				return Pantry.Get<ICardRolloverZoom>();
			}
			return _zoomHandler;
		}
	}

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	private DeckBuilderCardFilterProvider CardFilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private IReadOnlyCardFilter CardFilter => Pantry.Get<DeckBuilderCardFilterProvider>().Filter;

	public event Action<CardFilterType, bool> FilterValueChanged;

	public event Action<bool> LandFilterValueChanged;

	public event Action<string> SearchTextChanged;

	private void Awake()
	{
		_allViews = GetComponentsInChildren<CardFilterView>(includeInactive: true);
		CardFilterView[] allViews = _allViews;
		foreach (CardFilterView filterView in allViews)
		{
			filterView.FilterToggle.onValueChanged.AddListener(delegate
			{
				if (!_suppressValueChangeEvents)
				{
					CardFilterView cardFilterView = filterView;
					this.FilterValueChanged?.Invoke(cardFilterView.FilterType, cardFilterView.FilterToggle.isOn);
				}
			});
		}
		if (!PlatformUtils.IsHandheld())
		{
			SearchInput.ShowSearchTips += UpdateSearchTips;
		}
		SearchInput.onEndEdit.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
			UpdateSearchText();
		});
		ClearSearchButton.onClick.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
			SearchInput.text = string.Empty;
			UpdateSearchText();
		});
		LandsToggle.onValueChanged.AddListener(delegate(bool value)
		{
			this.LandFilterValueChanged?.Invoke(CardFilter.IsSet(CardFilterType.Type_Land));
			if (!_suppressValueChangeEvents)
			{
				if (value)
				{
					AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
				}
				else
				{
					AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, AudioManager.Default);
				}
			}
		});
		DeckFilterToggle.onValueChanged.AddListener(DeckFilterToggle_OnValueChanged);
		FilterValueChanged += Header_FilterValueChanged;
		SearchTextChanged += Header_SearchTextChanged;
	}

	public void Init(ICardRolloverZoom zoomHandler, bool hideHeaderWhenExpanded)
	{
		_zoomHandler = zoomHandler;
		_hideHeaderWhenExpanded = hideHeaderWhenExpanded;
		VisualsUpdater.OnLayoutUpdated += OnLayoutUpdated;
	}

	private void OnDestroy()
	{
		DeckFilterToggle.onValueChanged.RemoveListener(DeckFilterToggle_OnValueChanged);
		FilterValueChanged -= Header_FilterValueChanged;
		SearchTextChanged -= Header_SearchTextChanged;
		VisualsUpdater.OnLayoutUpdated -= OnLayoutUpdated;
	}

	public void OnEnable()
	{
		ModelProvider.TitleInPileChangedQuantity += OnAnyTitlesInPileChangedQuantity;
		CardFilterProvider.FilterValueChanged += OnFilterValueChanged;
		CardFilterProvider.OnFilterReset += OnCardFilterReset;
		CardFilterProvider.OnApplyFilters += UpdateFilterToggles;
		DeckFilterToggle.onValueChanged.AddListener(DeckFilterToggle_OnValueChanged);
		OnCardFilterReset(CardFilter);
	}

	public void OnDisable()
	{
		ModelProvider.TitleInPileChangedQuantity -= OnAnyTitlesInPileChangedQuantity;
		CardFilterProvider.FilterValueChanged -= OnFilterValueChanged;
		CardFilterProvider.OnFilterReset -= OnCardFilterReset;
		CardFilterProvider.OnApplyFilters -= UpdateFilterToggles;
		DeckFilterToggle.onValueChanged.RemoveListener(DeckFilterToggle_OnValueChanged);
		DeckFilterToggle.isOn = false;
	}

	public void SetModel()
	{
		_suppressValueChangeEvents = true;
		CardFilterView[] allViews = _allViews;
		foreach (CardFilterView cardFilterView in allViews)
		{
			cardFilterView.FilterToggle.isOn = CardFilter.IsSet(cardFilterView.FilterType);
		}
		SearchInput.text = CardFilter.SearchText;
		_searchText = (ValidateSearchText() ? CardFilter.SearchText : string.Empty);
		_suppressValueChangeEvents = false;
	}

	public void SetResetFilter(IReadOnlyCardFilter filter)
	{
		_resetFilters.Clear();
		_resetFilters.AddRange(filter.Filters);
	}

	public void UpdateFiltersUsedToggle(IReadOnlyCardFilter filter)
	{
		bool flag = filter.Filters.OrderBy((CardFilterType e) => e).SequenceEqual(_resetFilters.OrderBy((CardFilterType e) => e));
		FiltersUsedToggle.isOn = !flag && AdvancedFiltersButton.gameObject.activeSelf;
	}

	private void UpdateSearchTips(bool isActive)
	{
		_advancedSearchTips.SetActive(isActive);
	}

	private void UpdateSearchText()
	{
		if (ValidateSearchText() && _searchText != SearchInput.text)
		{
			_searchText = SearchInput.text;
			this.SearchTextChanged?.Invoke(_searchText);
		}
	}

	private bool ValidateSearchText()
	{
		if (new CardMatcher(SearchInput.text, Pantry.Get<CardDatabase>()).Successful)
		{
			SearchInput.textComponent.color = UnityEngine.Color.white;
			return true;
		}
		SearchInput.textComponent.color = UnityEngine.Color.red;
		return false;
	}

	public void UpdateActiveIcons(UpdateActiveIconsSettings activeIconsSettings)
	{
		SearchInput.gameObject.UpdateActive(!activeIconsSettings.hideAll);
		ClearSearchButton.gameObject.UpdateActive(!activeIconsSettings.hideAll);
		LandsToggle.gameObject.UpdateActive(!activeIconsSettings.hideAll && !activeIconsSettings.hideLand);
		if (LandsPreview != null)
		{
			LandsPreview.gameObject.UpdateActive(!activeIconsSettings.hideAll && !activeIconsSettings.hideLand);
		}
		AdvancedFiltersButton.gameObject.UpdateActive(!activeIconsSettings.hideAll && !activeIconsSettings.hideAdvanced);
		LargeCardsToggle.gameObject.UpdateActive(!activeIconsSettings.hideAll && !activeIconsSettings.hideLargeCards);
		_readOnlyHeader.gameObject.UpdateActive(active: false);
		if (_dropDownContainer != null)
		{
			_dropDownContainer.UpdateActive(!activeIconsSettings.hideAll && !activeIconsSettings.hideColors && !activeIconsSettings.hideLand);
		}
		foreach (GameObject item in _colorFiltersParent)
		{
			item.UpdateActive(!activeIconsSettings.hideAll && !activeIconsSettings.hideColors);
		}
		_rightOverflow.UpdateActive(!activeIconsSettings.hideAll || activeIconsSettings.forceRightOverflowActive);
	}

	public void OnAnyTitlesInPileChangedQuantity(DeckBuilderPile pile, CardData cardData)
	{
		if (CompanionFilterToggle.isOn && pile == DeckBuilderPile.MainDeck)
		{
			DeckBuilderContext context = Pantry.Get<DeckBuilderContextProvider>().Context;
			DeckBuilderModel model = Pantry.Get<DeckBuilderModelProvider>().Model;
			if (Pantry.Get<CompanionUtil>().UpdateValidation(model, context?.Format))
			{
				Pantry.Get<DeckBuilderCardFilterProvider>().ApplyFilters(FilterValueChangeSource.Header);
			}
		}
	}

	public void OnFilterValueChanged(FilterValueChangeSource source, CardFilterType type, bool isOn)
	{
		if (source != FilterValueChangeSource.Header)
		{
			SetModel();
		}
	}

	public void OnCardFilterReset(IReadOnlyCardFilter filter)
	{
		SetModel();
		SetResetFilter(filter);
	}

	private void DeckFilterToggle_OnValueChanged(bool value)
	{
		DeckBuilderCardFilterProvider deckBuilderCardFilterProvider = Pantry.Get<DeckBuilderCardFilterProvider>();
		DeckBuilderPreferredPrintingState deckBuilderPreferredPrintingState = Pantry.Get<DeckBuilderPreferredPrintingState>();
		deckBuilderPreferredPrintingState.CollapseAllExpandedPoolCards();
		deckBuilderPreferredPrintingState.ExpandAllOnce = value;
		AudioManager.PlayAudio(value ? WwiseEvents.sfx_ui_generic_click : WwiseEvents.sfx_ui_back, base.gameObject);
		deckBuilderCardFilterProvider.IsDeckFilterToggleOn = value;
		deckBuilderCardFilterProvider.ResetAndApplyFilters(FilterValueChangeSource.Header);
	}

	private void UpdateFilterToggles(IReadOnlyCardFilter filter)
	{
		DeckBuilderModel model = Pantry.Get<DeckBuilderModelProvider>().Model;
		DeckBuilderContext context = Pantry.Get<DeckBuilderContextProvider>().Context;
		bool flag = Pantry.Get<CompanionUtil>().GetFilterForDeckBuilder(context, model) != null;
		UpdateFiltersUsedToggle(filter);
		bool flag2 = context.IsEditingDeck && !context.IsReadOnly;
		CompanionFilterToggle.gameObject.UpdateActive(flag && flag2);
		DeckFilterToggle.gameObject.UpdateActive(flag2);
	}

	private void Header_FilterValueChanged(CardFilterType filter, bool value)
	{
		DeckBuilderContext context = Pantry.Get<DeckBuilderContextProvider>().Context;
		DeckBuilderCardFilterProvider deckBuilderCardFilterProvider = Pantry.Get<DeckBuilderCardFilterProvider>();
		DeckBuilderLayoutState deckBuilderLayoutState = Pantry.Get<DeckBuilderLayoutState>();
		DeckBuilderVisualsUpdater deckBuilderVisualsUpdater = Pantry.Get<DeckBuilderVisualsUpdater>();
		DeckBuilderModel model = Pantry.Get<DeckBuilderModelProvider>().Model;
		ITitleCountManager titleCountManager = Pantry.Get<ITitleCountManager>();
		CardMatcher.CardMatcherMetadata metadata = new CardMatcher.CardMatcherMetadata
		{
			TitleIdsToNumberOwned = titleCountManager?.OwnedTitleCounts
		};
		if (!DeckFilterToggle.isOn)
		{
			Pantry.Get<DeckBuilderPreferredPrintingState>().CollapseAllExpandedPoolCards();
		}
		deckBuilderCardFilterProvider.SetFilter(FilterValueChangeSource.Header, filter, value);
		if (context.IsLimited && deckBuilderLayoutState.IsColumnViewExpanded)
		{
			bool flag = false;
			ColorFilter colorFilter = new ColorFilter();
			colorFilter.Property = CardPropertyFilter.PropertyType.Color;
			colorFilter.Operator = TokenType.Equals;
			switch (filter)
			{
			case CardFilterType.White:
				colorFilter.ColorFlags = CardColorFlags.White;
				break;
			case CardFilterType.Blue:
				colorFilter.ColorFlags = CardColorFlags.Blue;
				break;
			case CardFilterType.Black:
				colorFilter.ColorFlags = CardColorFlags.Black;
				break;
			case CardFilterType.Red:
				colorFilter.ColorFlags = CardColorFlags.Red;
				break;
			case CardFilterType.Green:
				colorFilter.ColorFlags = CardColorFlags.Green;
				break;
			case CardFilterType.Color_Colorless:
				colorFilter.Type = ColorFilter.ColorFilterType.Colorless;
				break;
			case CardFilterType.Multicolor:
				colorFilter.Type = ColorFilter.ColorFilterType.Gold;
				break;
			default:
				flag = true;
				break;
			}
			ColorFilter colorFilter2 = new ColorFilter();
			colorFilter2.Property = CardPropertyFilter.PropertyType.ColorIdentity;
			colorFilter2.Operator = TokenType.LessThanOrEqual;
			if (deckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.White))
			{
				colorFilter2.ColorFlags |= CardColorFlags.White;
			}
			if (deckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.Blue))
			{
				colorFilter2.ColorFlags |= CardColorFlags.Blue;
			}
			if (deckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.Black))
			{
				colorFilter2.ColorFlags |= CardColorFlags.Black;
			}
			if (deckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.Red))
			{
				colorFilter2.ColorFlags |= CardColorFlags.Red;
			}
			if (deckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.Green))
			{
				colorFilter2.ColorFlags |= CardColorFlags.Green;
			}
			colorFilter2.Negate = !value;
			List<CardPrintingData> list = new List<CardPrintingData>();
			List<CardPrintingData> list2 = new List<CardPrintingData>();
			if (value)
			{
				foreach (CardPrintingData item in deckBuilderVisualsUpdater.UnsuggestedCardsInPool)
				{
					if (flag)
					{
						break;
					}
					if (Evaluate(item, colorFilter, colorFilter2))
					{
						ModelProvider.AddCardToDeckPile(DeckBuilderPile.MainDeck, new CardData(null, item), ZoomHandler);
						list.Add(item);
					}
				}
			}
			else
			{
				foreach (CardPrintingData item2 in deckBuilderVisualsUpdater.SuggestedCardsInDeck)
				{
					if (flag || Evaluate(item2, colorFilter, colorFilter2))
					{
						model.RemoveCardFromMainDeck(item2.GrpId);
						list2.Add(item2);
					}
				}
			}
			foreach (CardPrintingData item3 in list)
			{
				deckBuilderVisualsUpdater.UnsuggestedCardsInPool.Remove(item3);
				deckBuilderVisualsUpdater.SuggestedCardsInDeck.Add(item3);
			}
			foreach (CardPrintingData item4 in list2)
			{
				deckBuilderVisualsUpdater.SuggestedCardsInDeck.Remove(item4);
				deckBuilderVisualsUpdater.UnsuggestedCardsInPool.Add(item4);
			}
			model.UpdateMainDeck();
			BasicLandSuggester.SuggestLand();
			deckBuilderVisualsUpdater.UpdateAllDeckVisuals();
			WrapperDeckBuilder.CacheDeck(model, context);
		}
		else
		{
			deckBuilderCardFilterProvider.ApplyFilters(FilterValueChangeSource.Header);
		}
		bool Evaluate(CardPrintingData card, ColorFilter colorFilter3, ColorFilter landFilter)
		{
			if (card.Types.Contains(CardType.Land) && card.ColorIdentityFlags != CardColorFlags.None)
			{
				return landFilter.Evaluate(new CardFilterGroup(new List<CardFilterGroup.FilteredCard>
				{
					new CardFilterGroup.FilteredCard(card)
				}), metadata).AnyPassed();
			}
			return colorFilter3.Evaluate(new CardFilterGroup(new List<CardFilterGroup.FilteredCard>
			{
				new CardFilterGroup.FilteredCard(card)
			}), metadata).AnyPassed();
		}
	}

	private void Header_SearchTextChanged(string value)
	{
		DeckBuilderCardFilterProvider deckBuilderCardFilterProvider = Pantry.Get<DeckBuilderCardFilterProvider>();
		deckBuilderCardFilterProvider.TextFilter.SearchText = value;
		deckBuilderCardFilterProvider.SetFilterSearchText(value);
		if (Pantry.Get<DeckBuilderLayoutState>().IsColumnViewExpanded)
		{
			Pantry.Get<DeckBuilderVisualsUpdater>().UpdateAllDeckVisuals();
		}
		else
		{
			deckBuilderCardFilterProvider.ApplyFilters(FilterValueChangeSource.Header);
		}
	}

	public void OnLayoutUpdated()
	{
		DeckBuilderContext context = ContextProvider.Context;
		bool flag = context.Mode == DeckBuilderMode.ReadOnly;
		bool flag2 = context.Mode == DeckBuilderMode.ReadOnlyCollection;
		DecksManager deckManager = Pantry.Get<DecksManager>();
		bool isColumnViewExpanded = LayoutState.IsColumnViewExpanded;
		base.gameObject.UpdateActive(!flag || context.CanCraftDeck(deckManager));
		if (EventCardPoolTitleText != null)
		{
			EventCardPoolTitleText.gameObject.UpdateActive(flag2);
		}
		CraftingModeToggle.gameObject.UpdateActive(context.ShowCraftingButtons(deckManager));
		UpdateActiveIcons(new UpdateActiveIconsSettings
		{
			hideAll = ((LayoutState.IsColumnViewExpanded && _hideHeaderWhenExpanded) || flag || flag2),
			hideColors = (isColumnViewExpanded && !context.IsLimited),
			hideLand = isColumnViewExpanded,
			hideAdvanced = isColumnViewExpanded,
			hideLargeCards = !DeckBuilderWidgetUtilities.CanUseLargeCards(ContextProvider.Context, LayoutState),
			forceRightOverflowActive = (!context.IsSideboarding && context.CanCraftDeck(deckManager))
		});
	}
}
