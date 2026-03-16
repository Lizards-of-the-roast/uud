using System;
using UnityEngine;

public class GrowingBarButton : CustomButton
{
	[Serializable]
	public class GrowingBarData
	{
		public float BarRatio = 0.66667f;

		public float OffOffset = -56f;

		public float OverOffset;

		public SizeDeltaAnimationHandler SizeAnimation;
	}

	[SerializeField]
	private GrowingBarData _normalData;

	[SerializeField]
	private GrowingBarData _onClickData;

	private RectTransform _rectTransform;

	private float _lastWidth = float.MinValue;

	private void Start()
	{
		_rectTransform = base.transform as RectTransform;
		_lastWidth = _rectTransform.rect.size.x;
		ConfigureAnimation(_normalData);
		ConfigureAnimation(_onClickData);
	}

	private void Update()
	{
		float x = _rectTransform.rect.size.x;
		if (_lastWidth != x)
		{
			_lastWidth = x;
			ConfigureAnimation(_normalData);
			ConfigureAnimation(_onClickData);
			UpdateState();
		}
	}

	private void ConfigureAnimation(GrowingBarData data)
	{
		float width = (_lastWidth + data.OffOffset) / data.BarRatio;
		float width2 = (_lastWidth + data.OverOffset) / data.BarRatio;
		SetStateWidth(data.SizeAnimation.Disabled, width);
		SetStateWidth(data.SizeAnimation.MouseOff, width);
		SetStateWidth(data.SizeAnimation.MouseOver, width2);
		SetStateWidth(data.SizeAnimation.PressedOver, width2);
		SetStateWidth(data.SizeAnimation.PressedOff, width);
	}

	private void SetStateWidth(SizeDeltaAnimationState state, float width)
	{
		Vector2 value = state.Value;
		value.x = width;
		state.Value = value;
	}
}
