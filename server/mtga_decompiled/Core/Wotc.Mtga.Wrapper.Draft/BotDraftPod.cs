using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.Shared;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Draft;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Wrapper.Draft;

public class BotDraftPod : IDraftPod
{
	private int _currentPack;

	private int _currentPick;

	private List<int> _currentPackCards = new List<int>();

	private int _numCardsToPick = 1;

	private readonly CollationMapping[] _emptyBoosters = Array.Empty<CollationMapping>();

	public const string DISABLED_AVATAR_ID = "**";

	private readonly IEventsServiceWrapper _eventsServiceWrapper;

	private CollationMapping[] _boosterCollation;

	public DraftModes DraftMode => DraftModes.BotDraft;

	public string InternalEventName { get; private set; }

	public string DraftId { get; private set; }

	public DraftState DraftState { get; private set; } = DraftState.Picking;

	public float PickSecondsRemaining => 0f;

	public float PickSecondsTotal => 0f;

	public int PickNumCardsToTake => _numCardsToPick;

	public Action<PickInfo> OnDraftPacksUpdated { get; set; }

	public Action<TableInfo> OnDraftTableInfoReset { get; set; }

	public Action<DynamicDraftStateVisualData> OnDraftHeadersUpdated { get; set; }

	public Action OnDraftFinalized { get; set; }

	public StaticDraftStateVisualData InitialDraftStateVisualData { get; private set; }

	public IReadOnlyList<int> SuggestedCards => _currentPackCards.Take(_numCardsToPick).ToArray();

	public List<int> PickedCards { get; private set; }

	public Action<List<int>, Dictionary<uint, string>> OnPickedCardsUpdated { get; set; }

	public BotDraftPod(IEventsServiceWrapper eventsServiceWrapper)
	{
		_eventsServiceWrapper = eventsServiceWrapper;
	}

	public void SetDraftState(string internalEventName, string draftId, CollationMapping[] boosterCollation, string displayName, string localSeatAvatar)
	{
		InternalEventName = internalEventName;
		DraftId = draftId;
		_boosterCollation = boosterCollation;
		InitialDraftStateVisualData = new StaticDraftStateVisualData(isInteractable: false, displayName, localSeatAvatar, "**", "**");
	}

	public IEnumerator GetDraftStatus(Action<bool> onComplete)
	{
		Promise<ClientBotDraftResponse> draftPromise = _eventsServiceWrapper.GetBotDraftStatus(InternalEventName);
		yield return draftPromise.AsCoroutine();
		if (draftPromise.Successful && draftPromise.Result.result == Wizards.Mtga.FrontDoorModels.BotDraftCmdResult.Success)
		{
			List<int> list = new List<int>();
			foreach (string pickedCard in draftPromise.Result.pickedCards)
			{
				list.Add(int.Parse(pickedCard));
			}
			Dictionary<uint, string> arg = CosmeticsUtils.StylesStringToDictionary(draftPromise.Result.pickedStyles);
			OnPickedCardsUpdated?.Invoke(list, arg);
			UpdateDraftStatus(draftPromise.Result);
		}
		onComplete?.Invoke(draftPromise.Successful);
	}

	public IEnumerator MakePick(List<int> grpIds, bool autoPicked, Action<bool> onComplete)
	{
		OnDraftHeadersUpdated?.Invoke(CreateUpdateVisualData(preparePick: true));
		Promise<ClientBotDraftResponse> pickPromise = _eventsServiceWrapper.MakeBotDraftPick(InternalEventName, grpIds, _currentPack, _currentPick);
		yield return pickPromise.AsCoroutine();
		onComplete?.Invoke(pickPromise.Successful && pickPromise.Result.result == Wizards.Mtga.FrontDoorModels.BotDraftCmdResult.Success);
		if (pickPromise.Successful && pickPromise.Result.result == Wizards.Mtga.FrontDoorModels.BotDraftCmdResult.Success)
		{
			UpdateDraftStatus(pickPromise.Result);
		}
	}

	public IEnumerator ReserveCards(List<int> grpIds, Action<bool> onComplete)
	{
		yield break;
	}

	public IEnumerator ClearReservedCards(Action<bool> onComplete)
	{
		yield break;
	}

	public IEnumerator GetTableVisualData(Action<DynamicDraftStateVisualData, BustVisualData[], PlayerBoosterVisualData[]> onSuccess, Action<string> onFail)
	{
		onSuccess?.Invoke(CreateUpdateVisualData(), Array.Empty<BustVisualData>(), Array.Empty<PlayerBoosterVisualData>());
		yield break;
	}

	private DynamicDraftStateVisualData CreateUpdateVisualData(bool preparePick = false)
	{
		CollationMapping[] packsOnLocalSeat = ((preparePick || _boosterCollation.Length == 0) ? _emptyBoosters : new CollationMapping[1] { _boosterCollation[Mathf.Min(_currentPack, _boosterCollation.Length - 1)] });
		return new DynamicDraftStateVisualData(_currentPack != 1, (uint)(_currentPack + 1), (uint)(_currentPick + 1), _numCardsToPick, packsOnLocalSeat, _emptyBoosters, _emptyBoosters);
	}

	private void UpdateDraftStatus(ClientBotDraftResponse status)
	{
		if (status.draftStatus == Wizards.Mtga.FrontDoorModels.DraftStatus.Completed)
		{
			DraftState = DraftState.Completing;
			OnDraftFinalized?.Invoke();
			return;
		}
		DraftState = DraftState.Picking;
		_currentPack = status.packNumber;
		_currentPick = status.pickNumber;
		_currentPackCards = status.draftPack.Select(int.Parse).ToList();
		_numCardsToPick = status.numCardsToPick;
		PickInfo obj = new PickInfo
		{
			PassDirection = ((_currentPack == 1) ? EDirection.POS : EDirection.NEG),
			PackCards = _currentPackCards,
			PackStyles = status.packStyles,
			NumCardsToPick = status.numCardsToPick
		};
		OnDraftPacksUpdated?.Invoke(obj);
		OnDraftHeadersUpdated?.Invoke(CreateUpdateVisualData());
	}

	public void Cleanup()
	{
		OnDraftFinalized = null;
		OnDraftHeadersUpdated = null;
		OnDraftPacksUpdated = null;
		OnPickedCardsUpdated = null;
	}
}
