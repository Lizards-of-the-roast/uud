namespace AssetLookupTree.TreeLoading.PathPatterns;

public class CoreDataTreePathPattern : ITreePathPattern
{
	public string GetPath<T>() where T : class, IPayload
	{
		return "Assets/Core/Data/AssetLookupTrees/" + AssetLookupTreeUtils.GetPayloadTypeName<T>() + ".json";
	}
}
