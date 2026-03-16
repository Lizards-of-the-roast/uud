using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Rewards;

public class RewardDisplayDataPayload : IPayload
{
	public AltAssetReference<Sprite> Thumbnail1Ref = new AltAssetReference<Sprite>();

	public AltAssetReference<Sprite> Thumbnail2Ref = new AltAssetReference<Sprite>();

	public AltAssetReference<Sprite> Thumbnail3Ref = new AltAssetReference<Sprite>();

	public AltAssetReference<NotificationPopupReward> Popup3dObjectRef = new AltAssetReference<NotificationPopupReward>();

	public AltAssetReference<Texture> PopupObjectBackgroundTextureRef = new AltAssetReference<Texture>();

	public AltAssetReference<Texture> PopupObjectForegroundTextureRef = new AltAssetReference<Texture>();

	public string MainText;

	public string RewardText;

	public string DescriptionText;

	public string SecondaryText;

	public string ProgressText;

	public RewardDisplayData GetRewardDisplayData()
	{
		return new RewardDisplayData
		{
			Thumbnail1Path = Thumbnail1Ref.RelativePath,
			Thumbnail2Path = Thumbnail2Ref.RelativePath,
			Thumbnail3Path = Thumbnail3Ref.RelativePath,
			Popup3dObjectPath = Popup3dObjectRef.RelativePath,
			PopupObjectBackgroundTexturePath = PopupObjectBackgroundTextureRef.RelativePath,
			PopupObjectForegroundTexturePath = PopupObjectForegroundTextureRef.RelativePath,
			MainText = new MTGALocalizedString
			{
				Key = MainText
			},
			RewardText = new MTGALocalizedString
			{
				Key = RewardText
			},
			DescriptionText = new MTGALocalizedString
			{
				Key = DescriptionText
			},
			SecondaryText = new MTGALocalizedString
			{
				Key = SecondaryText
			},
			ProgressText = new MTGALocalizedString
			{
				Key = ProgressText
			}
		};
	}

	public IEnumerable<string> GetFilePaths()
	{
		yield return Thumbnail1Ref.RelativePath;
		yield return Thumbnail2Ref.RelativePath;
		yield return Thumbnail3Ref.RelativePath;
		yield return Popup3dObjectRef.RelativePath;
		yield return PopupObjectBackgroundTextureRef.RelativePath;
		yield return PopupObjectForegroundTextureRef.RelativePath;
	}
}
