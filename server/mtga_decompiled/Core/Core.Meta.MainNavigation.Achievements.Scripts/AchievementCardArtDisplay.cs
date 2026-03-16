using UnityEngine;
using UnityEngine.UI;
using Wizards.GeneralUtilities;
using Wizards.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

[RequireComponent(typeof(RawImage))]
public class AchievementCardArtDisplay : AchievementCardDataView
{
	[SerializeField]
	[ArtCropFormat]
	private string _cardArtCropFormat = string.Empty;

	[SerializeField]
	[ReadOnly]
	private RawImage _cardArt;

	private readonly AssetTracker _cardAssetTracker = new AssetTracker();

	private string _assetTrackerCardArtKeyName = string.Empty;

	private CardMaterialBuilder _cardMaterialBuilder;

	private void Awake()
	{
		_cardMaterialBuilder = Pantry.Get<CardMaterialBuilder>();
		GetRawImageComponent();
	}

	private void OnDestroy()
	{
		CleanupUsages();
		_achievementData = null;
	}

	protected override void CardViewUpdate()
	{
		CleanupUsages();
		if (_achievementData != null && !string.IsNullOrEmpty(_achievementData.ArtId) && _cardMaterialBuilder != null)
		{
			_assetTrackerCardArtKeyName = _achievementData.Id.ToString() + "CardArt";
			Texture2D texture2D = _cardMaterialBuilder.TextureLoader.AcquireCardArt(_cardAssetTracker, _assetTrackerCardArtKeyName, CardArtUtil.GetArtPath(_achievementData.ArtId));
			if (texture2D != null)
			{
				_cardArt.texture = texture2D;
				_cardMaterialBuilder.CropDatabase.GetCrop(CardArtUtil.GetArtPath(_achievementData.ArtId), _cardArtCropFormat)?.ApplyToUi(_cardArt);
			}
			else
			{
				SimpleLog.LogError("AchievementCard failed to load card art for artId: " + _achievementData.ArtId);
			}
		}
	}

	private void CleanupUsages()
	{
		if (!string.IsNullOrEmpty(_assetTrackerCardArtKeyName))
		{
			_cardAssetTracker.RemoveAssetReference(_assetTrackerCardArtKeyName);
			_assetTrackerCardArtKeyName = null;
		}
	}

	private void GetRawImageComponent()
	{
		if (!(_cardArt != null))
		{
			_cardArt = GetComponent<RawImage>();
		}
	}
}
