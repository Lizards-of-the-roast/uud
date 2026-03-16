using System.Collections.Generic;
using System.Linq;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class PacketColorCore
{
	private static Dictionary<string, string[]> _colorSortOverrides = new Dictionary<string, string[]> { 
	{
		"WG",
		new string[2] { "G", "W" }
	} };

	public static string[] SortColors(string[] colors)
	{
		return SortColors(colors, _colorSortOverrides);
	}

	public static string[] SortColors(string[] colors, Dictionary<string, string[]> overrides)
	{
		colors = colors.OrderBy((string color) => ConvertColorAbbrevationToInt(color)).ToArray();
		string key = string.Join("", colors);
		if (overrides.TryGetValue(key, out var value))
		{
			return value;
		}
		return colors;
	}

	public static int ConvertColorAbbrevationToInt(string color)
	{
		switch (color)
		{
		case "W":
			return 0;
		case "U":
			return 1;
		case "B":
			return 2;
		case "R":
			return 3;
		case "G":
			return 4;
		case "O":
		case "C":
		case "":
		case null:
			return 5;
		default:
			return 6;
		}
	}
}
