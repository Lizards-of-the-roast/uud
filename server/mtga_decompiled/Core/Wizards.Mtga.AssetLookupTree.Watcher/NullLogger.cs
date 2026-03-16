using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Nodes;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace Wizards.Mtga.AssetLookupTree.Watcher;

public class NullLogger : IWatcherLogger
{
	void IWatcherLogger.BeginRequestBookmark<T>(AssetLookupTree<T> tree, IBlackboard bb)
	{
	}

	void IWatcherLogger.EndRequestBookmark<T>(AssetLookupTree<T> tree, RequestResult result)
	{
	}

	void IWatcherLogger.RegisterNode<T>(ValueNode<T> node, ValueNodeResult result)
	{
	}

	void IWatcherLogger.RegisterNode<T>(ConditionNode<T> node, ConditionNodeResult result)
	{
	}

	void IWatcherLogger.RegisterNode<T, U>(BucketNode<T, U> node, BucketNodeResult result)
	{
	}

	void IWatcherLogger.RegisterNode<T>(IndirectionNode<T> node, IndirectionNodeResult result)
	{
	}

	void IWatcherLogger.RegisterNode<T>(PriorityNode<T> node, PriorityNodeResult result)
	{
	}
}
