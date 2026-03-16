using System;
using System.Collections.Generic;

namespace AssetLookupTree.TreeLoading.CachePatterns;

public class RuntimeTreeCachePattern : ITreeCachePattern
{
	private readonly Dictionary<Type, IAssetLookupTree> _cachedTrees = new Dictionary<Type, IAssetLookupTree>(255);

	public void PushTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
		PushTree(typeof(T), tree);
	}

	public void PushTree(Type type, IAssetLookupTree tree)
	{
		_cachedTrees[type] = tree;
	}

	public bool TryPullTree<T>(out AssetLookupTree<T> tree) where T : class, IPayload
	{
		if (_cachedTrees.TryGetValue(typeof(T), out var value) && value is AssetLookupTree<T> assetLookupTree)
		{
			tree = assetLookupTree;
			return true;
		}
		tree = null;
		return false;
	}

	public void ClearTreeFromCache(Type payloadType)
	{
		_cachedTrees.Remove(payloadType);
	}
}
