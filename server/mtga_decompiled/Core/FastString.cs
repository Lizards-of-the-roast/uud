public static class FastString
{
	public static bool StartsWith(string a, string b)
	{
		int length = a.Length;
		int length2 = b.Length;
		int i = 0;
		if (length < length2)
		{
			return false;
		}
		for (; i < length2 && a[i] == b[i]; i++)
		{
		}
		return i == length2;
	}

	public static bool EndsWith(string a, string b)
	{
		int num = a.Length - 1;
		int num2 = b.Length - 1;
		while (num >= 0 && num2 >= 0 && a[num] == b[num2])
		{
			num--;
			num2--;
		}
		if (num2 >= 0 || a.Length < b.Length)
		{
			if (num < 0)
			{
				return b.Length >= a.Length;
			}
			return false;
		}
		return true;
	}
}
