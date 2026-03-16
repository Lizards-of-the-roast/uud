using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wizards.Mtga.AssetLookupTree.Watcher;

namespace AssetLookupTree.Nodes;

public interface INode
{
	Guid NodeId { get; set; }

	string Label { get; }

	string Comment { get; }
}
public interface INode<T> : INode where T : class, IPayload
{
	T GetPayload(IBlackboard bb, IWatcherLogger watcherLogger);

	Guid GetValueNodeId(IBlackboard bb, IWatcherLogger watcherLogger);

	void GetPayloadLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<T> outPayloads, HashSet<string> outOccupiedLayers);

	void GetValueNodeIdsLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<Guid> outValueIds, HashSet<string> outOccupiedLayers);

	IEnumerable<INode<T>> EnumerateNodes();
}
