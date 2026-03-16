using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;

public class ResourceErrorLogger
{
	private static string _resourceErrorManifestPath;

	private static ResourceErrorManifest loadedManifest;

	public static string ResourceErrorManifestPath
	{
		get
		{
			if (_resourceErrorManifestPath == null)
			{
				_resourceErrorManifestPath = Path.Combine(Utilities.GetLogPath(), "Resources", "ResourceErrorManifest.json");
			}
			return _resourceErrorManifestPath;
		}
	}

	public static event Action<string, Dictionary<string, string>> OnAssetBundleError;

	public static void LoadErrorManifest(IBILogger biLogger)
	{
		if (loadedManifest == null)
		{
			if (File.Exists(ResourceErrorManifestPath))
			{
				loadedManifest = ResourceErrorManifest.LoadFromFile(biLogger, ResourceErrorManifestPath);
			}
			else
			{
				loadedManifest = new ResourceErrorManifest();
			}
		}
	}

	public static async void LoadErrorManifestAsync(BILogger biLogger, Action onComplete)
	{
		await Task.Run(delegate
		{
			LoadErrorManifest(biLogger);
		});
		onComplete();
	}

	public static void LogAssetBundleError(IBILogger biLogger, string message, Dictionary<string, string> details)
	{
		LogError(biLogger, "AssetBundle", message, details);
		ResourceErrorLogger.OnAssetBundleError?.SafeInvoke(message, details);
		if (!OverridesConfiguration.Local.GetFeatureToggleValue("debug"))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Error:AssetBundle");
		stringBuilder.AppendLine($"Message:{message}");
		if (details != null)
		{
			foreach (KeyValuePair<string, string> detail in details)
			{
				stringBuilder.AppendLine($"K:{detail.Key}, V:{detail.Value}");
			}
		}
		if (MDNPlayerPrefs.DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError)
		{
			stringBuilder.AppendLine("Info: To ignore this exception turn off from the Hacks Menu (Hold the Alt Key) via [Toggle ResourceErrorLoggerShouldThrowOnAssetBundleError].");
			throw new Exception(stringBuilder.ToString());
		}
		stringBuilder.Insert(0, "You are currently suppressing AssetBundleErrors. If you wish to see non-debug functionality, please set ResourceErrorLoggerShouldThrowOnAssetBundleError to False in the Debug Hacks menu\n");
		SimpleLog.LogException(new Exception(stringBuilder.ToString()));
	}

	public static void LogError(IBILogger biLogger, string errorType, string message, Dictionary<string, string> details)
	{
		if (details == null)
		{
			details = new Dictionary<string, string>();
		}
		string value = (string.IsNullOrWhiteSpace(Pantry.CurrentEnvironment?.name) ? "Unknown" : Pantry.CurrentEnvironment.name);
		details["CurrentEnvironmentName"] = value;
		LoadErrorManifest(biLogger);
		loadedManifest.AddErrorMessage(errorType, message, details);
		loadedManifest.SerializeToFile(ResourceErrorManifestPath);
	}

	public static void SendBILogMessages(IBILogger biLogger)
	{
		LoadErrorManifest(biLogger);
		if (loadedManifest.ErrorCount != 0)
		{
			loadedManifest.BILogErrors(biLogger);
			loadedManifest.ClearErrors();
			loadedManifest.SerializeToFile(ResourceErrorManifestPath);
		}
	}
}
