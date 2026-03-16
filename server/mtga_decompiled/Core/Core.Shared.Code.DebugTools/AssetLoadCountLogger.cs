using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;

namespace Core.Shared.Code.DebugTools;

public class AssetLoadCountLogger : IDisposable
{
	private readonly Dictionary<string, int> _assetLoadCounts = new Dictionary<string, int>();

	private readonly StreamWriter _fileStream;

	private readonly AssetBundleManager _assetBundleManager;

	public static bool Enabled => OverridesConfiguration.Local.GetFeatureToggleValue("LogAssetBundles");

	public AssetLoadCountLogger(AssetBundleManager assetBundleManager)
	{
		_assetBundleManager = assetBundleManager;
		_assetBundleManager.AssetLoaded += OnAssetLoaded;
		string text = Path.Combine(PlatformContext.GetStorageContext().LocalPersistedStoragePath, "AssetBundleLogs");
		string text2 = Path.Combine(text, "asset_load_counts_" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");
		SimpleLog.LogForRelease("Asset Load Counts reporting to file: " + text2);
		FileSystemUtils.CreateDirectory(text);
		_fileStream = FileSystemUtils.CreateText(text2);
		_fileStream.WriteLine(GenerateCSVHeader());
	}

	private void OnAssetLoaded(string asset)
	{
		if (_assetLoadCounts.ContainsKey(asset))
		{
			_assetLoadCounts[asset]++;
		}
		else
		{
			_assetLoadCounts[asset] = 1;
		}
	}

	private string GenerateCSVHeader()
	{
		return "Assets, Load Counts";
	}

	private string GenerateCSVData()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Clear();
		foreach (KeyValuePair<string, int> assetLoadCount in _assetLoadCounts)
		{
			stringBuilder.Append(assetLoadCount.Key);
			stringBuilder.Append(',');
			stringBuilder.Append(assetLoadCount.Value);
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		_fileStream.WriteLine(GenerateCSVData());
		_fileStream.Dispose();
		_assetBundleManager.AssetLoaded -= OnAssetLoaded;
	}
}
