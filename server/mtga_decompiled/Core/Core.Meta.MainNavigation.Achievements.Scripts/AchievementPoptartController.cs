using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.Shared;
using Core.Meta.Utilities;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

public sealed class AchievementPoptartController : AchievementCardDataView
{
	[SerializeField]
	private Localize _rewardTitle;

	[SerializeField]
	private Localize _achievementParenthetical;

	[SerializeField]
	private Localize _achievementDescription;

	[SerializeField]
	private Transform _rewardParent;

	protected override void CardViewUpdate()
	{
		if (_achievementData == null)
		{
			return;
		}
		if (_rewardTitle != null)
		{
			_rewardTitle?.SetText(_achievementData.Reward.TitleLocKey);
		}
		if (_achievementDescription != null)
		{
			_achievementDescription.SetText(_achievementData.DescriptionLocalizationKey, null, _achievementData.Description ?? "");
		}
		if (_achievementParenthetical != null)
		{
			_achievementParenthetical.SetText(_achievementData.ParentheticalTextLocalizationKey);
			_achievementParenthetical.gameObject.SetActive(!string.IsNullOrEmpty(_achievementData.ParentheticalTextLocalizationKey));
		}
		if (!(_rewardParent != null))
		{
			return;
		}
		NotificationPopupReward notificationPopupReward = AssetLoader.Instantiate<NotificationPopupReward>(ServerRewardUtils.FormatAssetFromServerReference(_achievementData.Reward.RewardIconPrefab, ServerRewardFileExtension.Prefab), _rewardParent);
		EmoteView componentInChildren = notificationPopupReward.GetComponentInChildren<EmoteView>();
		if ((object)componentInChildren != null)
		{
			string previewLocKey = EmoteUtils.GetPreviewLocKey(_achievementData.Reward.RewardChestDescription.referenceId, Pantry.Get<AssetLookupManager>().AssetLookupSystem);
			if (previewLocKey != null && !string.IsNullOrEmpty(previewLocKey))
			{
				componentInChildren.SetLocalizationKey(previewLocKey);
			}
			return;
		}
		RewardDisplayTitle componentInChildren2 = notificationPopupReward.GetComponentInChildren<RewardDisplayTitle>();
		if ((object)componentInChildren2 != null)
		{
			string referenceId = _achievementData.Reward.RewardChestDescription.referenceId;
			string titleLocKey = CosmeticsUtils.TitleLocKey(Pantry.Get<CosmeticsProvider>(), referenceId);
			componentInChildren2.Init(referenceId, titleLocKey, null);
		}
	}
}
