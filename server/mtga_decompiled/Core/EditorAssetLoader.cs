using System;
using System.Collections.Generic;
using System.IO;
using AssetLookupTree;
using UnityEngine;
using Wizards.Mtga.Assets;
using Wotc.Mtga;

public class EditorAssetLoader : IAssetLoader, IDisposable
{
	public T AcquireAsset<T>(string assetPath) where T : UnityEngine.Object
	{
		if ((UnityEngine.Object)null == (UnityEngine.Object)null)
		{
			Debug.LogError("Couldnt find asset " + assetPath);
		}
		return null;
	}

	public void ReleaseAsset(string assetPath)
	{
	}

	public bool HaveAsset(string assetPath)
	{
		return File.Exists(assetPath);
	}

	public void PrepareAssets(AssetBundleProvisioner bundleProvisioner, IAssetPathResolver embeddedPathResolver = null)
	{
	}

	public IEnumerable<string> GetFilePathsForAssetType(string assetType)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Path.Combine(new DirectoryInfo("BuildDataSources").FullName, DataSourceUtilities.GetCurrentDataSource()), assetType));
		if (directoryInfo.Exists)
		{
			List<string> list = new List<string>();
			FileInfo[] files = directoryInfo.GetFiles("*.txt");
			foreach (FileInfo fileInfo in files)
			{
				list.Add(fileInfo.FullName);
			}
			return list;
		}
		return Array.Empty<string>();
	}

	public IEnumerable<string> GetRawFilePaths(string subDirectory, string fileName)
	{
		bool flag = false;
		string path = ((!string.IsNullOrWhiteSpace(subDirectory)) ? Path.Combine(Application.streamingAssetsPath, subDirectory) : Application.streamingAssetsPath);
		if (Directory.Exists(path))
		{
			foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
			{
				if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(item).StartsWith(fileName))
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
		string path2 = ((!string.IsNullOrWhiteSpace(subDirectory)) ? Path.Combine("BuildDataSources", subDirectory) : "BuildDataSources");
		if (!Directory.Exists(path2))
		{
			yield break;
		}
		foreach (string item2 in Directory.EnumerateFiles(path2, "*", SearchOption.AllDirectories))
		{
			if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(item2).StartsWith(fileName))
			{
				yield return item2;
			}
		}
	}

	public IEnumerable<string> GetAudioPackageBasePaths()
	{
		yield return ClientPathUtilities.GetAudioDataSourceFolder(AkBasePathGetter.GetPlatformName());
	}

	public IEnumerable<string> GetAudioPackagePaths()
	{
		IEnumerable<string> audioPackageBasePaths = AssetLoader.GetAudioPackageBasePaths();
		foreach (string item in audioPackageBasePaths)
		{
			if (!Directory.Exists(item))
			{
				continue;
			}
			foreach (string item2 in Directory.EnumerateFiles(item, "*.pck"))
			{
				yield return item2;
			}
		}
	}

	public void Dispose()
	{
	}

	public Stream GetTree<T>() where T : IPayload
	{
		throw new InvalidOperationException("Attempting to load runtime ALT stream while running in Editor. Make sure you're using EditorTreeLoadPattern in Editor when not running with AssetBundles!");
	}

	public bool AddReferenceCount(string battlefieldPath)
	{
		return true;
	}
}
