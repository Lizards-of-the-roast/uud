using System.Collections.Generic;
using System.Linq;
using Assets.Core.Code.Doorbell;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

public class DoorbellManifestMetadataCollector : IManifestMetadataCollector
{
	public Promise<IEnumerable<AssetBundleManifestMetadata>> Collect()
	{
		DoorbellRingResponseV2 doorbellResponse = Pantry.Get<ConnectionManager>().DoorbellResponse;
		ManifestUtils.Logger.Info($"Found {(doorbellResponse?.BundleManifests?.Count).GetValueOrDefault()} manifest metadata entries from doorbell.");
		return new SimplePromise<IEnumerable<AssetBundleManifestMetadata>>(GetMetadataFromDoorbellResponse(doorbellResponse));
	}

	public static IEnumerable<AssetBundleManifestMetadata> GetMetadataFromDoorbellResponse(DoorbellRingResponseV2 ringResponse)
	{
		return ringResponse?.BundleManifests?.Where((AssetBundleManifestMetadata x) => !string.IsNullOrWhiteSpace(x.Hash)) ?? new List<AssetBundleManifestMetadata>();
	}
}
