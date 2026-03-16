using System;
using System.Collections;
using System.Globalization;
using Assets.Core.Shared.Code;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using WAS;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wotc.Mtga;
using _3rdParty.Steam;

public class GeneralPageGUI : IDebugGUIPage
{
	private ResourceErrorMessageManager _resourceErrorMessageManager = new ResourceErrorMessageManager();

	private Rect _guiConstructedButtonRect;

	private Rect _guiReplayButtonRect;

	private static readonly GUIContent _guiConstructedButtonText = new GUIContent("Constructed");

	private static readonly GUIContent _guiReplayButtonText = new GUIContent("ReplayAll");

	private DebugInfoIMGUIOnGui _GUI;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.General;

	public string TabName => "General";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		_GUI._clipboardString = string.Empty;
		GUIStyle style = new GUIStyle(GUI.skin.button);
		Rect rect = GUILayoutUtility.GetRect(_guiConstructedButtonText, style, GUILayout.MaxWidth(500f), GUILayout.Height(_GUI.ButtonHeight));
		if (rect.x > 0f || rect.y > 0f)
		{
			_guiConstructedButtonRect = rect;
		}
		if (GUI.Button(rect, _guiConstructedButtonText, style))
		{
			PAPA.StartGlobalCoroutine(GoToDebugConstructed());
		}
		GUIStyle style2 = new GUIStyle(GUI.skin.button);
		Rect rect2 = GUILayoutUtility.GetRect(_guiReplayButtonText, style2, GUILayout.MaxWidth(500f), GUILayout.Height(_GUI.ButtonHeight));
		if (rect2.x > 0f || rect2.y > 0f)
		{
			_guiReplayButtonRect = rect2;
		}
		if (GUI.Button(rect2, _guiReplayButtonText, style2))
		{
			Scenes.LoadScene("BatchReplayScene");
		}
		_GUI.ShowLabelInfo("UTC Timestamp", DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
		_GUI.ShowLabelInfo("Timewalk Offset", $"Offset:{ServerGameTime.TimewalkOffset.Negate()}\t Time: {DateTime.UtcNow.Add(ServerGameTime.TimewalkOffset.Negate()).ToString(CultureInfo.CurrentCulture)}");
		AccountInformation accountInformation = Pantry.Get<IAccountClient>()?.AccountInformation;
		if (accountInformation != null)
		{
			_GUI.ShowLabelInfo("Front Door Environment:", Pantry.CurrentEnvironment?.GetFullUri() ?? "");
			_GUI.ShowLabelInfo("Persona ID:", accountInformation.PersonaID);
			_GUI.ShowLabelInfo("Screen Name:", accountInformation.DisplayName);
			_GUI.ShowLabelInfo("Email:", accountInformation.Email);
			_GUI.ShowLabelInfo("Country Code:", accountInformation.CountryCode ?? "UNDEFINED");
			_GUI.ShowLabelInfo("Access Token:", accountInformation.Credentials?.Jwt ?? "");
		}
		_GUI.ShowLabelInfo("Application version:", Application.version);
		_GUI.ShowLabelInfo("Content Version:", Global.VersionInfo.ContentVersion.ToString());
		string arg = (string.IsNullOrWhiteSpace(Global.VersionInfo.BuildInfo) ? "" : ("  " + Global.VersionInfo.BuildInfo));
		_GUI.ShowLabelInfo("Build Info:", $"{Global.VersionInfo.GetBuildNumber()}.{Global.VersionInfo.SourceVersion}{arg}");
		_GUI.ShowLabelInfo("Cache path:", Application.temporaryCachePath);
		_GUI.ShowLabelInfo("Download path:", ClientPathUtilities.GetAssetDownloadPath());
		if (_GUI.ShowDebugButton("Copy To Clipboard", 500f))
		{
			GUIUtility.systemCopyBuffer = _GUI._clipboardString;
		}
		if (_GUI.ShowDebugButton("Open AAT", 500f))
		{
			string text = Pantry.CurrentEnvironment.GetFullUri().Replace("frontdoor", "adminportal").Replace($":{Pantry.CurrentEnvironment.fdPort}", "");
			Application.OpenURL("https://" + text + "/");
		}
		if (_GUI.ShowDebugButton("Open Player in AAT", 500f))
		{
			string text2 = Pantry.CurrentEnvironment.GetFullUri().Replace("frontdoor", "adminportal").Replace($":{Pantry.CurrentEnvironment.fdPort}", "");
			string personaID = Pantry.Get<IAccountClient>().AccountInformation.PersonaID;
			Application.OpenURL((personaID != null) ? ("https://" + text2 + "/player/" + personaID + "/quick") : ("https://" + text2 + "/"));
		}
		if (_GUI.ShowDebugButton("Test Asset Error Popup", 500f))
		{
			_resourceErrorMessageManager.ShowError("Test Error", "This is a test error.", ("Detail 1", "Something"), ("Detail 2", "Something Else"));
		}
		if (_GUI.ShowDebugButton("Test System Message - normal", 500f))
		{
			SystemMessageManager.Instance.ShowOk("normal Message Title", "normal message body");
		}
		if (_GUI.ShowDebugButton("Test System Message - fatal", 500f))
		{
			SystemMessageManager.Instance.ShowOk("Fatal Message Title", "Fatal message body", null, null, SystemMessageManager.SystemMessagePriority.FatalError);
		}
		_GUI.ShowLabelInfo("Device Id:", SystemInfo.deviceUniqueIdentifier);
		BuildInfoDebugDisplay.RenderDebugUi(_GUI);
		DrawDisplayEnvironmentAndBundleSelectorsToggle();
		SteamDebugDisplay.RenderDebugUI(_GUI);
		GamesightDebugDisplay.RenderDebugUI(_GUI);
	}

	private IEnumerator GoToDebugConstructed()
	{
		yield return Scenes.LoadSceneAsync("EmptyScene", LoadSceneMode.Additive);
		SceneManager.SetActiveScene(SceneManager.GetSceneByName("EmptyScene"));
		yield return SceneManager.UnloadSceneAsync("MainNavigation");
		Scenes.LoadScene("DuelSceneDebugLauncher");
	}

	private void DrawDisplayEnvironmentAndBundleSelectorsToggle()
	{
		bool displayEnvironmentAndBundleEndpointSelectors = MDNPlayerPrefs.DisplayEnvironmentAndBundleEndpointSelectors;
		bool flag = _GUI.ShowToggle(displayEnvironmentAndBundleEndpointSelectors, "Display Environment and Bundle Endpoint selectors");
		if (flag != displayEnvironmentAndBundleEndpointSelectors)
		{
			MDNPlayerPrefs.DisplayEnvironmentAndBundleEndpointSelectors = flag;
			BundleEndpointSelector bundleEndpointSelector = UnityEngine.Object.FindObjectOfType<BundleEndpointSelector>();
			UnityEngine.Object.FindObjectOfType<EnvironmentSelector>().ShowEnvironmentDropdown(flag);
			bundleEndpointSelector.ShowEndpointDropdown(flag);
		}
	}
}
