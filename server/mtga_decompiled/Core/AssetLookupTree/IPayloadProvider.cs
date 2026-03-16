using AssetLookupTree.Blackboard;

namespace AssetLookupTree;

public interface IPayloadProvider<T> where T : IPayload
{
	T GetPayload(IBlackboard blackboard);

	bool TryGetPayload(IBlackboard blackboard, out T payload)
	{
		payload = GetPayload(blackboard);
		return payload != null;
	}
}
