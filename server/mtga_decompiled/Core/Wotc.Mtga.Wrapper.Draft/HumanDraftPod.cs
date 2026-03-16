using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.Shared;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Unification.Models.Draft;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Wrapper.Draft;

public class HumanDraftPod : IDraftPod
{
	private readonly IEventsServiceWrapper _eventsServiceWrapper;

	private readonly Wizards.Arena.Client.Logging.ILogger _logger;

	private readonly IBILogger _biLogger;

	private readonly string _eventId;

	private TableInfo _currentTableInfo;

	private PickInfo _currentPickInfo;

	private PackInfo _currentPackInfo;

	private DateTime _pickInfoTimeout;

	public DraftModes DraftMode => DraftModes.HumanDraft;

	public string DraftId { get; private set; }

	public DraftState DraftState { get; set; }

	public float PickSecondsRemaining => (float)_pickInfoTimeout.Subtract(DateTime.UtcNow).TotalSeconds;

	public float PickSecondsTotal { get; private set; }

	public int PickNumCardsToTake => Math.Max(1, _currentPickInfo?.NumCardsToPick ?? 1);

	public Action<PickInfo> OnDraftPacksUpdated { get; set; }

	public Action<TableInfo> OnDraftTableInfoReset { get; set; }

	public Action<DynamicDraftStateVisualData> OnDraftHeadersUpdated { get; set; }

	public Action OnDraftFinalized { get; set; }

	public Action<List<int>, Dictionary<uint, string>> OnPickedCardsUpdated { get; set; }

	public string InternalEventName { get; private set; }

	public StaticDraftStateVisualData InitialDraftStateVisualData { get; private set; }

	public IReadOnlyList<int> SuggestedCards
	{
		get
		{
			IReadOnlyList<int> readOnlyList = _currentPickInfo?.SuggestedCards;
			return readOnlyList ?? Array.Empty<int>();
		}
	}

	public HumanDraftPod(IEventsServiceWrapper eventsServiceWrapper, Wizards.Arena.Client.Logging.ILogger logger, IBILogger biLogger, string eventId, string draftId = null)
	{
		_eventsServiceWrapper = eventsServiceWrapper;
		_logger = logger;
		_biLogger = biLogger;
		eventsServiceWrapper.AddDraftNotificationCallback(OnMsg_DraftNotification);
		_eventId = eventId;
		DraftState = DraftState.Podmaking;
		DraftId = draftId;
	}

	public void Cleanup()
	{
		OnDraftFinalized = null;
		OnDraftHeadersUpdated = null;
		OnDraftPacksUpdated = null;
		OnPickedCardsUpdated = null;
		_currentPickInfo = null;
		_currentPackInfo = null;
		_currentTableInfo = null;
		_eventsServiceWrapper.RemoveDraftNotificationCallback(OnMsg_DraftNotification);
	}

	public IEnumerator GetDraftStatus(Action<bool> onComplete)
	{
		while (_currentTableInfo == null)
		{
			yield return null;
		}
		if (_currentPickInfo != null && _currentPackInfo != null)
		{
			Dictionary<uint, string> arg = CosmeticsUtils.StylesStringToDictionary(_currentTableInfo.PickedStyles);
			OnPickedCardsUpdated?.Invoke(_currentTableInfo.PickedCards, arg);
			OnDraftPacksUpdated?.Invoke(_currentPickInfo);
			OnDraftHeadersUpdated?.Invoke(new DynamicDraftStateVisualData(_currentPickInfo.PassDirection == EDirection.NEG, (uint)_currentPickInfo.SelfPack, (uint)_currentPickInfo.SelfPick, _currentPickInfo.NumCardsToPick, GetCollationMappingArray(_currentPackInfo.SelfPacks), GetCollationMappingArray(_currentPackInfo.NegNeighborPacks), GetCollationMappingArray(_currentPackInfo.PosNeighborPacks)));
		}
		onComplete?.Invoke(_currentPickInfo != null && _currentPackInfo != null);
	}

