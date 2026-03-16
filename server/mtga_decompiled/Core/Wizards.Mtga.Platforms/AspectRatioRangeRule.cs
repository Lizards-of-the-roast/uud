using System;
using UnityEngine;

namespace Wizards.Mtga.Platforms;

[Serializable]
public struct AspectRatioRangeRule
{
	public enum Comparison
	{
		Between,
		GreaterThan,
		LessThan
	}

	public Comparison comparison;

	public bool minInclusive;

	public bool maxInclusive;

	public float min;

	public float max;

	public AspectRatioRangeRule(float min, float max, Comparison comparison, bool minInclusive, bool maxInclusive)
	{
		this.comparison = comparison;
		this.minInclusive = minInclusive;
		this.maxInclusive = maxInclusive;
		this.min = min;
		this.max = max;
	}

	public bool Contains(float aspectRatio)
	{
		if (comparison == Comparison.GreaterThan)
		{
			if (!minInclusive)
			{
				return aspectRatio > min;
			}
			if (!(aspectRatio > min))
			{
				return Mathf.Approximately(aspectRatio, min);
			}
			return true;
		}
		if (comparison == Comparison.LessThan)
		{
			if (!maxInclusive)
			{
				return aspectRatio < max;
			}
			if (!(aspectRatio < max))
			{
				return Mathf.Approximately(aspectRatio, max);
			}
			return true;
		}
		bool num;
		if (!minInclusive)
		{
			num = aspectRatio > min;
		}
		else
		{
			if (aspectRatio > min)
			{
				goto IL_0093;
			}
			num = Mathf.Approximately(aspectRatio, min);
		}
		if (!num)
		{
			return false;
		}
		goto IL_0093;
		IL_0093:
		if (!maxInclusive)
		{
			return aspectRatio < max;
		}
		if (!(aspectRatio < max))
		{
			return Mathf.Approximately(aspectRatio, max);
		}
		return true;
	}

	public override string ToString()
	{
		if (comparison == Comparison.GreaterThan)
		{
			return "Aspect Ratio " + (minInclusive ? ">= " : "> ") + min;
		}
		if (comparison == Comparison.LessThan)
		{
			return "Aspect Ratio " + (maxInclusive ? "<= " : "< ") + max;
		}
		return "Aspect Ratio " + (minInclusive ? ">= " : "> ") + min + " && Aspect Ratio " + (maxInclusive ? "<= " : "< ") + max;
	}

	public string ToStringMDNAsset()
	{
		if (comparison == Comparison.GreaterThan)
		{
			return (minInclusive ? "GrtEq-" : "Grt-") + min;
		}
		if (comparison == Comparison.LessThan)
		{
			return (maxInclusive ? "LessEq-" : "Less-") + max;
		}
		return (minInclusive ? "GrtEq-" : "Grt-") + min + "-" + (maxInclusive ? "LessEq-" : "Less-") + max;
	}
}
