using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AssetLookupTree;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.U2D;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.AssetBundles.Watcher;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Configuration;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;

public class AssetBundleManager : MonoBehaviour
{
	public class CachedUnityObject
	{
		public UnityEngine.Object Object;

		public string AssetBundleFileName;
	}

	public class LoadedAssetBundle
	{
		public struct DependencyInfo
		{
			public bool Loaded;

			public int DirectCount;

			public int DepOfDepsCount;
		}

		public AssetBundle AssetBundle;

		public int RefCount;

		public Dictionary<string, int> AssetsRefCount;

		public DependencyInfo Dependencies;

		private readonly IBILogger _biLogger;

		private readonly AssetBundleLogger _logger;

		private readonly string _assetBundleName;

		public float? unloadTime { get; private set; }

		public LoadedAssetBundle(AssetBundle assetBundle, string asset, AssetBundleLogger logger, IBILogger biLogger, double operationTimeMS)
		{
			AssetBundle = assetBundle;
			_assetBundleName = AssetBundle.name;
			RefCount = 1;
			AssetsRefCount = new Dictionary<string, int>();
			AssetsRefCount[asset] = 1;
			_logger = logger;
			_logger.AddLog(ABOperation.Loaded, this, asset, operationTimeMS);
			_biLogger = biLogger;
		}

		public void AddReference(string asset)
		{
			RefCount++;
			AssetsRefCount.TryGetValue(asset, out var value);
			AssetsRefCount[asset] = value + 1;
			_logger.AddLog(ABOperation.RefAdded, this, asset);
			unloadTime = null;
		}

		public void RemoveReference(string asset)
		{
			RefCount--;
			AssetsRefCount.TryGetValue(asset, out var value);
			AssetsRefCount[asset] = value - 1;
			_logger.AddLog(ABOperation.RefRemoved, this, asset);
			if (RefCount == 0)
			{
				if (!_assetBundleToUnloadCount.ContainsKey(_assetBundleName))
				{
					_assetBundleToUnloadCount[_assetBundleName] = 0u;
				}
				uint num = Math.Min(3u, _assetBundleToUnloadCount[_assetBundleName] + 1);
				_assetBundleToUnloadCount[_assetBundleName] = num;
				unloadTime = Time.time + (float)(num * num) * 15f;
			}
			if (RefCount < 0)
			{
				string message = "LoadedAssetBundle.RemoveReference: reference count to this AssetBundle is negative!";
				ReferenceCountError referenceCountError = new ReferenceCountError
				{
					Message = message,
					EventTime = DateTime.UtcNow,
					Count = RefCount,
					Bundle = _assetBundleName,
					Asset = asset
				};
				UnityEngine.Debug.LogError(JsonConvert.SerializeObject(referenceCountError));
				_biLogger.Send(ClientBusinessEventType.ReferenceCountError, referenceCountError);
			}
			int num2 = 0;
			foreach (KeyValuePair<string, int> item in AssetsRefCount)
			{
				if (item.Value < 0)
				{
					string message2 = "LoadedAssetBundle.RemoveReference: reference count to this asset is negative!";
					ReferenceCountError referenceCountError2 = new ReferenceCountError
					{
						Message = message2,
						EventTime = DateTime.UtcNow,
						Count = item.Value,
						Bundle = _assetBundleName,
						Asset = asset
					};
					UnityEngine.Debug.LogError(JsonConvert.SerializeObject(referenceCountError2));
					_biLogger.Send(ClientBusinessEventType.ReferenceCountError, referenceCountError2);
				}
				num2 += item.Value;
			}
			if (RefCount != num2)
			{
				string message3 = "LoadedAssetBundle.RemoveReference: reference count to this AssetBundle does not equal reference counts to its assets!";
				ReferenceCountError referenceCountError3 = new ReferenceCountError
				{
					Message = message3,
					EventTime = DateTime.UtcNow,
					Count = RefCount,
					Bundle = _assetBundleName,
					Asset = asset,
					AssetCount = num2
				};
				UnityEngine.Debug.LogError(JsonConvert.SerializeObject(referenceCountError3));
				_biLogger.Send(ClientBusinessEventType.ReferenceCountError, referenceCountError3);
			}
		}

