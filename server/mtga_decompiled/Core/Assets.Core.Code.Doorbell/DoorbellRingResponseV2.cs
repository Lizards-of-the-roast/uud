using System.Collections.Generic;
using Wizards.Mtga.Assets;

namespace Assets.Core.Code.Doorbell;

public class DoorbellRingResponseV2
{
	public string FdURI { get; set; }

	public List<AssetBundleManifestMetadata> BundleManifests { get; set; }
}
