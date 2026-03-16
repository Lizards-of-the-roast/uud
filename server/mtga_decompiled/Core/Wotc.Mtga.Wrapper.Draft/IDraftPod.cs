using System;
using System.Collections;
using System.Collections.Generic;
using Wizards.Unification.Models.Draft;

namespace Wotc.Mtga.Wrapper.Draft;

public interface IDraftPod
{
	string InternalEventName { get; }

	DraftModes DraftMode { get; }

	string DraftId { get; }

	DraftState DraftState { get; }

	float PickSecondsRemaining { get; }

	float PickSecondsTotal { get; }

	int PickNumCardsToTake { get; }

	IReadOnlyList<int> SuggestedCards { get; }

	Action<PickInfo> OnDraftPacksUpdated { get; set; }

	Action<TableInfo> OnDraftTableInfoReset { get; set; }

	Action<DynamicDraftStateVisualData> OnDraftHeadersUpdated { get; set; }

	Action OnDraftFinalized { get; set; }

	Action<List<int>, Dictionary<uint, string>> OnPickedCardsUpdated { get; set; }

	StaticDraftStateVisualData InitialDraftStateVisualData { get; }

	void Cleanup();

	IEnumerator GetDraftStatus(Action<bool> onComplete);

	IEnumerator MakePick(List<int> grpIds, bool autoPicked, Action<bool> onComplete);

	IEnumerator ReserveCards(List<int> grpIds, Action<bool> onComplete);

	IEnumerator ClearReservedCards(Action<bool> onComplete);

	IEnumerator GetTableVisualData(Action<DynamicDraftStateVisualData, BustVisualData[], PlayerBoosterVisualData[]> onSuccess, Action<string> onFail);
}
