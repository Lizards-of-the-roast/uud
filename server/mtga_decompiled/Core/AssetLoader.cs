using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetLookupTree;
using UnityAsyncAwaitUtil;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;

public static class AssetLoader
{
	public class AssetTracker<T> where T : UnityEngine.Object
	{
		private readonly string _key;

		private T _loadedObject;

		public string LastPath { get; private set; }

		public AssetTracker(string key)
		{
			_key = key;
		}

		public T Acquire(AltAssetReference<T> reference)
		{
			return Acquire(reference.RelativePath);
		}

		public T Acquire(string path)
		{
			if (LastPath != path)
			{
				Cleanup();
				if (string.IsNullOrWhiteSpace(path))
				{
					return null;
				}
				_loadedObject = AcquireAsset<T>(path);
				LastPath = path;
			}
			return _loadedObject;
		}

		public void Cleanup()
		{
			if (LastPath != null)
			{
				ReleaseAsset(LastPath);
				LastPath = null;
			}
			_loadedObject = null;
		}

		~AssetTracker()
		{
			SyncContextUtil.RunOnMainUnityThread(delegate
			{
				if (LastPath != null)
				{
					SimpleLog.LogWarningForRelease("Asset Tracker " + _key + " not cleaned up. Keeping asset path loaded: " + LastPath);
				}
			});
		}
	}

	private static IAssetLoader _instance;

	private static readonly bool TrackAssets = Debug.isDebugBuild;

	public static readonly Dictionary<string, int> loadedPaths = new Dictionary<string, int>();

	private static IAssetLoader Instance
	{
		get
		{
			if (_instance == null)
			{
				_ = Application.isPlaying;
			}
			return _instance;
		}
	}

	public static void Initialize(IBILogger biLogger, ResourceErrorMessageManager resourceErrorMessageManager)
	{
		if (_instance != null)
		{
			_instance.Dispose();
		}
		_instance = new AssetBundleAssetLoader(PlatformContext.GetStorageContext(), biLogger);
	}

	public static void PrepareAssets(AssetBundleProvisioner bundleProvisioner, IAssetPathResolver embeddedAssetResolver = null)
	{
		Instance.PrepareAssets(bundleProvisioner, embeddedAssetResolver);
	}

	public static void Dispose()
	{
		if (Instance != null)
		{
			Instance.Dispose();
		}
	}

	private static void TrackLoadedPath(string path)
	{
		if (TrackAssets && !loadedPaths.TryAdd(path, 1))
		{
			loadedPaths[path]++;
		}
	}

