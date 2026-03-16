using System.Collections.Generic;
using System.Runtime.InteropServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using UnityEngine;

public class UnityEOSSDK : MonoBehaviour
{
	private string ProductName = "YourProductNameHere";

	private string ProductVersion = "YourProductVersionHere";

	private void Start()
	{
		EOS_Unity_SessionStart();
		Result result = PlatformInterface.Initialize(new InitializeOptions
		{
			ProductName = ProductName,
			ProductVersion = ProductVersion
		});
		if (!new List<Result>
		{
			Result.Success,
			Result.AlreadyConfigured
		}.Contains(result))
		{
			Debug.Log($"Failed to initialize EOSSDK: {result}");
			return;
		}
		LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Verbose);
		LoggingInterface.SetCallback(delegate(LogMessage message)
		{
			Debug.Log(message.Message);
		});
	}

	private void OnDestroy()
	{
		EOS_Unity_SessionEnd();
	}

	[DllImport("GfxPluginEOSSDK")]
	private static extern void EOS_Unity_SessionStart();

	[DllImport("GfxPluginEOSSDK")]
	private static extern void EOS_Unity_SessionEnd();
}
