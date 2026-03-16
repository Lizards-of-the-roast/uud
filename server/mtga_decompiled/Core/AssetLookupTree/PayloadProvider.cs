using AssetLookupTree.Blackboard;

namespace AssetLookupTree;

public class PayloadProvider<T> : IPayloadProvider<T> where T : class, IPayload
{
	private readonly AssetLookupSystem _assetLookupSystem;

	public PayloadProvider(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	public T GetPayload(IBlackboard blackboard)
	{
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<T> loadedTree))
		{
			T payload = loadedTree.GetPayload(blackboard);
			if (payload != null)
			{
				return payload;
			}
		}
		return null;
	}
}
