using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace Wotc.Mtga;

public static class SharedUtilities
{
	private static readonly Regex RICH_TEXT_REGEX = new Regex("<[^>]*>", RegexOptions.Compiled);

	public static string StripRichText(string text)
	{
		return RICH_TEXT_REGEX.Replace(text, string.Empty);
	}

	public static string FormatDisplayName(string displayName, uint subTextSize = 100u)
	{
		string result = displayName;
		int num = displayName.LastIndexOf('#');
		if (num != -1)
		{
			result = $"{displayName.Substring(0, num)}<size={subTextSize.ToString()}%>#{displayName.Substring(num + 1)}</size>";
		}
		return result;
	}

	public static string FormatDisplayName(string displayName, Color color, uint subTextSize = 100u)
	{
		string result = displayName;
		int num = displayName.LastIndexOf('#');
		if (num != -1)
		{
			result = $"<color=#{color.ToHexString()}>{displayName.Substring(0, num)}<size={subTextSize.ToString()}%>#{displayName.Substring(num + 1)}</size></color>";
		}
		return result;
	}

	public static string FormatDisplayName(string displayName, Color mainTextColor, Color subTextColor, uint subTextSize = 100u)
	{
		if (mainTextColor.Equals(subTextColor))
		{
			return FormatDisplayName(displayName, mainTextColor, subTextSize);
		}
		int num = displayName.LastIndexOf('#');
		string result = displayName;
		if (num != -1)
		{
			result = $"<color=#{mainTextColor.ToHexString()}>{displayName.Substring(0, num)}</color><size={subTextSize.ToString()}%><color=#{subTextColor.ToHexString()}>#{displayName.Substring(num + 1)}</color></size>";
		}
		return result;
	}
}
