using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wizards.Mtga.AssetLookupTree.Watcher;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;
using Wizards.Mtga.Assets;

namespace AssetLookupTree.Nodes;

public class ValueNode<T> : INode<T>, INode, IValueNode where T : class, IPayload
{
	public T Payload;

	[NonSerialized]
	public readonly Type PayloadType = typeof(T);

	[NonSerialized]
	public readonly bool IsLayered = typeof(ILayeredPayload).IsAssignableFrom(typeof(T));

	public AssetPriority Priority { get; set; }

	IPayload IValueNode.Payload => Payload;

	public Guid NodeId { get; set; } = Guid.NewGuid();

	public List<string> Tests { get; set; }

	public string Comment { get; set; } = string.Empty;

	public string Label { get; set; } = string.Empty;

	public T GetPayload(IBlackboard bb, IWatcherLogger watcherLogger)
	{
		watcherLogger.RegisterNode(this, ValueNodeResult.Success);
		return Payload;
	}

	public Guid GetValueNodeId(IBlackboard bb, IWatcherLogger watcherLogger)
	{
		return NodeId;
	}

	private bool IsLayeredPayloadNode(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<string> outOccupiedLayers)
	{
		if (!IsLayered)
		{
			throw new InvalidOperationException("Type " + PayloadType.Name + " is not an ILayeredPayload. GetLayeredPayload() cannot be called.");
		}
		if (!(Payload is ILayeredPayload layeredPayload))
		{
			return false;
		}
		if (layeredPayload.Layers.Count == 0)
		{
			if (outOccupiedLayers.Contains("DEFAULT"))
			{
				watcherLogger?.RegisterNode(this, ValueNodeResult.LayerOccupied);
				return false;
			}
			outOccupiedLayers.Add("DEFAULT");
		}
		else
		{
			if (outOccupiedLayers.Overlaps(layeredPayload.Layers))
			{
				watcherLogger?.RegisterNode(this, ValueNodeResult.LayerOccupied);
				return false;
			}
			outOccupiedLayers.UnionWith(layeredPayload.Layers);
		}
		return true;
	}

	public void GetValueNodeIdsLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<Guid> outPayloads, HashSet<string> outOccupiedLayers)
	{
		if (IsLayeredPayloadNode(bb, watcherLogger, outOccupiedLayers))
		{
			watcherLogger.RegisterNode(this, ValueNodeResult.Success);
			outPayloads.Add(NodeId);
		}
	}

	public void GetPayloadLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<T> outPayloads, HashSet<string> outOccupiedLayers)
	{
		if (IsLayeredPayloadNode(bb, watcherLogger, outOccupiedLayers))
		{
			watcherLogger.RegisterNode(this, ValueNodeResult.Success);
			outPayloads.Add(Payload);
		}
	}

	public IEnumerable<INode<T>> EnumerateNodes()
	{
		yield return this;
	}
}
