using System;
using System.Collections;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Configuration;
using Wizards.Mtga.Installation;
using Wizards.Mtga.Notifications;
using Wizards.Mtga.Platforms.Windows;
using Wizards.Mtga.Review;
using Wizards.Mtga.Storage;
using Wotc.Mtga.Loc;
using _3rdParty.Steam;

namespace Wizards.Mtga.Platforms;

public class PlatformContext
{
	private static IReviewContext _reviewContext;

	private static IStorageContext _storageContext;

	private static ILoginContext _loginContext;

	private static NotificationsContext _notificationsContext = new NotificationsContext();

	public static IClientVersionInfo CreateVersionInfo(RuntimePlatform platform, int buildNumber, string sourceVersion = "", string buildInfo = "")
	{
		return CreateVersionInfo(platform, 2026, 57, 20, buildNumber, sourceVersion, buildInfo);
	}

	public static IClientVersionInfo CreateVersionInfo(RuntimePlatform platform, int year, int major, int revision, int build, string sourceVersion = "", string buildInfo = "")
	{
		int appVersionParts = 4;
		if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
		{
			string platform2 = "windows";
			return new ClientVersionInfo(year, major, revision, build, (Version v) => v.ToString(appVersionParts), platform2, sourceVersion, buildInfo);
		}
		throw new ArgumentException($"Unsupported runtime platform: {platform}", "platform");
	}

	public static IConfigurationLoader GetConfigurationLoader()
	{
		_ = Application.platform;
		return new DefaultConfigurationLoader();
	}

	public static IQualitySelector GetQualitySelector()
	{
		return WindowsQualitySelector.Default;
	}

	public static QualityModeProvider GetQualityModeProvider()
	{
		return new QualityModeProvider();
	}

	public static Matrix4x4 GetIMGUIScale()
	{
		if (PlatformUtils.IsHandheld())
		{
			return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.height / 750f, (float)Screen.height / 750f, 1f));
		}
		return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.height / 1080f, (float)Screen.height / 1080f, 1f));
	}

	public static IEnumerator TryEstablishPlatformAuthentication()
	{
		_ = Application.platform;
		yield break;
	}

	public static IReviewContext GetReviewContext()
	{
		if (_reviewContext == null)
		{
			_ = Application.platform;
			_reviewContext = new DefaultReviewContext();
		}
		return _reviewContext;
	}

	public static IStorageContext GetStorageContext()
	{
		if (_storageContext == null)
		{
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
			{
				_storageContext = new WindowsStorageContext();
			}
			else
			{
				_storageContext = new DefaultStorageContext();
			}
		}
		return _storageContext;
	}

	public static ILoginContext GetLoginContext()
	{
		if (_loginContext == null)
		{
			RuntimePlatform platform = Application.platform;
			if ((uint)platform <= 2u || platform == RuntimePlatform.WindowsEditor)
			{
				if (Steam.Status == Steam.SteamStatus.Available)
				{
					_loginContext = new SteamLoginContext();
				}
			}
			else
			{
				_loginContext = new CredentialLoginContext();
			}
			if (_loginContext == null || !_loginContext.IsLoggedIn)
			{
				_loginContext = new CredentialLoginContext();
			}
		}
		return _loginContext;
	}

	public static NotificationsContext GetNotificationsContext()
	{
		return _notificationsContext;
	}

	public static IInstallationController GetInstallationController()
	{
		_ = Application.platform;
		return new NoSupportInstallationController();
	}

	private static string GetSteamServiceString()
	{
		if (Steam.Status != Steam.SteamStatus.Available)
		{
			return "SystemMessage/System_Invalid_Client_Version_Text";
		}
		return "SystemMessage/System_Invalid_Client_Version_Text_Steam";
	}

	public static string GetDistributionServiceString()
	{
		string key;
		switch (Application.platform)
		{
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.WindowsPlayer:
			key = GetSteamServiceString();
			break;
		case RuntimePlatform.IPhonePlayer:
		case RuntimePlatform.Android:
			key = "SystemMessage/System_Invalid_Client_Version_Text_Mobile";
			break;
		default:
			key = "SystemMessage/System_Invalid_Client_Version_Text";
			break;
		}
		return Languages.ActiveLocProvider.GetLocalizedText(key);
	}

	public static IUnityObjectPool CreateUnityPool(string poolId, bool keepAlive, Transform poolParent, ISplineMovementSystem splineMovementSystem)
	{
		if (GetQualityModeProvider().DisableAssetPool)
		{
			return NullUnityObjectPool.Default;
		}
		return UnityObjectPool.CreatePool(poolId, keepAlive, poolParent, splineMovementSystem);
	}

	public static IObjectPool CreateObjectPool()
	{
		if (GetQualityModeProvider().DisableObjectPool)
		{
			return NullObjectPool.Default;
		}
		return new ObjectPool();
	}
}
