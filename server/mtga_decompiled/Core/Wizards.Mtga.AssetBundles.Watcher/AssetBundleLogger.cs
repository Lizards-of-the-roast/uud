using System;
using System.Collections.Generic;
using System.IO;
using Wizards.Mtga.Platforms;

namespace Wizards.Mtga.AssetBundles.Watcher;

public class AssetBundleLogger
{
	private bool _recordLogsOverride = OverridesConfiguration.Local.GetFeatureToggleValue("LogAssetBundles");

	private bool _recordLogsGui;

	public List<AssetBundleLog> Logs = new List<AssetBundleLog>();

	private StreamWriter _fileStream;

	public bool RecordLogs
	{
		get
		{
			if (!_recordLogsGui)
			{
				return _recordLogsOverride;
			}
			return true;
		}
		set
		{
			SetRecordLogsGui(value);
		}
	}

	public AssetBundleLogger()
	{
		SetRecordLogsGui(value: false);
	}

	private void SetRecordLogsGui(bool value)
	{
		_recordLogsGui = value;
		if (_fileStream == null && RecordLogs)
		{
			string text = Path.Combine(PlatformContext.GetStorageContext().LocalPersistedStoragePath, "AssetBundleLogs");
			string text2 = Path.Combine(text, "Log_" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");
			SimpleLog.LogForRelease("Asset Bundle Logging to file: " + text2);
			FileSystemUtils.CreateDirectory(text);
			_fileStream = FileSystemUtils.CreateText(text2);
			_fileStream.Write(AssetBundleLog.GenerateCSVHeader());
		}
	}

	public void AddLog(ABOperation operation, AssetBundleManager.LoadedAssetBundle assetBundle, string reference, double operationTimeMS = 0.0)
	{
		if (RecordLogs)
		{
			int value = 0;
			if (operation == ABOperation.RefAdded || operation == ABOperation.RefRemoved)
			{
				assetBundle.AssetsRefCount.TryGetValue(reference, out value);
			}
			AssetBundleLog item = new AssetBundleLog(operation, assetBundle.AssetBundle.name, assetBundle.RefCount, reference, value, operationTimeMS);
			_fileStream.Write(item.GenerateCSVLine());
			if (_recordLogsGui)
			{
				Logs.Add(item);
			}
		}
	}
}
