using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using DG.Tweening;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class AdvancedFiltersView : MonoBehaviour
{
	[Serializable]
	public class FilterGroupView
	{
		public string MTGSetGroup;

		public GameObject Container;

		public Toggle Toggle;

		[NonSerialized]
		public List<CardFilterView> AllFilterViews = new List<CardFilterView>();
	}

	private enum GroupsOfMTGSets
	{
		FormatLegalGroup,
		FormatNotLegalGroup
	}

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private CustomButton _doneButton;

	[SerializeField]
	private Button _resetButton;

	[SerializeField]
	private TMP_Dropdown _formatDropdown;

	[SerializeField]
	private GameObject FormatContainer;

	[SerializeField]
	private CustomButton FormatButton;

	[SerializeField]
	private TMP_Text _formatInfo;

	[SerializeField]
	private GameObject _formatInfoContainer;

	[SerializeField]
	private GameObject _formatDropdownSpacer1;

	[SerializeField]
	private GameObject _formatDropdownSpacer2;

	[SerializeField]
	private GameObject _formatSelectionAccordian;

	[SerializeField]
	private GameObject _collectedCardsGroup;

	[FormerlySerializedAs("filterGroups")]
	[FormerlySerializedAs("_expansionGroups")]
	[SerializeField]
	private List<FilterGroupView> _filterGroupsOfMTGSets;

	[SerializeField]
	private TMP_Text _formatLegalText;

	[SerializeField]
	private GameObject _formatNotLegalAccordian;

	private List<CardFilterView> _cardFilterViewsForEveryExpansion;

	private Dictionary<CardFilterView, FilterGroupView> _dropdownGroupForCardFilterViewMap = new Dictionary<CardFilterView, FilterGroupView>();

	[SerializeField]
	private float _fadeDuration = 0.25f;

	[SerializeField]
	private CardFilterView _setFilterViewPrefab;

	private CardFilterView[] _allViews;

	private List<DeckFormat> _availableFormats;

	private bool _suppressValueChangeEvents;

	private bool _suppressFilterUpdates;

	private bool _showCollectedMode;

	private ISetMetadataProvider _filterDataProvider;

	private FormatManager _formatManager;

	private EventManager _eventManager;

	private IClientLocProvider _locProvider;

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderCardFilterProvider FilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	public event Action<DeckFormat> OnFormatSelected;

	public event Action OnApply;

	public void Init(FormatManager formatManager, EventManager eventManager)
	{
		_formatManager = formatManager;
		_eventManager = eventManager;
	}

	private void Awake()
	{
		_resetButton.onClick.AddListener(ResetButton_OnClick);
		_doneButton.OnClick.AddListener(CancelButton_OnClick);
		foreach (FilterGroupView filterGroup in _filterGroupsOfMTGSets)
		{
			filterGroup.Toggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				ToggleGroup(isOn, filterGroup);
			});
		}
		_formatDropdown.onValueChanged.AddListener(FormatChangeEvent);
		_filterDataProvider = Pantry.Get<ISetMetadataProvider>();
		_locProvider = Pantry.Get<IClientLocProvider>();
		if (PlatformUtils.IsHandheld())
		{
			Canvas component = GetComponent<Canvas>();
			if (component != null)
			{
				component.sortingLayerName = "Foreground";
			}
		}
	}

	private void OnEnable()
	{
		FilterProvider.OnFilterReset += OnReset;
	}

	private void OnDisable()
	{
		FilterProvider.OnFilterReset -= OnReset;
	}

	private void FormatChangeEvent(int value)
	{
		ContextProvider.SelectFormat(_availableFormats[value]);
		UpdateForFormat(_availableFormats[value]);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private void UpdateForFormat(DeckFormat format)
	{
		if (format == null || format.FormatType == MDNEFormatType.Draft || format.FormatType == MDNEFormatType.Sealed)
		{
			ToggleFormatActive(shouldEnable: false);
			_formatLegalText.SetText(_locProvider.GetLocalizedText("MainNav/Collection/AllSets"));
		}
		else
		{
			ToggleFormatActive(shouldEnable: true);
			_formatInfo.SetText(format.GetLocalizedInfo());
			_formatLegalText.SetText(_locProvider.GetLocalizedText("MainNav/Collection/FormatLegal"));
		}
		UpdateFilterForFormat(format);
	}

	public void SetFormat(DeckFormat format, List<DeckFormat> availableFormats, bool interactable)
	{
		_formatDropdown.gameObject.SetActive(format != null);
		_formatDropdown.onValueChanged.RemoveListener(FormatChangeEvent);
		_availableFormats = availableFormats;
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		int value = 0;
		for (int i = 0; i < _availableFormats.Count; i++)
		{
			list.Add(new TMP_Dropdown.OptionData(_availableFormats[i].GetLocalizedName()));
			if (format == _availableFormats[i])
			{
				value = i;
			}
		}
		_formatDropdown.options = list;
		_formatDropdown.value = value;
		UpdateForFormat(format);
		if (interactable)
		{
			_formatDropdown.onValueChanged.AddListener(FormatChangeEvent);
		}
	}

	private List<CardFilterType> GetBaseCardFilters()
	{
		return new List<CardFilterType>
		{
			CardFilterType.White,
			CardFilterType.Blue,
			CardFilterType.Black,
			CardFilterType.Red,
			CardFilterType.Green,
			CardFilterType.Color_Colorless,
			CardFilterType.Multicolor,
			CardFilterType.Cost_0,
			CardFilterType.Cost_1,
			CardFilterType.Cost_2,
			CardFilterType.Cost_3,
			CardFilterType.Cost_4,
			CardFilterType.Cost_5,
			CardFilterType.Cost_6,
			CardFilterType.Cost_7OrGreater,
			CardFilterType.Rarity_BasicLand,
			CardFilterType.Rarity_Common,
			CardFilterType.Rarity_Uncommon,
			CardFilterType.Rarity_Rare,
			CardFilterType.Rarity_MythicRare,
			CardFilterType.Type_Creature,
			CardFilterType.Type_Planeswalker,
			CardFilterType.Type_Instant,
			CardFilterType.Type_Sorcery,
			CardFilterType.Type_Enchantment,
			CardFilterType.Type_Artifact,
			CardFilterType.Type_Battle,
			CardFilterType.Type_Land,
			CardFilterType.Collection_Collected,
			CardFilterType.Collection_Uncollected,
			CardFilterType.Collection_1_Of,
			CardFilterType.Collection_2_Of,
			CardFilterType.Collection_3_Of,
			CardFilterType.Collection_4_Of
		};
	}

	private void ToggleFormatActive(bool shouldEnable)
	{
		_formatNotLegalAccordian.SetActive(shouldEnable);
		_formatInfoContainer.SetActive(shouldEnable);
		_formatInfo.gameObject.SetActive(shouldEnable);
		_formatSelectionAccordian.SetActive(shouldEnable);
		_formatDropdownSpacer1.SetActive(shouldEnable);
		_formatDropdownSpacer2.SetActive(shouldEnable);
	}

	private void UpdateFilterForFormat(DeckFormat format)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		List<CardFilterType> list;
		CardFilterView[] allViews;
		if (format != null && format.FormatType != MDNEFormatType.Draft && format.FormatType != MDNEFormatType.Sealed)
		{
			list = GetBaseCardFilters();
			flag = format.IsPauper;
			flag2 = format.IsArtisan;
			flag3 = format.IsSingleton;
			flag4 = format.FormatIncludesCommandZone;
			List<CardFilterType> list2 = new List<CardFilterType>();
			SetMetadataCore.ExpansionsToFilters(format.FilterSets.ToList(), list2);
			UpdateMapWithExpansionFilterViews(list2);
			list.AddRange(list2);
		}
		else
		{
			list = new List<CardFilterType>();
			allViews = _allViews;
			foreach (CardFilterView cardFilterView in allViews)
			{
				list.Add(cardFilterView.FilterType);
			}
			UpdateMapWithExpansionFilterViews(_filterDataProvider.AllExpansionsAsFilterTypes);
			list.AddRange(_filterDataProvider.AllExpansionsAsFilterTypes);
		}
		if (flag)
		{
			list.Remove(CardFilterType.Rarity_Uncommon);
		}
		if (flag2 || flag)
		{
			list.Remove(CardFilterType.Rarity_Rare);
			list.Remove(CardFilterType.Rarity_MythicRare);
		}
		if (flag3)
		{
			list.Remove(CardFilterType.Collection_2_Of);
			list.Remove(CardFilterType.Collection_3_Of);
			list.Remove(CardFilterType.Collection_4_Of);
		}
		if (flag4)
		{
			list.Add(CardFilterType.Commanders);
		}
		foreach (FilterGroupView filterGroupsOfMTGSet in _filterGroupsOfMTGSets)
		{
			filterGroupsOfMTGSet.Toggle.gameObject.SetActive(value: false);
		}
		allViews = _allViews;
		foreach (CardFilterView cardFilterView2 in allViews)
		{
			if (list.Contains(cardFilterView2.FilterType))
			{
				cardFilterView2.CanvasGroup.alpha = 1f;
				cardFilterView2.FilterToggle.interactable = true;
				GetDropdownGroupForCardFilterView(cardFilterView2, _dropdownGroupForCardFilterViewMap)?.Toggle.gameObject.SetActive(value: true);
			}
			else
			{
				cardFilterView2.CanvasGroup.alpha = 0.5f;
				cardFilterView2.FilterToggle.interactable = false;
				cardFilterView2.FilterToggle.isOn = false;
			}
		}
	}

	private void ToggleViewList(bool value, IEnumerable<CardFilterView> filterViewList)
	{
		if (_suppressValueChangeEvents)
		{
			return;
		}
		foreach (CardFilterView filterView in filterViewList)
		{
			if (filterView.FilterToggle.interactable)
			{
				filterView.FilterToggle.isOn = value;
			}
		}
	}

	private void ToggleGroup(bool value, FilterGroupView filterGroup)
	{
		ToggleViewList(value, filterGroup.AllFilterViews);
	}

	private void UpdateMapWithExpansionFilterViews(List<CardFilterType> formatLegalExpansions)
	{
		if (_filterGroupsOfMTGSets.Count != Enum.GetNames(typeof(GroupsOfMTGSets)).Length)
		{
			Debug.LogError("Not enough expansion groups.");
			return;
		}
		List<CardFilterView> list = new List<CardFilterView>();
		List<CardFilterView> list2 = new List<CardFilterView>();
		FilterGroupView filterGroupView = _filterGroupsOfMTGSets[0];
		FilterGroupView filterGroupView2 = _filterGroupsOfMTGSets[1];
		filterGroupView.AllFilterViews = list;
		filterGroupView2.AllFilterViews = list2;
		foreach (CardFilterView item in _cardFilterViewsForEveryExpansion)
		{
			if (!formatLegalExpansions.Contains(item.FilterType))
			{
				item.gameObject.transform.parent = filterGroupView2.Container.transform;
				list.Remove(item);
				list2.Add(item);
			}
			else
			{
				item.gameObject.transform.parent = filterGroupView.Container.transform;
				list.Add(item);
				list2.Remove(item);
			}
		}
		UpdateFilterViewOrder(list, _filterDataProvider.AllExpansionsAsFilterTypes);
		UpdateFilterViewOrder(list2, _filterDataProvider.AllExpansionsAsFilterTypes);
		UpdateMapWithViews(filterGroupView2, _dropdownGroupForCardFilterViewMap);
	}

	private static void UpdateFilterViewOrder(List<CardFilterView> cardFilterViews, List<CardFilterType> expectedOrder)
	{
		Dictionary<CardFilterType, int> dictionary = new Dictionary<CardFilterType, int>();
		for (int i = 0; i < expectedOrder.Count; i++)
		{
			CardFilterType key = expectedOrder[i];
			dictionary[key] = i;
		}
		foreach (CardFilterView cardFilterView in cardFilterViews)
		{
			if (dictionary.TryGetValue(cardFilterView.FilterType, out var value))
			{
				cardFilterView.gameObject.transform.SetSiblingIndex(value);
			}
		}
	}

	public void SetModel(IReadOnlyCardFilter filter)
	{
		CardFilterView[] allViews;
		if (_allViews == null)
		{
			List<CardFilterType> allExpansionsAsFilterTypes = _filterDataProvider.AllExpansionsAsFilterTypes;
			_cardFilterViewsForEveryExpansion = InitializeCardFilterViews(_filterGroupsOfMTGSets[0], allExpansionsAsFilterTypes, _setFilterViewPrefab, _filterDataProvider);
			UpdateMapWithViews(_filterGroupsOfMTGSets[0], _dropdownGroupForCardFilterViewMap);
			_allViews = GetComponentsInChildren<CardFilterView>(includeInactive: true);
			allViews = _allViews;
			foreach (CardFilterView filterView in allViews)
			{
				filterView.FilterToggle.onValueChanged.AddListener(delegate
				{
					if (!_suppressValueChangeEvents)
					{
						AudioManager.PlayAudio(WwiseEvents.sfx_ui_filter_toggle, AudioManager.Default);
						CardFilterView cardFilterView2 = filterView;
						FilterValueChanged(cardFilterView2.FilterType, cardFilterView2.FilterToggle.isOn, _suppressFilterUpdates);
						_suppressValueChangeEvents = true;
						UpdateToggleForFilter(GetDropdownGroupForCardFilterView(filterView, _dropdownGroupForCardFilterViewMap), filterView);
						_suppressValueChangeEvents = false;
					}
				});
			}
		}
		_suppressValueChangeEvents = true;
		allViews = _allViews;
		foreach (CardFilterView cardFilterView in allViews)
		{
			cardFilterView.FilterToggle.isOn = filter.IsSet(cardFilterView.FilterType);
			UpdateToggleForFilter(GetDropdownGroupForCardFilterView(cardFilterView, _dropdownGroupForCardFilterViewMap), cardFilterView);
		}
		_suppressValueChangeEvents = false;
	}

	private static void UpdateToggleForFilter(FilterGroupView filterGroup, CardFilterView filterView)
	{
		if (filterGroup != null && !filterView.FilterToggle.isOn)
		{
			filterGroup.Toggle.isOn = false;
		}
	}

	private static FilterGroupView GetDropdownGroupForCardFilterView(CardFilterView filterView, Dictionary<CardFilterView, FilterGroupView> dropdownGroupForCardFilterViewMap)
	{
		if (dropdownGroupForCardFilterViewMap.TryGetValue(filterView, out var value))
		{
			return value;
		}
		return null;
	}

	private static void UpdateMapWithViews(FilterGroupView groupView, Dictionary<CardFilterView, FilterGroupView> dropdownGroupForCardFilterViewMap)
	{
		foreach (CardFilterView allFilterView in groupView.AllFilterViews)
		{
			dropdownGroupForCardFilterViewMap[allFilterView] = groupView;
		}
	}

	private static List<CardFilterView> InitializeCardFilterViews(FilterGroupView groupView, List<CardFilterType> filterTypes, CardFilterView setFilterViewPrefab, ISetMetadataProvider filterDataProvider)
	{
		CardFilterView[] componentsInChildren = groupView.Container.GetComponentsInChildren<CardFilterView>(includeInactive: true);
		groupView.AllFilterViews.AddRange(componentsInChildren);
		if (filterTypes != null)
		{
			foreach (CardFilterType filterType in filterTypes)
			{
				string text = filterDataProvider.SetCodeForFilter(filterType);
				if (!string.IsNullOrEmpty(text))
				{
					CardFilterView cardFilterView = UnityEngine.Object.Instantiate(setFilterViewPrefab, groupView.Container.transform);
					cardFilterView.Initialize(filterType, text);
					groupView.AllFilterViews.Add(cardFilterView);
				}
			}
		}
		return groupView.AllFilterViews;
	}

	private void CancelButton_OnClick()
	{
		Hide(applyFilters: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, AudioManager.Default);
	}

	private void ResetButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		FilterProvider.ResetAndApplyFilters(FilterValueChangeSource.AdvancedFilters);
		_suppressValueChangeEvents = true;
		foreach (FilterGroupView filterGroupsOfMTGSet in _filterGroupsOfMTGSets)
		{
			filterGroupsOfMTGSet.Toggle.isOn = false;
		}
		_suppressValueChangeEvents = false;
	}

	public void UpdateFilterMode(bool showCollectedMode)
	{
		_showCollectedMode = showCollectedMode;
		ToggleCollectedFilterVisibility(toggleOn: true);
	}

	public void ToggleCollectedFilterVisibility(bool toggleOn)
	{
		_collectedCardsGroup.SetActive(toggleOn && _showCollectedMode);
	}

	public void Show()
	{
		_suppressFilterUpdates = true;
		base.gameObject.UpdateActive(active: true);
		DOTween.Kill(this);
		_canvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.InOutSine).SetTarget(this);
	}

	public void OnReset(IReadOnlyCardFilter filter)
	{
		SetModel(filter);
	}

	public void Hide(bool applyFilters)
	{
		_suppressFilterUpdates = false;
		if (applyFilters)
		{
			FilterProvider.ApplyFilters(FilterValueChangeSource.AdvancedFilters);
		}
		DOTween.Kill(this);
		_canvasGroup.DOFade(0f, _fadeDuration).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			base.gameObject.UpdateActive(active: false);
		})
			.SetTarget(this);
	}

	private void FilterValueChanged(CardFilterType filter, bool value, bool suppressApply)
	{
		FilterProvider.SetFilter(FilterValueChangeSource.AdvancedFilters, filter, value);
		if (!suppressApply)
		{
			FilterProvider.ApplyFilters(FilterValueChangeSource.AdvancedFilters);
		}
	}

	public void OpenAdvancedFilters()
	{
		DeckBuilderContext context = ContextProvider.Context;
		SetModel(FilterProvider.Filter);
		UpdateCollectionFilterToggles(context);
		List<DeckFormat> availableFormats;
		bool interactable;
		if (context.IsEvent && context.Format != null)
		{
			availableFormats = new List<DeckFormat> { context.Format };
			interactable = false;
		}
		else
		{
			availableFormats = DeckBuilderWidgetUtilities.GetAvailableFormats(_formatManager.GetAllFormats(), _eventManager.EventContexts, context.Format);
			interactable = true;
		}
		SetFormat(context.Format, availableFormats, interactable);
		Show();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, AudioManager.Default);
	}

	private void UpdateCollectionFilterToggles(DeckBuilderContext context)
	{
		bool shouldShowCollectedToggles = context.ShouldShowCollectedToggles;
		UpdateFilterMode(shouldShowCollectedToggles);
	}
}
