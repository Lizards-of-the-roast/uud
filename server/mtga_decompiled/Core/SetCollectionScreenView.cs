using System;
using System.Collections.Generic;
using System.Linq;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

public class SetCollectionScreenView : MonoBehaviour
{
	private enum Filters
	{
		Historic,
		Standard,
		Alchemy,
		Default
	}

	private class SortOption
	{
		public string locKey;

		public SortType sortType;
	}

	private enum SortType
	{
		NewestToOldest,
		OldestToNewest
	}

	private class FilterOption
	{
		public string locKey;

		public DeckFormat deckFormat;
	}

	[SerializeField]
	private GameObject _badgeContainer;

	[SerializeField]
	private GameObject _metersContainer;

	[SerializeField]
	private RawImage _setLogo;

	[SerializeField]
	private TMP_Text _setReleaseDate;

	[SerializeField]
	private GameObject _metersWidget;

	[SerializeField]
	private GameObject _badgePrefab;

	private List<SetBadge> _setBadges;

	private SetBadge _selectedBadge;

	[SerializeField]
	private TMP_Dropdown _sortDropDown;

	private List<SortOption> _sortOptions = new List<SortOption>();

	[SerializeField]
	private TMP_Dropdown _filterDropDown;

	private List<FilterOption> _filterOptions = new List<FilterOption>();

	[SerializeField]
	private Toggle _standardToggle;

	[SerializeField]
	private Toggle _historicToggle;

	[SerializeField]
	private Toggle _alchemyToggle;

	[SerializeField]
	private GameObject _alchemyBanner;

	[SerializeField]
	private GameObject _universesBeyondBanner;

	[SerializeField]
	private CustomButton _storeButton;

	private SetCollectionController _controller;

	private Action _backButton_OnClicked;

	private RawImageReferenceLoader _setLogoLoader;

	private ISetMetadataProvider _setMetadataProvider;

	private DeckFormat _standard;

	private DeckFormat _historic;

	private DeckFormat _alchemy;

	private DateTime _biCollectionViewFilterStartTime;

	private DateTime _biCollectionNavigateStartTime;

	private string _biActiveFilter = "";

	private IBILogger _biLogger;

