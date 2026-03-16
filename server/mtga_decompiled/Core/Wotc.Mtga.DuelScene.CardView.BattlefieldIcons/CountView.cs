using System;
using TMPro;
using UnityEngine;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public class CountView : BadgeEntrySubView
{
	[SerializeField]
	protected TMP_Text _label;

	[SerializeField]
	private TMP_Text _modifierLabel;

	protected override void NumericInit(INumericBadgeCalculator calculator, NumericBadgeCalculatorInput input)
	{
		base.NumericInit(calculator, input);
		_badgeEntryData.NumberCalculator.GetNumber(input, out var number, out var modifier);
		_label.text = number.ToString();
		_label.enabled = true;
		if ((bool)_modifierLabel)
		{
			if (!string.IsNullOrWhiteSpace(modifier))
			{
				_modifierLabel.enabled = true;
				_modifierLabel.text = modifier;
			}
			else
			{
				_modifierLabel.enabled = false;
			}
		}
	}

	public override void Cleanup()
	{
		_label.enabled = false;
		if ((bool)_modifierLabel)
		{
			_modifierLabel.enabled = false;
		}
	}
}
