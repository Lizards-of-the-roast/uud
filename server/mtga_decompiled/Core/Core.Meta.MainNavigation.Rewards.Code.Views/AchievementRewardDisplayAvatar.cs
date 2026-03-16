using AssetLookupTree;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.Meta.MainNavigation.Rewards.Code.Views;

public class AchievementRewardDisplayAvatar : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("AvatarImage")]
	private Image _avatarImage;

	private AssetLoader.AssetTracker<Sprite> _avatarImageSpriteTracker;

	public void SetAvatar(AssetLookupSystem assetLookupSystem, string avatarName)
	{
		if (!string.IsNullOrEmpty(avatarName))
		{
			if (_avatarImageSpriteTracker == null)
			{
				_avatarImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RewardDisplayAvaterImageSprite");
			}
			if (_avatarImage == null)
			{
				_avatarImage = GetComponentInChildren<Image>();
			}
			AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarImageSpriteTracker, ProfileUtilities.GetAvatarFullImagePath(assetLookupSystem, avatarName));
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_avatarImage, _avatarImageSpriteTracker);
	}
}
