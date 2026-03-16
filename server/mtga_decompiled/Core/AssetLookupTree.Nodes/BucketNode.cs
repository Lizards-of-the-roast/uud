using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Extractors;
using AssetLookupTree.Payloads.Card;
using Wizards.Mtga.AssetLookupTree.Watcher;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace AssetLookupTree.Nodes;

public class BucketNode<T, U> : INode<T>, INode, INodeWithChildren<T> where T : class, IPayload
{
	public static BucketNode<MaterialOverride, int> _unusedMaterialOverideInt;

	public static BucketNode<TextureOverride, int> _unusedTextureOverideInt;

	public static BucketNode<ArtIdOverride, int> _unusedArtIdOverideInt;

	public static BucketNode<ColorOverride, int> _unusedTColorOverideInt;

	public IExtractor<U> Extractor;

	public readonly SortedDictionary<U, INode<T>> Children = new SortedDictionary<U, INode<T>>();

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

	private void LayeredLookup<O>(IBlackboard bb, IWatcherLogger watcherLogger, HashSet<O> outPayloads, HashSet<string> outOccupiedLayers, Action<IBlackboard, IWatcherLogger, INode<T>, HashSet<O>, HashSet<string>> performLookup)
	{
		if (!IsLayered)
		{
			throw new InvalidOperationException("Type " + PayloadType.Name + " is not an ILayeredPayload. GetLayeredPayload() cannot be called.");
		}
		if (Extractor == null)
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NoExtractor);
			return;
		}
		if (!Extractor.Execute(bb, out var value))
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.ExtractorFailed);
			return;
		}
		if (!Children.TryGetValue(value, out var value2))
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NoBucketForValue);
			return;
		}
		if (value2 == null)
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NullChildInBucket);
			return;
		}
		int count = outPayloads.Count;
		performLookup(bb, watcherLogger, value2, outPayloads, outOccupiedLayers);
		if (count == outPayloads.Count)
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NoResult);
		}
		else
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.Success);
		}
	}

	private O SingleLookup<O>(IBlackboard bb, IWatcherLogger watcherLogger, Func<IBlackboard, IWatcherLogger, INode<T>, O> performLookup)
	{
		if (Extractor == null)
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NoExtractor);
			return default(O);
		}
		if (!Extractor.Execute(bb, out var value))
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.ExtractorFailed);
			return default(O);
		}
		if (!Children.TryGetValue(value, out var value2))
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NoBucketForValue);
			return default(O);
		}
		if (value2 == null)
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NullChildInBucket);
			return default(O);
		}
		O val = performLookup(bb, watcherLogger, value2);
		if (val != null && !val.Equals(default(O)))
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.Success);
		}
		else
		{
			watcherLogger.RegisterNode(this, BucketNodeResult.NoResult);
		}
		return val;
	}

	public IEnumerable<INode<T>> EnumerateNodes()
	{
		yield return this;
		foreach (INode<T> value in Children.Values)
		{
			foreach (INode<T> item in value.EnumerateNodes())
			{
				yield return item;
			}
		}
	}

	public IReadOnlyCollection<INode<T>> GetChildren()
	{
		return new List<INode<T>>(Children.Values);
	}
}
