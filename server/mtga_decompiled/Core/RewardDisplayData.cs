using Core.Meta.Shared;
using Core.Meta.Utilities;
using Wizards.MDN.Objectives;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

public class RewardDisplayData
{
	public string Thumbnail1Path;

	public string Thumbnail2Path;

	public string Thumbnail3Path;

	public NotificationPopupReward OverridePopup3dObject;

	public string Popup3dObjectPath;

	public string PopupObjectBackgroundTexturePath;

	public string PopupObjectForegroundTexturePath;

	public MTGALocalizedString MainText = "";

	public MTGALocalizedString RewardText = "";

	public MTGALocalizedString DescriptionText = "";

	public MTGALocalizedString SecondaryText = "";

	public MTGALocalizedString ProgressText = "";

	public int Quantity;

	public uint WinsNeeded;

	public string ReferenceID = "";

	public RewardDisplayData()
	{
	}

	public RewardDisplayData(Client_ChestData chest, ICardDataProvider cardDatabase, CardMaterialBuilder cardMaterialBuilder)
	{
		MainText = chest.HeaderLocKey;
		MainText.Parameters = chest.LocParams;
		SecondaryText = chest.DescriptionLocKey;
		Quantity = chest.Quantity;
		if (!string.IsNullOrEmpty(chest.Image1))
		{
			Thumbnail1Path = ServerRewardUtils.FormatAssetFromServerReference(chest.Image1, ServerRewardFileExtension.PNG);
		}
		if (!string.IsNullOrEmpty(chest.Image2))
		{
			Thumbnail2Path = ServerRewardUtils.FormatAssetFromServerReference(chest.Image2, ServerRewardFileExtension.PNG);
		}
		if (!string.IsNullOrEmpty(chest.Image3))
		{
			Thumbnail3Path = ServerRewardUtils.FormatAssetFromServerReference(chest.Image3, ServerRewardFileExtension.PNG);
		}
		if (!string.IsNullOrEmpty(chest.Prefab))
		{
			Popup3dObjectPath = ServerRewardUtils.FormatAssetFromServerReference(chest.Prefab, ServerRewardFileExtension.Prefab);
		}
		ReferenceID = chest.ReferenceId;
		if (int.TryParse(chest.ReferenceId, out var result) && result != 0 && !string.IsNullOrEmpty(Popup3dObjectPath))
		{
			NotificationPopupReward objectData = AssetLoader.GetObjectData<NotificationPopupReward>(Popup3dObjectPath);
			if (objectData != null && objectData.HackDeckBox)
			{
				string originalAssetPath = cardDatabase.GetCardPrintingById((uint)result)?.ImageAssetPath ?? string.Empty;
				PopupObjectBackgroundTexturePath = cardMaterialBuilder.TextureLoader.GetCardArtPath(originalAssetPath);
			}
			else
			{
				TempRewardTranslation.LookupBoosterTextures(result, out PopupObjectBackgroundTexturePath, out PopupObjectForegroundTexturePath, WrapperController.Instance.AssetLookupSystem);
			}
		}
	}

	public RewardDisplayData(ClientObjectiveBubbleUXInfo objectiveBubbleUXInfo, Client_ChestData chest, ICardDataProvider cardDatabase, CardMaterialBuilder cardMaterialBuilder)
	{
		MainText = objectiveBubbleUXInfo?.PopupUXInfo?.HeaderLocKey2 ?? chest?.HeaderLocKey;
		MainText.Parameters = chest?.LocParams;
		SecondaryText = objectiveBubbleUXInfo?.PopupUXInfo?.DescriptionLocKey ?? chest?.DescriptionLocKey;
		Quantity = chest?.Quantity ?? 1;
		string text = objectiveBubbleUXInfo?.thumbnailImageName1 ?? chest?.Image1;
		if (!string.IsNullOrEmpty(text))
		{
			Thumbnail1Path = ServerRewardUtils.FormatAssetFromServerReference(text, ServerRewardFileExtension.PNG);
		}
		string text2 = objectiveBubbleUXInfo?.thumbnailImageName2 ?? chest?.Image2;
		if (!string.IsNullOrEmpty(text2))
		{
			Thumbnail2Path = ServerRewardUtils.FormatAssetFromServerReference(text2, ServerRewardFileExtension.PNG);
		}
		string text3 = objectiveBubbleUXInfo?.thumbnailImageName3 ?? chest?.Image3;
		if (!string.IsNullOrEmpty(text3))
		{
			Thumbnail3Path = ServerRewardUtils.FormatAssetFromServerReference(text3, ServerRewardFileExtension.PNG);
		}
		string text4 = objectiveBubbleUXInfo?.PopupUXInfo?.PrefabReferenceId ?? chest?.Prefab;
		if (!string.IsNullOrEmpty(text4))
		{
			Popup3dObjectPath = ServerRewardUtils.FormatAssetFromServerReference(text4, ServerRewardFileExtension.Prefab);
		}
		ReferenceID = chest?.ReferenceId;
		if (ReferenceID != null && int.TryParse(ReferenceID, out var result) && result != 0 && !string.IsNullOrEmpty(Popup3dObjectPath))
		{
			NotificationPopupReward objectData = AssetLoader.GetObjectData<NotificationPopupReward>(Popup3dObjectPath);
			if (objectData != null && objectData.HackDeckBox)
			{
				string originalAssetPath = cardDatabase.GetCardPrintingById((uint)result)?.ImageAssetPath ?? string.Empty;
				PopupObjectBackgroundTexturePath = cardMaterialBuilder.TextureLoader.GetCardArtPath(originalAssetPath);
			}
			else
			{
				TempRewardTranslation.LookupBoosterTextures(result, out PopupObjectBackgroundTexturePath, out PopupObjectForegroundTexturePath, WrapperController.Instance.AssetLookupSystem);
			}
		}
	}

	public static bool TryParseCard(string referenceId, out uint grpId, out string styleName)
	{
		grpId = 0u;
		styleName = null;
		if (string.IsNullOrEmpty(referenceId))
		{
			return false;
		}
		string[] array = referenceId.Split('.');
		if (array.Length == 1)
		{
			array = referenceId.Split(' ');
		}
		if (array.Length == 1)
		{
			if (!uint.TryParse(array[0], out grpId))
			{
				return false;
			}
		}
		else if (!uint.TryParse(array[0], out grpId))
		{
			if (!uint.TryParse(array[1], out grpId))
			{
				return false;
			}
			styleName = array[0];
		}
		else
		{
			styleName = array[1];
		}
		return true;
	}

	public string GetSleeveName(IClientLocProvider locProvider)
	{
		if (locProvider == null)
		{
			return string.Empty;
		}
		return locProvider.GetLocalizedText(CosmeticsUtils.LocKeyForSleeveId(ReferenceID));
	}
}
