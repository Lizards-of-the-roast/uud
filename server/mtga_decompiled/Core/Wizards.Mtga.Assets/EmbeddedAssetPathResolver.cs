using System.IO;
using System.Linq;
using Core.Code.AssetBundles.Manifest;
using UnityEngine;
using Wizards.Mtga.IO;
using Wizards.Mtga.Storage;

namespace Wizards.Mtga.Assets;

public class EmbeddedAssetPathResolver : IAssetPathResolver
{
	public const string StorageBasePath = "assets";

	public const string ManifestFileName = "assets.manifest";

	private readonly IAssetFileManifest? _manifest;

	private readonly string _basePath;

	public EmbeddedAssetPathResolver(IStorageContext storageContext)
	{
		_basePath = storageContext.GetEmbeddedAssetBundleStoragePath();
		string text = _basePath + "/assets.manifest";
		if (Application.platform == RuntimePlatform.Android)
		{
			_manifest = LoadAndroidEmbeddedManifest(text);
		}
		else if (WindowsSafePath.FileExists(text))
		{
			using (StreamReader reader = new StreamReader(File.OpenRead(text)))
			{
				_manifest = AssetFileManifestExtensions.LoadFrom(reader);
			}
		}
	}

	public string? GetAssetPath(IAssetFileInfo assetFile)
	{
		IAssetFileManifest? manifest = _manifest;
		if (manifest != null && manifest.Contains(assetFile.Name))
		{
			return Path.Combine(_basePath, assetFile.AssetType, assetFile.Name);
		}
		return null;
	}

	private IAssetFileManifest? LoadAndroidEmbeddedManifest(string manifestPath)
	{
		using Stream stream = EmbeddedContentUtil.LoadEmbeddedContent(manifestPath);
		if (stream == null)
		{
			return null;
		}
		using StreamReader reader = new StreamReader(stream);
		AssetFileManifest assetFileManifest = AssetFileManifestExtensions.LoadFrom(reader);
		return new AssetFileManifest(assetFiles: assetFileManifest.AssetFileInfoByName.Values.Where((AssetFileInfo info) => info.AssetType != "Loc" && info.AssetType != "Data" && info.AssetType != "Raw"), priority: assetFileManifest.Priority);
	}
}
