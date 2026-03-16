using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public class ThresholdView : BadgeEntrySubView
{
	[SerializeField]
	private TMP_Text _currentLabel;

	[SerializeField]
	private TMP_Text _thresholdLabel;

	[SerializeField]
	private TMP_Text _divider;

	[SerializeField]
	private Image _thresholdSlider;

	[SerializeField]
	private Material _defaultFontMaterial;

	[SerializeField]
	private Material _thresholdMetMaterial;

	protected override void NumericInit(INumericBadgeCalculator calculator, NumericBadgeCalculatorInput input)
	{
		base.NumericInit(calculator, input);
		_badgeEntryData.NumberCalculator.GetNumber(input, out var number, out var _);
		if (_badgeEntryData.NumberCalculator is BinaryTriggerConditionMetCalculator)
		{
			Setup(number, 1, 0f, useGlowMaterial: false);
		}
		else if (_badgeEntryData.NumberCalculator is IThresholdBadgeCalculator thresholdBadgeCalculator)
		{
			thresholdBadgeCalculator.GetThreshold(input, out var threshold);
			float sliderFillAmount = ((threshold == 0) ? 0f : ((float)number / (float)threshold));
			Setup(number, threshold, sliderFillAmount, number >= threshold);
		}
		else
		{
			Setup(number, 0, 0f, useGlowMaterial: false);
		}
	}

	protected override void ActivatorInit(IBadgeActivationCalculator calculator, BadgeActivationCalculatorInput input)
	{
		base.ActivatorInit(calculator, input);
		if ((bool)_thresholdMetMaterial && calculator.GetActive(input))
		{
			if ((bool)_currentLabel)
			{
				_currentLabel.fontSharedMaterial = _thresholdMetMaterial;
			}
			if ((bool)_thresholdLabel)
			{
				_thresholdLabel.fontSharedMaterial = _thresholdMetMaterial;
			}
			if ((bool)_thresholdSlider)
			{
				_thresholdSlider.fillAmount = 1f;
			}
		}
	}

	private void Setup(int currentNumber, int thresholdNumber, float sliderFillAmount, bool useGlowMaterial)
	{
		if ((bool)_currentLabel)
		{
			_currentLabel.text = currentNumber.ToString();
			_currentLabel.enabled = true;
		}
		if ((bool)_thresholdLabel)
		{
			_thresholdLabel.text = thresholdNumber.ToString();
			_thresholdLabel.enabled = true;
		}
		if ((bool)_thresholdSlider)
		{
			_thresholdSlider.fillAmount = sliderFillAmount;
			_thresholdSlider.enabled = true;
		}
		Material fontSharedMaterial = (useGlowMaterial ? _thresholdMetMaterial : _defaultFontMaterial);
		if ((bool)_currentLabel)
		{
			_currentLabel.fontSharedMaterial = fontSharedMaterial;
		}
		if ((bool)_thresholdLabel)
		{
			_thresholdLabel.fontSharedMaterial = fontSharedMaterial;
		}
		if ((bool)_divider)
		{
			_divider.fontSharedMaterial = fontSharedMaterial;
		}
	}

	public override void Cleanup()
	{
		if ((bool)_thresholdLabel)
		{
			_thresholdLabel.enabled = false;
			_thresholdLabel.fontSharedMaterial = _defaultFontMaterial;
		}
		if ((bool)_thresholdSlider)
		{
			_thresholdSlider.fillAmount = 0f;
			_thresholdSlider.enabled = false;
		}
		if ((bool)_currentLabel)
		{
			_currentLabel.fontSharedMaterial = _defaultFontMaterial;
			_currentLabel.enabled = false;
		}
		if ((bool)_divider)
		{
			_divider.fontSharedMaterial = _defaultFontMaterial;
		}
	}
}
