namespace AssetLookupTree.TreeLoading.PathPatterns;

public class NullTreePathPattern : ITreePathPattern
{
	public string GetPath<T>() where T : class, IPayload
	{
		return string.Empty;
	}
}
