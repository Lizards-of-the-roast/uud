namespace AssetLookupTree.TreeLoading.LoadPatterns;

public interface ITreeLoadPattern
{
	AssetLookupTree<T> LoadTree<T>() where T : class, IPayload;
}
