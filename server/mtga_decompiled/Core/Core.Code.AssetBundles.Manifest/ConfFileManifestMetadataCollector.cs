using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.Providers;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Configuration;

namespace Core.Code.AssetBundles.Manifest;

public class ConfFileManifestMetadataCollector : IManifestMetadataCollector
{
	public Promise<IEnumerable<AssetBundleManifestMetadata>> Collect()
	{
		AssetsConfiguration assetsConfiguration = Pantry.Get<EnvironmentManager>().AssetsConfiguration;
		ManifestUtils.Logger.Info($"Found {(assetsConfiguration?.ManifestSources?.Count()).GetValueOrDefault()} manifest metadata entries from configuration.");
		return new SimplePromise<IEnumerable<AssetBundleManifestMetadata>>(assetsConfiguration?.ManifestSources ?? new List<AssetBundleManifestMetadata>());
	}
}
