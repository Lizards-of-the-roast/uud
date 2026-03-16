using System;
using AssetLookupTree;
using UnityEngine;
using UnityEngine.UI;

public class RewardDisplayAvatar : MonoBehaviour
{
	private string _avatarID;

	public Image AvatarImage;

	public AudioEvent hoverSFX;

	public GameObject ApplyAvatarButton;

	public Animator ApplyAvatarButtonAnimator;

	private AssetLoader.AssetTracker<Sprite> _avatarImageSpriteTracker;

	public event Action<string> OnObjectClicked;

	public void SetAvatar(AssetLookupSystem assetLookupSystem, string avatarName)
	{
		if (!string.IsNullOrEmpty(avatarName))
		{
			_avatarID = avatarName;
			if (_avatarImageSpriteTracker == null)
			{
				_avatarImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("RewardDisplayAvaterImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(AvatarImage, _avatarImageSpriteTracker, ProfileUtilities.GetAvatarFullImagePath(assetLookupSystem, avatarName));
			hoverSFX = ProfileUtilities.GetAvatarVO(assetLookupSystem, avatarName);
		}
	}

	public void OnApplyButtonPressed()
	{
		this.OnObjectClicked?.Invoke(_avatarID);
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(AvatarImage, _avatarImageSpriteTracker);
	}
}