		public void SetDependencyInfo(int directCount, int depOfDepCount)
		{
			Dependencies.Loaded = true;
			Dependencies.DirectCount = directCount;
			Dependencies.DepOfDepsCount = depOfDepCount;
		}
	}

	private static readonly Dictionary<string, uint> _assetBundleToUnloadCount = new Dictionary<string, uint>();

	private IBILogger _biLogger;

	public AssetBundleLogger ABLogger = new AssetBundleLogger();

	private IAssetPathResolver _embeddedPathResolver;

	private string _defaultBundlePathFormat;

	private readonly Dictionary<string, AssetFileInfo> bundleNameToFileInfo = new Dictionary<string, AssetFileInfo>(12000);

	private readonly Dictionary<string, string> assetPathToBundleName = new Dictionary<string, string>(35000);

	private readonly Dictionary<string, string> bundleNameToFilePathCache = new Dictionary<string, string>(12000);

	private readonly Dictionary<string, IReadOnlyCollection<string>> assetTypeToFilePathListCache = new Dictionary<string, IReadOnlyCollection<string>>(6);

	private readonly Dictionary<string, LoadedAssetBundle> loadedBundlesByFileName = new Dictionary<string, LoadedAssetBundle>(5000);

	private readonly Dictionary<string, CachedUnityObject> cachedObjectsByPath = new Dictionary<string, CachedUnityObject>(1000);

	private readonly Stopwatch stopwatch = new Stopwatch();

	private Queue<string> pendingUnloadAssets = new Queue<string>(100);

	private uint assetLoadTimePreviousFrameInMilliseconds;

	private uint assetLoadTimeThisFrameInMilliseconds;

	public static AssetBundleManager Instance { get; private set; } = null;

	public static bool AssetBundlesActive
	{
		get
		{
			if (Application.isPlaying)
			{
				if (Application.isEditor)
				{
					return Instance?.Configuration?.HasBundleSources == true;
				}
				return true;
			}
			return false;
		}
	}

	public bool Initialized { get; private set; }

	public AssetsConfiguration Configuration { get; internal set; }

	public IReadOnlyList<LoadedAssetBundle> AllLoadedAssetBundles => loadedBundlesByFileName.Values.ToList();

	public event Action<string> AssetBundleLoaded;

	public event Action<string> AssetBundleUnloaded;

	public event Action<string> AssetLoaded;

	public static void Create(AssetsConfiguration assetsConfig)
	{
		if (Instance == null)
		{
			GameObject obj = new GameObject("AssetBundleManager");
			Instance = obj.AddComponent<AssetBundleManager>();
			UnityEngine.Object.DontDestroyOnLoad(obj);
		}
		Instance.Configuration = assetsConfig;
	}

	private void Awake()
	{
		Initialized = false;
	}

	public void Initialize(IBILogger biLogger, IEnumerable<AssetFileInfo> requiredAssetsList, IAssetPathResolver embeddedPathResolver = null)
	{
		_embeddedPathResolver = embeddedPathResolver;
		_defaultBundlePathFormat = Path.Combine(ClientPathUtilities.GetAssetDownloadPath(), "{0}", "{1}");
		_biLogger = biLogger;
		Initialized = false;
		bundleNameToFileInfo.Clear();
		assetPathToBundleName.Clear();
		assetTypeToFilePathListCache.Clear();
		foreach (AssetFileInfo requiredAssets in requiredAssetsList)
		{
			bundleNameToFileInfo.Add(requiredAssets.Name, requiredAssets);
			string[] indexedAssets = requiredAssets.IndexedAssets;
			foreach (string key in indexedAssets)
			{
				assetPathToBundleName.Add(key, requiredAssets.Name);
			}
		}
		Initialized = true;
	}

	protected void OnEnable()
	{
		SpriteAtlasManager.atlasRequested += GetSpriteAtlas;
	}

