using System.Collections.Generic;
using AssetLookupTree.Payloads.Avatar;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.AvatarView;

public abstract class AvatarColorOverridePart : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _sprite;

	private Color _defaultColor;

	private Sprite _defaultSprite;

	private Material _defaultMaterial;

	private readonly AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("AvatarFrameSprite");

	private readonly AssetLoader.AssetTracker<Material> _materialTracker = new AssetLoader.AssetTracker<Material>("AvatarFrameMaterial");

	public void GetDefaults()
	{
		_defaultColor = _sprite.color;
		_defaultSprite = _sprite.sprite;
		_defaultMaterial = _sprite.material;
	}

	public void Recolor(IEnumerable<AvatarFrameColorOverride> colorOverrides)
	{
		SetToDefaults();
		foreach (AvatarFrameColorOverride colorOverride in colorOverrides)
		{
			if (colorOverride == null)
			{
				break;
			}
			if (colorOverride.Type.HasFlag(AvatarFrameColorOverride.OverrideType.Tint))
			{
				_sprite.color = colorOverride.Tint;
			}
			if (colorOverride.Type.HasFlag(AvatarFrameColorOverride.OverrideType.SpriteSwap))
			{
				_sprite.sprite = _spriteTracker.Acquire(colorOverride.SpriteSwap);
			}
			if (colorOverride.Type.HasFlag(AvatarFrameColorOverride.OverrideType.MaterialSwap))
			{
				_sprite.material = _materialTracker.Acquire(colorOverride.MaterialSwap);
			}
		}
	}

	private void SetToDefaults()
	{
		_sprite.color = _defaultColor;
		_sprite.sprite = _defaultSprite;
		_sprite.material = _defaultMaterial;
	}

	public void Cleanup()
	{
		SetToDefaults();
		_spriteTracker.Cleanup();
		_materialTracker.Cleanup();
	}
}
