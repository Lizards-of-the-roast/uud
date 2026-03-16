using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Quality;

namespace Wizards.Mtga;

public class BILogFactory
{
	private Wizards.Arena.Client.Logging.ILogger _crossThreadLogger;

	public BILogFactory(Wizards.Arena.Client.Logging.ILogger crossThreadLogger)
	{
		_crossThreadLogger = crossThreadLogger;
	}

	public T Generate<T>(ClientBusinessEventType businessType) where T : IClientBusinessEventReq
	{
		IClientBusinessEventReq clientBusinessEventReq;
		if (businessType == ClientBusinessEventType.PlayerInventoryReport)
		{
			ClientPlayerInventory inventory = Pantry.Get<IInventoryServiceWrapper>().Inventory;
			Pantry.Get<IFrontDoorConnectionServiceWrapper>();
			clientBusinessEventReq = new PlayerInventoryReport
			{
				SealedTokens = inventory.sealedTokens,
				DraftTokens = inventory.draftTokens,
				WcMythic = inventory.wcMythic,
				WcRare = inventory.wcRare,
				WcUncommon = inventory.wcUncommon,
				WcCommon = inventory.wcCommon,
				Gems = inventory.gems,
				Gold = inventory.gold,
				CustomTokens = inventory.CustomTokens,
				EventTime = DateTime.UtcNow
			};
		}
		else
		{
			clientBusinessEventReq = default(T);
			_crossThreadLogger.ErrorFormat("[Wotc.Mtga.BI] Factory Failed to Generate payload:\t " + typeof(T).Name + "\t eventType:\t " + businessType.ToString() + "\n");
		}
		return (T)clientBusinessEventReq;
	}

	public static ClientConnected CreateClientConnected(Guid clientSessionId)
	{
		return new ClientConnected
		{
			ClientSessionId = clientSessionId.ToString(),
			EventTime = DateTime.UtcNow,
			ClientPlatform = PlatformUtils.GetClientPlatform(),
			ClientVersion = Global.VersionInfo.ContentVersion.ToString(),
			AudioMaster = MDNPlayerPrefs.PLAYERPREFS_KEY_MASTERVOLUME,
			AudioMusic = MDNPlayerPrefs.PLAYERPREFS_KEY_MUSICVOLUME,
			AudioEffects = MDNPlayerPrefs.PLAYERPREFS_KEY_SFXVOLUME,
			AudioVoice = MDNPlayerPrefs.PLAYERPREFS_KEY_VOVOLUME,
			AudioAmbience = MDNPlayerPrefs.PLAYERPREFS_KEY_AMBIENCEVOLUME,
			AudioPlayInBackground = MDNPlayerPrefs.PLAYERPREFS_KEY_BACKGROUNDAUDIO,
			GameplayDisableEmotes = MDNPlayerPrefs.DisableEmotes,
			GameplayEvergreenKeywordReminders = MDNPlayerPrefs.ShowEvergreenKeywordReminders,
			GameplayAutoTap = MDNPlayerPrefs.AutoPayMana,
			GameplayAutoOrderTriggeredAbilities = MDNPlayerPrefs.AutoOrderTriggers,
			GameplayAutoChooseReplacementEffects = MDNPlayerPrefs.AutoChooseReplacementEffects,
			GameplayShowPhaseLadder = MDNPlayerPrefs.ShowPhaseLadder,
			GameplayAllPlayModesToggle = MDNPlayerPrefs.AllPlayModesToggle,
			Language = Languages.CurrentLanguage,
			GraphicsQuality = QualitySettingsUtil.Instance.GlobalQualityLevel,
			GraphicsVSync = QualitySettings.vSyncCount,
			GraphicsTargetFrameRate = Application.targetFrameRate,
			GraphicsMotionBlur = (QualitySettingsUtil.Instance.MotionBlur ? 1 : 0),
			GraphicsAmbientOcclusion = (QualitySettingsUtil.Instance.AmbientOcclusion ? 1 : 0)
		};
	}

	public static ClientUserDeviceSpecs CreateClientUserDeviceSpecs(Guid clientSessionId)
	{
		List<Resolution> validWindowResolutions = QualitySettingsHelpers.GetValidWindowResolutions();
		List<Resolution> validFullscreenResolutions = QualitySettingsHelpers.GetValidFullscreenResolutions();
		List<SupportedResolutionInfo> list = new List<SupportedResolutionInfo>();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution res = resolutions[i];
			Predicate<Resolution> match = (Resolution e) => e.width == res.width && e.height == res.height;
			list.Add(new SupportedResolutionInfo
			{
				Width = res.width,
				Height = res.height,
				ValidForWindow = validWindowResolutions.Exists(match),
				ValidForFullscreen = validFullscreenResolutions.Exists(match)
			});
		}
		(float, float)? tuple = AndroidDPIDetail();
		return new ClientUserDeviceSpecs
		{
			ClientSessionId = clientSessionId.ToString(),
			EventTime = DateTime.UtcNow,
			ClientPlatform = PlatformUtils.GetClientPlatform(),
			GraphicsDeviceName = SystemInfo.graphicsDeviceName,
			GraphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
			GraphicsDeviceVendor = SystemInfo.graphicsDeviceVendor,
			GraphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
			GraphicsMemorySize = SystemInfo.graphicsMemorySize,
			GraphicsMultiThreaded = SystemInfo.graphicsMultiThreaded,
			GraphicsShaderLevel = SystemInfo.graphicsShaderLevel,
			DeviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
			DeviceModel = SystemInfo.deviceModel,
			DeviceType = SystemInfo.deviceType.ToString(),
			OperatingSystem = SystemInfo.operatingSystem,
			OperatingSystemFamily = SystemInfo.operatingSystemFamily.ToString(),
			ProcessorCount = SystemInfo.processorCount,
			ProcessorFrequency = SystemInfo.processorFrequency,
			ProcessorType = SystemInfo.processorType,
			SystemMemorySize = SystemInfo.systemMemorySize,
			MaxTextureSize = SystemInfo.maxTextureSize,
			IsWindowed = !Screen.fullScreen,
			GameResolution = new ResolutionInfo
			{
				Width = Screen.width,
				Height = Screen.height
			},
			MonitorResolution = new ResolutionInfo
			{
				Width = Screen.currentResolution.width,
				Height = Screen.currentResolution.height
			},
			MonitorSupportedResolutions = list,
			DotsPerInch = Screen.dpi,
			AndroidDotsPerInchX = tuple?.Item1,
			AndroidDotsPerInchY = tuple?.Item2
		};
	}

	public static (float xdpi, float ydpi)? AndroidDPIDetail()
	{
		return null;
	}
}
