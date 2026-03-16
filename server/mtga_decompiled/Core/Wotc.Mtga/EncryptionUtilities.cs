using System.IO;
using System.Security.Cryptography;

namespace Wotc.Mtga;

public static class EncryptionUtilities
{
	public static void EncryptFile(string sourcePath, string targetPath, byte[] key, byte[] iv)
	{
		using FileStream fileStream = FileSystemUtils.OpenRead(sourcePath);
		using FileStream stream = FileSystemUtils.Create(targetPath);
		using AesManaged aesManaged = new AesManaged();
		aesManaged.Key = key;
		aesManaged.IV = iv;
		ICryptoTransform transform = aesManaged.CreateEncryptor(aesManaged.Key, aesManaged.IV);
		using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
		byte[] array = new byte[4096];
		for (int num = fileStream.Read(array, 0, array.Length); num != 0; num = fileStream.Read(array, 0, array.Length))
		{
			cryptoStream.Write(array, 0, num);
		}
	}

	public static void DecryptFile(string sourcePath, string targetPath, byte[] key, byte[] iv)
	{
		using FileStream stream = FileSystemUtils.OpenRead(sourcePath);
		using FileStream fileStream = FileSystemUtils.Create(targetPath);
		using AesManaged aesManaged = new AesManaged();
		aesManaged.Key = key;
		aesManaged.IV = iv;
		ICryptoTransform transform = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);
		using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		byte[] array = new byte[4096];
		for (int num = cryptoStream.Read(array, 0, array.Length); num != 0; num = cryptoStream.Read(array, 0, array.Length))
		{
			fileStream.Write(array, 0, num);
		}
	}
}
