using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

public class AutoLandsToggle : MonoBehaviour
{
	[SerializeField]
	private CustomToggle _customToggle;

	private DeckBuilderVisualsUpdater _visualUpdater;

	private bool _isInitialized;

	private DeckBuilderCardFilterProvider DeckBuilderCardFilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private bool EditingADeck => Pantry.Get<DeckBuilderContextProvider>().Context.IsEditingDeck;

	private bool IsLandFilterOn => DeckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.Type_Land);

	public void Awake()
	{
		Deactivate();
	}

	public void OnEnable()
	{
		DeckBuilderCardFilterProvider.AutoSuggestLandFilterToggleSet += OnAutoSuggestLandFilterToggleSet;
		OnAutoSuggestLandFilterToggleSet(DeckBuilderCardFilterProvider.IsAutoSuggestLandsToggleOn);
		DeckBuilderCardFilterProvider.FilterValueChanged += OnFilterValueChanged;
		DeckBuilderCardFilterProvider.OnReparentAutoSuggestLand += OnReparentAutoSuggestLand;
	}

	public void OnDisable()
	{
		DeckBuilderCardFilterProvider.AutoSuggestLandFilterToggleSet -= OnAutoSuggestLandFilterToggleSet;
		DeckBuilderCardFilterProvider.FilterValueChanged -= OnFilterValueChanged;
		DeckBuilderCardFilterProvider.OnReparentAutoSuggestLand -= OnReparentAutoSuggestLand;
	}

	public void Initialize(bool isAutoLandsToggleOn, SearchAndFilterBar filterBar, CardPoolHolder cardPoolHolder, DeckBuilderVisualsUpdater visualUpdater)
	{
		if (!_isInitialized)
		{
			_customToggle.Value = isAutoLandsToggleOn;
			filterBar.LandFilterValueChanged += _onFilterValueChanged;
			cardPoolHolder.OnPageChanged += _onPageChanged;
			_visualUpdater = visualUpdater;
			_isInitialized = true;
		}
	}

	public void Activate()
	{
		_customToggle.OnValueChanged.AddListener(_onToggleValueChanged);
		base.gameObject.UpdateActive(active: true);
	}

	public void Deactivate()
	{
		_customToggle.OnValueChanged.RemoveListener(_onToggleValueChanged);
		base.gameObject.UpdateActive(active: false);
	}

	private void _onToggleValueChanged()
	{
		DeckBuilderCardFilterProvider.IsAutoSuggestLandsToggleOn = _customToggle.Value;
	}

	private void _onPageChanged(int pageIndex)
	{
		if (IsLandFilterOn && EditingADeck)
		{
			if (pageIndex == 0)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}
	}

	private void OnFilterValueChanged(FilterValueChangeSource source, CardFilterType type, bool isOn)
	{
		if (type == CardFilterType.Type_Land)
		{
			if (isOn && EditingADeck)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}
	}

	private void _onFilterValueChanged(bool isOn)
	{
		if (isOn && EditingADeck)
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	private void OnAutoSuggestLandFilterToggleSet(bool value)
	{
		_customToggle.Value = value;
		if (value)
		{
			BasicLandSuggester.SuggestLand();
			_visualUpdater.UpdateAllDeckVisuals();
		}
	}

	private void OnReparentAutoSuggestLand(Transform newParent)
	{
		base.transform.SetParent(newParent);
		base.transform.ZeroOut();
	}
}
