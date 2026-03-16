using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetLookupTree;
using UnityEngine;
using Wizards.Mtga.Assets;

public class CompositeAssetLoader : IAssetLoader, IDisposable
{
	private readonly AssetBundleAssetLoader _assetBundleLoader;

	private readonly EditorAssetLoader _editorLoader;

	public CompositeAssetLoader(AssetBundleAssetLoader assetBundleLoader, EditorAssetLoader editorLoader)
	{
		_assetBundleLoader = assetBundleLoader;
		_editorLoader = editorLoader;
	}

	public void Dispose()
	{
		_assetBundleLoader.Dispose();
		_editorLoader.Dispose();
	}

	public T AcquireAsset<T>(string assetPath) where T : UnityEngine.Object
	{
		if (_assetBundleLoader.HaveAsset(assetPath))
		{
			return _assetBundleLoader.AcquireAsset<T>(assetPath);
		}
		return _editorLoader.AcquireAsset<T>(assetPath);
	}

	public IEnumerable<string> GetAudioPackageBasePaths()
	{
		return _assetBundleLoader.GetAudioPackageBasePaths().Concat(_editorLoader.GetAudioPackageBasePaths());
	}

	public IEnumerable<string> GetAudioPackagePaths()
	{
		return _assetBundleLoader.GetAudioPackagePaths().Concat(_editorLoader.GetAudioPackagePaths());
	}

	public IEnumerable<string> GetRawFilePaths(string subDirectory, string fileName)
	{
		return _assetBundleLoader.GetRawFilePaths(subDirectory, fileName).Concat(_editorLoader.GetRawFilePaths(subDirectory, fileName));
	}

	public IEnumerable<string> GetFilePathsForAssetType(string assetType)
	{
		return _assetBundleLoader.GetFilePathsForAssetType(assetType).Concat(_editorLoader.GetFilePathsForAssetType(assetType));
	}

	public bool HaveAsset(string assetPath)
	{
		bool flag = _assetBundleLoader.HaveAsset(assetPath);
		if (!flag)
		{
			flag = _editorLoader.HaveAsset(assetPath);
		}
		return flag;
	}

	public void PrepareAssets(AssetBundleProvisioner bundleProvisioner, IAssetPathResolver embeddedAssetResolver = null)
	{
		_assetBundleLoader.PrepareAssets(bundleProvisioner, embeddedAssetResolver);
	}

	public void ReleaseAsset(string assetPath)
	{
		if (_assetBundleLoader.HaveAsset(assetPath))
		{
			_assetBundleLoader.ReleaseAsset(assetPath);
		}
	}

	public Stream GetTree<T>() where T : IPayload
	{
		return _assetBundleLoader.GetTree<T>();
	}

	public bool AddReferenceCount(string battlefieldPath)
	{
		if (_assetBundleLoader.HaveAsset(battlefieldPath))
		{
			return _assetBundleLoader.AddReferenceCount(battlefieldPath);
		}
		return _editorLoader.AddReferenceCount(battlefieldPath);
	}
}
