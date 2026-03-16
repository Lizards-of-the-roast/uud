using System;

namespace Wizards.Mtga.Platforms;

[Serializable]
public struct AspectRatioRange
{
	public enum RuleType
	{
		Custom,
		Preset_4x3,
		Preset_16x9,
		Preset_18x9
	}

	public static AspectRatioRangeRule RulePreset_4x3 = new AspectRatioRangeRule(0f, 1.5f, AspectRatioRangeRule.Comparison.LessThan, minInclusive: false, maxInclusive: false);

	public static AspectRatioRangeRule RulePreset_16x9 = new AspectRatioRangeRule(1.5f, 1.9f, AspectRatioRangeRule.Comparison.Between, minInclusive: true, maxInclusive: false);

	public static AspectRatioRangeRule RulePreset_18x9 = new AspectRatioRangeRule(1.9f, 0f, AspectRatioRangeRule.Comparison.GreaterThan, minInclusive: true, maxInclusive: false);

	public RuleType ruleType;

	public AspectRatioRangeRule customRule;

	public bool Contains(float aspectRatio)
	{
		return GetRule().Contains(aspectRatio);
	}

	public AspectRatioRangeRule GetRule()
	{
		return ruleType switch
		{
			RuleType.Preset_4x3 => RulePreset_4x3, 
			RuleType.Preset_16x9 => RulePreset_16x9, 
			RuleType.Preset_18x9 => RulePreset_18x9, 
			_ => customRule, 
		};
	}

	public override string ToString()
	{
		return ruleType.ToString().Replace("_", " ") + " (" + GetRule().ToString() + ")";
	}

	public string ToStringMDNAsset()
	{
		if (ruleType == RuleType.Custom)
		{
			return "Custom-" + customRule.ToStringMDNAsset();
		}
		return ruleType.ToString().Replace("_", "-");
	}
}
