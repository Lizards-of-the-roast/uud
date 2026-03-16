using System;
using UnityEngine;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public class VividView : BadgeEntrySubView
{
	[SerializeField]
	private GameObject _whitePart;

	[SerializeField]
	private GameObject _bluePart;

	[SerializeField]
	private GameObject _blackPart;

	[SerializeField]
	private GameObject _redPart;

	[SerializeField]
	private GameObject _greenPart;

	protected override void NumericInit(INumericBadgeCalculator calculator, NumericBadgeCalculatorInput calcInput)
	{
		base.NumericInit(calculator, calcInput);
		if (!(calculator is ActivationWordAdditionalDetailCount activationWordAdditionalDetailCount))
		{
			return;
		}
		int[] values = activationWordAdditionalDetailCount.GetValues(calcInput);
		if (values == null)
		{
			return;
		}
		int[] array = values;
		for (int i = 0; i < array.Length; i++)
		{
			switch ((CardColor)array[i])
			{
			case CardColor.White:
				_whitePart.SetActive(value: true);
				break;
			case CardColor.Blue:
				_bluePart.SetActive(value: true);
				break;
			case CardColor.Black:
				_blackPart.SetActive(value: true);
				break;
			case CardColor.Red:
				_redPart.SetActive(value: true);
				break;
			case CardColor.Green:
				_greenPart.SetActive(value: true);
				break;
			}
		}
	}

	public override void Cleanup()
	{
		_whitePart.SetActive(value: false);
		_bluePart.SetActive(value: false);
		_blackPart.SetActive(value: false);
		_redPart.SetActive(value: false);
		_greenPart.SetActive(value: false);
	}
}
