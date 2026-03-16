using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Meta.Utilities;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Profile;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Wrapper;

namespace ProfileUI;

public class ProfileDetailsPanel : MonoBehaviour
{
	[Header("Ranks")]
	[SerializeField]
	private RankDisplay _constructedRankDisplay;

	[SerializeField]
	private RankDisplay _limitedRankDisplay;

	[SerializeField]
	private TMP_Text _seasonNameRankText;

	[SerializeField]
	private GameObject _mythicInvitationalQualifierObject;

	[Space(5f)]
	[Header("Bubbles")]
	[SerializeField]
	private ObjectiveBubble _battlePassBubble;

	[SerializeField]
	private MasteryEndText _masteryEndText;

	[Space(5f)]
	[Header("Set Collection")]
	[SerializeField]
	private SetBadge _profileSetBadge;

	private SeasonAndRankDataProvider _seasonDataProvider;

	private SetMasteryDataProvider _masteryPassProvider;

	private SetCollectionController _collectionController;

	private IClientLocProvider _locManager;

	private CardDatabase _cardDatabase;

	private CardMaterialBuilder _cardMaterialBuilder;

	private ISetMetadataProvider _setMetadataProvider;

	private Action<RankType> _seasonRewards_onClick;

	private Action<ProfileScreenModeEnum> _setCollection_onClick;

	private static readonly int Idle = Animator.StringToHash("Idle");

	private AssetLookupSystem _assetLookupSystem;

	public void Initialize(SetMasteryDataProvider masteryPassProvider, IClientLocProvider localizationManager, SeasonAndRankDataProvider seasonDataProvider, SetCollectionController collectionController, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem, bool mythicQualified, Action<RankType> seasonRewards_onClick, Action<ProfileScreenModeEnum> setCollection_onClick, ISetMetadataProvider setMetadataProvider)
	{
		_seasonDataProvider = seasonDataProvider;
		_masteryPassProvider = masteryPassProvider;
		_collectionController = collectionController;
		_locManager = localizationManager;
		_cardDatabase = cardDatabase;
		_cardMaterialBuilder = cardMaterialBuilder;
		_assetLookupSystem = assetLookupSystem;
		_seasonRewards_onClick = seasonRewards_onClick;
		_setCollection_onClick = setCollection_onClick;
		_mythicInvitationalQualifierObject.SetActive(mythicQualified);
		_battlePassBubble.Init(cardDatabase, cardViewBuilder);
		_masteryEndText.Init(masteryPassProvider, localizationManager);
		_setMetadataProvider = setMetadataProvider;
	}

	public void Display()
	{
		IPlayerRankServiceWrapper playerRankServiceWrapper = Pantry.Get<IPlayerRankServiceWrapper>();
		CombinedRankInfo combinedRank = playerRankServiceWrapper.CombinedRank;
		bool num = playerRankServiceWrapper.CombinedRank.constructed.rankClass == RankingClassType.Spark;
		base.gameObject.SetActive(value: true);
		string item = "TEST";
		if (!num)
		{
			if (_seasonDataProvider.SeasonInfo?.currentSeason != null)
			{
				item = SeasonUtilities.GetSeasonDisplayName(_seasonDataProvider.SeasonInfo.currentSeason.seasonOrdinal);
			}
			_seasonNameRankText.text = _locManager.GetLocalizedText("MainNav/Season/SeasonRankHeader", ("seasonName", item));
			new Dictionary<string, string>().Add("dateTime", WrapperController.Instance.RenewalManager.GetCurrentRenewalStartDate().ToString("MMMM dd"));
		}
		else
		{
			_seasonNameRankText.text = "";
		}
		if (_constructedRankDisplay != null)
		{
			_constructedRankDisplay.RefreshRank(_assetLookupSystem);
		}
		_limitedRankDisplay.gameObject.SetActive(combinedRank != null && ShouldShowLimitedRank());
		if (_limitedRankDisplay != null && _limitedRankDisplay.gameObject.activeSelf)
		{
			_limitedRankDisplay.RefreshRank(_assetLookupSystem);
		}
		UpdateProgressionBubbles();
		UpdateSetCollectionBadge();
	}

