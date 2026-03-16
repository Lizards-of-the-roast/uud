using System;
using System.Collections;
using System.IO;
using Assets.Core.Meta.Utilities;
using Core.Shared.Code.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Loc;

internal class SettingsPanelReportIssue : SettingsMenuPanel
{
	[SerializeField]
	private Button _reportIssueButton;

	[SerializeField]
	private Button _captureLogButton;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private TextMeshProUGUI _versionNumber;

	[SerializeField]
	private Button _versionNumberButton;

	private void Awake()
	{
		_reportIssueButton.onClick.AddListener(OnReportIssuePressed);
		_captureLogButton.onClick.AddListener(OnCaptureLogPressed);
		_backButton.OnClick.AddListener(BackButton_OnClick);
		_versionNumberButton.onClick.AddListener(OnVersionCopyClick);
	}

	public override void ShowPanel()
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/ReportABug/Version", ("version", Global.VersionInfo.GetFullVersionString()));
		_versionNumber.text = localizedText;
		_captureLogButton.gameObject.SetActive(value: true);
	}

	public override void HidePanel()
	{
		_captureLogButton.gameObject.SetActive(value: false);
	}

	private void OnReportIssuePressed()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/ReportABug/Support_URL"));
	}

	private void OnCaptureLogPressed()
	{
		StartCoroutine(CaptureLogCoroutine());
	}

	private IEnumerator CaptureLogCoroutine()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		string documentsPath = Utilities.GetLogPath() + "Issue Reports/";
		Directory.CreateDirectory(documentsPath);
		LogToFile logToFile = LoggingUtils.LogToFile;
		if (logToFile != null)
		{
			string targetPath = documentsPath + "Log" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
			yield return logToFile.CopyCurrentLogToPath(targetPath);
			try
			{
				UnityUtilities.OpenDirectory(Path.GetDirectoryName(documentsPath));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void OnVersionCopyClick()
	{
		GUIUtility.systemCopyBuffer = Global.VersionInfo.GetFullVersionString();
	}

	private void BackButton_OnClick()
	{
		_settingsMenu.GoToMainMenu();
	}

	private void OnDestroy()
	{
		_reportIssueButton.onClick.RemoveAllListeners();
		_captureLogButton.onClick.RemoveAllListeners();
		_backButton.OnClick.RemoveAllListeners();
		_versionNumberButton.onClick.RemoveAllListeners();
	}
}
