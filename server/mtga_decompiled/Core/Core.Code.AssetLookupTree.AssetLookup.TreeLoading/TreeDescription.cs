using System.Collections.Generic;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetLookupTree.AssetLookup.TreeLoading;

public struct TreeDescription
{
	public string Root;

	public AssetPriority Priority;

	public AssetPriority DefaultPayloadPriority;

	public uint AssetsPerBundle;

	public bool MustReturnPayload;

	public int TotalNodeCount;

	public List<string> Partitions;

	public List<string> Trees;
}
