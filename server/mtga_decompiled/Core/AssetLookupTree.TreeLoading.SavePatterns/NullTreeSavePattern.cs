namespace AssetLookupTree.TreeLoading.SavePatterns;

public class NullTreeSavePattern : ITreeSavePattern
{
	public void SaveTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
	}
}