	private static void UntrackLoadedPath(string path)
	{
		if (TrackAssets && loadedPaths.ContainsKey(path))
		{
			loadedPaths[path]--;
			if (loadedPaths[path] <= 0)
			{
				loadedPaths.Remove(path);
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void UntrackLoadedPaths()
	{
		loadedPaths.Clear();
	}

	private static T AcquireAsset<T>(string assetPath) where T : UnityEngine.Object
	{
		TrackLoadedPath(assetPath);
		return Instance.AcquireAsset<T>(assetPath);
	}

	public static void ReleaseAsset(string assetPath)
	{
		UntrackLoadedPath(assetPath);
		Instance.ReleaseAsset(assetPath);
	}

	public static bool AddReferenceCount(string assetPath)
	{
		return Instance.AddReferenceCount(assetPath);
	}

	public static T Instantiate<T>(AltAssetReference<T> assetReference, Transform parent = null) where T : Component
	{
		return Instantiate<T>(assetReference.RelativePath, parent);
	}

	public static GameObject Instantiate(AltAssetReference<GameObject> assetReference, Transform parent = null)
	{
		return Instantiate(assetReference.RelativePath, parent);
	}

	public static GameObject Instantiate(string assetPath, Transform parent = null)
	{
		return Instantiate(assetPath, (GameObject go) => go, parent);
	}

	public static T Instantiate<T>(string assetPath, Transform parent = null) where T : Component
	{
		return Instantiate(assetPath, (T t) => t.gameObject, parent);
	}

	public static T InstantiateSO<T>(string assetPath) where T : ScriptableObject
	{
		return AcquireAsset<T>(assetPath);
	}

	private static T Instantiate<T>(string assetPath, Func<T, GameObject> getGameObject, Transform parent = null) where T : UnityEngine.Object
	{
		T val = AcquireAsset<T>(assetPath);
		if ((bool)val)
		{
			T val2 = UnityEngine.Object.Instantiate(val);
			GameObject gameObject = getGameObject(val2);
			ReleaseOnDestroy(gameObject, assetPath);
			if ((bool)parent)
			{
				gameObject.transform.SetParent(parent, worldPositionStays: false);
			}
			return val2;
		}
		return null;
	}

	private static void ReleaseOnDestroy(GameObject go, string assetPath)
	{
		if ((bool)go)
		{
			FreeAssetReferencesOnDestroy freeAssetReferencesOnDestroy = go.GetComponent<FreeAssetReferencesOnDestroy>();
			if (!freeAssetReferencesOnDestroy)
			{
				freeAssetReferencesOnDestroy = go.AddComponent<FreeAssetReferencesOnDestroy>();
			}
			if (freeAssetReferencesOnDestroy.AssetPaths.Add(assetPath) && !go.activeSelf)
			{
				go.SetActive(value: true);
				go.SetActive(value: false);
			}
		}
	}

	public static T AcquireAndTrackAsset<T>(AssetTracker assetTracker, string assetTrackerKey, AltAssetReference<T> assetReference) where T : UnityEngine.Object
	{
		return AcquireAndTrackAsset<T>(assetTracker, assetTrackerKey, assetReference.RelativePath);
	}

	public static T AcquireAndTrackAsset<T>(GameObject go, string assetTrackerKey, AltAssetReference<T> assetReference) where T : UnityEngine.Object
	{
		return AcquireAndTrackAsset<T>(go, assetTrackerKey, assetReference.RelativePath);
	}

	public static T AcquireAndTrackAsset<T>(GameObject go, string assetTrackerKey, string assetPath) where T : UnityEngine.Object
	{
		return AcquireAndTrackAsset<T>(go.AddOrGetComponent<FreeAssetTrackerOnDestroy>().AssetTracker, assetTrackerKey, assetPath);
	}

	public static T AcquireAndTrackAsset<T>(AssetTracker assetTracker, string assetTrackerKey, string assetPath) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			assetTracker.RemoveAssetReference(assetTrackerKey);
			return null;
		}
		T val = AcquireAsset<T>(assetPath);
		if (val == null)
		{
			ReleaseAsset(assetPath);
			assetTracker.RemoveAssetReference(assetTrackerKey);
			return null;
		}
		assetTracker.AddAssetReference(assetTrackerKey, assetPath);
		return val;
	}

	public static T GetObjectData<T>(AltAssetReference<T> assetReference) where T : UnityEngine.Object
	{
		return GetObjectData<T>(assetReference?.RelativePath);
	}

	public static T GetObjectData<T>(string assetPath) where T : UnityEngine.Object
	{
		if (assetPath == null)
		{
			return null;
		}
		T result = AcquireAsset<T>(assetPath);
		ReleaseAsset(assetPath);
		return result;
	}

	public static bool HaveAsset<T>(AltAssetReference<T> assetReference) where T : UnityEngine.Object
	{
		return HaveAsset(assetReference.RelativePath);
	}

	public static bool HaveAsset(string assetPath)
	{
		return Instance.HaveAsset(assetPath);
	}

	public static IEnumerable<string> GetFilePathsForAssetType(string assetType)
	{
		return Instance.GetFilePathsForAssetType(assetType);
	}

	public static Stream GetTree<T>() where T : class, IPayload
	{
		return Instance.GetTree<T>();
	}

	public static string GetRawFilePath(string subDirectory, string fileName)
	{
		return GetRawFilePaths(subDirectory, fileName).FirstOrDefault();
	}

	public static IEnumerable<string> GetRawFilePaths(string subDirectory, string fileName)
	{
		return Instance.GetRawFilePaths(subDirectory, fileName);
	}

	public static IEnumerable<string> GetAudioPackageBasePaths()
	{
		return Instance.GetAudioPackageBasePaths();
	}

	public static IEnumerable<string> GetAudioPackagePaths()
	{
		return Instance.GetAudioPackagePaths();
	}
}
