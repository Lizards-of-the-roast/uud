using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Player.PlayerRankSprites;
using Core.Shared.Code.ClientModels;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class RankDisplay : MonoBehaviour
{
	[SerializeField]
	private Image rankImage;

	[SerializeField]
	private TooltipTrigger _rankTooltip;

	[SerializeField]
	private bool _isLimited;

	[SerializeField]
	private bool _displayText = true;

	[SerializeField]
	private bool _displayPips = true;

	[SerializeField]
	private bool _displayTooltip = true;

	[SerializeField]
	private bool _displayBacker = true;

	[SerializeField]
	private bool _displayImageNativeSize = true;

	[SerializeField]
	private TextMeshProUGUI _rankFormatText;

	[SerializeField]
	private TextMeshProUGUI _rankTierText;

	[SerializeField]
	private Animator _pipAnimator;

	[SerializeField]
	private TextMeshProUGUI _mythicPlacementText;

	[SerializeField]
	private GameObject _mythicPlacementArrow;

	[SerializeField]
	private GameObject _pipParentGameObject;

	[SerializeField]
	private GameObject _mythicGameObject;

	[SerializeField]
	private GameObject _backer;

	[SerializeField]
	private GameObject _mustPlayGameWarning;

	private AssetLoader.AssetTracker<Sprite> _rankImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RankDisplayImageSprite");

	private RankProgress _rankProgress;

	public bool RankUp;

	private bool eligibleRank;

	private int maxPips;

	private int oldStep;

	private int newStep;

	private SeasonAndRankDataProvider _seasonDataProvider;

	private IClientLocProvider _locManager
	{
		get
		{
			if (Languages.ActiveLocProvider != null)
			{
				return Languages.ActiveLocProvider;
			}
			return null;
		}
	}

	public bool IsLimited
	{
		set
		{
			_isLimited = value;
		}
	}

	public void ForceLimited(bool isLimited)
	{
		_isLimited = isLimited;
	}

	private void Awake()
	{
		_seasonDataProvider = Pantry.Get<SeasonAndRankDataProvider>();
	}

	private void OnEnable()
	{
		if (_pipAnimator != null && _pipAnimator.isActiveAndEnabled)
		{
			_pipAnimator.SetInteger("PipCount", maxPips);
			_pipAnimator.SetInteger("PipFill_Current", oldStep);
			_pipAnimator.SetInteger("PipFill_New", newStep);
		}
	}

	public void EndGameRankProgressDisplay(AssetLookupSystem assetLookupSystem, RankProgress progress, bool isLimited, MythicRatingUpdated mythicRatingUpdated = null)
	{
		_isLimited = isLimited;
		_rankProgress = progress;
		RankInfo rankInfo = new RankInfo
		{
			rankClass = progress.oldClass,
			level = progress.oldLevel,
			steps = progress.oldStep
		};
		if (mythicRatingUpdated != null)
		{
			rankInfo.mythicLeaderboardPlace = mythicRatingUpdated.newMythicLeaderboardPlacement;
			rankInfo.mythicPercentile = mythicRatingUpdated.newMythicPercentile;
		}
		if (_backer != null)
		{
			_backer.SetActive(_displayBacker);
		}
		CalculateRankFormatText(rankInfo);
		CalculateRankTierText(rankInfo);
		CalculateRankTooltip(rankInfo);
		CalculateRankMythicUpdate(rankInfo);
		PlayerRankSprites rankSprite = RankIconUtils.GetRankSprite(assetLookupSystem, _rankProgress.oldClass, _rankProgress.oldLevel, !_isLimited);
		if (rankSprite != null)
		{
			AssetLoaderUtils.TrySetSprite(rankImage, _rankImageSpriteTracker, rankSprite.SpriteRef.RelativePath);
		}
		if (_displayImageNativeSize)
		{
			rankImage.SetNativeSize();
		}
		CalculateRankProgress(assetLookupSystem);
	}

	public void CalculateRankProgress(AssetLookupSystem assetLookupSystem)
	{
		oldStep = _rankProgress.oldStep;
		newStep = _rankProgress.newStep;
		if (_rankProgress == null)
		{
			return;
		}
		if (_rankProgress.newClass == _rankProgress.oldClass)
		{
			if (_rankProgress.newLevel != _rankProgress.oldLevel)
			{
				if (_rankProgress.newLevel < _rankProgress.oldLevel)
				{
					newStep = CalculateMaxPips();
					RankUp = true;
				}
				else
				{
					oldStep = CalculateMaxPips();
					_rankTierText.text = _locManager.GetLocalizedText("Rank/Rank_Tier_Tooltip", ("rankDisplayName", RankUtilities.GetClassDisplayName(_rankProgress.newClass)), ("playerTier", _rankProgress.newLevel.ToString()));
					PlayerRankSprites rankSprite = RankIconUtils.GetRankSprite(assetLookupSystem, _rankProgress.newClass, _rankProgress.newLevel, !_isLimited);
					if (rankSprite != null)
					{
						rankImage.sprite = _rankImageSpriteTracker.Acquire(rankSprite.SpriteRef.RelativePath);
					}
				}
			}
		}
		else if (_rankProgress.newClass > _rankProgress.oldClass)
		{
			newStep = CalculateMaxPips();
			RankUp = true;
		}
		if (_displayPips && _rankProgress.newClass != RankingClassType.Mythic)
		{
			maxPips = CalculateMaxPips();
			_pipParentGameObject.SetActive(value: true);
			if (_pipAnimator.isActiveAndEnabled)
			{
				_pipAnimator.SetInteger("PipCount", maxPips);
				_pipAnimator.SetInteger("PipFill_Current", oldStep);
				_pipAnimator.SetInteger("PipFill_New", newStep);
			}
		}
		else
		{
			_pipParentGameObject.SetActive(value: false);
		}
	}

	public int GetPipCount()
	{
		return _pipAnimator.GetInteger("PipFill_Current");
	}

	public void SetPipCount(int count)
	{
		_pipAnimator.SetInteger("PipFill_Current", count);
	}

	public void RefreshRank(AssetLookupSystem assetLookupSystem)
	{
		CombinedRankInfo combinedRank = Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank;
		RankInfo rankInfo = (_isLimited ? combinedRank.limited : combinedRank.constructed);
		if (_mustPlayGameWarning != null && rankInfo.rankClass != RankingClassType.Spark)
		{
			int num = (_isLimited ? (combinedRank.limitedMatchesDrawn + combinedRank.limitedMatchesLost + combinedRank.limitedMatchesWon) : (combinedRank.constructedMatchesDrawn + combinedRank.constructedMatchesLost + combinedRank.constructedMatchesWon));
			eligibleRank = false;
			if (_seasonDataProvider.SeasonInfo != null)
			{
				eligibleRank = _seasonDataProvider.SeasonInfo.currentSeason.minMatches <= num;
			}
			_mustPlayGameWarning.SetActive(!eligibleRank);
			rankImage.color = (eligibleRank ? new Color(1f, 1f, 1f, 1f) : new Color(21f / 64f, 21f / 64f, 21f / 64f));
		}
		CalculateRankDisplay(rankInfo, assetLookupSystem);
	}

	public void CalculateRankDisplay(RankInfo playerRankInfo, AssetLookupSystem assetLookupSystem)
	{
		if (_backer != null)
		{
			_backer.SetActive(_displayBacker);
		}
		CalculateRankFormatText(playerRankInfo);
		CalculateRankTierText(playerRankInfo);
		CalculateRankTooltip(playerRankInfo);
		CalculateRankMythicUpdate(playerRankInfo);
		if (rankImage != null)
		{
			rankImage.gameObject.SetActive(value: true);
		}
		if (!(rankImage != null))
		{
			return;
		}
		PlayerRankSprites rankSprite = RankIconUtils.GetRankSprite(assetLookupSystem, playerRankInfo.rankClass, playerRankInfo.level, !_isLimited);
		if (rankSprite != null)
		{
			rankImage.sprite = _rankImageSpriteTracker.Acquire(rankSprite.SpriteRef.RelativePath);
		}
		if (_displayImageNativeSize)
		{
			rankImage.SetNativeSize();
		}
		if (!(_pipParentGameObject != null))
		{
			return;
		}
		if (_displayPips && playerRankInfo.rankClass != RankingClassType.Mythic)
		{
			_pipParentGameObject.SetActive(value: true);
			if ((bool)_pipAnimator)
			{
				maxPips = SeasonUtilities.GetStepsForRank(playerRankInfo, !_isLimited, _seasonDataProvider.SeasonInfo);
				oldStep = playerRankInfo.steps;
				newStep = playerRankInfo.steps;
				if (_pipAnimator.isActiveAndEnabled)
				{
					_pipAnimator.SetInteger("PipCount", maxPips);
					_pipAnimator.SetInteger("PipFill_Current", oldStep);
					_pipAnimator.SetInteger("PipFill_New", newStep);
				}
			}
		}
		else
		{
			_pipParentGameObject.SetActive(value: false);
		}
	}

	private int CalculateMaxPips()
	{
		if (_rankProgress.oldClass == RankingClassType.Mythic)
		{
			return 0;
		}
		if (_seasonDataProvider == null)
		{
			_seasonDataProvider = Pantry.Get<SeasonAndRankDataProvider>();
		}
		Client_SeasonAndRankInfo client_SeasonAndRankInfo = _seasonDataProvider.SeasonInfo;
		if (client_SeasonAndRankInfo == null)
		{
			client_SeasonAndRankInfo = GetSavedRankInfos();
		}
		List<Client_RankDefinition> list = ((!_isLimited) ? client_SeasonAndRankInfo.constructedRankInfo : client_SeasonAndRankInfo.limitedRankInfo);
		if (list == null)
		{
			return 0;
		}
		return list.Find((Client_RankDefinition ri) => ri != null && ri.rankClass == _rankProgress.oldClass && ri.level == _rankProgress.oldLevel)?.steps ?? 0;
	}

	private static Client_SeasonAndRankInfo GetSavedRankInfos()
	{
		return new Client_SeasonAndRankInfo
		{
			constructedRankInfo = JsonConvert.DeserializeObject<List<Client_RankDefinition>>(MDNPlayerPrefs.ReconnectConstructedRankInfo),
			limitedRankInfo = JsonConvert.DeserializeObject<List<Client_RankDefinition>>(MDNPlayerPrefs.ReconnectLimitedRankInfo),
			currentSeason = null
		};
	}

	private void CalculateRankFormatText(RankInfo playerRankInfo)
	{
		if (_rankFormatText != null && _displayText)
		{
			_rankFormatText.gameObject.SetActive(value: true);
			_rankFormatText.text = (_isLimited ? _locManager.GetLocalizedText("MainNav/HomePage/EventBlade/LimitedRank") : _locManager.GetLocalizedText("MainNav/HomePage/EventBlade/ConstructedRank"));
		}
		else if (_rankFormatText != null)
		{
			_rankFormatText.gameObject.SetActive(value: false);
		}
	}

	private void CalculateRankTierText(RankInfo playerRankInfo)
	{
		if (!(_rankTierText != null))
		{
			return;
		}
		if (!_displayText)
		{
			_rankTierText.gameObject.SetActive(value: false);
			return;
		}
		_rankTierText.gameObject.SetActive(value: true);
		if (playerRankInfo.rankClass == RankingClassType.Mythic)
		{
			_rankTierText.text = RankUtilities.GetClassDisplayName(playerRankInfo.rankClass);
			return;
		}
		_rankTierText.text = _locManager.GetLocalizedText("Rank/Rank_Tier_Tooltip", ("rankDisplayName", RankUtilities.GetClassDisplayName(playerRankInfo.rankClass)), ("playerTier", playerRankInfo.level.ToString()));
	}

	private void CalculateRankTooltip(RankInfo playerRankInfo)
	{
		if (_rankTooltip == null)
		{
			return;
		}
		if (_displayTooltip)
		{
			_rankTooltip.IsActive = true;
			if (playerRankInfo.rankClass == RankingClassType.Spark)
			{
				_rankTooltip.LocString = "Rank/Spark_Ranked";
			}
			else if (eligibleRank)
			{
				_rankTooltip.LocString = null;
				_rankTooltip.IsActive = false;
			}
			else
			{
				_rankTooltip.LocString = "Rank/MustPlayGameWarning";
			}
		}
		else
		{
			_rankTooltip.IsActive = false;
		}
	}

	private void CalculateRankMythicUpdate(RankInfo playerRankInfo)
	{
		if (playerRankInfo.rankClass == RankingClassType.Mythic)
		{
			_mythicGameObject.SetActive(value: true);
			if (playerRankInfo.mythicLeaderboardPlace > 0)
			{
				_mythicPlacementText.text = "#" + playerRankInfo.mythicLeaderboardPlace;
				_mythicPlacementArrow.SetActive(value: false);
			}
			else
			{
				int num = (int)playerRankInfo.mythicPercentile;
				_mythicPlacementText.text = num + "%";
				_mythicPlacementArrow.SetActive(value: false);
			}
		}
		else
		{
			_mythicGameObject.SetActive(value: false);
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(rankImage, _rankImageSpriteTracker);
	}
}
