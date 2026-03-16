using System;
using System.Collections.Generic;
using System.IO;
using AssetLookupTree;
using UnityEngine;
using Wizards.Mtga.Assets;

public interface IAssetLoader : IDisposable
{
	T AcquireAsset<T>(string assetPath) where T : UnityEngine.Object;

	IEnumerable<string> GetAudioPackageBasePaths();

	IEnumerable<string> GetAudioPackagePaths();

	IEnumerable<string> GetRawFilePaths(string subDirectory, string fileName);

	IEnumerable<string> GetFilePathsForAssetType(string assetType);

	bool HaveAsset(string assetPath);

	void PrepareAssets(AssetBundleProvisioner bundleProvisioner, IAssetPathResolver embeddedAssetResolver = null);

	void ReleaseAsset(string assetPath);

	Stream GetTree<T>() where T : IPayload;

	bool AddReferenceCount(string battlefieldPath);
}
