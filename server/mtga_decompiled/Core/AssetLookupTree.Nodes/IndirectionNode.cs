using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Indirectors;
using Wizards.Mtga.AssetLookupTree.Watcher;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace AssetLookupTree.Nodes;

public class IndirectionNode<T> : INodeWithChild<T>, INode<T>, INode where T : class, IPayload
{
	public IIndirector Indirector;

	[NonSerialized]
	public readonly Type PayloadType = typeof(T);

	[NonSerialized]
	public readonly bool IsLayered = typeof(ILayeredPayload).IsAssignableFrom(typeof(T));

	public INode<T> Child { get; set; }

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
		if (Child == null)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.NoChild);
			return default(O);
		}
		if (Indirector == null)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.NoIndirector);
			return default(O);
		}
		O val = default(O);
		uint num = 0u;
		Indirector.SetCache(bb);
		foreach (IBlackboard item in Indirector.Execute(bb))
		{
			num++;
			val = performLookup(item, watcherLogger, Child);
			if (val != null)
			{
				object obj = default(O);
				if (!val.Equals(obj))
				{
					break;
				}
			}
		}
		Indirector.ClearCache(bb);
		if (val != null)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.Success);
		}
		else if (num == 0)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.EmptyIndirector);
		}
		else
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.NoResult);
		}
		return val;
	}

	private void LayeredLookup<O>(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<O> lookupOutput, HashSet<string> outOccupiedLayers, Action<IBlackboard, IWatcherLogger, INode<T>, HashSet<O>, HashSet<string>> preformLookup)
	{
		if (!IsLayered)
		{
			throw new InvalidOperationException("Type " + PayloadType.Name + " is not an ILayeredPayload. GetLayeredPayload() cannot be called.");
		}
		if (Child == null)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.NoChild);
			return;
		}
		if (Indirector == null)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.NoIndirector);
			return;
		}
		int count = lookupOutput.Count;
		uint num = 0u;
		Indirector.SetCache(bb);
		foreach (IBlackboard item in Indirector.Execute(bb))
		{
			num++;
			preformLookup(item, watcherLogger, Child, lookupOutput, outOccupiedLayers);
		}
		Indirector.ClearCache(bb);
		if (count != lookupOutput.Count)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.Success);
		}
		else if (num == 0)
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.EmptyIndirector);
		}
		else
		{
			watcherLogger.RegisterNode(this, IndirectionNodeResult.NoResult);
		}
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
