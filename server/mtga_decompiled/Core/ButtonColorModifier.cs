using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonColorModifier : ButtonModifier
{
	[Serializable]
	private struct HighlightStateColor
	{
		public ButtonStateModifier.ButtonState state;

		public Color color;
	}

	[SerializeField]
	private List<HighlightStateColor> stateColors;

	[SerializeField]
	private Graphic graphicToColor;

	private Dictionary<ButtonStateModifier.ButtonState, Color> stateToColor;

	private void Awake()
	{
		stateToColor = new Dictionary<ButtonStateModifier.ButtonState, Color>();
		foreach (HighlightStateColor stateColor in stateColors)
		{
			stateToColor.Add(stateColor.state, stateColor.color);
		}
	}

	public override void UpdateModifier(ButtonStateModifier.ButtonState buttonState)
	{
		graphicToColor.color = stateToColor[buttonState];
	}
}
