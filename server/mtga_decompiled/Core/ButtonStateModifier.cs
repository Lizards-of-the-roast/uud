using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonStateModifier : MonoBehaviour
{
	public enum ButtonState
	{
		normal,
		hover,
		clicked,
		disabled
	}

	[SerializeField]
	private Button button;

	private ButtonState previousState;

	[SerializeField]
	private List<ButtonModifier> modifiers = new List<ButtonModifier>();

	private void Start()
	{
		ButtonState buttonState = GetButtonState();
		UpdateModifiers(buttonState);
		previousState = buttonState;
	}

	private void Update()
	{
		ButtonState buttonState = GetButtonState();
		if (buttonState != previousState)
		{
			UpdateModifiers(buttonState);
			previousState = buttonState;
		}
	}

	protected ButtonState GetButtonState()
	{
		if (button.image.overrideSprite == button.spriteState.pressedSprite)
		{
			return ButtonState.clicked;
		}
		if (button.image.overrideSprite == button.spriteState.highlightedSprite)
		{
			return ButtonState.hover;
		}
		if (button.image.overrideSprite == button.spriteState.disabledSprite)
		{
			return ButtonState.disabled;
		}
		return ButtonState.normal;
	}

	private void UpdateModifiers(ButtonState buttonState)
	{
		foreach (ButtonModifier modifier in modifiers)
		{
			if (!(modifier == null))
			{
				modifier.UpdateModifier(buttonState);
			}
		}
	}
}
