using System;
using UnityEngine;

[Serializable]
public class TooltipProperties
{
	public enum Alignment
	{
		Default,
		Center,
		TopLeft,
		TopCenter,
		TopRight,
		MiddleRight,
		BottomRight,
		BottomCenter,
		BottomLeft,
		MiddleLeft
	}

	public Alignment TooltipAlignment;

	public Vector2 Padding = new Vector2(10f, 10f);

	public Vector2 Offset = new Vector2(0f, 0f);

	public int MaxVisibleLines = 3;

	public float HoverDurationUntilShow = 1f;

	public float DelayDisableDuration;

	public float FontSize = 19.1f;

	public bool UseMousePosition;
}
