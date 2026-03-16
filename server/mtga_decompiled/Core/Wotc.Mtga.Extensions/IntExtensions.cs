using System;

namespace Wotc.Mtga.Extensions;

public static class IntExtensions
{
	public static string ToRomanNumeral(this int value)
	{
		string text = string.Empty;
		int num = Math.DivRem(value, 100, out var result);
		if (num > 0)
		{
			text = new string('C', num);
		}
		value = (byte)result;
		num = Math.DivRem(value, 50, out result);
		if (num == 1)
		{
			text += "L";
		}
		value = (byte)result;
		num = Math.DivRem(value, 10, out result);
		if (num > 0)
		{
			text += new string('X', num);
		}
		value = (byte)result;
		num = Math.DivRem(value, 5, out result);
		if (num > 0)
		{
			text += "V";
		}
		value = (byte)result;
		if (result > 0)
		{
			text += new string('I', result);
		}
		int num2 = text.IndexOf("XXXX");
		if (num2 >= 0)
		{
			int num3 = text.IndexOf("L");
			text = ((!(num3 >= 0 && num3 == num2 - 1)) ? text.Replace("XXXX", "XL") : text.Replace("LXXXX", "XC"));
		}
		num2 = text.IndexOf("IIII");
		if (num2 >= 0)
		{
			text = ((text.IndexOf("V") < 0) ? text.Replace("IIII", "IV") : text.Replace("VIIII", "IX"));
		}
		return text;
	}

	public static string ToQuantity(this int quantity, string singular, string plural = null)
	{
		if (quantity == 1)
		{
			return $"{quantity} {singular}";
		}
		if (plural == null)
		{
			return $"{quantity} {singular}s";
		}
		return $"{quantity} {plural}";
	}

	public static bool Appx(this float local, float target, float maxDifference = float.Epsilon)
	{
		return Math.Abs(local - target) < maxDifference;
	}
}
