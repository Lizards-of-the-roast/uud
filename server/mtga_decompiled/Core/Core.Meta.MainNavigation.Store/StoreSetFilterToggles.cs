using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Meta.MainNavigation.Store.Data;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Mtga.Utils;

namespace Core.Meta.MainNavigation.Store;

public class StoreSetFilterToggles : MonoBehaviour
{
	private List<StoreSetFilterModel> _setFilters = new List<StoreSetFilterModel>();

	private Dictionary<StoreSetFilterModel, StoreSetFilterToggle> _setFilterViews = new Dictionary<StoreSetFilterModel, StoreSetFilterToggle>();

	[SerializeField]
	private StoreSetFilterToggle _togglePrefab;

	[SerializeField]
	private Transform container;

	private AssetLookupSystem _assetLookupSystem;

	private ISetMetadataProvider _filterDataProvider;

	private Action<StoreSetFilterModel> _onSetRefreshed;

	public StoreSetFilterModel SelectedModel { get; private set; }

	public int SelectedIndex
	{
		get
		{
			if (SelectedModel == null)
			{
				return 0;
			}
			return _setFilters.IndexOf(SelectedModel);
		}
	}

	public void Init(AssetLookupSystem assetLookupSystem, Action<StoreSetFilterModel> onSetRefreshed)
	{
		SelectedModel = null;
		_assetLookupSystem = assetLookupSystem;
		_onSetRefreshed = onSetRefreshed;
		GameObjectCore.DestroyComponents(_setFilterViews.Values);
		_setFilterViews.Clear();
	}

	public void SetSetFilters(List<StoreSetFilterModel> setFilters)
	{
		Reset();
		_setFilters = setFilters;
		foreach (StoreSetFilterModel setFilter in _setFilters)
		{
			StoreSetFilterToggle storeSetFilterToggle = UnityEngine.Object.Instantiate(_togglePrefab, container);
			_setFilterViews.Add(setFilter, storeSetFilterToggle);
			storeSetFilterToggle.Initialize(_assetLookupSystem, setFilter, OnValueSelected, setFilter.Availability);
		}
	}

	private void Reset()
	{
		foreach (KeyValuePair<StoreSetFilterModel, StoreSetFilterToggle> setFilterView in _setFilterViews)
		{
			UnityEngine.Object.Destroy(setFilterView.Value.gameObject);
		}
		_setFilterViews.Clear();
	}

	public void OnValueSelected(StoreSetFilterModel model)
	{
		if (model != SelectedModel)
		{
			Select(model);
			_onSetRefreshed?.Invoke(model);
		}
	}

	public void Clicked_PrevSetRefresh()
	{
		playAudio();
		int selectedIndex = SelectedIndex;
		selectedIndex = ((selectedIndex == 0) ? (_setFilters.Count - 1) : (selectedIndex - 1));
		StoreSetFilterModel storeSetFilterModel = _setFilters[selectedIndex];
		Select(storeSetFilterModel);
		_onSetRefreshed?.Invoke(storeSetFilterModel);
	}

	public void Clicked_NextSetRefresh()
	{
		playAudio();
		int index = (SelectedIndex + 1) % _setFilters.Count;
		StoreSetFilterModel storeSetFilterModel = _setFilters[index];
		Select(storeSetFilterModel);
		_onSetRefreshed?.Invoke(storeSetFilterModel);
	}

	private void playAudio()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void Select(int index)
	{
		StoreSetFilterModel model = _setFilters[index];
		Select(model);
	}

	public void SelectForGivenSet(string setName)
	{
		StoreSetFilterModel model = _setFilters.Where((StoreSetFilterModel x) => x.HashSetSet.Contains(setName)).First();
		Select(model);
	}

	public void Select(StoreSetFilterModel model)
	{
		if (model == null)
		{
			return;
		}
		if (SelectedModel != null)
		{
			if (SelectedModel == model)
			{
				return;
			}
			ViewFor(SelectedModel).Deselect();
		}
		SelectedModel = model;
		ViewFor(model).Select();
	}

	public void RefreshCurrentlySelected()
	{
		ViewFor(SelectedModel).ForceRefresh();
	}

	private StoreSetFilterToggle ViewFor(StoreSetFilterModel model)
	{
		return _setFilterViews[model];
	}

	public void CleanUp()
	{
		foreach (StoreSetFilterToggle value in _setFilterViews.Values)
		{
			value.CleanUp();
		}
	}
}
