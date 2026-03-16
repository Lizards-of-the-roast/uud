using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors;

public interface IExtractor<T>
{
	bool Execute(IBlackboard bb, out T value);
}
