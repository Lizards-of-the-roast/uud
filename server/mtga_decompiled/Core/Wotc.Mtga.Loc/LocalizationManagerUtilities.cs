using System.Collections.Generic;
using MTGA.Loc;
using UnityEngine;

namespace Wotc.Mtga.Loc;

public static class LocalizationManagerUtilities
{
	public static FontMaterialMap LoadFontMaterialFromResources(string fontName)
	{
		string fontResourcePath = LocLibrary.Instance.GetFontResourcePath(fontName);
		if (fontResourcePath != null && !string.IsNullOrWhiteSpace(fontResourcePath))
		{
			return Resources.Load<FontMaterialMap>(fontResourcePath + "_MaterialMap");
		}
		return null;
	}

	public static IEnumerable<string> GetSubKeys(string input)
	{
		string startToken = "$/";
		string endToken = "$";
		int strLength = input.Length;
		int num = 0;
		int iterations = 0;
		int maxIterations = 10;
		while (num >= 0 && num < strLength)
		{
			num = input.IndexOf(startToken, num);
			if (num >= 0)
			{
				int endIndex = input.IndexOf(endToken, num + startToken.Length);
				if (endIndex >= 0)
				{
					num += 2;
					yield return input.Substring(num, endIndex - num);
					num = endIndex;
				}
				else
				{
					num = -1;
				}
			}
			iterations++;
			if (iterations == maxIterations)
			{
				break;
			}
		}
	}

	internal static string FillSubKey(string loc, string subKey, string subLoc)
	{
		return loc.Replace("$/" + subKey + "$", subLoc);
	}
}
