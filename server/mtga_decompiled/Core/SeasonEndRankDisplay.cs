using AssetLookupTree;
using AssetLookupTree.Payloads.Player.PlayerRankSprites;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

public class SeasonEndRankDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text RankDisplayPreface;

	[SerializeField]
	private TMP_Text RankDisplayTitle;

	[SerializeField]
	private Image BadgeImage;

	[SerializeField]
	private TMP_Text Details;

	private AssetLoader.AssetTracker<Sprite> _badgeImageSpriteTracker;

	public void SetRank(RankingClassType rankClass, int rankLevel, int rankStep, bool isConstructed, string seasonNameLocalized, AssetLookupSystem assetLookupSystem)
	{
		RankDisplayPreface.text = (isConstructed ? Languages.ActiveLocProvider.GetLocalizedText("MainNav/Landing/Landing_RankButton_Constructed_Label") : Languages.ActiveLocProvider.GetLocalizedText("MainNav/Landing/Landing_RankButton_Limited_Label"));
		Details.text = Languages.ActiveLocProvider.GetLocalizedText("Rank/Rank_Tier_Tooltip", ("rankDisplayName", RankUtilities.GetClassDisplayName(rankClass)), ("playerTier", rankLevel.ToString()));
		PlayerRankSprites rankSprite = RankIconUtils.GetRankSprite(assetLookupSystem, rankClass, rankLevel, isConstructed);
		if (rankSprite != null)
		{
			if (_badgeImageSpriteTracker == null)
			{
				_badgeImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("SeasonEndBadgeImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(BadgeImage, _badgeImageSpriteTracker, rankSprite.SpriteRef.RelativePath);
		}
		BadgeImage.SetNativeSize();
		RankDisplayTitle.text = seasonNameLocalized;
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(BadgeImage, _badgeImageSpriteTracker);
	}
}