	protected void OnDisable()
	{
		SpriteAtlasManager.atlasRequested -= GetSpriteAtlas;
	}

	private void GetSpriteAtlas(string atlasTag, Action<SpriteAtlas> onFound)
	{
		SpriteAtlas spriteAtlas = null;
		string spriteAtlasAssetPath = SpriteAtlasBundle.GetSpriteAtlasAssetPath(atlasTag);
		if (Initialized && IsBundledAsset(spriteAtlasAssetPath))
		{
			spriteAtlas = LoadAssetFromBundle<SpriteAtlas>(spriteAtlasAssetPath);
			UnloadAssetFromBundle(spriteAtlasAssetPath);
		}
		if (spriteAtlas == null)
		{
			ResourceErrorLogger.LogAssetBundleError(_biLogger, "Could not find requested sprite atlas", new Dictionary<string, string> { { "SpriteAtlasTag", atlasTag } });
		}
		else
		{
			onFound(spriteAtlas);
		}
	}

	public bool IsBundledAsset(string assetPath)
	{
		return assetPathToBundleName.ContainsKey(assetPath);
	}

	public IEnumerable<string> GetAllBundledFilePaths()
	{
		foreach (KeyValuePair<string, AssetFileInfo> item in bundleNameToFileInfo)
		{
			string[] indexedAssets = item.Value.IndexedAssets;
			for (int i = 0; i < indexedAssets.Length; i++)
			{
				yield return indexedAssets[i];
			}
		}
	}

	private string GetBundlePath(string bundleName)
	{
		if (bundleNameToFilePathCache.TryGetValue(bundleName, out var value))
		{
			return value;
		}
		if (bundleNameToFileInfo.TryGetValue(bundleName, out var value2))
		{
			return bundleNameToFilePathCache[bundleName] = _embeddedPathResolver?.GetAssetPath(value2) ?? string.Format(_defaultBundlePathFormat, value2.AssetType, value2.Name);
		}
		return bundleNameToFilePathCache[bundleName] = string.Empty;
	}

	public string GetAltPath(Type type)
	{
		string text = AssetLookupTreeUtils.GetPayloadTypeName(type).Split('.')[0];
		string strB = "ALT_" + text;
		foreach (string item in GetFilePathsForAssetType("ALT"))
		{
			string fileName = Path.GetFileName(item);
			fileName = fileName.Substring(0, fileName.LastIndexOf("_"));
			if (string.Compare(fileName, strB) == 0)
			{
				return item;
			}
		}
		return null;
	}

	public bool LoadBundleForAsset(string assetPath)
	{
		if (!assetPathToBundleName.TryGetValue(assetPath, out var value))
		{
			MDNPlayerPrefs.HashAllFilesOnStartup = true;
			ResourceErrorLogger.LogAssetBundleError(_biLogger, "Attempting to load bundle for unknown asset", new Dictionary<string, string> { { "AssetPath", assetPath } });
			return false;
		}
		return LoadAssetBundleAndDependencies(value, assetPath) != null;
	}

	private void CacheObject<T>(T objectToCache, string assetName, string pathName) where T : UnityEngine.Object
	{
		if (objectToCache != null)
		{
			CachedUnityObject cachedUnityObject = null;
			cachedUnityObject = new CachedUnityObject
			{
				Object = objectToCache,
				AssetBundleFileName = assetName
			};
			cachedObjectsByPath.Add(pathName, cachedUnityObject);
		}
	}