	public void Init(SetCollectionController controller, ISetMetadataProvider setMetadataProvider, Action backButton_OnClicked, IBILogger biLogger)
	{
		_controller = controller;
		_setMetadataProvider = setMetadataProvider;
		_backButton_OnClicked = backButton_OnClicked;
		_setLogoLoader = new RawImageReferenceLoader(_setLogo);
		_metersContainer.SetActive(value: false);
		_setBadges = new List<SetBadge>();
		_selectedBadge = null;
		_sortOptions = new List<SortOption>
		{
			new SortOption
			{
				locKey = "MainNav/Profile/SetCollection/NewestToOldest",
				sortType = SortType.NewestToOldest
			},
			new SortOption
			{
				locKey = "MainNav/Profile/SetCollection/OldestToNewest",
				sortType = SortType.OldestToNewest
			}
		};
		_sortDropDown.options = _sortOptions.Select((SortOption so) => new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText(so.locKey))).ToList();
		_sortDropDown.value = 0;
		InitializeFormatDropdown();
		_filterDropDown.onValueChanged.AddListener(FilterDropdown_OnValueChanged);
		_standardToggle.onValueChanged.AddListener(OnStandardToggle);
		_historicToggle.onValueChanged.AddListener(OnHistoricToggle);
		_alchemyToggle.onValueChanged.AddListener(OnAlchemyToggle);
		_standard = WrapperController.Instance.FormatManager?.GetAllFormats().Find((DeckFormat x) => x.IsStandard);
		_historic = WrapperController.Instance.FormatManager?.GetAllFormats().Find((DeckFormat x) => x.IsHistoric);
		_alchemy = WrapperController.Instance.FormatManager?.GetAllFormats().Find((DeckFormat x) => x.IsAlchemy);
		_storeButton.OnClick.AddListener(MoveToStore);
		Languages.LanguageChangedSignal.Listeners += OnLocalize;
		_biCollectionViewFilterStartTime = DateTime.Now;
		_biCollectionNavigateStartTime = DateTime.Now;
		_biActiveFilter = Filters.Default.ToString();
		_biLogger = biLogger;
	}

	private void MoveToStore()
	{
		if (_selectedBadge == null)
		{
			SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Packs, "buy arbitrary packs from set collection");
			return;
		}
		string expansionCode = (_selectedBadge._isAlchemy ? _selectedBadge._expansionCode.ToString().Substring(4, 3) : _selectedBadge._expansionCode.ToString());
		SceneLoader.GetSceneLoader().GoToStoreSetPacks(expansionCode, "buy " + _selectedBadge._expansionCode.ToString() + " packs from set collection");
	}

	private void UpdateBadges()
	{
		for (int num = _setBadges.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(_setBadges[num].gameObject);
		}
		_setBadges.Clear();
		foreach (string validSetCollectionSet in _controller.ValidSetCollectionSets)
		{
			CollationMapping itemSubType = CollationMappingUtils.FromString(validSetCollectionSet);
			bool isAlchemySet = _setMetadataProvider.IsAlchemy(itemSubType);
			bool isUniversesBeyondSet = _setMetadataProvider.IsUniversesBeyond(itemSubType);
			if (_controller.GetMetricTotals(validSetCollectionSet).numAvailable > 20)
			{
				SetBadge component = UnityEngine.Object.Instantiate(_badgePrefab, _badgeContainer.transform).GetComponent<SetBadge>();
				InitializeBadge(validSetCollectionSet, component, isAlchemySet, isUniversesBeyondSet);
				_setBadges.Add(component);
			}
		}
		SortBadges(_sortDropDown.value);
	}

	private void InitializeBadge(string expansionCode, SetBadge badgeScript, bool isAlchemySet, bool isUniversesBeyondSet)
	{
		CollationMapping expansionCode2 = CollationMappingUtils.FromString(expansionCode);
		DateTime releaseDate = _controller.GetReleaseDate(expansionCode2);
		badgeScript.Init(expansionCode2, releaseDate, UpdateMeters, isAlchemySet, isUniversesBeyondSet);
		Sprite setIcon = _controller.GetSetIcon(expansionCode2);
		Sprite alchemyIcon = (isAlchemySet ? _controller.GetAlchemyIcon(expansionCode2) : null);
		int numOwned = _controller.GetMetricTotals(expansionCode).numOwned;
		int numAvailable = _controller.GetMetricTotals(expansionCode).numAvailable;
		if (!_controller.IsCollectionComplete(expansionCode, SetCollectionController.Metrics.None, CountMode.UsePlayerInvOneOf, isAlchemySet))
		{
			badgeScript.UpdateUI(setIcon, numOwned, numAvailable);
		}
		else
		{
			badgeScript.UpdateUI(setIcon, numOwned, numAvailable, useFourOf: true);
		}
		badgeScript._isStandard = _standard.LegalSets.Contains(expansionCode);
		badgeScript._isHistoric = _historic.LegalSets.Contains(expansionCode);
		if (isAlchemySet)
		{
			badgeScript.SetAlchemyIcon(alchemyIcon);
		}
	}

	private void SortBadges(int value)
	{
		switch (_sortOptions[value].sortType)
		{
		case SortType.NewestToOldest:
		{
			_setBadges.Sort(delegate(SetBadge b1, SetBadge b2)
			{
				DateTime releaseDate = b1._releaseDate;
				DateTime releaseDate2 = b2._releaseDate;
				return releaseDate2.CompareTo(releaseDate);
			});
			for (int num3 = 0; num3 < _setBadges.Count; num3++)
			{
				_setBadges[num3].transform.SetSiblingIndex(num3);
			}
			break;
		}
		case SortType.OldestToNewest:
		{
			_setBadges.Sort(delegate(SetBadge b1, SetBadge b2)
			{
				DateTime releaseDate = b1._releaseDate;
				DateTime releaseDate2 = b2._releaseDate;
				return releaseDate.CompareTo(releaseDate2);
			});
			for (int num2 = 0; num2 < _setBadges.Count; num2++)
			{
				_setBadges[num2].transform.SetSiblingIndex(num2);
			}
			break;
		}
		default:
		{
			_setBadges.Sort(delegate(SetBadge b1, SetBadge b2)
			{
				DateTime releaseDate = b1._releaseDate;
				DateTime releaseDate2 = b2._releaseDate;
				return releaseDate2.CompareTo(releaseDate);
			});
			for (int num = 0; num < _setBadges.Count; num++)
			{
				_setBadges[num].transform.SetSiblingIndex(num);
			}
			break;
		}
		}
	}

	private void FilterBadges(int value)
	{
		FilterOption filterOption = _filterOptions[value];
		_metersContainer.UpdateActive(active: false);
		if (_selectedBadge != null)
		{
			_selectedBadge.SetSelected(selected: false);
		}
		_selectedBadge = null;
		foreach (SetBadge badge in _setBadges)
		{
			badge.gameObject.UpdateActive(filterOption.deckFormat.LegalSets.Exists((string x) => x == badge._expansionCode.ToString()));
		}
	}

	private void InitializeFormatDropdown()
	{
		List<DeckFormat> evergreenFormats = WrapperController.Instance.FormatManager.EvergreenFormats;
		_filterOptions.AddRange(new List<FilterOption>
		{
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Collection/AllSets"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.IsHistoric)
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Standard"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.IsStandard)
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Alchemy"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.IsAlchemy)
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Explorer"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.IsExplorer)
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Historic"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.IsHistoric)
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Timeless"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.FormatName == "Timeless")
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/HistoricBrawl"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.FormatName == "HistoricBrawl")
			},
			new FilterOption
			{
				locKey = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Brawl"),
				deckFormat = evergreenFormats.FirstOrDefault((DeckFormat x) => x.FormatName == "Brawl")
			}
		});
		_filterDropDown.options = _filterOptions.Select((FilterOption x) => new TMP_Dropdown.OptionData(x.locKey)).ToList();
	}

	private void FilterDropdown_OnValueChanged(int value)
	{
		FilterBadges(value);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private void UpdateMeters(SetBadge selectedBadge)
	{
		if (selectedBadge == null)
		{
			return;
		}
		BICollectionViewFilter(_biActiveFilter, (_selectedBadge == null) ? "None" : _selectedBadge._expansionCode.ToString(), DateTime.Now - _biCollectionViewFilterStartTime);
		if (!_metersContainer.activeInHierarchy)
		{
			_metersContainer.SetActive(value: true);
		}
		if (_selectedBadge != null)
		{
			_selectedBadge.SetSelected(selected: false);
		}
		_selectedBadge = selectedBadge;
		_selectedBadge.SetSelected(selected: true);
		CollationMapping expansionCode = _selectedBadge._expansionCode;
		string expansionCode2 = expansionCode.ToString();
		DateTime releaseDate = _selectedBadge._releaseDate;
		CollationMapping expansionCode3 = (_selectedBadge._isAlchemy ? CollationMappingUtils.FromString(expansionCode.ToString().Substring(4, 3)) : expansionCode);
		MetricMeter[] componentsInChildren = _metersWidget.GetComponentsInChildren<MetricMeter>();
		_controller.GetSetLogo(expansionCode, ref _setLogoLoader, ref _setLogo);
		_alchemyBanner.UpdateActive(selectedBadge._isAlchemy);
		_universesBeyondBanner.UpdateActive(selectedBadge._isUniversesBeyond);
		_setReleaseDate.SetText(string.Format("{0} {1:MMMM dd, yyyy}", Languages.ActiveLocProvider.GetLocalizedText("MainNav/Profile/SetCollection/ReleaseDate"), releaseDate));
		MetricMeter[] array = componentsInChildren;
		foreach (MetricMeter metricMeter in array)
		{
			int numOwned = _controller.GetMetricTotals(expansionCode2, metricMeter._metric).numOwned;
			int numAvailable = _controller.GetMetricTotals(expansionCode2, metricMeter._metric).numAvailable;
			if (metricMeter._metric.IsRarityMetric())
			{
				Sprite setIcon = _controller.GetSetIcon(expansionCode3, metricMeter._metric.AsRarity());
				metricMeter.UpdateMetricIcon(setIcon);
			}
			if (!_controller.IsCollectionComplete(expansionCode2, metricMeter._metric, CountMode.UsePlayerInvOneOf, _selectedBadge._isAlchemy))
			{
				metricMeter.UpdateUI(numOwned, numAvailable);
			}
			else
			{
				metricMeter.UpdateUI(numOwned, numAvailable, isFourOf: true);
			}
		}
	}

	private void OnStandardToggle(bool toggleValue)
	{
		if (toggleValue)
		{
			BICollectionViewFilter(_biActiveFilter, (_selectedBadge == null) ? CollationMapping.None.ToString() : _selectedBadge._expansionCode.ToString(), DateTime.Now - _biCollectionViewFilterStartTime);
			_metersContainer.SetActive(value: false);
			if (_selectedBadge != null)
			{
				_selectedBadge.SetSelected(selected: false);
			}
			_selectedBadge = null;
			_historicToggle.SetIsOnWithoutNotify(value: false);
			_alchemyToggle.SetIsOnWithoutNotify(value: false);
			foreach (SetBadge setBadge in _setBadges)
			{
				setBadge.gameObject.SetActive(setBadge._isStandard);
			}
			_biActiveFilter = Filters.Standard.ToString();
		}
		else
		{
			_standardToggle.SetIsOnWithoutNotify(value: true);
		}
	}

	private void OnHistoricToggle(bool toggleValue)
	{
		if (toggleValue)
		{
			BICollectionViewFilter(_biActiveFilter, (_selectedBadge == null) ? CollationMapping.None.ToString() : _selectedBadge._expansionCode.ToString(), DateTime.Now - _biCollectionViewFilterStartTime);
			_metersContainer.SetActive(value: false);
			if (_selectedBadge != null)
			{
				_selectedBadge.SetSelected(selected: false);
			}
			_selectedBadge = null;
			_standardToggle.SetIsOnWithoutNotify(value: false);
			_alchemyToggle.SetIsOnWithoutNotify(value: false);
			foreach (SetBadge setBadge in _setBadges)
			{
				setBadge.gameObject.SetActive(setBadge._isHistoric);
			}
			_biActiveFilter = Filters.Historic.ToString();
		}
		else
		{
			_historicToggle.SetIsOnWithoutNotify(value: true);
		}
	}

	private void OnAlchemyToggle(bool toggleValue)
	{
		if (toggleValue)
		{
			BICollectionViewFilter(_biActiveFilter, (_selectedBadge == null) ? CollationMapping.None.ToString() : _selectedBadge._expansionCode.ToString(), DateTime.Now - _biCollectionViewFilterStartTime);
			_metersContainer.SetActive(value: false);
			if (_selectedBadge != null)
			{
				_selectedBadge.SetSelected(selected: false);
			}
			_selectedBadge = null;
			_standardToggle.SetIsOnWithoutNotify(value: false);
			_historicToggle.SetIsOnWithoutNotify(value: false);
			foreach (SetBadge setBadge in _setBadges)
			{
				setBadge.gameObject.SetActive(setBadge._isAlchemy);
			}
			_biActiveFilter = Filters.Alchemy.ToString();
		}
		else
		{
			_alchemyToggle.SetIsOnWithoutNotify(value: true);
		}
	}

	private void OnLocalize()
	{
		if (_selectedBadge != null)
		{
			CollationMapping expansionCode = _selectedBadge._expansionCode;
			DateTime releaseDate = _selectedBadge._releaseDate;
			_controller.GetSetLogo(expansionCode, ref _setLogoLoader, ref _setLogo);
			_setReleaseDate.SetText(string.Format("{0} {1:MMMM dd, yyyy}", Languages.ActiveLocProvider.GetLocalizedText("MainNav/Profile/SetCollection/ReleaseDate"), releaseDate));
		}
		_sortDropDown.options = _sortOptions.Select((SortOption so) => new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText(so.locKey))).ToList();
	}

	public void BackButtonClicked()
	{
		_metersContainer.SetActive(value: false);
		_backButton_OnClicked?.Invoke();
	}

	public void CollectionButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		string text = ((_selectedBadge == null) ? null : _selectedBadge._expansionCode.ToString());
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		string setToFilter = text;
		sceneLoader.GoToDeckBuilder(new DeckBuilderContext(null, null, sideboarding: false, firstEdit: false, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, default(Guid), null, null, null, cachingEnabled: false, isPlayblade: false, null, isInvalidForEventFormat: false, null, isDrafting: false, setToFilter));
	}

	public void Display()
	{
		base.gameObject.SetActive(value: true);
		UpdateBadges();
		_filterDropDown.value = 0;
		_metersContainer.SetActive(value: false);
		_biCollectionViewFilterStartTime = DateTime.Now;
		_biCollectionNavigateStartTime = DateTime.Now;
		_biActiveFilter = Filters.Default.ToString();
		SelectDefaultBadge();
	}

	public void SelectDefaultBadge()
	{
		SelectBadgeByExpansionCode((_selectedBadge != null) ? _selectedBadge._expansionCode : _controller.GetMostRecentExpansion());
	}

	public void SelectBadgeByExpansionCode(CollationMapping expansionCode)
	{
		UpdateMeters(_setBadges?.FirstOrDefault((SetBadge setBadge) => setBadge._expansionCode == expansionCode));
	}

	private void BICollectionViewFilter(string filter, string set, TimeSpan durationOnSetting)
	{
		CollectionViewFilter payload = new CollectionViewFilter
		{
			EventTime = DateTime.Now,
			Filter = filter,
			SetCode = set,
			Duration = durationOnSetting
		};
		_biLogger.Send(ClientBusinessEventType.CollectionViewFilter, payload);
		_biCollectionViewFilterStartTime = DateTime.Now;
	}

	private void BICollectionViewNavigate(TimeSpan durationOnSetting)
	{
		CollectionViewNavigate payload = new CollectionViewNavigate
		{
			EventTime = DateTime.Now,
			Duration = durationOnSetting
		};
		_biLogger.Send(ClientBusinessEventType.CollectionViewNavigate, payload);
	}

	private void OnDisable()
	{
		BICollectionViewFilter(_biActiveFilter, (_selectedBadge == null) ? "None" : _selectedBadge._expansionCode.ToString(), DateTime.Now - _biCollectionViewFilterStartTime);
		BICollectionViewNavigate(DateTime.Now - _biCollectionNavigateStartTime);
	}

	private void OnDestroy()
	{
		_storeButton.OnClick.RemoveListener(MoveToStore);
	}
}