	public IEnumerator MakePick(List<int> grpIds, bool autoPicked, Action<bool> onComplete)
	{
		int curPack = _currentPickInfo.SelfPack;
		int curPick = _currentPickInfo.SelfPick;
		List<int> pack = _currentPickInfo.PackCards;
		Promise<PickResponse> pickPromise = _eventsServiceWrapper.MakeHumanDraftPick(DraftId, grpIds, curPack, curPick);
		yield return pickPromise.AsCoroutine();
		if (!pickPromise.Successful || pickPromise.Result == null)
		{
			onComplete?.Invoke(obj: false);
			yield break;
		}
		IEnumerable<uint> enumerable = grpIds.Select((int grpId) => (uint)grpId);
		if (pickPromise.Result.IsPickSuccessful)
		{
			foreach (uint item in enumerable)
			{
				ClientPlayerDraftPickMade payload = new ClientPlayerDraftPickMade
				{
					DraftId = DraftId,
					EventId = _eventId,
					EventTime = DateTime.UtcNow,
					SeatNumber = _currentTableInfo?.SelfSeat,
					PackNumber = curPack,
					PickNumber = curPick,
					PickGrpId = item,
					CardsInPack = pack.ToArray(),
					TimeRemainingOnPick = PickSecondsRemaining,
					AutoPick = autoPicked
				};
				_biLogger.Send(ClientBusinessEventType.PlayerDraftPickMade, payload);
			}
		}
		if (!pickPromise.Result.IsPickSuccessful)
		{
			foreach (uint item2 in enumerable)
			{
				ClientPlayerDraftPickUnsuccessful payload2 = new ClientPlayerDraftPickUnsuccessful
				{
					DraftId = DraftId,
					EventId = _eventId,
					EventTime = DateTime.UtcNow,
					SeatNumber = _currentTableInfo?.SelfSeat,
					PackNumber = curPack,
					PickNumber = curPick,
					PickGrpId = item2,
					CardsInPack = pack.ToArray(),
					TimeRemainingOnPick = PickSecondsRemaining,
					AutoPick = autoPicked
				};
				_biLogger.Send(ClientBusinessEventType.PlayerDraftPickUnsuccessful, payload2);
			}
			ResetTableInfo(pickPromise.Result.TableInfo, pickPromise.Result.PickInfo, pickPromise.Result.PackInfo);
			Dictionary<uint, string> arg = CosmeticsUtils.StylesStringToDictionary(pickPromise.Result.TableInfo.PickedStyles);
			OnPickedCardsUpdated?.Invoke(pickPromise.Result.TableInfo.PickedCards, arg);
		}
		onComplete?.Invoke(pickPromise.Result.IsPickSuccessful);
		if (pickPromise.Result.IsPickingCompleted)
		{
			OnDraftFinalized?.Invoke();
		}
	}

	public IEnumerator ReserveCards(List<int> grpIds, Action<bool> onComplete)
	{
		int selfPack = _currentPickInfo.SelfPack;
		int selfPick = _currentPickInfo.SelfPick;
		Promise<PickResponse> pickPromise = _eventsServiceWrapper.PlayerDraftReserveCard(DraftId, grpIds, selfPack, selfPick);
		yield return pickPromise.AsCoroutine();
		if (!pickPromise.Successful || pickPromise.Result == null)
		{
			onComplete?.Invoke(obj: false);
		}
	}

	public IEnumerator ClearReservedCards(Action<bool> onComplete)
	{
		Promise<PickResponse> pickPromise = _eventsServiceWrapper.PlayerDraftClearReservedCards(DraftId);
		yield return pickPromise.AsCoroutine();
		if (!pickPromise.Successful || pickPromise.Result == null)
		{
			onComplete?.Invoke(obj: false);
		}
	}

