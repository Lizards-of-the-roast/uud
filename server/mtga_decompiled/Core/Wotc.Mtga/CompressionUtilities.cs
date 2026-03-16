using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Wotc.Mtga;

public static class CompressionUtilities
{
	public static byte[] CompressBytes(byte[] original)
	{
		if (original.Length == 0)
		{
			return original;
		}
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
			{
				gZipStream.Write(original, 0, original.Length);
			}
			array = memoryStream.ToArray();
		}
		Debug.LogFormat("Compressed data from {0} to {1} bytes.", original.Length, array.Length);
		return array;
	}

	public static byte[] DecompressBytes(byte[] original)
	{
		if (original.Length == 0)
		{
			return original;
		}
		byte[] array = new byte[0];
		using (GZipStream gZipStream = new GZipStream(new MemoryStream(original), CompressionMode.Decompress))
		{
			byte[] array2 = new byte[4096];
			using MemoryStream memoryStream = new MemoryStream();
			int count;
			while ((count = gZipStream.Read(array2, 0, array2.Length)) > 0)
			{
				memoryStream.Write(array2, 0, count);
			}
			array = memoryStream.ToArray();
		}
		Debug.LogFormat("Decompressed data from {0} to {1} bytes.", original.Length, array.Length);
		return array;
	}

	public static FileInfo CompressFile(FileInfo inFile)
	{
		if (inFile.Extension.EndsWith(".gz"))
		{
			return inFile;
		}
		FileInfo fileInfo = new FileInfo(inFile.FullName + ".gz");
		File.WriteAllBytes(fileInfo.FullName, CompressBytes(File.ReadAllBytes(inFile.FullName)));
		Debug.LogFormat("Compressed {0} from {1} to {2} bytes.", inFile.Name, inFile.Length, fileInfo.Length);
		return fileInfo;
	}

	public static FileInfo DecompressFile(FileInfo inFile)
	{
		if (!inFile.Extension.EndsWith(".gz"))
		{
			return inFile;
		}
		FileInfo fileInfo = new FileInfo(inFile.FullName.Remove(inFile.FullName.Length - 3));
		File.WriteAllBytes(fileInfo.FullName, DecompressBytes(File.ReadAllBytes(inFile.FullName)));
		Debug.LogFormat("Decompressed {0} from {1} to {2} bytes.", inFile.Name, inFile.Length, fileInfo.Length);
		return fileInfo;
	}

	public static void CompressFile(string sourcePath, string targetPath)
	{
		using FileStream fileStream = FileSystemUtils.OpenRead(sourcePath);
		using FileStream stream = FileSystemUtils.Create(targetPath);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress);
		byte[] array = new byte[4096];
		for (int num = fileStream.Read(array, 0, array.Length); num != 0; num = fileStream.Read(array, 0, array.Length))
		{
			gZipStream.Write(array, 0, num);
		}
	}

	public static void DecompressFile(string sourcePath, string targetPath)
	{
		using FileStream stream = FileSystemUtils.OpenRead(sourcePath);
		using FileStream fileStream = FileSystemUtils.Create(targetPath);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		byte[] array = new byte[4096];
		for (int num = gZipStream.Read(array, 0, array.Length); num != 0; num = gZipStream.Read(array, 0, array.Length))
		{
			fileStream.Write(array, 0, num);
		}
	}
}
