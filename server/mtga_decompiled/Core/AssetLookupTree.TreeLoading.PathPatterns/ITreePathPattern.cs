namespace AssetLookupTree.TreeLoading.PathPatterns;

public interface ITreePathPattern
{
	string GetPath<T>() where T : class, IPayload;
}
