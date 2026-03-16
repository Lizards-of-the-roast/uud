using System.Collections.Generic;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

public interface IAssetFileManifest
{
	AssetPriority Priority { get; }

	IReadOnlyDictionary<string, AssetFileInfo> AssetFileInfoByName { get; }

	string ManifestName { get; set; }

	string Hash { get; set; }

	string Category { get; set; }
}
