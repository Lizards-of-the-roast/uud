using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wizards.Mtga.AssetLookupTree.Watcher;

namespace AssetLookupTree.Nodes;

public class OrganizationNode<T> : INodeWithChild<T>, INode<T>, INode where T : class, IPayload
{
	public Guid NodeId { get; set; } = Guid.NewGuid();

	public string Label { get; set; } = string.Empty;

	public string Comment { get; set; } = string.Empty;

	public INode<T> Child { get; set; }

	public bool SerializeIndependently { get; set; } = true;

	public T GetPayload(IBlackboard bb, IWatcherLogger watcherLogger)
	{
		INode<T> child = Child;
		if (child == null)
		{
			return null;
		}
		return child.GetPayload(bb, watcherLogger);
	}

	public void GetPayloadLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<T> outPayloads, HashSet<string> outOccupiedLayers)
	{
		Child?.GetPayloadLayered(bb, watcherLogger, outPayloads, outOccupiedLayers);
	}

	public Guid GetValueNodeId(IBlackboard bb, IWatcherLogger watcherLogger)
	{
		return Child?.GetValueNodeId(bb, watcherLogger) ?? default(Guid);
	}

	public void GetValueNodeIdsLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<Guid> outValueIds, HashSet<string> outOccupiedLayers)
	{
		Child?.GetValueNodeIdsLayered(bb, watcherLogger, outValueIds, outOccupiedLayers);
	}

	public IEnumerable<INode<T>> EnumerateNodes()
	{
		yield return this;
		if (Child == null)
		{
			yield break;
		}
		foreach (INode<T> item in Child.EnumerateNodes())
		{
			yield return item;
		}
	}
}
