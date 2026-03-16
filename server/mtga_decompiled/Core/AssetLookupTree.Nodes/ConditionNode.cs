using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Evaluators;
using Wizards.Mtga.AssetLookupTree.Watcher;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace AssetLookupTree.Nodes;

public class ConditionNode<T> : INodeWithChild<T>, INode<T>, INode where T : class, IPayload
{
	public IEvaluator Evaluator;

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
		return SinglePayload(bb, watcherLogger, (IBlackboard blackboard, IWatcherLogger logger, INode<T> node) => node.GetPayload(blackboard, logger));
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
		return SinglePayload(bb, watcherLogger, (IBlackboard blackboard, IWatcherLogger logger, INode<T> node) => node.GetValueNodeId(blackboard, logger));
	}

	public void GetValueNodeIdsLayered(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<Guid> outValueIds, HashSet<string> outOccupiedLayers)
	{
		LayeredLookup(bb, watcherLogger, outValueIds, outOccupiedLayers, delegate(IBlackboard blackboard, IWatcherLogger logger, INode<T> node, HashSet<Guid> output, HashSet<string> layers)
		{
			node.GetValueNodeIdsLayered(blackboard, logger, output, layers);
		});
	}

	private O SinglePayload<O>(IBlackboard bb, IWatcherLogger watcherLogger, Func<IBlackboard, IWatcherLogger, INode<T>, O> performLookup)
	{
		if (Child == null)
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.NoChild);
			return default(O);
		}
		if (Evaluator == null)
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.NoEvaluator);
			return default(O);
		}
		if (!Evaluator.Execute(bb))
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.EvaluatorFailed);
			return default(O);
		}
		O val = performLookup(bb, watcherLogger, Child);
		if (val != null && !val.Equals(default(O)))
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.Success);
		}
		else
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.NoResult);
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
			watcherLogger.RegisterNode(this, ConditionNodeResult.NoChild);
			return;
		}
		if (Evaluator == null)
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.NoEvaluator);
			return;
		}
		if (!Evaluator.Execute(bb))
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.EvaluatorFailed);
			return;
		}
		int count = lookupOutput.Count;
		preformLookup(bb, watcherLogger, Child, lookupOutput, outOccupiedLayers);
		if (count != lookupOutput.Count)
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.Success);
		}
		else
		{
			watcherLogger.RegisterNode(this, ConditionNodeResult.NoResult);
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
