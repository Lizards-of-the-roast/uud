using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Promises;
using UnityEngine;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.PlayBlade;

public class EventBladeContentView : BladeContentView
{
	[Header("Scaffolds")]
	[SerializeField]
	private RectTransform _eventTileContainer;

	[Header("Prefabs")]
	[SerializeField]
	private PlayBladeEventTile EventTilePrefab;

	[SerializeField]
	private Localize _titleKey;

	[SerializeField]
	private Localize _subTitleKey;

	private EventBladeData _data;

	private BladeSelectionInfo _selectionInfo;

	private ViewedEventsDataProvider _viewedEventsDataProvider;

	private List<PlayBladeEventTile> _views = new List<PlayBladeEventTile>();

	private IColorChallengeStrategy _strategy;

	private static readonly int AttractHash = Animator.StringToHash("Attract");

	public override BladeType Type => BladeType.Event;

	private EventSelectionInfo CurrentSelection => _selectionInfo?.EventInfo;

	protected override void OnAwakeCalled()
	{
	}

	protected override void OnDestroyCalled()
	{
		CleanUp();
		_data = null;
		_views = null;
	}

	public override void OnSelectionInfoUpdated(BladeSelectionInfo selectionInfo)
	{
		CleanUp();
		_selectionInfo = selectionInfo;
		UpdateViewFromSelection();
	}

	public override void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem)
	{
		CleanUp();
		_data = new EventBladeData(model);
		_selectionInfo = selectionInfo;
		_viewedEventsDataProvider = Pantry.Get<ViewedEventsDataProvider>();
		_strategy = Pantry.Get<IColorChallengeStrategy>();
		UpdateViewFromSelection();
	}

	private void UpdateViewFromSelection()
	{
		BladeEventFilter filter = _selectionInfo?.EventInfo?.SelectedFilter ?? _data.FiltersList[0];
		List<BladeEventInfo> filteredEvents = _data.GetFilteredEvents(filter);
		UpdateTitleText(_selectionInfo?.EventInfo?.SelectedFilter);
		_viewedEventsDataProvider.AddViewedEvents(filteredEvents.Select((BladeEventInfo evt) => evt.EventName));
		foreach (BladeEventInfo item in filteredEvents)
		{
			PlayBladeEventTile playBladeEventTile = CreateTile(item, _eventTileContainer, _strategy);
			if (item.IsInProgress)
			{
				Animator component = playBladeEventTile.GetComponent<Animator>();
				if (component.isActiveAndEnabled)
				{
					component.SetBool(AttractHash, item.IsInProgress);
				}
				else
				{
					Debug.LogError("Can't find event tile animator for event: " + item.EventName);
				}
			}
			_views.Add(playBladeEventTile);
		}
	}

	private void UpdateTitleText(BladeEventFilter filter)
	{
		_titleKey.SetText(filter.LocTitle);
		_subTitleKey.SetText((!string.IsNullOrEmpty(filter.LocSubtitle)) ? filter.LocSubtitle : "EMPTY_NO_SPACE");
	}

	public override void Show()
	{
		OnUpdate = (Action)Delegate.Combine(OnUpdate, new Action(UpdateTimers));
		base.Show();
	}

	public override void Hide()
	{
		OnUpdate = (Action)Delegate.Remove(OnUpdate, new Action(UpdateTimers));
		base.Hide();
	}

	private void UpdateTimers()
	{
		DateTime utcNow = DateTime.UtcNow;
		foreach (PlayBladeEventTile view in _views)
		{
			if ((bool)view)
			{
				view.UpdateTimerText(utcNow);
			}
		}
	}

	private PlayBladeEventTile CreateTile(BladeEventInfo bladeEventInfo, RectTransform parent, IColorChallengeStrategy strategy)
	{
		PlayBladeEventTile view = base.UnityObjectPool.PopObject(EventTilePrefab.gameObject, parent).GetComponent<PlayBladeEventTile>();
		view.gameObject.transform.localScale = Vector3.one;
		view.SetModel(bladeEventInfo, _strategy);
		view.SetOnClick(OnTileClicked);
		_viewedEventsDataProvider.HasBeenViewed(bladeEventInfo.EventName).ThenOnMainThread(delegate(bool viewed)
		{
			view.ShowNewEventFlag(!viewed);
		});
		return view;
	}

	private void OnTileClicked(BladeEventInfo bladeEventInfo)
	{
		base.Signals.GoToEventPageSignal.Dispatch(new GoToEventPageSignalArgs(this, bladeEventInfo.EventName));
	}

	private void CleanUp()
	{
		foreach (PlayBladeEventTile view in _views)
		{
			if ((bool)view)
			{
				base.UnityObjectPool.PushObject(view.gameObject);
			}
		}
		_views.Clear();
	}
}
