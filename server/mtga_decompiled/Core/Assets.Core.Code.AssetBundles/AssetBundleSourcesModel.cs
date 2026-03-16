using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Configuration;

namespace Assets.Core.Code.AssetBundles;

public class AssetBundleSourcesModel
{
	private struct HashedAssetBundleSource
	{
		public IAssetBundleSource AssetBundleSource { get; set; }

		public int Hash { get; set; }
	}

	private readonly List<HashedAssetBundleSource> _assetBundleSources = new List<HashedAssetBundleSource>();

	private static HashAlgorithm HashAlgorithm { get; } = SHA256.Create();

	public IEnumerable<IAssetBundleSource> Sources
	{
		get
		{
			foreach (HashedAssetBundleSource assetBundleSource in _assetBundleSources)
			{
				yield return assetBundleSource.AssetBundleSource;
			}
		}
	}

	public int CurrentSourceIndex
	{
		get
		{
			int result = 0;
			int selectedBundleSourceHashCode = MDNPlayerPrefs.SelectedBundleSourceHashCode;
			for (int i = 0; i < _assetBundleSources.Count; i++)
			{
				if (_assetBundleSources[i].Hash == selectedBundleSourceHashCode)
				{
					return i;
				}
			}
			return result;
		}
		set
		{
			MDNPlayerPrefs.SelectedBundleSourceHashCode = _assetBundleSources[value].Hash;
		}
	}

	public IAssetBundleSource CurrentSource
	{
		get
		{
			if (_assetBundleSources.Count == 0)
			{
				return null;
			}
			return _assetBundleSources[CurrentSourceIndex].AssetBundleSource;
		}
		set
		{
			MDNPlayerPrefs.SelectedBundleSourceHashCode = GetAssetBundleSourceKey(value);
		}
	}

	public bool HasMultipleOptionsToSelect => _assetBundleSources.Count > 1;

	public static AssetBundleSourcesModel Create()
	{
		return new AssetBundleSourcesModel(AssetBundleManager.Instance.Configuration);
	}

	private AssetBundleSourcesModel(AssetsConfiguration assetsConfiguration)
	{
		_assetBundleSources.AddRange(GetBundleSources(assetsConfiguration));
	}

	private static IAssetBundleSource GetDefaultAssetBundleSource()
	{
		return new AssetBundleSource(BundleSource.DefaultSourceLocation, AssetBundleSourceType.Remote, AssetBundleSource.GetManifestPointerName("External", Global.VersionInfo), "[EXTERNAL - PROD] " + BundleSource.DefaultSourceLocation.AbsoluteUri);
	}

	private static IAssetBundleSource GetDefaultAssetBundleSourceEditor()
	{
		return new AssetBundleSource(BundleSource.DefaultInternalSourceLocation, AssetBundleSourceType.Remote, AssetBundleSource.GetManifestPointerName("Internal", Global.VersionInfo), "[INTERNAL] " + BundleSource.DefaultInternalSourceLocation.AbsoluteUri);
	}

	private static int GetAssetBundleSourceKey(IAssetBundleSource source)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(source.GetBundleUrl(string.Empty));
		stringBuilder.Append(source.EndpointHashId);
		stringBuilder.Append(source.HostingSource);
		return BitConverter.ToInt32(HashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(stringBuilder.ToString())), 0);
	}

	private static HashedAssetBundleSource GetHashedAssetBundleSource(IAssetBundleSource assetBundleSource)
	{
		return new HashedAssetBundleSource
		{
			AssetBundleSource = assetBundleSource,
			Hash = GetAssetBundleSourceKey(assetBundleSource)
		};
	}

	public void PopulateBundleSources(AssetsConfiguration assetsConfiguration)
	{
		_assetBundleSources.Clear();
		_assetBundleSources.AddRange(GetBundleSources(assetsConfiguration));
	}

	private static IEnumerable<HashedAssetBundleSource> GetBundleSources(AssetsConfiguration assetsConfiguration)
	{
		List<HashedAssetBundleSource> list = new List<HashedAssetBundleSource>();
		foreach (BundleSource bundleSource in assetsConfiguration.BundleSources)
		{
			Uri location = bundleSource.Location;
			Uri categorizedLocation = bundleSource.CategorizedLocation;
			string text = (string.IsNullOrEmpty(assetsConfiguration.BundleVersion) ? AssetBundleSource.GetManifestPointerName(bundleSource.Partition, Global.VersionInfo) : AssetBundleSource.GetManifestPointerName(bundleSource.Partition, assetsConfiguration.BundleVersion));
			AssetBundleSourceType hostingSource = ((bundleSource.SourceType != BundleSourceType.FileSystem) ? AssetBundleSourceType.Remote : AssetBundleSourceType.LocalEndpoint);
			IAssetBundleSource assetBundleSource = new AssetBundleSource(location, hostingSource, text, "[" + text + "] " + location.AbsoluteUri + " " + categorizedLocation?.AbsoluteUri, categorizedLocation);
			list.Add(GetHashedAssetBundleSource(assetBundleSource));
		}
		if (list.Count == 0)
		{
			IAssetBundleSource assetBundleSource2 = (Application.isEditor ? GetDefaultAssetBundleSourceEditor() : GetDefaultAssetBundleSource());
			list.Add(GetHashedAssetBundleSource(assetBundleSource2));
		}
		return list;
	}

	[Conditional("UNITY_EDITOR")]
	public void OverrideBundleSourceForBuilds()
	{
		_assetBundleSources.Clear();
		_assetBundleSources.Add(GetHashedAssetBundleSource(GetDefaultAssetBundleSourceEditor()));
		CurrentSourceIndex = 0;
	}
}