	public bool ShouldShowLimitedRank()
	{
		return Pantry.Get<EventManager>().EventContexts.Any((EventContext x) => (x.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Draft || x.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Sealed) && x.PlayerEvent.EventInfo.IsRanked);
	}

	public void SeasonRankImageClicked(int rank)
	{
		_seasonRewards_onClick?.Invoke((RankType)rank);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void ActiveBPClicked()
	{
		ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(_masteryPassProvider.CurrentBpName, NavContentType.Profile, NavContentType.Profile);
		SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From Profile Objective Click");
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void SetCollectionClicked()
	{
		_setCollection_onClick?.Invoke(ProfileScreenModeEnum.SetCollection);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void MythicQualifierBadgeInfoClicked()
	{
		UrlOpener.OpenURL(_locManager.GetLocalizedText("MainNav/Profile/MythicQualifier/QualifierLink"));
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	private void UpdateProgressionBubbles()
	{
		string currentBpName = _masteryPassProvider.CurrentBpName;
		if (currentBpName != null)
		{
			CurrentProgressionSummary currentProgressionSummary = _masteryPassProvider.GetCurrentProgressionSummary(currentBpName, _cardDatabase, _cardMaterialBuilder);
			UpdateBubble(ref _battlePassBubble, currentProgressionSummary);
			_battlePassBubble.SetInactive(_masteryPassProvider.HasTrackExpired(currentBpName));
		}
		else
		{
			_battlePassBubble.SetMasteryLevelLabel(new ProgressionTrackLevel
			{
				RawLevel = 0
			});
			_battlePassBubble.SetInactive(inactive: true);
		}
	}

	private static void UpdateBubble(ref ObjectiveBubble progBubble, CurrentProgressionSummary levelSummary)
	{
		if (levelSummary.LevelInfo.IsProgressionComplete)
		{
			progBubble.SetUpAsCompletedFinalLevel();
		}
		else
		{
			progBubble.SetProgressText(levelSummary.ProgressText);
			progBubble.SetReward(levelSummary.CurrentReward);
			progBubble.SetUnOwnedRewardOverlay(levelSummary.ShouldTease);
			progBubble.SetLocked(levelSummary.ShouldTease);
		}
		progBubble.ActivatePremiumWreath(levelSummary.Tier > 0);
		float radialFill = (float)levelSummary.LevelInfo.EXPProgressIfIsCurrent / (float)levelSummary.LevelInfo.ServerLevel.xpToComplete;
		progBubble.SetRadialFill(radialFill);
		progBubble.SetSidebarVisible(visible: false);
		progBubble.SetPopupDescription("EPP/Objective/ClickToView");
		progBubble.SetFooterText("MainNav/General/Empty_String");
		progBubble.Reference_levelData = levelSummary.LevelInfo;
		progBubble.SetMasteryLevelLabel(levelSummary.LevelInfo);
		progBubble.Reference_endProgress = levelSummary.LevelInfo.EXPProgressIfIsCurrent;
		progBubble.Reference_curGoalProgress = levelSummary.LevelInfo.ServerLevel.xpToComplete;
	}

	private void UpdateSetCollectionBadge()
	{
		_collectionController.UpdateOwnedCards();
		_collectionController.UpdateMetricTotalsPerSet();
		_profileSetBadge.Init();
		CollationMapping mostRecentExpansion = _collectionController.GetMostRecentExpansion();
		string expansionCode = mostRecentExpansion.ToString();
		bool isAlchemySet = _setMetadataProvider.IsAlchemy(mostRecentExpansion);
		Sprite setIcon = _collectionController.GetSetIcon(mostRecentExpansion);
		if (!_collectionController.IsCollectionComplete(expansionCode, SetCollectionController.Metrics.None, CountMode.UsePlayerInvOneOf, isAlchemySet))
		{
			int numOwned = _collectionController.GetMetricTotals(expansionCode).numOwned;
			int numAvailable = _collectionController.GetMetricTotals(expansionCode).numAvailable;
			_profileSetBadge.UpdateUI(setIcon, numOwned, numAvailable);
		}
		else
		{
			int numOwned = _collectionController.GetMetricTotals(expansionCode).numOwned;
			int numAvailable = _collectionController.GetMetricTotals(expansionCode).numAvailable;
			_profileSetBadge.UpdateUI(setIcon, numOwned, numAvailable, useFourOf: true);
		}
	}
}
