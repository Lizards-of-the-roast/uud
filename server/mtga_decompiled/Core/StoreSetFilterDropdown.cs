using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Data;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Card;
using cTMP;

public class StoreSetFilterDropdown : MonoBehaviour
{
	[SerializeField]
	private StoreSetFilterDropdownItem _dropdownItemPrefab;

	[SerializeField]
	private GameObject _historicHeaderPrefab;

	[SerializeField]
	private Button _dropdownButton;

	[SerializeField]
	private StoreSetFilterDropdownItem _buttonDisplay;

	[SerializeField]
	private Transform _scrollRectContent;

	[SerializeField]
	private CanvasGroup _scrollRectCanvasGroup;

	[SerializeField]
	private GameObject _blocker;

	[SerializeField]
	private float _fadeDuration = 0.2f;

	private TweenRunner<FloatTween> _fadeTweenRunner;

	private StoreSetFilterToggles _setToggles;

	private bool _showingDropdown;

	private AssetLookupSystem _assetLookupSystem;

	public void Initialize(AssetLookupSystem assetLookupSystem, StoreSetFilterToggles setToggles, List<StoreSetFilterModel> setFilters)
	{
		_fadeTweenRunner = new TweenRunner<FloatTween>();
		_fadeTweenRunner.Init(this);
		_setToggles = setToggles;
		_assetLookupSystem = assetLookupSystem;
		bool flag = false;
		foreach (StoreSetFilterModel setFilter in setFilters)
		{
			bool flag2 = setFilter.Availability == SetAvailability.EternalOnly || setFilter.Availability == SetAvailability.HistoricOnly;
			if (!flag && flag2)
			{
				UnityEngine.Object.Instantiate(_historicHeaderPrefab, _scrollRectContent);
				flag = true;
			}
			StoreSetFilterDropdownItem storeSetFilterDropdownItem = UnityEngine.Object.Instantiate(_dropdownItemPrefab, _scrollRectContent);
			storeSetFilterDropdownItem.Initialize(_assetLookupSystem, setFilter);
			storeSetFilterDropdownItem.OnClicked = (Action<StoreSetFilterModel>)Delegate.Combine(storeSetFilterDropdownItem.OnClicked, new Action<StoreSetFilterModel>(OnDropdownItemClicked));
		}
		_dropdownButton.onClick.AddListener(OnDropdownButtonClicked);
	}

	public void UpdateButtonDisplay(StoreSetFilterModel selectedModel)
	{
		_buttonDisplay.Initialize(_assetLookupSystem, selectedModel);
	}

	public void OnDropdownButtonClicked()
	{
		ToggleScrollRect(!_showingDropdown);
	}

	public void ToggleScrollRect(bool enabled)
	{
		_showingDropdown = enabled;
		float end = (enabled ? 1f : 0f);
		float alpha = _scrollRectCanvasGroup.alpha;
		FadeScrollRect(alpha, end);
		_scrollRectCanvasGroup.interactable = enabled;
		_scrollRectCanvasGroup.blocksRaycasts = enabled;
		_blocker.SetActive(enabled);
	}

	private void OnDropdownItemClicked(StoreSetFilterModel model)
	{
		_setToggles.OnValueSelected(model);
	}

	private void FadeScrollRect(float start, float end)
	{
		if (!end.Equals(start))
		{
			FloatTween info = new FloatTween
			{
				duration = _fadeDuration,
				startValue = start,
				targetValue = end
			};
			info.AddOnChangedCallback(delegate(float alpha)
			{
				_scrollRectCanvasGroup.alpha = alpha;
			});
			info.ignoreTimeScale = true;
			_fadeTweenRunner.StartTween(info);
		}
	}
}
