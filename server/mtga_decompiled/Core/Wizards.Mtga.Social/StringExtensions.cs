using System.Linq;
using System.Text;

namespace Wizards.Mtga.Social;

public static class StringExtensions
{
	public static string RemoveSurrogatePairs(this string input)
	{
		if (input == null)
		{
			return null;
		}
		if (!input.Any(char.IsSurrogate))
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder(input.Length);
		foreach (char item in input.Where((char x) => !char.IsSurrogate(x)))
		{
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}
}
