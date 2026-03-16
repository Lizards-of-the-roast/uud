using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using UnityEngine;
using Wizards.MDN;
using Wizards.MDN.Services.Models.Event;
using Wizards.Mtga.Decks;

namespace Wizards.Mtga.PlayBlade;

public class LastPlayedBladeContentView : BladeContentView
{
	[Header("Scaffolding")]
	[SerializeField]
	private Transform _LastPlayedContainer;

	[Header("Prefabs")]
	[SerializeField]
	private LastGamePlayedTile _LastPlayedPrefab;

	private LastPlayedBladeData _data;

	private List<RecentlyPlayedInfo> _models;

	private readonly List<LastGamePlayedTile> _tiles = new List<LastGamePlayedTile>();

	private BladeSelectionInfo _selectionInfo;

	public override BladeType Type => BladeType.LastPlayed;

	protected override void OnAwakeCalled()
	{
	}

	protected override void OnDestroyCalled()
	{
		Cleanup();
		_data = null;
	}

	public override void OnSelectionInfoUpdated(BladeSelectionInfo selectionInfo)
	{
		_selectionInfo = selectionInfo;
	}

	public override void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem)
	{
		_data = new LastPlayedBladeData(model);
		_selectionInfo = selectionInfo;
		int num = _data.RecentlyPlayed.Count - 1;
		int count = ((num > 0) ? num : 0);
		List<RecentlyPlayedInfo> list = _data.RecentlyPlayed.Take(count).ToList();
		if (list == _models)
		{
			return;
		}
		Cleanup();
		_models = list;
		foreach (RecentlyPlayedInfo model2 in _models)
		{
			_tiles.Add(Hydrate(model2, _LastPlayedContainer, assetLookupSystem));
		}
	}

	private LastGamePlayedTile Hydrate(RecentlyPlayedInfo recentlyPlayedInfo, Transform parent, AssetLookupSystem assetLookupSystem)
	{
		LastGamePlayedTile component = base.UnityObjectPool.PopObject(_LastPlayedPrefab.gameObject, parent).GetComponent<LastGamePlayedTile>();
		component.transform.localScale = Vector3.one;
		component.Initialize();
		component.SetModel(recentlyPlayedInfo, assetLookupSystem);
		component.Inject(null, OnDeckClicked, null, OnPlayButtonClicked);
		return component;
	}

	private void OnPlayButtonClicked(RecentlyPlayedInfo recentlyPlayedInfo)
	{
		if (recentlyPlayedInfo.EventInfo == null)
		{
			return;
		}
		if (recentlyPlayedInfo.IsQueueEvent)
		{
			if (recentlyPlayedInfo.SelectedDeckInfo != null)
			{
				base.Signals.JoinMatchSignal.Dispatch(new EnterMatchMakingSignalArgs(this, recentlyPlayedInfo.EventInfo.EventName, recentlyPlayedInfo.SelectedDeckInfo.deckId));
			}
		}
		else
		{
			base.Signals.GoToEventPageSignal.Dispatch(new GoToEventPageSignalArgs(this, recentlyPlayedInfo.EventInfo.EventName));
		}
	}

	private void OnDeckClicked(DeckViewInfo selectedDeck, RecentlyPlayedInfo recentlyPlayedInfo)
	{
		if (selectedDeck != null)
		{
			BladeQueueInfo bladeQueueInfoByEvent = _data.Queues.GetBladeQueueInfoByEvent(recentlyPlayedInfo.EventInfo);
			if (bladeQueueInfoByEvent != null)
			{
				_selectionInfo.FindMatchInfo.SelectedQueueType = bladeQueueInfoByEvent.PlayBladeQueueType;
				_selectionInfo.FindMatchInfo.SelectedBladeQueueInfo = bladeQueueInfoByEvent;
				_selectionInfo.FindMatchInfo.UseBO3 = recentlyPlayedInfo.EventInfo.WinCondition == MatchWinCondition.BestOf3;
				_selectionInfo.FindMatchInfo.SelectedDeckId = selectedDeck.deckId;
				base.Signals.SelectTabSignal.Dispatch(new SelectTabSignalArgs(this, BladeType.FindMatch));
			}
			else
			{
				EventContext eventContext = WrapperController.Instance.EventManager.GetEventContext(recentlyPlayedInfo.EventInfo.EventName);
				WrapperController.Instance.SceneLoader.GoToEventScreen(eventContext, reloadIfAlreadyLoaded: false, SceneLoader.NavMethod.PlayButton);
			}
		}
	}

	private void Cleanup()
	{
		foreach (LastGamePlayedTile tile in _tiles)
		{
			if ((bool)tile)
			{
				base.UnityObjectPool.PushObject(tile.gameObject);
			}
		}
		_tiles.Clear();
	}
}
