using System;

namespace AssetLookupTree.TreeLoading.CachePatterns;

public interface ITreeCachePattern
{
	void PushTree<T>(AssetLookupTree<T> tree) where T : class, IPayload;

	bool TryPullTree<T>(out AssetLookupTree<T> tree) where T : class, IPayload;

	void PushTree(Type type, IAssetLookupTree tree);
}
