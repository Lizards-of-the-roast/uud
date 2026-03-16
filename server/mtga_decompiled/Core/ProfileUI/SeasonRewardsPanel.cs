using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Shared.Code.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN.Objectives;
using Wotc.Mtga.Extensions;

namespace ProfileUI;

public class SeasonRewardsPanel : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _usernameText;

	[Header("Constructed")]
	[SerializeField]
	private GameObject _constructedLayoutScrollView;

	[SerializeField]
	private GameObject _constructedLayout;

	[SerializeField]
	private RewardBar _constructedRewardBarPrefab;

	[SerializeField]
	private Toggle _constructedToggle;

	[SerializeField]
	private RankDisplay _constructedRankDisplay;

	[Header("Limited")]
	[SerializeField]
	private GameObject _limitedLayoutScrollView;

	[SerializeField]
	private GameObject _limitedLayout;

	[SerializeField]
	private RewardBar _limitedRewardBarPrefab;

	[SerializeField]
	private Toggle _limitedToggle;

	[SerializeField]
	private RankDisplay _limitedRankDisplay;

	private Action _backButton_OnClicked;

	private Action<RankType> _rankDisplay_onClicked;

	private Client_SeasonInfo _currentSeason;

	private bool _shouldShowLimitedRank = true;

	private bool _dirty;

	private AssetLookupSystem _assetLookupSystem;

	public void Initialize(Client_SeasonInfo currentSeason, Action backButton_OnClicked, AssetLookupSystem assetLookupSystem, Action<RankType> rankDisplay_onClicked)
	{
		if (_currentSeason == null || _currentSeason.seasonOrdinal != currentSeason.seasonOrdinal)
		{
			RewardBar[] componentsInChildren = _constructedLayout.GetComponentsInChildren<RewardBar>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
			componentsInChildren = _limitedLayout.GetComponentsInChildren<RewardBar>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
			_dirty = true;
		}
		_currentSeason = currentSeason;
		_backButton_OnClicked = backButton_OnClicked;
		_rankDisplay_onClicked = rankDisplay_onClicked;
		_assetLookupSystem = assetLookupSystem;
	}

	public void InitializeWithoutSeason(Action backButton_OnClicked, AssetLookupSystem assetLookupSystem, Action<RankType> rankDisplay_onClicked)
	{
		_backButton_OnClicked = backButton_OnClicked;
		_rankDisplay_onClicked = rankDisplay_onClicked;
		_assetLookupSystem = assetLookupSystem;
	}

	private void _createRewardBars()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		Dictionary<string, Client_ChestData> seasonConstructedRewards = _currentSeason.seasonConstructedRewards;
		Dictionary<string, Client_ChestData> seasonLimitedRewards = _currentSeason.seasonLimitedRewards;
		string[] names = Enum.GetNames(typeof(RewardLevels));
		for (int i = 0; i < names.Length; i++)
		{
			if (seasonConstructedRewards.TryGetValue(names[i], out var value))
			{
				UnityEngine.Object.Instantiate(_constructedRewardBarPrefab, _constructedLayout.transform, worldPositionStays: false).Initialize((RewardLevels)i, value);
			}
			if (seasonLimitedRewards.TryGetValue(names[i], out var value2))
			{
				UnityEngine.Object.Instantiate(_limitedRewardBarPrefab, _limitedLayout.transform, worldPositionStays: false).Initialize((RewardLevels)i, value2);
			}
		}
	}

	public void SetRankType(RankType type)
	{
		switch (type)
		{
		case RankType.Constructed:
			_constructedToggle.isOn = true;
			_limitedToggle.isOn = false;
			break;
		case RankType.Limited:
			_limitedToggle.isOn = true;
			_constructedToggle.isOn = false;
			break;
		}
	}

	public RankType GetRankType()
	{
		if (_constructedToggle.isOn)
		{
			return RankType.Constructed;
		}
		return RankType.Limited;
	}

	public void PlayClickNoise()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void RankTypeToggled(int type)
	{
		switch ((RankType)type)
		{
		case RankType.Constructed:
			_constructedLayoutScrollView.UpdateActive(active: true);
			_limitedLayoutScrollView.UpdateActive(active: false);
			break;
		case RankType.Limited:
			_constructedLayoutScrollView.UpdateActive(active: false);
			_limitedLayoutScrollView.UpdateActive(active: true);
			break;
		}
	}

	public void BackButtonClicked()
	{
		_backButton_OnClicked?.Invoke();
	}

	public void RankedDisplayClicked(int rank)
	{
		_rankDisplay_onClicked?.Invoke((RankType)rank);
	}

	public void DisplayAndRefreshConstructedRank()
	{
		base.gameObject.SetActive(value: true);
		_constructedRankDisplay.RefreshRank(_assetLookupSystem);
		if (_shouldShowLimitedRank)
		{
			_limitedRankDisplay.RefreshRank(_assetLookupSystem);
		}
	}

	public void Display(string username)
	{
		_createRewardBars();
		base.gameObject.SetActive(value: true);
		_constructedRankDisplay.RefreshRank(_assetLookupSystem);
		_limitedRankDisplay.RefreshRank(_assetLookupSystem);
		_usernameText.text = username;
	}

	public void ShowLimitedRankDisplay(bool active)
	{
		_shouldShowLimitedRank = active;
		_limitedRankDisplay.gameObject.SetActive(active);
	}
}
