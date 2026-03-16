using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions;

public static class NumericInputValidation
{
	public static bool CanSubmit(uint current, INumericInputRequest request)
	{
		if (request == null)
		{
			return false;
		}
		return CanSubmit(current, request.Min, request.Max, request.InputType, request.DisallowedValues ?? Array.Empty<uint>(), request.SuggestedValues ?? Array.Empty<uint>(), request.DisallowEven, request.DisallowOdd);
	}

	public static bool CanSubmit(uint current, uint min, uint max, NumericInputType numericInputType, IEnumerable<uint> disallowedValues, IEnumerable<uint> suggestedValues, bool disallowEven, bool disallowOdd)
	{
		foreach (uint disallowedValue in disallowedValues)
		{
			if (current == disallowedValue)
			{
				return false;
			}
		}
		if (disallowEven && current % 2 == 0)
		{
			return false;
		}
		if (disallowOdd && current % 2 == 1)
		{
			return false;
		}
		uint[] array = (suggestedValues as uint[]) ?? suggestedValues.ToArray();
		if (array.Any() && numericInputType == NumericInputType.ChooseX)
		{
			uint[] array2 = array;
			foreach (uint num in array2)
			{
				if (current == num)
				{
					return true;
				}
			}
			return false;
		}
		if (min <= current)
		{
			return current <= max;
		}
		return false;
	}
}
