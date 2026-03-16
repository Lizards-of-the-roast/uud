using System;
using UnityEngine;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public class DomainView : BadgeEntrySubView
{
	[SerializeField]
	private GameObject _plainsColor;

	[SerializeField]
	private GameObject _islandColor;

	[SerializeField]
	private GameObject _swampColor;

	[SerializeField]
	private GameObject _mountainColor;

	[SerializeField]
	private GameObject _forestColor;

	protected override void NumericInit(INumericBadgeCalculator calculator, NumericBadgeCalculatorInput input)
	{
		base.NumericInit(calculator, input);
		if (!(calculator is ActivationWordAdditionalDetailCount activationWordAdditionalDetailCount))
		{
			return;
		}
		int[] values = activationWordAdditionalDetailCount.GetValues(input);
		if (values == null)
		{
			return;
		}
		int[] array = values;
		for (int i = 0; i < array.Length; i++)
		{
			switch ((SubType)array[i])
			{
			case SubType.Plains:
				_plainsColor.SetActive(value: true);
				break;
			case SubType.Island:
				_islandColor.SetActive(value: true);
				break;
			case SubType.Swamp:
				_swampColor.SetActive(value: true);
				break;
			case SubType.Mountain:
				_mountainColor.SetActive(value: true);
				break;
			case SubType.Forest:
				_forestColor.SetActive(value: true);
				break;
			}
		}
	}

	public override void Cleanup()
	{
		_plainsColor.SetActive(value: false);
		_islandColor.SetActive(value: false);
		_swampColor.SetActive(value: false);
		_mountainColor.SetActive(value: false);
		_forestColor.SetActive(value: false);
	}
}
