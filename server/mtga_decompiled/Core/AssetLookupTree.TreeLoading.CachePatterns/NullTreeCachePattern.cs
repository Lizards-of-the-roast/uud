using System;

namespace AssetLookupTree.TreeLoading.CachePatterns;

public class NullTreeCachePattern : ITreeCachePattern
{
	public void PushTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
	}

	public bool TryPullTree<T>(out AssetLookupTree<T> tree) where T : class, IPayload
	{
		tree = null;
		return false;
	}

	public void PushTree(Type type, IAssetLookupTree tree)
	{
	}
}
