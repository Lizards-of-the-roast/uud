using Unity.Profiling;

namespace AssetLookupTree.TreeLoading;

public static class TreeProfiling
{
	public static readonly ProfilerCategory Category = new ProfilerCategory("Asset Lookup Tree");

	private static readonly string Prefix = "ALT";

	public static readonly string PrefixDeserialize = Prefix + ".Deserialization";
}
