using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Decks;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.PlayBlade;

public class FindMatchBladeView : BladeView
{
	[Serializable]
	private class BladeQueueTypePair
	{
		public PlayBladeQueueType type;

		public CustomTab tab;
	}

	private enum ChangeInitiator
	{
		INVALID = -1,
		Tab = 1,
		List = 2,
		Toggle = 3
	}

	[Header("Misc")]
	[SerializeField]
	private Localize _subHeader;

	[SerializeField]
	private Transform _optionsContainer;

	[SerializeField]
	private CustomButton _playButton;

	[SerializeField]
	private List<BladeQueueTypePair> _tabs;

	[SerializeField]
	private Image _rankImage;

	private AssetLoader.AssetTracker<Sprite> _rankImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("FindMatchBladeRankImageSprite");

	[Header("B03 Toggle")]
	[SerializeField]
	private GameObject _bo3ToggleContainer;

	[SerializeField]
	private Toggle _bo3Toggle;

	[SerializeField]
	private Animator _bo3ToggleAnimator;

	[Header("Dynamically Created Items")]
	[SerializeField]
	private BladeQueueListItem _listItemPrefab;

	private FindMatchBladeData _data;

	private BladeSelectionInfo _selectionInfo;

	private static readonly int TOGGLE_BO3_HASH = Animator.StringToHash("Toggle_Bo3");

	private readonly FindMatchSelectionInfo DEFAULT_SELECTION = new FindMatchSelectionInfo
	{
		SelectedQueueType = PlayBladeQueueType.Unranked,
		UseBO3 = false
	};

	private Dictionary<BladeQueueInfo, BladeQueueListItem> _listViewsByModel = new Dictionary<BladeQueueInfo, BladeQueueListItem>();

	public override BladeType Type => BladeType.FindMatch;

	private FindMatchSelectionInfo CurrentSelection => _selectionInfo?.FindMatchInfo ?? DEFAULT_SELECTION;

	private void Awake()
	{
		_bo3Toggle.onValueChanged.AddListener(OnBestOfToggle);
		_playButton.OnClick.AddListener(OnPlayButtonClicked);
		_playButton.Interactable = false;
		foreach (BladeQueueTypePair bladeQueueTypePair in _tabs)
		{
			bladeQueueTypePair.tab.SetClicked(delegate
			{
				SelectQueueTab(bladeQueueTypePair.type);
			});
		}
	}

	private void OnDestroy()
	{
		CleanupListItems();
		AssetLoaderUtils.CleanupImage(_rankImage, _rankImageSpriteTracker);
	}

	public override void OnSelectionInfoUpdated(BladeSelectionInfo selected)
	{
		_selectionInfo = selected;
		UpdateViewFromSelection(ChangeInitiator.Tab);
	}

	public override void SetModel(IBladeModel model, BladeSelectionInfo selectionInfo, AssetLookupSystem assetLookupSystem)
	{
		ResetTabState();
		CleanupListItems();
		_data = new FindMatchBladeData(model);
		_selectionInfo = selectionInfo;
		if (CurrentSelection.SelectedBladeQueueInfo == null)
		{
			_selectionInfo.FindMatchInfo = DEFAULT_SELECTION;
		}
		LockEmptyTabs();
		UpdateViewFromSelection(ChangeInitiator.Tab);
	}

	private void UpdateRankIcon()
	{
		string text = CurrentSelection.SelectedEvent?.RankImagePath ?? string.Empty;
		_rankImage.gameObject.UpdateActive(!string.IsNullOrEmpty(text));
		AssetLoaderUtils.TrySetSprite(_rankImage, _rankImageSpriteTracker, text);
	}

	private void LockEmptyTabs()
	{
		List<PlayBladeQueueType> list = new List<PlayBladeQueueType>();
		foreach (BladeQueueTypePair tab in _tabs)
		{
			if (!_data.Queues.TryGetValue(tab.type, out var value) || value.Count == 0)
			{
				tab.tab.SetLocked(locked: true);
				list.Add(tab.type);
			}
		}
		if (list.Contains(CurrentSelection.SelectedQueueType))
		{
			CurrentSelection.SelectedQueueType = DEFAULT_SELECTION.SelectedQueueType;
		}
	}

