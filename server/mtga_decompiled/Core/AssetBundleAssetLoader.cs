using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetLookupTree;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Storage;

public class AssetBundleAssetLoader : IAssetLoader, IDisposable
{
	protected readonly IStorageContext _storageContext;

	protected readonly IBILogger _biLogger;

	public AssetBundleAssetLoader(IStorageContext storageContext, IBILogger biLogger)
	{
		_storageContext = storageContext;
		_biLogger = biLogger;
	}

	public T AcquireAsset<T>(string assetPath) where T : UnityEngine.Object
	{
		T val = AssetBundleManager.Instance.LoadAssetFromBundle<T>(assetPath);
		if (val == null)
		{
			Debug.LogError("[Assets] Couldnt find asset " + assetPath);
		}
		return val;
	}

	public void ReleaseAsset(string assetPath)
	{
		AssetBundleManager.Instance.UnloadAssetFromBundle(assetPath);
	}

	public bool HaveAsset(string assetPath)
	{
		return AssetBundleManager.Instance.IsBundledAsset(assetPath);
	}

	public virtual void PrepareAssets(AssetBundleProvisioner bundleProvisioner, IAssetPathResolver embeddedAssetPaths = null)
	{
		AssetBundleManager.Instance.UnloadAll();
		IEnumerable<AssetFileInfo> requiredAssetsList = bundleProvisioner.AvailableBundles.Where((AssetFileInfo bundle) => bundle.Priority <= AssetPriority.General);
		PAPA.ClearPapaCache();
		AssetBundleManager.Instance.Initialize(_biLogger, requiredAssetsList, embeddedAssetPaths);
	}

	private string GetAltPath<T>()
	{
		Type typeFromHandle = typeof(T);
		return GetAltPath(typeFromHandle);
	}

	private string GetAltPath(Type type)
	{
		return AssetBundleManager.Instance.GetAltPath(type);
	}

	public IEnumerable<string> GetFilePathsForAssetType(string assetType)
	{
		return AssetBundleManager.Instance.GetFilePathsForAssetType(assetType);
	}

	public IEnumerable<string> GetRawFilePaths(string subDirectory, string fileName)
	{
		bool flag = false;
		string fileNameCheck = ((!string.IsNullOrWhiteSpace(fileName)) ? ("Raw_" + Path.GetFileNameWithoutExtension(fileName)) : string.Empty);
		if (_storageContext.UseEmbeddedBundles())
		{
			string path = ((!string.IsNullOrWhiteSpace(subDirectory)) ? Path.Combine(_storageContext.GetEmbeddedAssetBundleStoragePath(), subDirectory) : _storageContext.GetEmbeddedAssetBundleStoragePath());
			if (Directory.Exists(path))
			{
				foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
				{
					if (Path.GetFileName(item).StartsWith(fileNameCheck))
					{
						yield return item;
						flag = true;
					}
				}
				if (flag)
				{
					yield break;
				}
			}
		}
		IEnumerable<string> filePathsForAssetType = GetFilePathsForAssetType("Raw");
		foreach (string item2 in filePathsForAssetType)
		{
			if (Path.GetFileName(item2).StartsWith(fileNameCheck))
			{
				yield return item2;
			}
		}
	}

	public virtual IEnumerable<string> GetAudioPackageBasePaths()
	{
		yield return Path.Combine(_storageContext.GetAssetBundleStoragePath(), "Audio");
		if (_storageContext.UseEmbeddedBundles())
		{
			string text = _storageContext.GetEmbeddedAssetBundleStoragePath() + "/Audio";
			if (Directory.Exists(text))
			{
				yield return text;
			}
		}
	}

	public IEnumerable<string> GetAudioPackagePaths()
	{
		return AssetBundleManager.Instance.GetFilePathsForAssetType("Audio");
	}

	public virtual void Dispose()
	{
		AssetBundleManager.Instance.UnloadAllExtreme();
	}

	public Stream GetTree<T>() where T : IPayload
	{
		string altPath = GetAltPath<T>();
		if (FileSystemUtils.FileExists(altPath))
		{
			return FileSystemUtils.OpenRead(altPath);
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			return EmbeddedContentUtil.LoadEmbeddedContent(altPath);
		}
		return null;
	}

	public bool AddReferenceCount(string assetPath)
	{
		return AssetBundleManager.Instance.LoadBundleForAsset(assetPath);
	}
}
