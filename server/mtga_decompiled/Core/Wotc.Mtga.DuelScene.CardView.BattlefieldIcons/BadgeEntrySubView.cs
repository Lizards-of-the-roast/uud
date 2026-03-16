using System;
using AssetLookupTree.Payloads.Ability.Metadata;
using UnityEngine;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

[Serializable]
public abstract class BadgeEntrySubView
{
	[SerializeField]
	private bool _active;

	protected IBadgeEntryData _badgeEntryData;

	public bool Active => _active;

	public void Init(IBadgeEntryData badgeEntryData, NumericBadgeCalculatorInput? numericInput = null, BadgeActivationCalculatorInput? activationInput = null)
	{
		Init(badgeEntryData);
		if (numericInput.HasValue && badgeEntryData.NumberCalculator != null)
		{
			NumericInit(badgeEntryData.NumberCalculator, numericInput.Value);
		}
		if (activationInput.HasValue && badgeEntryData.ActivationCalculator != null)
		{
			ActivatorInit(badgeEntryData.ActivationCalculator, activationInput.Value);
		}
	}

	protected virtual void Init(IBadgeEntryData badgeEntryData)
	{
		_badgeEntryData = badgeEntryData;
	}

	protected virtual void NumericInit(INumericBadgeCalculator calculator, NumericBadgeCalculatorInput input)
	{
	}

	protected virtual void ActivatorInit(IBadgeActivationCalculator calculator, BadgeActivationCalculatorInput input)
	{
	}

	public abstract void Cleanup();
}
