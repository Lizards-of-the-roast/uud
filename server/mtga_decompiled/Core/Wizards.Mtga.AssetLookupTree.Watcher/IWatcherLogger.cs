using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Nodes;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace Wizards.Mtga.AssetLookupTree.Watcher;

public interface IWatcherLogger
{
	void BeginRequestBookmark<T>(AssetLookupTree<T> tree, IBlackboard bb) where T : class, IPayload;

	void EndRequestBookmark<T>(AssetLookupTree<T> tree, RequestResult result) where T : class, IPayload;

	void RegisterNode<T>(ValueNode<T> node, ValueNodeResult result) where T : class, IPayload;

	void RegisterNode<T>(ConditionNode<T> node, ConditionNodeResult result) where T : class, IPayload;

	void RegisterNode<T, U>(BucketNode<T, U> node, BucketNodeResult result) where T : class, IPayload;

	void RegisterNode<T>(IndirectionNode<T> node, IndirectionNodeResult result) where T : class, IPayload;

	void RegisterNode<T>(PriorityNode<T> node, PriorityNodeResult result) where T : class, IPayload;
}
