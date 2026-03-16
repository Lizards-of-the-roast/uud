using System.Diagnostics;
using System.IO;
using UnityEngine;
using Wizards.Mtga.Storage;

namespace Wizards.Mtga.Assets;

public static class AssetBundleExtensions
{
	private static EmbeddedAssetPathResolver _cachedResolver;

	private const string BUNDLE_FIX_33_70 = "BundleFix3370";

	public static string GetAssetBundleStoragePath(this IStorageContext storage)
	{
		return Path.Combine(storage.LocalPersistedStoragePath, "Downloads");
	}

	public static string GetEmbeddedAssetBundleStoragePath(this IStorageContext storage)
	{
		return Path.Combine(storage.StreamingAssetsPath, "assets");
	}

	public static bool EnsureDownloadDirectoryExists(this IStorageContext storage)
	{
		string assetBundleStoragePath = storage.GetAssetBundleStoragePath();
		if (!Directory.Exists(assetBundleStoragePath))
		{
			Directory.CreateDirectory(assetBundleStoragePath);
			return true;
		}
		return false;
	}

	public static bool UseEmbeddedBundles(this IStorageContext storage)
	{
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android)
		{
			return true;
		}
		return false;
	}

	public static EmbeddedAssetPathResolver GetEmbeddedBundlePathResolver(this IStorageContext storageContext)
	{
		if (_cachedResolver == null && storageContext.UseEmbeddedBundles())
		{
			_cachedResolver = new EmbeddedAssetPathResolver(storageContext);
		}
		return _cachedResolver;
	}

	public static long GetDownloadBytes(this AssetFileInfo entry)
	{
		if (entry.CompressedLength <= 0)
		{
			return entry.Length;
		}
		return entry.CompressedLength;
	}

	[Conditional("UNITY_STANDALONE_OSX")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("UNITY_IOS")]
	public static void PreDownloadBundleFix(this IStorageContext storage)
	{
		if (PlayerPrefs.HasKey("BundleFix3370"))
		{
			SimpleLog.LogForRelease("Download directory previously cleared, skipping.");
			return;
		}
		string assetBundleStoragePath = storage.GetAssetBundleStoragePath();
		if (Directory.Exists(assetBundleStoragePath))
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			SimpleLog.LogForRelease("Clearing download directory " + assetBundleStoragePath);
			Directory.Delete(assetBundleStoragePath, recursive: true);
			SimpleLog.LogForRelease($"Download directory cleared in {stopwatch.Elapsed}");
		}
		else
		{
			SimpleLog.LogForRelease("No download directory found");
		}
		PlayerPrefs.SetInt("BundleFix3370", 1);
	}
}
