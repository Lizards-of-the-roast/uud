using System.Collections.Generic;
using AssetLookupTree;
using UnityAsyncAwaitUtil;
using UnityEngine;

public class AssetTracker
{
	private readonly Dictionary<string, string> _loadedAssetPaths = new Dictionary<string, string>();

	public T AcquireAndTrack<T>(string name, string assetPath) where T : Object
	{
		return AssetLoader.AcquireAndTrackAsset<T>(this, name, assetPath);
	}

	public T AcquireAndTrack<T>(string name, AltAssetReference<T> assetReference) where T : Object
	{
		return AssetLoader.AcquireAndTrackAsset(this, name, assetReference);
	}

	public string GetLoadedPath(string name)
	{
		_loadedAssetPaths.TryGetValue(name, out var value);
		return value;
	}

	public void AddAssetReference(string key, string path)
	{
		if (_loadedAssetPaths.TryGetValue(key, out var value))
		{
			AssetLoader.ReleaseAsset(value);
		}
		_loadedAssetPaths[key] = path;
	}

	public void RemoveAssetReference(string key)
	{
		if (_loadedAssetPaths.TryGetValue(key, out var value))
		{
			AssetLoader.ReleaseAsset(value);
			_loadedAssetPaths.Remove(key);
		}
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<string, string> loadedAssetPath in _loadedAssetPaths)
		{
			loadedAssetPath.Deconstruct(out var _, out var value);
			AssetLoader.ReleaseAsset(value);
		}
		_loadedAssetPaths.Clear();
	}

	~AssetTracker()
	{
		SyncContextUtil.RunOnMainUnityThread(delegate
		{
			foreach (KeyValuePair<string, string> loadedAssetPath in _loadedAssetPaths)
			{
				SimpleLog.LogWarningForRelease("Reference " + loadedAssetPath.Value + " named " + loadedAssetPath.Key + " not cleaned up properly.");
			}
		});
	}
}
