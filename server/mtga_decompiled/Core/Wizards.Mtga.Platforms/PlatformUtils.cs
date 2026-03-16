using UnityEngine;
using Wizards.Models;
using _3rdParty.Steam;

namespace Wizards.Mtga.Platforms;

public class PlatformUtils
{
	public const bool UseAndroidSqlFix = false;

	public static bool IsWifiOrCableReachable => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;

	public static DeviceType GetCurrentDeviceType()
	{
		return SystemInfo.deviceType;
	}

	public static bool IsValidDeviceType()
	{
		return GetCurrentDeviceType() != DeviceType.Unknown;
	}

	public static bool IsHandheld()
	{
		return GetCurrentDeviceType() == DeviceType.Handheld;
	}

	public static bool IsAspectRatio4x3()
	{
		return AspectRatioRange.RulePreset_4x3.Contains(GetCurrentAspectRatio());
	}

	public static bool IsHandheldNon4x3()
	{
		if (IsHandheld())
		{
			return !IsAspectRatio4x3();
		}
		return false;
	}

	public static bool IsDesktop()
	{
		return GetCurrentDeviceType() == DeviceType.Desktop;
	}

	public static bool TouchSupported()
	{
		return Input.touchSupported;
	}

	public static float GetCurrentAspectRatio()
	{
		return (float)Screen.width / (float)Screen.height;
	}

	public static string GetClientPlatform()
	{
		string text = string.Empty;
		if (Steam.Status == Steam.SteamStatus.Available)
		{
			text = "Steam";
		}
		else if (EOSClient.State != EOSClientState.Uninitialized)
		{
			text = "Epic";
		}
		return text + ClientPlatform.Windows;
	}
}
