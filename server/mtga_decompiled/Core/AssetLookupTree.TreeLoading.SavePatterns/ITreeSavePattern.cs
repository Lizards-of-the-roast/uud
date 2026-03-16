namespace AssetLookupTree.TreeLoading.SavePatterns;

public interface ITreeSavePattern
{
	void SaveTree<T>(AssetLookupTree<T> tree) where T : class, IPayload;
}
