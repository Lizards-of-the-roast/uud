using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wotc.Mtga;

public static class HashingUtilities
{
	public static string GetHash(string filePath)
	{
		return FileSystemUtils.GetHash(filePath, GetHash);
	}

	public static string GetHash(Stream stream)
	{
		using MD5 mD = MD5.Create();
		return GetHexStringFromBytes(mD.ComputeHash(stream));
	}

	public static string GetHash(byte[] bytes)
	{
		using MD5 mD = MD5.Create();
		return GetHexStringFromBytes(mD.ComputeHash(bytes));
	}

	public static string GetHexStringFromBytes(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < bytes.Length; i++)
		{
			stringBuilder.Append(bytes[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static byte[] GetBytesFromHexString(string text)
	{
		int num = text.Length / 2;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = byte.Parse(text.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}
		return array;
	}
}
