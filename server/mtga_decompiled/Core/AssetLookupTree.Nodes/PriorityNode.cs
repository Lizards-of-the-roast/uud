using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wizards.Mtga.AssetLookupTree.Watcher;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace AssetLookupTree.Nodes;

public class PriorityNode<T> : INode<T>, INode, INodeWithChildren<T> where T : class, IPayload
{
	public readonly List<INode<T>> Children = new List<INode<T>>(10);

	[NonSerialized]
	public readonly Type PayloadType = typeof(T);

	[NonSerialized]
	public readonly bool IsLayered = typeof(ILayeredPayload).IsAssignableFrom(typeof(T));

	public int ChildCount => Children.Count;

	public Guid NodeId { get; set; } = Guid.NewGuid();

	public string Comment { get; set; } = string.Empty;

	public string Label { get; set; } = string.Empty;

	public T GetPayload(IBlackboard bb, IWatcherLogger watcherLogger)
	{
		return SingleLookup(bb, watcherLogger, (IBlackboard blackboard, IWatcherLogger logger, INode<T> node) => node.GetPayload(blackboard, logger));
	}

	public void GetPayloadLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<T> outPayloads, HashSet<string> outOccupiedLayers)
	{
		LayeredLookup(bb, watcherLogger, outPayloads, outOccupiedLayers, delegate(IBlackboard blackboard, IWatcherLogger logger, INode<T> node, HashSet<T> output, HashSet<string> layers)
		{
			node.GetPayloadLayered(blackboard, logger, output, layers);
		});
	}

	public Guid GetValueNodeId(IBlackboard bb, IWatcherLogger watcherLogger)
	{
		return SingleLookup(bb, watcherLogger, (IBlackboard blackboard, IWatcherLogger logger, INode<T> node) => node.GetValueNodeId(blackboard, logger));
	}

	public void GetValueNodeIdsLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<Guid> outValueIds, HashSet<string> outOccupiedLayers)
	{
		LayeredLookup(bb, watcherLogger, outValueIds, outOccupiedLayers, delegate(IBlackboard blackboard, IWatcherLogger logger, INode<T> node, HashSet<Guid> output, HashSet<string> layers)
		{
			node.GetValueNodeIdsLayered(blackboard, logger, output, layers);
		});
	}

	private O SingleLookup<O>(IBlackboard bb, IWatcherLogger watcherLogger, Func<IBlackboard, IWatcherLogger, INode<T>, O> performLookup)
	{
		if (Children.Count == 0)
		{
			watcherLogger.RegisterNode(this, PriorityNodeResult.NoChildren);
			return default(O);
		}
		foreach (INode<T> child in Children)
		{
			if (child != null)
			{
				O val = performLookup(bb, watcherLogger, child);
				if (val != null && !val.Equals(default(O)))
				{
					watcherLogger.RegisterNode(this, PriorityNodeResult.Success);
					return val;
				}
			}
		}
		watcherLogger.RegisterNode(this, PriorityNodeResult.NoResult);
		return default(O);
	}

	private void LayeredLookup<O>(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<O> lookupOutput, HashSet<string> outOccupiedLayers, Action<IBlackboard, IWatcherLogger, INode<T>, HashSet<O>, HashSet<string>> preformLookup)
	{
		if (!IsLayered)
		{
			throw new InvalidOperationException("Type " + PayloadType.Name + " is not an ILayeredPayload. GetLayeredPayload() cannot be called.");
		}
		if (Children.Count == 0)
		{
			watcherLogger.RegisterNode(this, PriorityNodeResult.NoChildren);
			return;
		}
		int count = lookupOutput.Count;
		foreach (INode<T> child in Children)
		{
			if (child != null)
			{
				preformLookup(bb, watcherLogger, child, lookupOutput, outOccupiedLayers);
			}
		}
		if (count != lookupOutput.Count)
		{
			watcherLogger.RegisterNode(this, PriorityNodeResult.Success);
		}
		else
		{
			watcherLogger.RegisterNode(this, PriorityNodeResult.NoResult);
		}
	}

	public IEnumerable<INode<T>> EnumerateNodes()
	{
		yield return this;
		foreach (INode<T> child in Children)
		{
			foreach (INode<T> item in child.EnumerateNodes())
			{
				yield return item;
			}
		}
	}

	public IReadOnlyCollection<INode<T>> GetChildren()
	{
		return new List<INode<T>>(Children);
	}
}
