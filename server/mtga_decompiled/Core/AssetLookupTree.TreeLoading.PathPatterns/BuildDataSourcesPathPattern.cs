using System.IO;

namespace AssetLookupTree.TreeLoading.PathPatterns;

public class BuildDataSourcesPathPattern : ITreePathPattern
{
	public static readonly ITreePathPattern Default = new BuildDataSourcesPathPattern();

	public string GetPath<T>() where T : class, IPayload
	{
		string payloadTypeName = AssetLookupTreeUtils.GetPayloadTypeName<T>();
		return Path.Combine("BuildDataSources", "AssetLookupTrees", payloadTypeName, payloadTypeName + ".json");
	}
}
