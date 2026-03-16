using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.BI;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Deeplink;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Threading;
using Wizards.Mtga.UI;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class AssetPrepScene : MonoBehaviour, IProgress<AssetBundleProvisionProgress>
{
	[SerializeField]
	private Transform _assetPrepScreenParent;

	private AssetPrepScreen _assetPrepScreen;

	private AssetBundleProvisionProgress bundleProvisionProgress;

	private bool hasBundleProvisionProgressUpdate;

	private long _downloadSizeBytes = -1L;

	public bool hasAllowedDownloading { get; private set; }

	public bool continueWithoutDownloading { get; private set; }

	public bool HasAllowedRetry { get; private set; }

	public static async Task<AssetPrepScene> LoadAndInit()
	{
		AssetPrepScene obj = await "AssetPrep".LoadSceneAsync<AssetPrepScene>();
		obj.Init();
		return obj;
	}

	private void Awake()
	{
		SpawnAssetPrepScreen();
		_assetPrepScreen.DownloadUI.SetActive(value: true);
		_assetPrepScreen.InfoText.gameObject.SetActive(value: false);
		_assetPrepScreen.OverlayDialogInfoTextPrefab.SetActive(value: false);
		_assetPrepScreen.RetryButton.gameObject.SetActive(value: false);
		_assetPrepScreen.ProgressPips.gameObject.SetActive(value: false);
		_assetPrepScreen.DownloadButton.gameObject.SetActive(value: false);
		_assetPrepScreen.NpeWithoutDownloadButton.gameObject.SetActive(value: false);
		_assetPrepScreen.WifiWarning.SetActive(value: false);
		_assetPrepScreen.RetryButton.onClick.AddListener(OnRetryButtonPressed);
		_assetPrepScreen.DownloadButton.onClick.AddListener(OnDownloadButtonPressed);
		_assetPrepScreen.NpeWithoutDownloadButton.onClick.AddListener(OnWithoutDownloadButtonPressed);
		_assetPrepScreen.LoadingBar.gameObject.SetActive(value: false);
		hasAllowedDownloading = false;
		continueWithoutDownloading = false;
		ScreenKeepAlive.KeepScreenAwake();
	}

	private void SpawnAssetPrepScreen()
	{
		string path = "AssetPrepScreen_Desktop_16x9";
		if (PlatformUtils.GetCurrentDeviceType() == DeviceType.Handheld)
		{
			path = ((!((double)PlatformUtils.GetCurrentAspectRatio() < 1.5)) ? "AssetPrepScreen_Handheld_16x9" : "AssetPrepScreen_Handheld_4x3");
		}
		AssetPrepScreen original = Resources.Load<AssetPrepScreen>(path);
		_assetPrepScreen = UnityEngine.Object.Instantiate(original, _assetPrepScreenParent);
	}

	public void Init()
	{
		IClientVersionInfo versionInfo = Global.VersionInfo;
		SetInitialMenuState();
		Application.deepLinkActivated += OnDeepLink;
		_assetPrepScreen.BuildVersionText.text = (versionInfo.IsDevBuild() ? versionInfo.ApplicationVersion : $"{versionInfo.ApplicationVersion} ({versionInfo.GetBuildNumber()}.{versionInfo.SourceVersion})");
	}

	private void SetInitialMenuState()
	{
		_assetPrepScreen.InfoText.text = string.Empty;
		_assetPrepScreen.OverlayDialogInfoText.text = string.Empty;
		_assetPrepScreen.InfoText.gameObject.SetActive(value: true);
		_assetPrepScreen.ProgressPips.SetActive(value: true);
		if (_assetPrepScreen.UseLoadingBar)
		{
			_assetPrepScreen.LoadingBar.SetActive(value: true);
		}
		_assetPrepScreen.RetryButton.gameObject.SetActive(value: false);
		_assetPrepScreen.DownloadButton.gameObject.SetActive(value: false);
		_assetPrepScreen.OverlayDialogInfoTextPrefab.SetActive(value: false);
		_assetPrepScreen.NpeWithoutDownloadButton.gameObject.SetActive(value: false);
		_assetPrepScreen.WifiWarning.SetActive(value: false);
	}

	public IEnumerator ShowDownloadPrompt(long downloadSizeBytes)
	{
		LogDownloadPromptBI("show");
		_downloadSizeBytes = downloadSizeBytes;
		_assetPrepScreen.DownloadButton.gameObject.SetActive(value: true);
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("Boot/Bootscene_Prompt_DownloadAmount", ("downloadsize", BytesToDisplayString(downloadSizeBytes)));
		_assetPrepScreen.InfoText.text = localizedText;
		if (_assetPrepScreen.UseOverlayDialogInfoText)
		{
			_assetPrepScreen.InfoText.gameObject.SetActive(value: false);
			_assetPrepScreen.OverlayDialogInfoTextPrefab.SetActive(value: true);
			_assetPrepScreen.OverlayDialogInfoText.text = localizedText;
		}
		bool isWifiOrCableReachable = PlatformUtils.IsWifiOrCableReachable;
		_assetPrepScreen.WifiWarning.SetActive(_assetPrepScreen.ShowWifiWarning && !isWifiOrCableReachable);
		yield return new WaitUntil(() => hasAllowedDownloading);
	}

	public IEnumerator ShowDownloadInBackgroundPrompt(long downloadSizeBytes)
	{
		LogDownloadPromptBI("show");
		_assetPrepScreen.DownloadButton.gameObject.SetActive(value: true);
		_assetPrepScreen.NpeWithoutDownloadButton.gameObject.SetActive(value: true);
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("Boot/Bootscene_Prompt_NPEBackgroundDownloadAmount", ("downloadsize", BytesToDisplayString(downloadSizeBytes)));
		_assetPrepScreen.InfoText.text = localizedText;
		if (_assetPrepScreen.UseOverlayDialogInfoText)
		{
			_assetPrepScreen.InfoText.gameObject.SetActive(value: false);
			_assetPrepScreen.OverlayDialogInfoTextPrefab.SetActive(value: true);
			_assetPrepScreen.OverlayDialogInfoText.text = localizedText;
		}
		bool isWifiOrCableReachable = PlatformUtils.IsWifiOrCableReachable;
		_assetPrepScreen.WifiWarning.SetActive(_assetPrepScreen.ShowWifiWarning && !isWifiOrCableReachable);
		yield return new WaitUntil(() => hasAllowedDownloading || continueWithoutDownloading);
	}

	public void HideDownloadPrompt()
	{
		_assetPrepScreen.DownloadButton.gameObject.SetActive(value: false);
		_assetPrepScreen.InfoText.gameObject.SetActive(value: true);
		_assetPrepScreen.WifiWarning.SetActive(value: false);
		_assetPrepScreen.NpeWithoutDownloadButton.gameObject.SetActive(value: false);
		LogDownloadPromptBI("hide");
	}

	private void Update()
	{
		if (hasBundleProvisionProgressUpdate)
		{
			UpdateProgressText(bundleProvisionProgress);
			hasBundleProvisionProgressUpdate = false;
		}
	}

	public void Report(AssetBundleProvisionProgress progress)
	{
		bundleProvisionProgress = progress;
		hasBundleProvisionProgressUpdate = true;
	}

	public IEnumerator Download_AttemptWithRetries(CancellationToken cancellationToken, Action handleFailure, int totalExpected)
	{
		AssetBundleProvisioner bundleProvisioner = Pantry.Get<AssetBundleProvisioner>();
		int totalDownloaded = 0;
		Task<AssetBundleDownloadResult> task;
		yield return bundleProvisioner.DoDownload(this, cancellationToken).WaitYield<AssetBundleDownloadResult>(out task);
		for (totalDownloaded += task.Result?.TotalCompleted ?? 0; totalDownloaded < totalExpected || (task.Result?.IsFailure ?? true); totalDownloaded += task.Result?.TotalCompleted ?? 0)
		{
			handleFailure?.Invoke();
			List<Exception> list = new List<Exception>();
			if (task.Exception?.InnerExceptions != null)
			{
				list.AddRange(task.Exception.InnerExceptions);
			}
			if (task.Result?.Exceptions != null)
			{
				list.AddRange(task.Result.Exceptions);
			}
			if (list.Count == 0 && totalDownloaded < totalExpected)
			{
				list.Add(new Exception($"Unexpected asset count downloaded ({task.Result?.TotalCompleted ?? 0}), total {totalDownloaded}/{totalExpected}."));
			}
			foreach (Exception item in list)
			{
				Report(new AssetBundleProvisionProgress(item));
				SimpleLog.LogException(item);
			}
			yield return WaitUntilRetryYield();
			yield return bundleProvisioner.DoDownload(this, cancellationToken).WaitYield<AssetBundleDownloadResult>(out task);
		}
	}

	public IEnumerator Coroutine_AttemptWithRetries(Func<Task> attempt, Action handleFailure)
	{
		Task task;
		yield return attempt().WaitYield(out task);
		if (task.Status == TaskStatus.RanToCompletion)
		{
			yield break;
		}
		while (task.IsFaulted)
		{
			handleFailure?.Invoke();
			IEnumerable<Exception> enumerable = task.Exception?.InnerExceptions;
			foreach (Exception item in enumerable ?? Enumerable.Empty<Exception>())
			{
				Report(new AssetBundleProvisionProgress(item));
				SimpleLog.LogException(item);
			}
			yield return WaitUntilRetryYield();
			yield return attempt().WaitYield(out task);
			if (task.Status == TaskStatus.RanToCompletion)
			{
				break;
			}
		}
	}

	public IEnumerator WaitForAllowDownloadConfirmationYield(long requiredBytes)
	{
		yield return ShowDownloadPrompt(requiredBytes);
		HideDownloadPrompt();
	}

	public IEnumerator WaitForDownloadInBackgroundConfirmationYield(long requiredBytes)
	{
		yield return ShowDownloadInBackgroundPrompt(requiredBytes);
		HideDownloadPrompt();
	}

	private void UpdateProgressText(AssetBundleProvisionProgress progress)
	{
		switch (progress.Stage)
		{
		case AssetBundleProvisionStage.GetManifest:
			_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_RetrievingAssetManifest");
			break;
		case AssetBundleProvisionStage.UnpackBuiltInBundles:
			_assetPrepScreen.InfoText.text = Utils.GetLocalizedPercentage((int)progress.Completed, (int)progress.Total, "Boot/BootScene_GatheringFilesProgress");
			break;
		case AssetBundleProvisionStage.CollectCompletedBundles:
			_assetPrepScreen.InfoText.text = Utils.GetLocalizedPercentage((int)progress.Completed, (int)progress.Total, "Boot/BootScene_GatheringFilesProgress");
			break;
		case AssetBundleProvisionStage.SafeModeHash:
			_assetPrepScreen.InfoText.text = Utils.GetLocalizedPercentage((int)progress.Completed, (int)progress.Total, "Boot/BootScene_SafeModeProgress");
			break;
		case AssetBundleProvisionStage.CollectDownloadList:
			_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_PreparingDownloadList");
			break;
		case AssetBundleProvisionStage.DownloadBundles:
			_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_DownloadedXOfYBytes", ("current", BytesToDisplayString(progress.Completed)), ("total", BytesToDisplayString(progress.Total)));
			break;
		case AssetBundleProvisionStage.Error:
			DisplayError(progress.Exception);
			break;
		default:
			_assetPrepScreen.InfoText.text = string.Empty;
			break;
		}
	}

	public void SetPreparingAssetsString(string locKey)
	{
		_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText(locKey);
	}

	private void DisplayError(Exception error)
	{
		if (!(error is InsufficientStorageException ex))
		{
			if (error is IOException)
			{
				_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_DiskWriteError");
			}
			else
			{
				_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_Error");
			}
		}
		else if (ex.BytesNeeded > 0)
		{
			long num = ex.BytesNeeded / 1048576 + 1;
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_DiskSpaceError_MBNeeded", ("spaceNeeded", num.ToString("N0")));
			_assetPrepScreen.InfoText.text = localizedText;
		}
		else
		{
			_assetPrepScreen.InfoText.text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_DiskSpaceError_Generic");
		}
		_assetPrepScreen.ProgressPips.SetActive(value: false);
		_assetPrepScreen.LoadingBar.SetActive(value: false);
	}

	public CustomYieldInstruction WaitUntilRetryYield()
	{
		_assetPrepScreen.RetryButton.gameObject.SetActive(value: true);
		HasAllowedRetry = false;
		return new WaitUntil(() => HasAllowedRetry);
	}

	private void OnRetryButtonPressed()
	{
		SetInitialMenuState();
		HasAllowedRetry = true;
		LogDownloadPromptBI("retry");
	}

	private void OnDownloadButtonPressed()
	{
		SetInitialMenuState();
		hasAllowedDownloading = true;
		continueWithoutDownloading = false;
		LogDownloadPromptBI("accept");
	}

	private void OnWithoutDownloadButtonPressed()
	{
		SetInitialMenuState();
		hasAllowedDownloading = false;
		continueWithoutDownloading = true;
		LogDownloadPromptBI("cancel");
	}

	public void OnDestroy()
	{
		_assetPrepScreen.RetryButton.onClick.RemoveAllListeners();
		ScreenKeepAlive.AllowScreenTimeout();
		Application.deepLinkActivated -= OnDeepLink;
	}

	public void OnDeepLink(string url)
	{
		DeepLinking.LogDeepLinkNotUsed(url, "In AssetPrepScene, DeepLink ignored", Pantry.Get<IBILogger>());
	}

	private static string BytesToDisplayString(long bytes)
	{
		double num = bytes;
		int num2 = 0;
		while (num >= 1024.0 && num2 < 3)
		{
			num /= 1024.0;
			num2++;
		}
		string item = ((num2 != 0) ? num.ToString("N2") : bytes.ToString("N0"));
		return Languages.ActiveLocProvider?.GetLocalizedText(num2 switch
		{
			0 => "MainNav/NumberOfBytes", 
			1 => "MainNav/NumberOfKilobytes", 
			2 => "MainNav/NumberOfMegabytes", 
			_ => "MainNav/NumberOfGigabytes", 
		}, ("quantity", item)) ?? string.Empty;
	}

	private void LogDownloadPromptBI(string action)
	{
		bool isWifiOrCableReachable = PlatformUtils.IsWifiOrCableReachable;
		string item = PAPA.ClientSessionId.ToString();
		string item2 = Pantry.Get<IAccountClient>()?.AccountInformation?.PersonaID;
		string sessionId = Pantry.Get<IFrontDoorConnectionServiceWrapper>().SessionId;
		BIEventType.AssetBundleDownloadPrompt.SendWithDefaults(("Action", action), ("DownloadSizeBytes", _downloadSizeBytes.ToString()), ("PlayerId", item2), ("FrontDoorSessionId", sessionId), ("ClientSessionId", item), ("ConnectionType", isWifiOrCableReachable ? "wifi" : "mobile"));
	}
}
