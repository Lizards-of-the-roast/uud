using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

public interface IManifestMetadataCollector
{
	Promise<IEnumerable<AssetBundleManifestMetadata>> Collect();
}
