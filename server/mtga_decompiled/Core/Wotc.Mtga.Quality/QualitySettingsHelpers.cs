using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.Quality;

public static class QualitySettingsHelpers
{
	public static readonly string[] ShadowLevelQualityNames = new string[5] { "Off", "Low", "Medium", "High", "Ultra" };

	public static readonly string[] FPSCapLevelNames = new string[4] { "VSync", "60", "120", "Unlimited" };

	public static readonly int[] FPSLevels = new int[4] { 0, 60, 120, 0 };

	private const float TARGET_ASPECT_RATIO = 1.7777778f;

	public static readonly Resolution MIN_DEFAULT_RESOLUTION = new Resolution
	{
		width = 768,
		height = 432
	};

	public static readonly List<Resolution> WINDOWED_RESOLUTIONS = new List<Resolution>
	{
		new Resolution
		{
			width = 960,
			height = 540
		},
		new Resolution
		{
			width = 1024,
			height = 576
		},
		new Resolution
		{
			width = 1280,
			height = 720
		},
		new Resolution
		{
			width = 1366,
			height = 768
		},
		new Resolution
		{
			width = 1600,
			height = 900
		},
		new Resolution
		{
			width = 1920,
			height = 1080
		},
		new Resolution
		{
			width = 2048,
			height = 1152
		},
		new Resolution
		{
			width = 2560,
			height = 1440
		},
		new Resolution
		{
			width = 3200,
			height = 1800
		},
		new Resolution
		{
			width = 3840,
			height = 2160
		},
		new Resolution
		{
			width = 5120,
			height = 2880
		},
		new Resolution
		{
			width = 7680,
			height = 4320
		},
		new Resolution
		{
			width = 15360,
			height = 8640
		}
	};

	public static Resolution lastWindowedScreenResolution;

	public static List<Resolution> GetValidResolutions(int maxWidth, int maxHeight, bool fullScreen)
	{
		List<Resolution> list = ((!fullScreen) ? (from res in WINDOWED_RESOLUTIONS
			group res by new { res.width, res.height } into res
			select res.FirstOrDefault() into res
			where maxWidth == 0 || (res.width <= maxWidth && res.height <= maxHeight)
			select res).Reverse().ToList() : (from res in Screen.resolutions
			group res by new { res.width, res.height } into res
			select res.FirstOrDefault() into res
			where IsResolutionNative(res) || ((maxWidth == 0 || (res.width <= maxWidth && res.height <= maxHeight)) && ResolutionHasValidAspectRatio(res))
			select res).Reverse().ToList());
		if (list.Count == 0)
		{
			list.Add(MIN_DEFAULT_RESOLUTION);
		}
		return list;
	}

	public static List<Resolution> GetValidWindowResolutions()
	{
		return GetValidResolutions(Display.main.systemWidth, Display.main.systemHeight, fullScreen: false);
	}

	public static List<Resolution> GetValidFullscreenResolutions()
	{
		return GetValidResolutions(Display.main.systemWidth, Display.main.systemHeight, fullScreen: true);
	}

	public static bool ResolutionHasValidAspectRatio(Resolution resolution)
	{
		return Mathf.Approximately((float)resolution.width / (float)resolution.height, 1.7777778f);
	}

	private static bool IsResolutionNative(Resolution resolution)
	{
		if (resolution.width == Display.main.systemWidth)
		{
			return resolution.height == Display.main.systemHeight;
		}
		return false;
	}

	[Conditional("UNITY_STANDALONE")]
	public static void ForceAllowableResolution()
	{
		if (!SettingsPanelGraphics.IsScreenAllowableRatio)
		{
			UnityEngine.Debug.LogWarning($"Current screen dimensions of {Screen.width}x{Screen.height} are not an allowable ratio ({(float)Screen.width / (float)Screen.height}) so we are forcing windowed mode.");
			SettingsPanelGraphics.ForceWindowed();
		}
		else if (Screen.width > Screen.currentResolution.width || Screen.height > Screen.currentResolution.height)
		{
			UnityEngine.Debug.LogWarning($"Current screen dimensions of {Screen.width}x{Screen.height} are outside the bounds of the current screen resolution of {Screen.currentResolution.width}x{Screen.currentResolution.height} so we are forcing windowed mode.");
			SettingsPanelGraphics.ForceWindowed();
		}
		else if (Screen.width < MIN_DEFAULT_RESOLUTION.width || Screen.height < MIN_DEFAULT_RESOLUTION.height)
		{
			UnityEngine.Debug.LogWarning($"Current screen dimensions of {Screen.width}x{Screen.height} are below minimum resolution of {MIN_DEFAULT_RESOLUTION.width}x{MIN_DEFAULT_RESOLUTION.height} so we are forcing windowed mode.");
			SettingsPanelGraphics.ForceWindowed();
		}
	}
}
