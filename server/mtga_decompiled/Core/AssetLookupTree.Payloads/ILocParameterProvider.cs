using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public interface ILocParameterProvider
{
	string GetKey();

	bool TryGetValue(IBlackboard filledBB, out string paramValue);
}
