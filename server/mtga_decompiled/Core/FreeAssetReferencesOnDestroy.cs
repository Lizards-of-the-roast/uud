using System.Collections.Generic;
using UnityAsyncAwaitUtil;
using UnityEngine;

public class FreeAssetReferencesOnDestroy : MonoBehaviour
{
	public readonly HashSet<string> AssetPaths = new HashSet<string>();

	private void OnDestroy()
	{
		ReleaseAssets();
	}

	private void ReleaseAssets()
	{
		foreach (string assetPath in AssetPaths)
		{
			AssetLoader.ReleaseAsset(assetPath);
		}
		AssetPaths.Clear();
	}

	~FreeAssetReferencesOnDestroy()
	{
		SyncContextUtil.RunOnMainUnityThread(delegate
		{
			if (AssetPaths != null)
			{
				foreach (string assetPath in AssetPaths)
				{
					SimpleLog.LogWarningForRelease("FreeAssetReferencesOnDestroy not cleaned up properly. Keeping asset in memory: " + assetPath);
				}
			}
		});
	}
}
