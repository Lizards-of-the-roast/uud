using System.Security.Cryptography;
using System.Text;

namespace Core.Code.Utils;

public static class ObfuscatedEmailHasher
{
	public static byte[] Hash(string email)
	{
		string text = email.ToLowerInvariant();
		if (text.EndsWith("@googlemail.com"))
		{
			text = text.Replace("@googlemail.com", "@gmail.com");
		}
		if (text.EndsWith("@gmail.com"))
		{
			int num = text.IndexOf('@');
			string text2 = text.Substring(0, num);
			string text3 = text;
			int num2 = num;
			text = string.Concat(str1: text3.Substring(num2, text3.Length - num2), str0: text2.Replace(".", "").Replace('i', 'l').Replace('1', 'l')
				.Replace('0', 'o')
				.Replace('2', 'z')
				.Replace('5', 's'));
		}
		using SHA256 sHA = SHA256.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		return sHA.ComputeHash(bytes);
	}
}