	private void DrawListItems(List<BladeQueueInfo> queueInfos, List<string> npeQueues)
	{
		CleanupListItems();
		bool useBO = CurrentSelection.UseBO3;
		foreach (BladeQueueInfo queueInfo in queueInfos)
		{
			if (!useBO || queueInfo.CanBestOf3)
			{
				BladeQueueListItem component = base.UnityObjectPool.PopObject(_listItemPrefab.gameObject, _optionsContainer).GetComponent<BladeQueueListItem>();
				component.gameObject.transform.localScale = Vector3.one;
				component.SetModel(queueInfo);
				component.SetQuestBadgeDisplayed(npeQueues.Contains(queueInfo.QueueId));
				component.SetOnClick(SelectQueueInfo);
				_listViewsByModel.Add(queueInfo, component);
			}
		}
		DispatchSelectQueueSignal();
	}

	private void OnPlayButtonClicked()
	{
		if (CurrentSelection.SelectedBladeQueueInfo != null && CurrentSelection.SelectedDeckId != Guid.Empty)
		{
			string selectedEventName = CurrentSelection.SelectedEventName;
			Guid selectedDeckId = CurrentSelection.SelectedDeckId;
			base.Signals.JoinMatchSignal.Dispatch(new EnterMatchMakingSignalArgs(this, selectedEventName, selectedDeckId));
		}
	}

