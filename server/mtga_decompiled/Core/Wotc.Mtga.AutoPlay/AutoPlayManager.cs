using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayManager : IDisposable
{
	private const string AutoplayFile = "_AutoPlay.autoplay";

	private readonly AutoPlayComponentGetters _componentGetters = new AutoPlayComponentGetters();

	public readonly ICardDatabaseAdapter CardDatabase;

	public readonly CardViewBuilder CardViewBuilder;

	public readonly CardMaterialBuilder CardMaterialBuilder;

	public readonly IAccountClient AccountClient;

	private readonly Guid _sessionId;

	public List<string> GuiLogs = new List<string>();

	private AutoPlayScriptRunner _scriptRunner;

	private readonly string _logFile;

	private bool _logToGui;

	public bool IsRunning => _scriptRunner != null;

	public static string GetConfigRoot
	{
		get
		{
			if (!Application.isEditor)
			{
				return Application.persistentDataPath + "/ArenaAutoplayConfigs/";
			}
			return Application.dataPath + "/../Autoplay/ArenaAutoplayConfigs";
		}
	}

	public AutoPlayManager(ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, IAccountClient accountClient, Guid sessionId)
	{
		CardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		CardViewBuilder = cardViewBuilder;
		CardMaterialBuilder = cardMaterialBuilder;
		_sessionId = sessionId;
		AccountClient = accountClient;
		string text = Path.Combine(Utilities.GetLogPath(), "AutoplayLogs", "Canary");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture).Replace(":", "-").Replace(" ", "_");
		_logFile = Path.Combine(text, text2 + ".log");
		if (ShouldRunAutoplayOnStart())
		{
			PlayerPrefsExt.DeleteKey("WAS-RefreshToken-PreProd");
			PlayerPrefsExt.DeleteKey("WAS-RememberMe-PreProd");
			string text3 = MDNPlayerPrefs.QueuedAutoplayFile;
			MDNPlayerPrefs.QueuedAutoplayFile = null;
			if (string.IsNullOrEmpty(text3))
			{
				text3 = "_AutoPlay.autoplay";
			}
			StartScript(text3, logToGUI: true);
		}
	}

	public void StartScript(string fileName, bool logToGUI)
	{
		_logToGui = logToGUI;
		LogAction($"clientsessionId {_sessionId}");
		LogAction("deviceId " + SystemInfo.deviceUniqueIdentifier);
		LogAction("deviceModel " + SystemInfo.deviceModel);
		AutoPlayScript autoPlayScript = AutoPlayScriptFactory.CreateAutoPlayScript(fileName, LogAction, _componentGetters, this);
		if (autoPlayScript != null)
		{
			_scriptRunner = new AutoPlayScriptRunner(autoPlayScript, LogAction);
		}
	}

	public void Update()
	{
		if (_scriptRunner == null)
		{
			return;
		}
		_scriptRunner.Update();
		if (!_scriptRunner.IsFinished())
		{
			return;
		}
		GenerateScriptReport();
		_scriptRunner.Dispose();
		_scriptRunner = null;
		LogAction("_scriptRunner = null");
		if (OverridesConfiguration.Local.HasFeatureToggleValue("quit_after_automation"))
		{
			LogAction("overrides.conf 'quit_after_automation' is marked 'true'. Quitting the application");
			if (!Application.isEditor)
			{
				Application.Quit();
			}
		}
	}

	private void GenerateScriptReport()
	{
		AutoPlayReport report = _scriptRunner.Report;
		bool flag = report.Success;
		string text = report.FailDetails?.Message;
		if (flag && report.AssetErrors > 0)
		{
			flag = false;
			text = "There are asset errors";
		}
		string text2;
		if (flag)
		{
			text2 = "Status: " + AutoPlayReport.Color("Success", "green") + "\n" + report.GenerateStringReport();
		}
		else
		{
			string text3 = "Status: " + AutoPlayReport.Color("Fail", "red") + "\n";
			string text4 = "Fail Message: " + text + "\n";
			string text5 = report.FailDetails?.StringifyStackTrace();
			text2 = text3 + text4 + text5 + report.GenerateStringReport();
		}
		LogAction(_scriptRunner.RunningScriptName + " Outcome: " + (flag ? "Success" : "Fail"));
		LogAction(_scriptRunner.RunningScriptName + " Report:\n" + text2);
		SystemMessageManager.Instance.ShowMessage("Autoplay Report: " + _scriptRunner.RunningScriptName, text2, new List<SystemMessageManager.SystemMessageButtonData>
		{
			new SystemMessageManager.SystemMessageButtonData
			{
				Text = "Continue",
				Callback = delegate
				{
				},
				HideOnClick = true,
				IsCancel = false,
				IsConfirm = false
			}
		});
	}

	private void LogAction(string msg)
	{
		msg = "[AutoPlay] " + msg + "\n";
		File.AppendAllText(_logFile, msg);
		Debug.Log(msg);
		if (_logToGui)
		{
			GuiLogs.Add(msg);
		}
	}

	public static bool CanRunAutoPlay()
	{
		if (Directory.Exists(GetConfigRoot))
		{
			return Directory.GetFiles(GetConfigRoot, "*.autoplay").Any();
		}
		return false;
	}

	private static bool ShouldRunAutoplayOnStart()
	{
		if (!File.Exists(Path.Combine(GetConfigRoot, "_AutoPlay.autoplay")))
		{
			return !string.IsNullOrEmpty(MDNPlayerPrefs.QueuedAutoplayFile);
		}
		return true;
	}

	public static string[] GetAutoplayFileNames()
	{
		if (!CanRunAutoPlay())
		{
			return null;
		}
		return Directory.GetFiles(GetConfigRoot, "*.autoplay").Select(Path.GetFileName).ToArray();
	}

	public void Dispose()
	{
		_scriptRunner?.Dispose();
	}
}
