using System;
using System.Collections.Generic;
using Backtrace.Unity;
using Backtrace.Unity.Model;
using Backtrace.Unity.Types;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Storage;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Unity.Profiling;

namespace Wizards.Mtga.Diagnostics;

public class BacktraceIntegration : IDisposable
{
	public const int ErrorMessageMaxLength = 127;

	private const string ServerAddress = "https://submit.backtrace.io/mtgaint/d2b34346c32cd1ad0719437380ad50ed34001b3c9809178421c6b9b95087375d/json";

	private Func<string> _playerIDGetter;

	private readonly ArenaMemoryProfiler _arenaMemoryProfiler;

	private static BacktraceClient backtraceClient;

	public BacktraceIntegration(IStorageContext storageContext, Func<string> playerIDGetter, MonoBehaviour coroutineContext)
	{
		_playerIDGetter = playerIDGetter;
		Dictionary<string, string> attributes = new Dictionary<string, string>
		{
			{
				"application.version",
				Global.VersionInfo.GetFullVersionString()
			},
			{
				"application.content_version",
				Global.VersionInfo.ContentVersion.ToString(3)
			}
		};
		backtraceClient = BacktraceClient.Initialize(CreateConfiguration(storageContext), attributes);
		backtraceClient.BeforeSend = BeforeSend;
		_arenaMemoryProfiler = new ArenaMemoryProfiler(trackGc: true, trackGfx: true, trackAudio: true, trackVideo: true);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public void Dispose()
	{
		_playerIDGetter = null;
		_arenaMemoryProfiler?.Dispose();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		backtraceClient = null;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		AddSceneBreadcrumb(scene.name);
	}

	public static void AddSceneBreadcrumb(string scene)
	{
		backtraceClient.Breadcrumbs.Info("Scene:" + scene);
	}

	private BacktraceData BeforeSend(BacktraceData data)
	{
		data.Attributes.Attributes.Remove("device.name");
		data.Attributes.Attributes.Remove("hostname");
		IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		data.Attributes.Attributes["arena.playerid"] = _playerIDGetter();
		data.Attributes.Attributes["arena.platform"] = PlatformUtils.GetClientPlatform();
		data.Attributes.Attributes["arena.clientSessionId"] = PAPA.ClientSessionId.ToString();
		data.Attributes.Attributes["arena.frontDoorSessionId"] = frontDoorConnectionServiceWrapper.SessionId;
		data.Attributes.Attributes["arena.deviceId"] = SystemInfo.deviceUniqueIdentifier;
		EventSystem current = EventSystem.current;
		if ((object)current != null)
		{
			data.Attributes.Attributes["arena.isFocused"] = current.isFocused.ToString();
		}
		if (data.Attributes.Attributes.TryGetValue("error.message", out var value) && value.Length > 127)
		{
			data.Attributes.Attributes["error.message"] = value.Substring(0, 127);
		}
		_arenaMemoryProfiler.AddMetricsToDictionary(data.Attributes.Attributes);
		return data;
	}

	private static BacktraceConfiguration CreateConfiguration(IStorageContext storageContext)
	{
		BacktraceConfiguration backtraceConfiguration = ScriptableObject.CreateInstance<BacktraceConfiguration>();
		backtraceConfiguration.ServerUrl = "https://submit.backtrace.io/mtgaint/d2b34346c32cd1ad0719437380ad50ed34001b3c9809178421c6b9b95087375d/json";
		backtraceConfiguration.HandleUnhandledExceptions = true;
		backtraceConfiguration.ReportPerMin = 10;
		backtraceConfiguration.IgnoreSslValidation = false;
		backtraceConfiguration.UseNormalizedExceptionMessage = true;
		backtraceConfiguration.NumberOfLogs = 10u;
		backtraceConfiguration.PerformanceStatistics = false;
		backtraceConfiguration.DestroyOnLoad = false;
		backtraceConfiguration.Sampling = 0.01;
		backtraceConfiguration.GameObjectDepth = -1;
		backtraceConfiguration.Enabled = true;
		backtraceConfiguration.CreateDatabase = true;
		backtraceConfiguration.DatabasePath = storageContext.CacheDirPath + "/BacktraceDatabase";
		backtraceConfiguration.DeduplicationStrategy = DeduplicationStrategy.Default;
		backtraceConfiguration.AddUnityLogToReport = false;
		backtraceConfiguration.EnableBreadcrumbsSupport = true;
		backtraceConfiguration.AutoSendMode = true;
		backtraceConfiguration.GenerateScreenshotOnException = false;
		backtraceConfiguration.MaxRecordCount = 8;
		backtraceConfiguration.MaxDatabaseSize = 15L;
		backtraceConfiguration.RetryInterval = 60;
		backtraceConfiguration.RetryLimit = 3;
		backtraceConfiguration.RetryOrder = RetryOrder.Queue;
		return backtraceConfiguration;
	}

	public static void LogGameStartGameEndDiagnostics(ClientBusinessEventType eventType, string breadcrumbMessage)
	{
		backtraceClient.Breadcrumbs.Info("DuelSceneBI:" + breadcrumbMessage);
		backtraceClient.Send($"MDN-170854 Diagnostics: sending {eventType}");
	}
}
