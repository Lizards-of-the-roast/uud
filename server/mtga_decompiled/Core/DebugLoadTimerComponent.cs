using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Wotc.Mtga;

public class DebugLoadTimerComponent : MonoBehaviour
{
	private struct AssetLoadInfo
	{
		public string Path;

		public long LoadTime;

		public int BundleCount;

		public int TextureCount;

		public AssetLoadInfo(string path, long loadTime, int bundleCount, int textureCount)
		{
			Path = path;
			LoadTime = loadTime;
			BundleCount = bundleCount;
			TextureCount = textureCount;
		}

		public override string ToString()
		{
			return $"loadtime: {LoadTime}, bundleCount: {BundleCount}, textureCount {TextureCount}, path: {Path}";
		}
	}

	private AssetTracker tracker = new AssetTracker();

	private List<AssetLoadInfo> assetToLoadTime = new List<AssetLoadInfo>();

	private List<string> assetsToLoad = new List<string>();

	private int tickDelay;

	private string currentAsset = string.Empty;

	private int countLeft;

	private void Start()
	{
		foreach (string item in from x in AssetBundleManager.Instance.GetAllBundledFilePaths()
			where x.EndsWith(".prefab")
			select x)
		{
			assetsToLoad.Add(item);
		}
		AssetBundleManager.Instance.UnloadAllExtreme();
	}

	private void Update()
	{
		if (assetsToLoad.Count > 0)
		{
			if (tickDelay > 0)
			{
				tickDelay--;
				return;
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			string text = assetsToLoad[0];
			assetsToLoad.RemoveAt(0);
			currentAsset = text;
			countLeft = assetsToLoad.Count;
			tracker.AcquireAndTrack<GameObject>(text, text);
			IReadOnlyCollection<AssetBundleManager.LoadedAssetBundle> allLoadedAssetBundles = AssetBundleManager.Instance.AllLoadedAssetBundles;
			int num = 0;
			foreach (AssetBundleManager.LoadedAssetBundle item in allLoadedAssetBundles)
			{
				if (item.AssetBundle.name.StartsWith("Texture"))
				{
					num++;
				}
			}
			stopwatch.Stop();
			assetToLoadTime.Add(new AssetLoadInfo(text, stopwatch.ElapsedMilliseconds, allLoadedAssetBundles.Count, num));
			AssetBundleManager.Instance.UnloadAllExtreme();
			tickDelay = 1;
		}
		else
		{
			if (assetToLoadTime.Count <= 0)
			{
				return;
			}
			assetToLoadTime.Sort((AssetLoadInfo x, AssetLoadInfo y) => y.LoadTime.CompareTo(x.LoadTime));
			using (StreamWriter streamWriter = File.CreateText(Path.Combine(Utilities.GetLogPath(), "LoadTimes.log")))
			{
				foreach (AssetLoadInfo item2 in assetToLoadTime)
				{
					streamWriter.WriteLine(item2.ToString());
				}
			}
			assetToLoadTime.Clear();
		}
	}

	private void OnDestroy()
	{
		tracker.Cleanup();
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)), $"{countLeft} left - {currentAsset}");
	}
}
