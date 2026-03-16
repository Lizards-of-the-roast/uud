using System.Collections.Generic;
using System.Text;
using Core.Code.AssetBundles.Manifest;
using Core.Code.Utils.PlayerPrefsUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

namespace Core;

public class BuildInfoDebugDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text client;

	[SerializeField]
	private TMP_Text source;

	[SerializeField]
	private TMP_Text bundles;

	[SerializeField]
	private TMP_Text frontdoor;

	[SerializeField]
	private TMP_Text matchdoor;

	[SerializeField]
	private VerticalLayoutGroup layout;

	[SerializeField]
	private int _truncatedHashLength = 20;

	private const string EditorAssetDatabaseSource = "AssetDatabase";

	private const string BundleStringPrefix = "B: {0}";

	private const string ClientCategory = "Client";

	private const string ENABLED = "BuildInfoDebugDisplayEnabled";

	private const string VERTICAL = "BuildInfoDebugDisplayVertical";

	private const string HORIZONTAL = "BuildInfoDebugDisplayHorizontal";

	private const string TOP = "Top";

	private const string BOT = "Bot";

	private const string LEFT = "Left";

	private const string RIGHT = "Right";

	private void Start()
	{
		client.text = "C: " + Global.VersionInfo.ApplicationVersion;
		source.text = "S: " + Global.VersionInfo.SourceVersion;
		frontdoor.text = "FD: " + Pantry.CurrentEnvironment.fdHost;
		SetMatchDoorHostText(Pantry.CurrentEnvironment.mdHost);
		UpdateBundlesText(null);
		UpdatePositioning();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void SetMatchDoorHostText(string text)
	{
		matchdoor.text = "MD: " + text;
	}

	public void UpdateBundlesText(IAssetBundleSource bundleSource, IEnumerable<IAssetFileManifest> manifests = null)
	{
		string arg;
		if (bundleSource != null)
		{
			StringBuilder stringBuilder = new StringBuilder(bundleSource.EndpointHashId + " " + AssetBundleManager.Instance.Configuration.BundleVersion);
			if (manifests != null)
			{
				foreach (IAssetFileManifest manifest in manifests)
				{
					stringBuilder.Append("\n");
					if (manifest.Priority == AssetPriority.Future)
					{
						stringBuilder.Append("Future ");
					}
					string text = "ERROR";
					if (manifest.Hash != null)
					{
						text = ((manifest.Hash.Length > _truncatedHashLength) ? (manifest.Hash.Substring(0, _truncatedHashLength - 3) + "...") : manifest.Hash);
					}
					string text2 = (string.IsNullOrEmpty(manifest.Category) ? "Client" : manifest.Category);
					stringBuilder.Append(text2 + ": " + text);
				}
			}
			if (Application.isEditor)
			{
				stringBuilder.Append("\nClient: AssetDatabase");
			}
			arg = stringBuilder.ToString();
		}
		else
		{
			arg = "AssetDatabase";
		}
		bundles.text = $"B: {arg}";
	}

	private void UpdatePositioning()
	{
		bool flag = GetEnabled();
		layout.gameObject.SetActive(flag);
		if (flag)
		{
			bool renderTop = GetRenderTop();
			bool renderLeft = GetRenderLeft();
			HorizontalAlignmentOptions horizontalAlignment = HorizontalAlignmentOptions.Left;
			if (renderLeft)
			{
				layout.childAlignment = ((!renderTop) ? TextAnchor.LowerLeft : TextAnchor.UpperLeft);
			}
			else
			{
				layout.childAlignment = (renderTop ? TextAnchor.UpperRight : TextAnchor.LowerRight);
				horizontalAlignment = HorizontalAlignmentOptions.Right;
			}
			client.horizontalAlignment = horizontalAlignment;
			source.horizontalAlignment = horizontalAlignment;
			bundles.horizontalAlignment = horizontalAlignment;
			frontdoor.horizontalAlignment = horizontalAlignment;
			matchdoor.horizontalAlignment = horizontalAlignment;
		}
	}

	private static bool GetEnabled()
	{
		return PlayerPrefsExt.GetBool("BuildInfoDebugDisplayEnabled", defaultValue: true);
	}

	private static bool GetRenderTop()
	{
		return CachedPlayerPrefs.GetString("BuildInfoDebugDisplayVertical", "Top").Equals("Top");
	}

	private static bool GetRenderLeft()
	{
		return CachedPlayerPrefs.GetString("BuildInfoDebugDisplayHorizontal", "Left").Equals("Left");
	}

	public static void RenderDebugUi(DebugInfoIMGUIOnGui gui)
	{
		bool flag = false;
		bool flag2 = GetEnabled();
		if (gui.ShowToggle(flag2, "Display Build Info") != flag2)
		{
			flag2 = !flag2;
			PlayerPrefsExt.SetBool("BuildInfoDebugDisplayEnabled", flag2);
			flag = true;
		}
		bool renderTop = GetRenderTop();
		if (gui.ShowToggle(renderTop, "Display Build Info at Top") != renderTop)
		{
			renderTop = !renderTop;
			CachedPlayerPrefs.SetString("BuildInfoDebugDisplayVertical", renderTop ? "Top" : "Bot");
			flag = true;
		}
		bool renderLeft = GetRenderLeft();
		if (gui.ShowToggle(renderLeft, "Display Build Info at Left") != renderLeft)
		{
			renderLeft = !renderLeft;
			CachedPlayerPrefs.SetString("BuildInfoDebugDisplayHorizontal", renderLeft ? "Left" : "Right");
			flag = true;
		}
		if (flag)
		{
			Object.FindObjectOfType<BuildInfoDebugDisplay>().UpdatePositioning();
		}
	}
}
