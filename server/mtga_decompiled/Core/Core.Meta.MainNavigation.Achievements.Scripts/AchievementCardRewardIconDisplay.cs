using Core.Shared.Code;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

public sealed class AchievementCardRewardIconDisplay : AchievementCardDataView
{
	[SerializeField]
	private Image _rewardIcon;

	private CardMaterialBuilder _cardMaterialBuilder;

	private GlobalCoroutineExecutor _globalCoroutineExecutor;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private NotificationPopupReward _rewardPrefab;

	private AssetLoader.AssetTracker<Sprite> _rewardSpriteAssetTracker;

	private string _assetTrackerSpriteKeyName;

	private void Awake()
	{
		_cardMaterialBuilder = Pantry.Get<CardMaterialBuilder>();
		_globalCoroutineExecutor = Pantry.Get<GlobalCoroutineExecutor>();
		_cardDatabase = Pantry.Get<CardDatabase>();
		_cardViewBuilder = Pantry.Get<CardViewBuilder>();
	}

	private void OnDestroy()
	{
		CleanUpUsages();
	}

	private void CleanUpUsages()
	{
		_rewardSpriteAssetTracker?.Cleanup();
		_rewardSpriteAssetTracker = null;
		if (_rewardPrefab != null)
		{
			Object.Destroy(_rewardPrefab);
		}
	}

	protected override void CardViewUpdate()
	{
		CleanUpUsages();
		if (_achievementData?.Reward?.RewardChestDescription != null && _cardDatabase != null)
		{
			RewardDisplayData rewardDisplayData = TempRewardTranslation.ChestDescriptionToDisplayData(_achievementData.Reward.RewardChestDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder);
			if (rewardDisplayData != null && !string.IsNullOrEmpty(rewardDisplayData.Thumbnail1Path))
			{
				DisplaySpriteRewardIcon(rewardDisplayData);
			}
		}
	}

	private void DisplaySpriteRewardIcon(RewardDisplayData rewardDisplayData)
	{
		_rewardIcon.gameObject.SetActive(value: true);
		_assetTrackerSpriteKeyName = _achievementData.Id.ToString() + "RewardSprite";
		if (_rewardSpriteAssetTracker == null)
		{
			_rewardSpriteAssetTracker = new AssetLoader.AssetTracker<Sprite>(_assetTrackerSpriteKeyName);
		}
		if (!AssetLoaderUtils.TrySetSprite(_rewardIcon, _rewardSpriteAssetTracker, rewardDisplayData.Thumbnail1Path))
		{
			SimpleLog.LogError("AchievementCard failed to load reward art sprite for reward Id: " + rewardDisplayData.Thumbnail1Path);
		}
	}
}
