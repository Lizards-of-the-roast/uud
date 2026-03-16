using AssetLookupTree.Blackboard;

namespace AssetLookupTree;

public class NullPayloadProvider<T> : IPayloadProvider<T> where T : IPayload
{
	public static IPayloadProvider<T> Default = new NullPayloadProvider<T>();

	public T GetPayload(IBlackboard blackboard)
	{
		return default(T);
	}
}
