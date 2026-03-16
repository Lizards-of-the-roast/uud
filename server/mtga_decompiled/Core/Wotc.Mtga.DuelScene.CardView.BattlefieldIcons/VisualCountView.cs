using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public class VisualCountView : BadgeEntrySubView
{
	[SerializeField]
	private List<GameObject> _visualLevels;

	protected override void NumericInit(INumericBadgeCalculator calculator, NumericBadgeCalculatorInput input)
	{
		base.NumericInit(calculator, input);
		calculator.GetNumber(input, out var number, out var _);
		if (_visualLevels.Count > number)
		{
			_visualLevels[number].SetActive(value: true);
		}
		else if (_visualLevels.Any())
		{
			_visualLevels.Last().SetActive(value: true);
		}
	}

	public override void Cleanup()
	{
		for (int i = 1; i < _visualLevels.Count; i++)
		{
			_visualLevels[i].SetActive(value: false);
		}
	}
}
