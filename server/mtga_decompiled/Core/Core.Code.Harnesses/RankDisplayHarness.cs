using System;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class RankDisplayHarness : Harness<RankDisplay, HarnessDropdownUI>
{
	[Serializable]
	public class RankDisplayViewModel
	{
		public string Description;

		public string playerId;

		public int seasonOrdinal;

		public RankingClassType newClass;

		public RankingClassType oldClass;

		public int newLevel;

		public int oldLevel;

		public int oldStep;

		public int newStep;

		public bool wasLossProtected;

		public string rankUpdateType;

		public bool IsLimited;

		public MythicRatingUpdated MythicRatingUpdated;

		public override string ToString()
		{
			return Description;
		}
	}

	[SerializeField]
	private RankDisplayViewModel[] _viewModels;

	private AssetLookupManager _assetLookupManager;

	public override void Initialize(Transform root, Transform uiRoot)
	{
		base.Initialize(root, uiRoot);
		_UI.Init(HarnessUICore.CreateOptions(_viewModels), OnValueChanged);
		_assetLookupManager = Pantry.Get<AssetLookupManager>();
	}

	public override void DisplayViewForIndex(int index)
	{
		base.DisplayViewForIndex(index);
		HarnessUICore.SetValueForDropdown(_UI.Dropdown, _UI.DropdownIndex);
		_Instance.gameObject.SetActive(value: true);
		HarnessCore.CenterView(_Instance.GetComponent<RectTransform>());
	}

	private void OnValueChanged(int newValue)
	{
		RankDisplayViewModel rankDisplayViewModel = _viewModels[newValue];
		_Instance.EndGameRankProgressDisplay(_assetLookupManager.AssetLookupSystem, RankProgressFromViewModel(rankDisplayViewModel), rankDisplayViewModel.IsLimited, MythicRatingFromViewModel(rankDisplayViewModel.MythicRatingUpdated));
	}

	public static RankProgress RankProgressFromViewModel(RankDisplayViewModel viewModel)
	{
		return new RankProgress
		{
			playerId = viewModel.playerId,
			seasonOrdinal = viewModel.seasonOrdinal,
			newClass = viewModel.newClass,
			oldClass = viewModel.oldClass,
			newLevel = viewModel.newLevel,
			oldLevel = viewModel.oldLevel,
			oldStep = viewModel.oldStep,
			newStep = viewModel.newStep,
			wasLossProtected = viewModel.wasLossProtected,
			rankUpdateType = viewModel.rankUpdateType
		};
	}

	public static MythicRatingUpdated MythicRatingFromViewModel(MythicRatingUpdated ratingUpdated)
	{
		if (ratingUpdated.newMythicPercentile == 0f && ratingUpdated.oldMythicPercentile == 0f && ratingUpdated.newMythicLeaderboardPlacement == 0)
		{
			return null;
		}
		return ratingUpdated;
	}
}
