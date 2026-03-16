using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Storage;

namespace Wotc.Mtga;

public static class ClientPathUtilities
{
	public const string CLIENT_LOCALIZATION_FOLDER = "ClientLocalization";

	public const string LOC_SQLITE_DATABASE_NAME = "ClientLocalization.sqlite";

	public const string CLIENT_ART_CROP_ROOT_FOLDER = "CardArt";

	public const string CLIENT_ART_CROP_DEFINITION_FOLDER = "CardArt/Definitions";

	public const string CLIENT_ART_CROP_FORMAT_FOLDER = "CardArt/Formats";

	public const string CLIENT_ART_CROP_DATABASE_SQL = "ArtCropDatabase.sqlite";

	public const string CLIENT_ART_CROP_DATABASE_PACKED_JSON = "ArtCropDatabase_Packed.json";

	public const string CLIENT_ALT_ROOT_FOLDER = "AssetLookupTrees";

	private static IStorageContext _storageContext;

	public static readonly string DefaultLocFolder = Path.Combine("BuildDataSources", "ClientLocalization");

	public static string[] GetLocPathsInTree(string exportPath)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(exportPath);
		if (directoryInfo.Exists)
		{
			return (from _ in directoryInfo.GetFiles("*.json", SearchOption.AllDirectories)
				select _.FullName).ToArray();
		}
		return Array.Empty<string>();
	}

	public static string GetLocDatabasePathInTree(string exportPath)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(exportPath);
		if (directoryInfo.Exists)
		{
			return (from x in directoryInfo.GetFiles("*.sqlite", SearchOption.TopDirectoryOnly)
				select x.FullName).SingleOrDefault();
		}
		return null;
	}

	public static string GetAudioDataSourceFolder(string platform)
	{
		return Path.Combine("BuildDataSources", "Audio", platform);
	}

	public static string GetAssetDownloadPath()
	{
		if (_storageContext == null)
		{
			_storageContext = PlatformContext.GetStorageContext();
		}
		return _storageContext.GetAssetBundleStoragePath();
	}

	public static void ForceCopy(string filePath, string destPath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(destPath));
				if (File.Exists(destPath))
				{
					File.SetAttributes(destPath, FileAttributes.Normal);
				}
				File.Copy(filePath, destPath, overwrite: true);
			}
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"Failed to copy {filePath} to {destPath}\n{arg}");
		}
	}
}
