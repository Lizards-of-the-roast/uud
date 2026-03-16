using System.Linq;
using AssetLookupTree;
using UnityEngine;
using Wizards.MDN;
using Wizards.MDN.Services.Models.Event;
using Wizards.Mtga.Decks;

namespace Wizards.Mtga.PlayBlade;

public class LastPlayedBladeView : BladeView
{
	[Header("Scaffolding")]
	[SerializeField]
	private RectTransform _LastPlayedContainer;

	[Header("Prefabs")]
	[SerializeField]
	private LastGamePlayedTile _LastPlayedPrefab;

	private LastPlayedBladeData _data;

	private LastGamePlayedTile _lastPlayedTile;

	private BladeSelectionInfo _selectionInfo;

	public override BladeType Type => BladeType.LastPlayed;

	private void OnDestroy()
	{
		CleanupTile();
		_data = null;
	}

	public override void OnSelectionInfoUpdated(BladeSelectionInfo current)
	{
		_selectionInfo = current;
	}

	public override void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem)
	{
		_data = new LastPlayedBladeData(model);
		_selectionInfo = selectionInfo;
		RecentlyPlayedInfo recentlyPlayedInfo = _data.RecentlyPlayed.LastOrDefault();
		if (recentlyPlayedInfo != null)
		{
			CleanupTile();
			Hydrate(recentlyPlayedInfo, _LastPlayedContainer, assetLookupSystem);
		}
	}

	private void OnPlayButtonClicked(RecentlyPlayedInfo selectedInfo)
	{
		if (selectedInfo.EventInfo == null)
		{
			return;
		}
		if (selectedInfo.IsQueueEvent)
		{
			if (selectedInfo.SelectedDeckInfo != null)
			{
				base.Signals.JoinMatchSignal.Dispatch(new EnterMatchMakingSignalArgs(this, selectedInfo.EventInfo.EventName, selectedInfo.SelectedDeckInfo.deckId));
			}
		}
		else
		{
			base.Signals.GoToEventPageSignal.Dispatch(new GoToEventPageSignalArgs(this, selectedInfo.EventInfo.EventName));
		}
	}

	private void Hydrate(RecentlyPlayedInfo recentlyPlayedInfo, Transform parent, AssetLookupSystem assetLookupSystem)
	{
		_lastPlayedTile = base.UnityObjectPool.PopObject(_LastPlayedPrefab.gameObject, parent).GetComponent<LastGamePlayedTile>();
		RectTransform component = _lastPlayedTile.GetComponent<RectTransform>();
		component.localPosition = Vector3.zero;
		component.localScale = Vector3.one;
		component.offsetMax = Vector2.zero;
		component.offsetMin = Vector2.zero;
		_lastPlayedTile.Initialize();
		_lastPlayedTile.SetModel(recentlyPlayedInfo, assetLookupSystem);
		_lastPlayedTile.Inject(null, OnDeckClicked, OnPlayButtonClicked, null);
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

	public void OnDismissClicked()
	{
		base.Signals.ClosePlayBladeSignal.Dispatch(new ClosePlayBladeSignalArgs(this));
	}

	private void CleanupTile()
	{
		if ((bool)_lastPlayedTile)
		{
			base.UnityObjectPool.PushObject(_lastPlayedTile.gameObject);
		}
		_lastPlayedTile = null;
	}
}
