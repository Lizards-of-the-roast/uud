using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using Core.Meta.Utilities;
using EventPage.Components.NetworkModels;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Wrapper;

internal static class TempRewardTranslation
{
	private static Dictionary<string, string> TRANSLATEXP_TONONXP = new Dictionary<string, string>
	{
		{ "ObjectiveIcon_CoinAndMasteryXP", "ObjectiveIcon_CoinsLarge" },
		{ "ObjectiveReward_XPCoinLarge", "ObjectiveIcon_CoinsLarge" },
		{ "ObjectiveReward_XPSmallCoinLarge", "ObjectiveIcon_CoinsLarge" },
		{ "ObjectiveReward_XPSmallCoinSmall", "ObjectiveIcon_CoinsSmall" },
		{ "ObjectiveReward_XPCard", null },
		{ "ObjectiveReward_XPCoinSmall", "ObjectiveIcon_CoinsSmall" },
		{ "RewardPopup3DIcon_XP", null },
		{ "RewardPopup3DIcon_XPCoin", "RewardPopup3DIcon_Coin" },
		{ "RewardPopup3DIcon_XPCard", "RewardPopup3DIcon_Card" },
		{ "MainNav/EventRewards/Card_And_XP", "MainNav/EventRewards/Card" },
		{ "MainNav/EventRewards/Gold_And_XP_Reward", "MainNav/EventRewards/Gold_Reward" },
		{ "MainNav/QuestRewards/Gold_And_XP_Reward", "MainNav/EventRewards/Gold_Reward" },
		{ "MainNav/EventRewards/Basic_Land_And_Generic_XP", "MainNav/EventRewards/Basic_Land" },
		{ "MainNav/EventRewards/Basic_Land_And_XP", "MainNav/EventRewards/Basic_Land" },
		{ "ObjectiveIcons_SecondaryXP", null },
		{ "ObjectiveIcon_MasteryXP", null }
	};

