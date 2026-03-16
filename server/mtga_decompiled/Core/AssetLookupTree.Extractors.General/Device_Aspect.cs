using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Extractors.General;

public class Device_Aspect : IExtractor<int>
{
	public enum RatioCategory
	{
		FourByThree,
		ThreeByTwo,
		SixteenByTen,
		SixteenByNine,
		EighteenByNine,
		TwentyOneByNine,
		ThirtyTwoByNine
	}

	public static readonly IReadOnlyDictionary<RatioCategory, float> RatioFloats = new Dictionary<RatioCategory, float>
	{
		{
			RatioCategory.FourByThree,
			1.3333334f
		},
		{
			RatioCategory.ThreeByTwo,
			1.5f
		},
		{
			RatioCategory.SixteenByTen,
			1.6f
		},
		{
			RatioCategory.SixteenByNine,
			1.7777778f
		},
		{
			RatioCategory.EighteenByNine,
			2f
		},
		{
			RatioCategory.TwentyOneByNine,
			2.3333333f
		},
		{
			RatioCategory.ThirtyTwoByNine,
			3.5555556f
		}
	};

	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.AspectRatio.Appx(0f))
		{
			value = 0;
			return false;
		}
		foreach (KeyValuePair<RatioCategory, float> ratioFloat in RatioFloats)
		{
			if (bb.AspectRatio.Appx(ratioFloat.Value, 0.09f))
			{
				value = (int)ratioFloat.Key;
				return true;
			}
		}
		value = 0;
		return false;
	}
}
