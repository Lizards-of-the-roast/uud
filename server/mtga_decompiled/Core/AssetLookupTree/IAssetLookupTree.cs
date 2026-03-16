using System;
using Wizards.Mtga.Assets;

namespace AssetLookupTree;

public interface IAssetLookupTree
{
	AssetPriority Priority { get; }

	AssetPriority DefaultPayloadPriority { get; }

	uint AssetsPerBundle { get; }

	Type GetPayloadType();
}
