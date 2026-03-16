namespace AssetLookupTree.TreeLoading.LoadPatterns;

public class NullTreeLoadPattern : ITreeLoadPattern
{
	public AssetLookupTree<T> LoadTree<T>() where T : class, IPayload
	{
		return null;
	}
}
