using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Promises;
using UnityEngine;
using UnityEngine.Serialization;
using Wizards.Arena.Promises;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.PlayBlade;

public class EventBladeView : BladeView
{
	private enum UpdateInitiator
	{
		Invalid,
		TabOpened,
		SelectionChanged
	}

	private EventBladeData _data;

	private Dictionary<BladeEventFilter, BladeFilterItem> _listViewsByModel = new Dictionary<BladeEventFilter, BladeFilterItem>();

	private BladeSelectionInfo _selectionInfo;

	private ViewedEventsDataProvider _viewedEventsDataProvider;

	[SerializeField]
	private Transform _optionsContainer;

	[FormerlySerializedAs("_filterItemList")]
	[SerializeField]
	private BladeFilterItem _filterItemPrefab;

	public override BladeType Type => BladeType.Event;

	private EventSelectionInfo _currentSelection => _selectionInfo.EventInfo;

	private List<BladeFilterItem> ActiveListViews => _listViewsByModel?.Values.ToList() ?? new List<BladeFilterItem>();

	private void OnDestroy()
	{
		CleanupListItems();
		_viewedEventsDataProvider.SaveViewedEvents();
		_data = null;
	}

	private void OnApplicationQuit()
	{
		_viewedEventsDataProvider.SaveViewedEvents();
	}

	public override void OnSelectionInfoUpdated(BladeSelectionInfo current)
	{
		_selectionInfo = current;
	}

	public override void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem)
	{
		_data = new EventBladeData(model);
		_selectionInfo = selectionInfo;
		_viewedEventsDataProvider = Pantry.Get<ViewedEventsDataProvider>();
		if (_currentSelection?.SelectedFilter == null)
		{
			_selectionInfo.EventInfo.SelectedFilter = _data.FiltersList[0];
		}
		UpdateViewFromSelection();
	}

	private void DrawListItems(List<BladeEventFilter> filterInfos)
	{
		CleanupListItems();
		foreach (BladeEventFilter filterInfo in filterInfos)
		{
			BladeFilterItem item = base.UnityObjectPool.PopObject(_filterItemPrefab.gameObject, _optionsContainer).GetComponent<BladeFilterItem>();
			item.gameObject.transform.localScale = Vector3.one;
			item.SetModel(filterInfo);
			item.SetOnClick(SelectFilter);
			new SimplePromise<IReadOnlyCollection<BladeEventInfo>>(_data.GetFilteredEvents(filterInfo)).Scatter((BladeEventInfo bladeEventInfo) => _viewedEventsDataProvider.HasBeenViewed(bladeEventInfo.EventName)).Gather().ThenOnMainThread(delegate(IEnumerable<bool> hasBeenViewedItems)
			{
				bool attract = hasBeenViewedItems.Exists((bool viewed) => !viewed);
				item.SetAttract(attract);
			});
			_listViewsByModel.Add(filterInfo, item);
		}
	}

	private void CleanupListItems()
	{
		foreach (BladeFilterItem activeListView in ActiveListViews)
		{
			if ((bool)activeListView)
			{
				activeListView.Cleanup();
				base.UnityObjectPool.PushObject(activeListView.gameObject);
			}
		}
		_listViewsByModel.Clear();
	}

	private void UpdateViewFromSelection(UpdateInitiator updateInitiator = UpdateInitiator.TabOpened)
	{
		_ = _currentSelection;
		if (updateInitiator != UpdateInitiator.TabOpened)
		{
			_ = 2;
			return;
		}
		DrawListItems(_data.FiltersList);
		_listViewsByModel[_currentSelection.SelectedFilter].SetToggleValue(state: true);
	}

	public void SelectFilter(BladeEventFilter filter)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		if (filter == _currentSelection.SelectedFilter)
		{
			_listViewsByModel[_currentSelection.SelectedFilter].SetToggleValue(state: true);
			return;
		}
		if (_currentSelection?.SelectedFilter != null)
		{
			_listViewsByModel[_currentSelection.SelectedFilter].SetToggleValue(state: false);
		}
		_currentSelection.SelectedFilter = filter;
		_selectionInfo.EventInfo.SelectedFilter = filter;
		_listViewsByModel[_currentSelection.SelectedFilter].SetToggleValue(state: true);
		UpdateViewFromSelection(UpdateInitiator.SelectionChanged);
		DispatchSelectFilterSignal();
		UpdateSelection(_selectionInfo);
	}

	private void DispatchSelectFilterSignal()
	{
		BladeEventFilter selectedFilter = _currentSelection.SelectedFilter;
		base.Signals.FilterSelectedSignal.Dispatch(new FilterSelectedSignalArgs(this, selectedFilter));
	}
}