	public static void LookupBoosterTextures(int collationId, out string bgPath, out string fgPath, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = (CollationMapping)collationId;
		Background background = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Background> loadedTree))
		{
			background = loadedTree.GetPayload(assetLookupSystem.Blackboard);
		}
		Logo logo = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree2))
		{
			logo = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
		}
		bgPath = null;
		fgPath = null;
		if (background != null)
		{
			bgPath = background.TextureRef.RelativePath;
		}
		if (logo != null)
		{
			fgPath = logo.TextureRef.RelativePath;
		}
	}

	public static RewardDisplayData ChestDescriptionToDisplayData(EventChestDescription cd, ICardDataProvider cardDatabase, CardMaterialBuilder cardMaterialBuilder)
	{
		RewardDisplayData rewardDisplayData = ChestDescriptionToDisplayData(cd.ChestDescription, cardDatabase, cardMaterialBuilder);
		if (rewardDisplayData != null)
		{
			rewardDisplayData.WinsNeeded = cd.Wins;
		}
		return rewardDisplayData;
	}

	public static RewardDisplayData ChestDescriptionToDisplayData(ClientChestDescription cd, ICardDataProvider cardDatabase, CardMaterialBuilder cardMaterialBuilder, bool replaceXp = false, int? quantityLabelOverride = null)
	{
		if (cd == null)
		{
			return null;
		}
		RewardDisplayData rewardDisplayData = new RewardDisplayData();
		if (replaceXp)
		{
			cd = RemoveXPFromChests(cd);
		}
		rewardDisplayData.MainText = cd.headerLocKey;
		if (cd.locParams != null && cd.locParams.Count > 0)
		{
			rewardDisplayData.MainText.Parameters = new Dictionary<string, string>();
			foreach (KeyValuePair<string, int> locParam in cd.locParams)
			{
				rewardDisplayData.MainText.Parameters.Add(locParam.Key, locParam.Value.ToString());
			}
		}
		rewardDisplayData.SecondaryText = cd.descriptionLocKey;
		rewardDisplayData.SecondaryText.Parameters = rewardDisplayData.MainText.Parameters;
		rewardDisplayData.Quantity = 0;
		int.TryParse(cd.quantity, out rewardDisplayData.Quantity);
		if (quantityLabelOverride.HasValue)
		{
			rewardDisplayData.Quantity = quantityLabelOverride.Value;
		}
		if (!string.IsNullOrEmpty(cd.image1))
		{
			rewardDisplayData.Thumbnail1Path = ServerRewardUtils.FormatAssetFromServerReference(cd.image1, ServerRewardFileExtension.PNG);
		}
		if (!string.IsNullOrEmpty(cd.image2))
		{
			rewardDisplayData.Thumbnail2Path = ServerRewardUtils.FormatAssetFromServerReference(cd.image2, ServerRewardFileExtension.PNG);
		}
		if (!string.IsNullOrEmpty(cd.image3))
		{
			rewardDisplayData.Thumbnail3Path = ServerRewardUtils.FormatAssetFromServerReference(cd.image3, ServerRewardFileExtension.PNG);
		}
		if (!string.IsNullOrEmpty(cd.prefab))
		{
			rewardDisplayData.Popup3dObjectPath = ServerRewardUtils.FormatAssetFromServerReference(cd.prefab, ServerRewardFileExtension.Prefab);
		}
		rewardDisplayData.ReferenceID = cd.referenceId;
		if (int.TryParse(cd.referenceId, out var result) && result != 0)
		{
			if (rewardDisplayData.OverridePopup3dObject != null)
			{
				if (rewardDisplayData.OverridePopup3dObject.HackDeckBox)
				{
					string originalAssetPath = cardDatabase.GetCardPrintingById((uint)result)?.ImageAssetPath ?? string.Empty;
					rewardDisplayData.PopupObjectBackgroundTexturePath = cardMaterialBuilder.TextureLoader.GetCardArtPath(originalAssetPath);
				}
				else
				{
					LookupBoosterTextures(result, out rewardDisplayData.PopupObjectBackgroundTexturePath, out rewardDisplayData.PopupObjectForegroundTexturePath, WrapperController.Instance.AssetLookupSystem);
				}
			}
			else if (!string.IsNullOrEmpty(rewardDisplayData.Popup3dObjectPath))
			{
				NotificationPopupReward objectData = AssetLoader.GetObjectData<NotificationPopupReward>(rewardDisplayData.Popup3dObjectPath);
				if (objectData != null && objectData.HackDeckBox)
				{
					string originalAssetPath2 = cardDatabase.GetCardPrintingById((uint)result)?.ImageAssetPath ?? string.Empty;
					rewardDisplayData.PopupObjectBackgroundTexturePath = cardMaterialBuilder.TextureLoader.GetCardArtPath(originalAssetPath2);
				}
				else
				{
					LookupBoosterTextures(result, out rewardDisplayData.PopupObjectBackgroundTexturePath, out rewardDisplayData.PopupObjectForegroundTexturePath, WrapperController.Instance.AssetLookupSystem);
				}
			}
		}
		return rewardDisplayData;
	}

	private static ClientChestDescription RemoveXPFromChests(ClientChestDescription cd)
	{
		if (!string.IsNullOrEmpty(cd.image1) && TRANSLATEXP_TONONXP.TryGetValue(cd.image1, out var value))
		{
			cd.image1 = value;
		}
		if (!string.IsNullOrEmpty(cd.image2) && TRANSLATEXP_TONONXP.TryGetValue(cd.image2, out var value2))
		{
			cd.image2 = value2;
		}
		if (!string.IsNullOrEmpty(cd.image3) && TRANSLATEXP_TONONXP.TryGetValue(cd.image3, out var value3))
		{
			cd.image3 = value3;
		}
		if (!string.IsNullOrEmpty(cd.prefab) && TRANSLATEXP_TONONXP.TryGetValue(cd.prefab, out var value4))
		{
			cd.prefab = value4;
		}
		if (!string.IsNullOrEmpty(cd.headerLocKey) && TRANSLATEXP_TONONXP.TryGetValue(cd.headerLocKey, out var value5))
		{
			cd.headerLocKey = value5;
		}
		return cd;
	}
}
