using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchEndDisplay : MonoBehaviour
{
	public Animator Anim;

	public GameObject RootObj;

	public MatchEndButtons ButtonsPrefab;

	[HideInInspector]
	public MatchEndButtons Buttons;

	public Image Avatar;

	public RankDisplay RankDisplay;

	public Image NewRankSprite;

	public Image NewRankGemSprite;

	public Image NewRankBase;

	public TextMeshProUGUI NewRankText;

	public TextMeshProUGUI EventTypeText;

	private AssetLoader.AssetTracker<Sprite> _avatarSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _newRankSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _newRankGemSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _newRankBaseSpriteTracker;

	public void SpawnButtons(Transform matchEndParent)
	{
		Buttons = Object.Instantiate(ButtonsPrefab, matchEndParent);
	}

	public void SetAvatar(string spritePath)
	{
		if (_avatarSpriteTracker == null)
		{
			_avatarSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MatchEndAvatarSpriteTracker");
		}
		AssetLoaderUtils.TrySetSprite(Avatar, _avatarSpriteTracker, spritePath);
	}

	public void SetAvatar(Sprite sprite)
	{
		_avatarSpriteTracker?.Cleanup();
		Avatar.sprite = sprite;
	}

	public void SetNewRankSprite(string spritePath)
	{
		if (_newRankSpriteTracker == null)
		{
			_newRankSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MatchEndNewRankSpriteTrakcer");
		}
		AssetLoaderUtils.TrySetSprite(NewRankSprite, _newRankSpriteTracker, spritePath);
	}

	public void SetGemOverlaySprite(string spritePath)
	{
		if (_newRankGemSpriteTracker == null)
		{
			_newRankGemSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MatchEndNewGemSpriteTracker");
		}
		AssetLoaderUtils.TrySetSprite(NewRankGemSprite, _newRankGemSpriteTracker, spritePath);
	}

	public void SetRankBaseSprite(string spritePath)
	{
		if (_newRankBaseSpriteTracker == null)
		{
			_newRankBaseSpriteTracker = new AssetLoader.AssetTracker<Sprite>("MatchEndNewRankBaseSpriteTracker");
		}
		AssetLoaderUtils.TrySetSprite(NewRankBase, _newRankBaseSpriteTracker, spritePath);
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(NewRankSprite, _newRankSpriteTracker);
		AssetLoaderUtils.CleanupImage(NewRankGemSprite, _newRankGemSpriteTracker);
		AssetLoaderUtils.CleanupImage(NewRankBase, _newRankBaseSpriteTracker);
		AssetLoaderUtils.CleanupImage(Avatar, _avatarSpriteTracker);
	}
}