	public T LoadAssetFromBundle<T>(string assetPath) where T : UnityEngine.Object
	{
		if (cachedObjectsByPath.TryGetValue(assetPath, out var value))
		{
			if (value != null && !(value.Object == null))
			{
				UpdateLoadedBundle(value.AssetBundleFileName, assetPath);
				if (typeof(Component).IsAssignableFrom(typeof(T)) && value.Object is GameObject gameObject)
				{
					return gameObject.GetComponent<T>();
				}
				return value.Object as T;
			}
			cachedObjectsByPath.Remove(assetPath);
		}
		if (!assetPathToBundleName.TryGetValue(assetPath, out var value2))
		{
			MDNPlayerPrefs.HashAllFilesOnStartup = true;
			ResourceErrorLogger.LogAssetBundleError(_biLogger, "Attempting to load unknown asset", new Dictionary<string, string> { { "AssetPath", assetPath } });
			UnityEngine.Debug.LogError("Attempting to load unknown asset: " + assetPath);
			return null;
		}
		T val = null;
		AssetBundle assetBundle = LoadAssetBundleAndDependencies(value2, assetPath);
		if ((bool)assetBundle)
		{
			if (typeof(Component).IsAssignableFrom(typeof(T)))
			{
				int num;
				if (!MDNPlayerPrefs.HashAllFilesOnStartup)
				{
					num = ((MDNPlayerPrefs.FileToHashOnStartup != string.Empty) ? 1 : 0);
					if (num == 0)
					{
						MDNPlayerPrefs.FileToHashOnStartup = GetBundlePath(value2);
					}
				}
				else
				{
					num = 1;
				}
				stopwatch.Restart();
				GameObject gameObject2 = assetBundle.LoadAsset<GameObject>(assetPath);
				stopwatch.Stop();
				this.AssetLoaded.SafeInvoke(assetPath);
				if (num == 0)
				{
					MDNPlayerPrefs.FileToHashOnStartup = string.Empty;
				}
				if ((bool)gameObject2)
				{
					val = gameObject2.GetComponent<T>();
					CacheObject(gameObject2, value2, assetPath);
				}
			}
			else
			{
				stopwatch.Restart();
				if (value2.StartsWith("Atlas_") && typeof(Sprite).IsAssignableFrom(typeof(T)))
				{
					string key = assetBundle.GetAllAssetNames().First((string p) => p.EndsWith(".spriteatlas"));
					SpriteAtlas spriteAtlas;
					if (cachedObjectsByPath.TryGetValue(key, out var value3))
					{
						spriteAtlas = (SpriteAtlas)value3.Object;
					}
					else
					{
						spriteAtlas = assetBundle.LoadAsset<SpriteAtlas>(key);
						cachedObjectsByPath.Add(key, new CachedUnityObject
						{
							Object = spriteAtlas,
							AssetBundleFileName = value2
						});
						this.AssetLoaded.SafeInvoke(assetPath);
					}
					val = spriteAtlas.GetSpriteByAssetPath(assetPath) as T;
				}
				else
				{
					val = assetBundle.LoadAsset<T>(assetPath);
					if (val == null)
					{
						UnityEngine.Debug.LogError("Loaded asset returned null value: " + assetPath);
					}
					this.AssetLoaded.SafeInvoke(assetPath);
				}
				stopwatch.Stop();
				if ((bool)val)
				{
					CacheObject(val, value2, assetPath);
				}
			}
			assetLoadTimeThisFrameInMilliseconds += (uint)(int)stopwatch.ElapsedMilliseconds;
			if (!val)
			{
				MDNPlayerPrefs.FileToHashOnStartup = GetBundlePath(value2);
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Failed to load asset of type", new Dictionary<string, string>
				{
					{ "AssetPath", assetPath },
					{
						"AssetType",
						typeof(T).Name
					}
				});
				return null;
			}
		}
		return val;
	}

	public void UnloadAssetFromBundle(string assetPath)
	{
		if (!assetPathToBundleName.TryGetValue(assetPath, out var value))
		{
			MDNPlayerPrefs.HashAllFilesOnStartup = true;
			ResourceErrorLogger.LogAssetBundleError(_biLogger, "Attempting to unload bundle for unknown asset", new Dictionary<string, string> { { "AssetPath", assetPath } });
		}
		else
		{
			if (!loadedBundlesByFileName.TryGetValue(value, out var value2))
			{
				return;
			}
			if (value2 == null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Bundle Name"] = (string.IsNullOrEmpty(value) ? "NULL/EMPTY" : value);
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Found Null LoadedAssetBundle when trying to unload asset from bundle", dictionary);
				return;
			}
			value2.RemoveReference(assetPath);
			if (value2.AssetsRefCount.TryGetValue(assetPath, out var value3) && value3 <= 0)
			{
				cachedObjectsByPath.Remove(assetPath);
			}
		}
	}

	private void UnloadAssetBundle(string bundleName)
	{
		if (!loadedBundlesByFileName.TryGetValue(bundleName, out var value))
		{
			return;
		}
		if (value != null)
		{
			bool loaded = value.Dependencies.Loaded;
			ABLogger.AddLog(ABOperation.Unloaded, value, null);
			try
			{
				value.AssetBundle.Unload(unloadAllLoadedObjects: false);
			}
			catch
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Bundle Name"] = (string.IsNullOrEmpty(bundleName) ? "NULL/EMPTY" : bundleName);
				dictionary["Bundle RefCount"] = value.RefCount.ToString();
				if (!value.Dependencies.Equals(null))
				{
					dictionary["Dependencies_Loaded"] = value.Dependencies.Loaded.ToString();
					dictionary["Dependencies_DirectCount"] = value.Dependencies.DirectCount.ToString();
					dictionary["Dependencies_DepOfDepsCount"] = value.Dependencies.DepOfDepsCount.ToString();
				}
				else
				{
					dictionary["Dependencies"] = "None";
				}
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Failed to unload asset bundle", dictionary);
			}
			if (loaded)
			{
				HashSet<string> hashSet = new HashSet<string>();
				hashSet.Add(bundleName);
				getDependencies(bundleNameToFileInfo, bundleName, hashSet);
				hashSet.Remove(bundleName);
				foreach (string item in hashSet)
				{
					if (loadedBundlesByFileName.TryGetValue(item, out var value2))
					{
						if (value2 == null)
						{
							Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
							dictionary2["Dependency Name"] = (string.IsNullOrEmpty(item) ? "NULL/EMPTY" : item);
							ResourceErrorLogger.LogAssetBundleError(_biLogger, "Found Null LoadedAssetBundle when trying to unload bundle dependency", dictionary2);
						}
						else
						{
							value2.RemoveReference(bundleName);
						}
					}
					else
					{
						ResourceErrorLogger.LogAssetBundleError(_biLogger, "Attempting to release reference to an unloaded bundle dependency", new Dictionary<string, string>
						{
							{ "BundleName", bundleName },
							{ "Dependency", item }
						});
					}
				}
			}
		}
		else
		{
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			dictionary3["Bundle Name"] = (string.IsNullOrEmpty(bundleName) ? "NULL/EMPTY" : bundleName);
			ResourceErrorLogger.LogAssetBundleError(_biLogger, "Found Null LoadedAssetBundle when trying to unload bundle ", dictionary3);
		}
		loadedBundlesByFileName.Remove(bundleName);
		this.AssetBundleUnloaded.SafeInvoke(bundleName);
		static void getDependencies(Dictionary<string, AssetFileInfo> bundleNameToFileInfo2, string depBundleName, HashSet<string> dependencies2)
		{
			string[] dependencies3 = bundleNameToFileInfo2[depBundleName].Dependencies;
			foreach (string text in dependencies3)
			{
				if (!dependencies2.Contains(text))
				{
					dependencies2.Add(text);
					getDependencies(bundleNameToFileInfo2, text, dependencies2);
				}
			}
		}
	}

	public void UnloadAll()
	{
		foreach (LoadedAssetBundle value in loadedBundlesByFileName.Values)
		{
			if (value?.AssetBundle != null)
			{
				ABLogger.AddLog(ABOperation.Unloaded, value, null);
				value.AssetBundle.Unload(unloadAllLoadedObjects: true);
			}
		}
		loadedBundlesByFileName.Clear();
	}

	public void UnloadAllExtreme()
	{
		UnloadAll();
		AssetBundle.UnloadAllAssetBundles(unloadAllObjects: true);
	}

	private AssetBundle LoadAssetBundleAndDependencies(string bundleName, string assetName)
	{
		LoadedAssetBundle loadedAssetBundle = LoadAssetBundle(bundleName, assetName);
		if (loadedAssetBundle?.AssetBundle != null && !loadedAssetBundle.Dependencies.Loaded)
		{
			HashSet<string> hashSet = new HashSet<string>();
			hashSet.Add(bundleName);
			addDependencies(bundleName, hashSet);
			hashSet.Remove(bundleName);
			foreach (string item in hashSet)
			{
				if (LoadAssetBundle(item, bundleName)?.AssetBundle == null)
				{
					ResourceErrorLogger.LogAssetBundleError(_biLogger, "Failed to load bundle dependency", new Dictionary<string, string>
					{
						{ "Dependency", item },
						{ "BundleName", bundleName }
					});
					return null;
				}
			}
			loadedAssetBundle.SetDependencyInfo(bundleNameToFileInfo[bundleName].Dependencies.Length, hashSet.Count);
		}
		return loadedAssetBundle?.AssetBundle;
		void addDependencies(string depBundleName, HashSet<string> deps)
		{
			if (!bundleNameToFileInfo.TryGetValue(depBundleName, out var value))
			{
				MDNPlayerPrefs.FileToHashOnStartup = (bundleNameToFilePathCache.TryGetValue(depBundleName, out var value2) ? value2 : depBundleName);
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Bundle not found in dependency list", new Dictionary<string, string> { { "BundleName", depBundleName } });
			}
			else
			{
				string[] dependencies = value.Dependencies;
				foreach (string text in dependencies)
				{
					if (deps.Add(text))
					{
						addDependencies(text, deps);
					}
				}
			}
		}
	}

	private LoadedAssetBundle LoadAssetBundle(string bundleName, string assetName)
	{
		string bundlePath = GetBundlePath(bundleName);
		if (!bundleNameToFileInfo.ContainsKey(bundleName))
		{
			MDNPlayerPrefs.FileToHashOnStartup = bundlePath;
			ResourceErrorLogger.LogAssetBundleError(_biLogger, "Bundle not found in dependency list", new Dictionary<string, string> { { "BundleName", bundleName } });
			return null;
		}
		if (!loadedBundlesByFileName.TryGetValue(bundleName, out var value) || value?.AssetBundle == null)
		{
			if (!bundlePath.StartsWith("jar:file://") && !File.Exists(bundlePath))
			{
				MDNPlayerPrefs.FileToHashOnStartup = bundlePath;
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Bundle doesn't exist at path", new Dictionary<string, string>
				{
					{ "BundleName", bundleName },
					{
						"Directory",
						bundlePath.Substring(0, bundlePath.LastIndexOf('/') + 1)
					}
				});
				return null;
			}
			bool flag = MDNPlayerPrefs.HashAllFilesOnStartup || MDNPlayerPrefs.FileToHashOnStartup != string.Empty;
			if (!flag)
			{
				MDNPlayerPrefs.FileToHashOnStartup = bundlePath;
			}
			stopwatch.Restart();
			AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
			stopwatch.Stop();
			assetLoadTimeThisFrameInMilliseconds += (uint)(int)stopwatch.ElapsedMilliseconds;
			if (assetBundle != null)
			{
				value = AddLoadedBundle(assetBundle, bundleName, assetName, stopwatch.Elapsed.TotalMilliseconds);
				if (!flag)
				{
					MDNPlayerPrefs.FileToHashOnStartup = string.Empty;
				}
			}
			else
			{
				MDNPlayerPrefs.FileToHashOnStartup = bundlePath;
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Failed to load bundle at path", new Dictionary<string, string>
				{
					{ "BundleName", bundleName },
					{
						"Directory",
						Directory.GetParent(bundlePath).FullName
					}
				});
			}
		}
		else
		{
			value.AddReference(assetName);
		}
		return value;
	}

	private LoadedAssetBundle AddLoadedBundle(AssetBundle assetBundle, string bundleName, string assetName, double bundleLoadTimeInMilliseconds)
	{
		if (loadedBundlesByFileName.TryGetValue(bundleName, out var value))
		{
			if (value == null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Bundle Name"] = (string.IsNullOrEmpty(bundleName) ? "NULL/EMPTY" : bundleName);
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Found Null LoadedAssetBundle when trying to add loaded bundle", dictionary);
			}
			else
			{
				if (value.AssetBundle == null)
				{
					value.AssetBundle = assetBundle;
				}
				value.AddReference(assetName);
			}
		}
		else
		{
			value = new LoadedAssetBundle(assetBundle, assetName, ABLogger, _biLogger, bundleLoadTimeInMilliseconds);
			loadedBundlesByFileName[bundleName] = value;
			this.AssetBundleLoaded.SafeInvoke(bundleName);
		}
		return value;
	}

	private void UpdateLoadedBundle(string fileName, string assetName)
	{
		if (loadedBundlesByFileName.TryGetValue(fileName, out var value))
		{
			if (value == null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["Bundle Name"] = (string.IsNullOrEmpty(fileName) ? "NULL/EMPTY" : fileName);
				ResourceErrorLogger.LogAssetBundleError(_biLogger, "Found Null LoadedAssetBundle when trying to update loaded bundle", dictionary);
			}
			else
			{
				value.AddReference(assetName);
			}
		}
	}

	public IEnumerable<string> GetFilePathsForAssetType(string assetType)
	{
		if (assetTypeToFilePathListCache.TryGetValue(assetType, out var value))
		{
			return value;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, AssetFileInfo> item2 in bundleNameToFileInfo)
		{
			if (item2.Value.AssetType == assetType)
			{
				string item = _embeddedPathResolver?.GetAssetPath(item2.Value) ?? string.Format(_defaultBundlePathFormat, item2.Value.AssetType, item2.Value.Name);
				hashSet.Add(item);
			}
		}
		Dictionary<string, IReadOnlyCollection<string>> dictionary = assetTypeToFilePathListCache;
		IReadOnlyCollection<string> readOnlyCollection2;
		if (hashSet.Count <= 0)
		{
			IReadOnlyCollection<string> readOnlyCollection = (IReadOnlyCollection<string>)(object)Array.Empty<string>();
			readOnlyCollection2 = readOnlyCollection;
		}
		else
		{
			IReadOnlyCollection<string> readOnlyCollection = hashSet;
			readOnlyCollection2 = readOnlyCollection;
		}
		IReadOnlyCollection<string> result = readOnlyCollection2;
		dictionary[assetType] = readOnlyCollection2;
		return result;
	}

	private void Update()
	{
		assetLoadTimePreviousFrameInMilliseconds = assetLoadTimeThisFrameInMilliseconds;
		assetLoadTimeThisFrameInMilliseconds = 0u;
		float time = Time.time;
		foreach (KeyValuePair<string, LoadedAssetBundle> item in loadedBundlesByFileName)
		{
			string key = item.Key;
			LoadedAssetBundle value = item.Value;
			if (value.RefCount <= 0 && value.unloadTime.HasValue && time >= value.unloadTime.Value)
			{
				pendingUnloadAssets.Enqueue(key);
			}
		}
		while (pendingUnloadAssets.Count > 0)
		{
			UnloadAssetBundle(pendingUnloadAssets.Dequeue());
		}
	}

	public void ClearCache()
	{
		foreach (CachedUnityObject value in cachedObjectsByPath.Values)
		{
			value.Object = null;
		}
		cachedObjectsByPath.Clear();
	}

	public void ForceCull()
	{
		_assetBundleToUnloadCount.Clear();
		foreach (KeyValuePair<string, LoadedAssetBundle> item in loadedBundlesByFileName)
		{
			string key = item.Key;
			if (item.Value.RefCount <= 0)
			{
				pendingUnloadAssets.Enqueue(key);
			}
		}
		while (pendingUnloadAssets.Count > 0)
		{
			UnloadAssetBundle(pendingUnloadAssets.Dequeue());
		}
	}

	public uint GetPreviousFrameLoadTimeInMilliseconds()
	{
		return assetLoadTimePreviousFrameInMilliseconds;
	}
}
