using System;
using System.Text.RegularExpressions;

namespace Core.Rewards;

public static class VanityItemRewardUtil
{
	private static readonly Regex VanityItemRegex = new Regex("(.*?)\\.(.*)", RegexOptions.IgnoreCase);

	public static void AddVanityItemIfMatch(IVanityItemReward reward, string vanityItemName)
	{
		Match match = VanityItemRegex.Match(vanityItemName);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			if (reward.VanityItemPrefix.Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				reward.AddVanityItem(value2);
			}
		}
	}
}
