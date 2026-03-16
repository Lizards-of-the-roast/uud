using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions;

public static class NumericInputConversion
{
	public static NumericInputVisualState ToVisualState(uint current, INumericInputRequest request)
	{
		if (request == null)
		{
			return NumericInputVisualState.None;
		}
		return ToVisualState(current, request.Min, request.Max, request.InputType, request.DisallowedValues ?? Array.Empty<uint>(), request.SuggestedValues ?? Array.Empty<uint>(), request.DisallowEven, request.DisallowOdd);
	}

	public static NumericInputVisualState ToVisualState(uint current, uint min, uint max, NumericInputType numericInputType, IEnumerable<uint> disallowedValues, IEnumerable<uint> suggestedValues, bool disallowEven, bool disallowOdd)
	{
		NumericInputVisualState numericInputVisualState = NumericInputVisualState.None;
		if (NumericInputValidation.CanSubmit(current, min, max, numericInputType, disallowedValues, suggestedValues, disallowEven, disallowOdd))
		{
			numericInputVisualState |= NumericInputVisualState.CanSubmit;
		}
		bool flag = max - min > 5;
		if (current <= max)
		{
			numericInputVisualState |= NumericInputVisualState.IncrementEnabled;
			if (flag)
			{
				numericInputVisualState |= NumericInputVisualState.IncrementManyEnabled;
			}
		}
		if (current >= min)
		{
			numericInputVisualState |= NumericInputVisualState.DecrementEnabled;
			if (flag)
			{
				numericInputVisualState |= NumericInputVisualState.DecrementManyEnabled;
			}
		}
		return numericInputVisualState;
	}
}