	public IEnumerator GetTableVisualData(Action<DynamicDraftStateVisualData, BustVisualData[], PlayerBoosterVisualData[]> onSuccess, Action<string> onFail)
	{
		Promise<TablePacksResponse> packsPromise = _eventsServiceWrapper.GetHumanDraftTablePacks(DraftId);
		yield return packsPromise.AsCoroutine();
		if (!packsPromise.Successful)
		{
			onFail($"GetTableVisualData: GetHumanDraftTablePacks error: {packsPromise.Error.Message}: {packsPromise.Error.Exception}");
			yield break;
		}
		if (packsPromise.Result?.PacksBySeat == null)
		{
			onFail("GetTableVisualData: GetHumanDraftTablePacks error: PacksBySeat was null");
			yield break;
		}
		List<List<int>> packsBySeat = packsPromise.Result.PacksBySeat;
		PlayerBoosterVisualData[] array = new PlayerBoosterVisualData[packsBySeat.Count];
		for (int i = 0; i < packsBySeat.Count; i++)
		{
			CollationMapping[] collationMappingArray = GetCollationMappingArray(packsBySeat[i]);
			int num = Wrap(i - _currentTableInfo.SelfSeat, _currentTableInfo.Players.Count);
			array[num] = new PlayerBoosterVisualData(collationMappingArray);
		}
		BustVisualData[] array2 = new BustVisualData[_currentTableInfo.Players.Count];
		for (int j = 0; j < array2.Length; j++)
		{
			PlayerInfo playerInfo = _currentTableInfo.Players[j];
			int num2 = Wrap(j - _currentTableInfo.SelfSeat, _currentTableInfo.Players.Count);
			array2[num2] = new BustVisualData(playerInfo.ScreenName, playerInfo.Avatar);
		}
		DynamicDraftStateVisualData arg = new DynamicDraftStateVisualData(_currentPickInfo.PassDirection == EDirection.NEG, (uint)_currentPickInfo.SelfPack, (uint)_currentPickInfo.SelfPick, _currentPickInfo.NumCardsToPick, GetCollationMappingArray(_currentPackInfo.SelfPacks), GetCollationMappingArray(_currentPackInfo.NegNeighborPacks), GetCollationMappingArray(_currentPackInfo.PosNeighborPacks));
		onSuccess?.Invoke(arg, array2, array);
	}

	private void ResetTableInfo(TableInfo tableInfo, PickInfo pickInfo, PackInfo packInfo)
	{
		ProcessPickAndPacks(pickInfo, packInfo);
		ProcessTableInfo(tableInfo);
	}

	private void ProcessTableInfo(TableInfo tableInfo)
	{
		if (tableInfo != null)
		{
			_currentTableInfo = tableInfo;
			int selfSeat = _currentTableInfo.SelfSeat;
			int index = Wrap(selfSeat - 1, _currentTableInfo.Players.Count);
			int index2 = Wrap(selfSeat + 1, _currentTableInfo.Players.Count);
			PlayerInfo playerInfo = _currentTableInfo.Players[selfSeat];
			PlayerInfo playerInfo2 = _currentTableInfo.Players[index];
			PlayerInfo playerInfo3 = _currentTableInfo.Players[index2];
			InitialDraftStateVisualData = new StaticDraftStateVisualData(isInteractable: true, playerInfo.ScreenName, playerInfo.Avatar, playerInfo2.Avatar, playerInfo3.Avatar);
			OnDraftTableInfoReset?.Invoke(tableInfo);
		}
	}

	private void ProcessPickAndPacks(PickInfo pickInfo, PackInfo packInfo)
	{
		if (pickInfo != null)
		{
			_currentPickInfo = pickInfo;
			PickSecondsTotal = _currentPickInfo.TimeoutSec;
			_pickInfoTimeout = DateTime.UtcNow.AddSeconds(PickSecondsTotal);
			OnDraftPacksUpdated?.Invoke(_currentPickInfo);
		}
		ProcessPackInfo(packInfo);
	}

