using System.Collections.Generic;
using Newtonsoft.Json;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

[JsonConverter(typeof(AssetFileManifestConverter))]
public sealed class AssetFileManifest : IAssetFileManifest
{
	private readonly AssetPriority _priority;

	private readonly Dictionary<string, AssetFileInfo> _assetFileInfoByName = new Dictionary<string, AssetFileInfo>(10000);

	public AssetPriority Priority => _priority;

	public IReadOnlyDictionary<string, AssetFileInfo> AssetFileInfoByName => _assetFileInfoByName;

	public string ManifestName { get; set; }

	public string Hash { get; set; }

	public string Category { get; set; }

	public AssetFileManifest(AssetPriority priority, IEnumerable<AssetFileInfo> assetFiles)
	{
		_priority = priority;
		foreach (AssetFileInfo assetFile in assetFiles)
		{
			_assetFileInfoByName[assetFile.Name] = assetFile;
		}
	}
}