	private void OnBestOfToggle(bool val)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_toggle, base.gameObject);
		CurrentSelection.UseBO3 = val;
		UpdateViewFromSelection(ChangeInitiator.Toggle);
		SendUpdatedSelection();
	}

	private void SelectQueueTab(PlayBladeQueueType type)
	{
		if (_data != null && CurrentSelection.SelectedQueueType != type)
		{
			CurrentSelection.SelectedQueueType = type;
			UpdateTabVisuals(type);
			_subHeader.SetText(GetLocKeyForQueueType(type));
			UpdateViewFromSelection(ChangeInitiator.Tab);
			SendUpdatedSelection();
		}
	}

	private void UpdateTabVisuals(PlayBladeQueueType type)
	{
		foreach (BladeQueueTypePair tab in _tabs)
		{
			tab.tab.SetTabActiveVisuals(tab.type == type);
		}
	}

	private void SelectQueueInfo(BladeQueueInfo selectedQueueInfo)
	{
		if (CurrentSelection.SelectedBladeQueueInfo == selectedQueueInfo)
		{
			SetListItemState(CurrentSelection.SelectedBladeQueueInfo, value: true);
			return;
		}
		SetListItemState(CurrentSelection.SelectedBladeQueueInfo, value: false);
		CurrentSelection.SelectedBladeQueueInfo = selectedQueueInfo;
		SetListItemState(CurrentSelection.SelectedBladeQueueInfo, value: true);
		UpdateViewFromSelection(ChangeInitiator.List);
		SendUpdatedSelection();
		DispatchSelectQueueSignal();
	}

	private void DispatchSelectQueueSignal()
	{
		BladeQueueInfo selectedBladeQueueInfo = CurrentSelection.SelectedBladeQueueInfo;
		if (selectedBladeQueueInfo != null)
		{
			BladeEventInfo bladeEventInfo = (CurrentSelection.UseBO3 ? selectedBladeQueueInfo.EventInfo_BO3 : selectedBladeQueueInfo.EventInfo_BO1);
			base.Signals.QueueSelectedSignal.Dispatch(new QueueSelectedSignalArgs(this, bladeEventInfo));
		}
	}

	private string GetLocKeyForQueueType(PlayBladeQueueType playBladeQueueType)
	{
		string empty = string.Empty;
		return playBladeQueueType switch
		{
			PlayBladeQueueType.Ranked => "PlayBlade/FindMatch/PlayBlade_FindMatch_Title_Ranked", 
			PlayBladeQueueType.Unranked => "PlayBlade/FindMatch/PlayBlade_FindMatch_Title_UnRanked", 
			PlayBladeQueueType.Brawl => "PlayBlade/FindMatch/PlayBlade_FindMatch_Title_Brawl", 
			_ => "MainNav/General/Empty_String", 
		};
	}

	private void UpdateViewFromSelection(ChangeInitiator changeInitiator)
	{
		_playButton.Interactable = false;
		if (CurrentSelection.SelectedDeckId != Guid.Empty)
		{
			DeckViewInfo deckViewInfo = _data.Decks.FirstOrDefault((DeckViewInfo d) => d.deckId == CurrentSelection.SelectedDeckId);
			if (deckViewInfo != null)
			{
				string formatName = CurrentSelection.SelectedEvent?.Format.FormatName ?? "";
				DeckDisplayInfo validationForFormat = deckViewInfo.GetValidationForFormat(formatName);
				_playButton.Interactable = validationForFormat.IsValid;
			}
		}
		bool flag = CurrentSelection.SelectedBladeQueueInfo?.CanBestOf3 ?? true;
		List<string> activeNPEQuests = GetActiveNPEQuests();
		switch (changeInitiator)
		{
		case ChangeInitiator.Tab:
		{
			List<BladeQueueInfo> list2 = _data.Queues[CurrentSelection.SelectedQueueType];
			BladeQueueInfo bladeQueueInfo = list2.FirstOrDefault((BladeQueueInfo x) => x.LocTitle == CurrentSelection.SelectedBladeQueueInfo?.LocTitle) ?? list2.First();
			CurrentSelection.SelectedBladeQueueInfo = bladeQueueInfo;
			flag = bladeQueueInfo.CanBestOf3;
			if (!flag)
			{
				CurrentSelection.UseBO3 = false;
			}
			UpdateTabVisuals(CurrentSelection.SelectedQueueType);
			DrawListItems(list2, activeNPEQuests);
			SetListItemState(CurrentSelection.SelectedBladeQueueInfo, value: true);
			_subHeader.SetText(GetLocKeyForQueueType(CurrentSelection.SelectedQueueType));
			break;
		}
		case ChangeInitiator.List:
			flag = CurrentSelection.SelectedBladeQueueInfo.CanBestOf3;
			if (!flag)
			{
				CurrentSelection.UseBO3 = false;
			}
			break;
		case ChangeInitiator.Toggle:
		{
			List<BladeQueueInfo> list = _data.Queues[CurrentSelection.SelectedQueueType];
			if (CurrentSelection.UseBO3)
			{
				list = list.Where((BladeQueueInfo x) => x.CanBestOf3).ToList();
			}
			DrawListItems(list, activeNPEQuests);
			SetListItemState(CurrentSelection.SelectedBladeQueueInfo, value: true);
			break;
		}
		}
		_bo3ToggleContainer.UpdateActive(flag);
		if (flag)
		{
			_bo3ToggleAnimator.SetBool(TOGGLE_BO3_HASH, CurrentSelection.UseBO3);
			_bo3Toggle.SetIsOnWithoutNotify(CurrentSelection.UseBO3);
		}
		UpdateRankIcon();
	}

	private List<string> GetActiveNPEQuests()
	{
		List<string> list = new List<string>();
		if (CampaignGraphMilestonesUtilities.CheckIfUserIsBetweenMilestones(CampaignGraphMilestones.OpenSparkQueue, CampaignGraphMilestones.GraduateSparkRank) == true)
		{
			list.Add("SparkAlchemyRanked");
		}
		if (CampaignGraphMilestonesUtilities.CheckIfUserIsBetweenMilestones(CampaignGraphMilestones.OpenAlchemyRankQueue, CampaignGraphMilestones.GraduateAlchemyBronzeRank) == true)
		{
			list.Add("StandardRanked");
		}
		if (CampaignGraphMilestonesUtilities.CheckIfUserIsBetweenMilestones(CampaignGraphMilestones.OpenHistoricBrawlQueue, CampaignGraphMilestones.Play5BrawlGames) == true)
		{
			list.Add("HistoricBrawl");
		}
		return list;
	}

	private void SetListItemState(BladeQueueInfo selection, bool value)
	{
		if (_listViewsByModel.TryGetValue(selection, out var value2))
		{
			value2.SetToggleValue(value);
		}
	}

	private void SendUpdatedSelection()
	{
		UpdateSelection(_selectionInfo);
	}

	private void ResetTabState()
	{
		foreach (BladeQueueTypePair tab in _tabs)
		{
			tab.tab.SetLocked(locked: false);
		}
	}

	private void CleanupListItems()
	{
		foreach (BladeQueueListItem value in _listViewsByModel.Values)
		{
			value.Cleanup();
			base.UnityObjectPool.PushObject(value.gameObject);
		}
		_listViewsByModel.Clear();
	}
}
