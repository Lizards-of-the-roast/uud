using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators;

public interface IEvaluator
{
	bool Execute(IBlackboard bb);
}
