using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteModifier : ButtonModifier
{
	[Serializable]
	private struct StateSpriteInfo
	{
		public ButtonStateModifier.ButtonState state;

		public Sprite sprite;
	}

	[SerializeField]
	private List<StateSpriteInfo> stateSprites;

	[SerializeField]
	private List<Image> targetImages;

	private Dictionary<ButtonStateModifier.ButtonState, Sprite> stateToSprite;

	private void Awake()
	{
		stateToSprite = new Dictionary<ButtonStateModifier.ButtonState, Sprite>();
		foreach (StateSpriteInfo stateSprite in stateSprites)
		{
			stateToSprite.Add(stateSprite.state, stateSprite.sprite);
		}
	}

	public override void UpdateModifier(ButtonStateModifier.ButtonState buttonState)
	{
		foreach (Image targetImage in targetImages)
		{
			if (!(targetImage == null))
			{
				targetImage.sprite = stateToSprite[buttonState];
			}
		}
	}
}