	private void ProcessPackInfo(PackInfo packInfo)
	{
		if (packInfo != null)
		{
			_currentPackInfo = packInfo;
			if (_currentPickInfo != null)
			{
				OnDraftHeadersUpdated?.Invoke(new DynamicDraftStateVisualData(_currentPickInfo.PassDirection == EDirection.NEG, (uint)_currentPickInfo.SelfPack, (uint)_currentPickInfo.SelfPick, _currentPickInfo.NumCardsToPick, GetCollationMappingArray(_currentPackInfo.SelfPacks), GetCollationMappingArray(_currentPackInfo.NegNeighborPacks), GetCollationMappingArray(_currentPackInfo.PosNeighborPacks)));
			}
		}
	}

	private void OnMsg_DraftNotification(DraftNotification draftNotify)
	{
		if (string.IsNullOrWhiteSpace(DraftId) && draftNotify.Type != EDraftNotificationType.ReadyReq)
		{
			Debug.LogError(string.Format("{0}.{1}: Receiving draft events before DraftId has been set for draft pod: {2} {3}", "HumanDraftPod", "OnMsg_DraftNotification", _eventId, draftNotify.Type));
			return;
		}
		if (!string.IsNullOrWhiteSpace(DraftId) && DraftId != draftNotify.DraftId && draftNotify.Type != EDraftNotificationType.ReadyReq)
		{
			Debug.LogError(string.Format("{0}.{1}: Received draft notification for incorrect DraftId: Expected draftId {2}, but was {3}, {4} {5}", "HumanDraftPod", "OnMsg_DraftNotification", DraftId, draftNotify.DraftId, _eventId, draftNotify.Type));
			return;
		}
		switch (draftNotify.Type)
		{
		case EDraftNotificationType.Close:
			DraftState = DraftState.None;
			break;
		case EDraftNotificationType.ReadyReq:
			DraftId = draftNotify.DraftId;
			DraftState = DraftState.Podmaking;
			break;
		case EDraftNotificationType.ReadyUpdate:
			DraftState = DraftState.Podmaking;
			break;
		case EDraftNotificationType.TableInfo:
			DraftState = DraftState.Picking;
			ResetTableInfo(draftNotify.TableInfo, draftNotify.PickInfo, draftNotify.PackInfo);
			break;
		case EDraftNotificationType.PickReq:
			if (!string.IsNullOrWhiteSpace(DraftId))
			{
				DraftState = DraftState.Picking;
				ProcessPickAndPacks(draftNotify.PickInfo, draftNotify.PackInfo);
				JObject additionalContext = new JObject
				{
					["draftId"] = draftNotify.DraftId,
					["SelfPick"] = draftNotify.PickInfo.SelfPick,
					["SelfPack"] = draftNotify.PickInfo.SelfPack,
					["PackCards"] = string.Join(",", draftNotify.PickInfo.PackCards)
				};
				_logger.Info("Draft.Notify", additionalContext);
			}
			break;
		case EDraftNotificationType.PackInfo:
			ProcessPackInfo(draftNotify.PackInfo);
			break;
		}
	}

	private CollationMapping[] GetCollationMappingArray(List<int> boosterIds)
	{
		CollationMapping[] array = new CollationMapping[boosterIds.Count];
		for (int i = 0; i < boosterIds.Count; i++)
		{
			array[i] = (CollationMapping)boosterIds[i];
		}
		return array;
	}

	private static int Wrap(int value, int max)
	{
		int num = value % max;
		if (num >= 0)
		{
			return num;
		}
		return num + max;
	}

	public ClientPlayerDraftMismatchedPick GetMismatchedPickPayload()
	{
		return new ClientPlayerDraftMismatchedPick
		{
			DraftId = DraftId,
			EventId = _eventId,
			EventTime = DateTime.UtcNow,
			SeatNumber = _currentTableInfo?.SelfSeat,
			PackNumber = (_currentPickInfo?.SelfPack ?? (-1)),
			PickNumber = (_currentPickInfo?.SelfPack ?? (-1)),
			CardsInPack = _currentPickInfo?.PackCards?.ToArray(),
			TimeRemainingOnPick = PickSecondsRemaining
		};
	}
}
