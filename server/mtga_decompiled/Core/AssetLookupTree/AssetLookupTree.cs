using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Nodes;
using Wizards.Mtga.AssetLookupTree.Watcher;
using Wizards.Mtga.Assets;

namespace AssetLookupTree;

public class AssetLookupTree<T> : IAssetLookupTree where T : class, IPayload
{
	public INode<T> Root;

	public readonly Type PayloadType = typeof(T);

	public readonly bool IsLayered = typeof(ILayeredPayload).IsAssignableFrom(typeof(T));

	private readonly HashSet<string> _tmpLayerMap = new HashSet<string>();

	private IWatcherLogger _watcherLogger = new NullLogger();

	public AssetPriority Priority { get; set; }

	public AssetPriority DefaultPayloadPriority { get; set; }

	public uint AssetsPerBundle { get; set; } = 20u;

	public bool MustReturnPayload { get; set; }

	public void InjectLogger(IWatcherLogger watcherLogger)
	{
		if (watcherLogger != null)
		{
			_watcherLogger = watcherLogger;
		}
	}

	public T GetPayload(IBlackboard bb)
	{
		return SingleLookup(bb, (IBlackboard blackboard, INode<T> root, IWatcherLogger logger) => root.GetPayload(blackboard, logger));
	}

	public bool GetPayloadLayered(IBlackboard bb, HashSet<T> outPayloads)
	{
		return LayeredLookup(bb, outPayloads, (_watcherLogger, _tmpLayerMap), delegate(IBlackboard blackboard, INode<T> root, HashSet<T> hashSet, (IWatcherLogger Logger, HashSet<string> Layers) dataBlob)
		{
			root.GetPayloadLayered(blackboard, dataBlob.Logger, hashSet, dataBlob.Layers);
		});
	}

	public Guid GetValueNodeId(IBlackboard bb)
	{
		return SingleLookup(bb, (IBlackboard blackboard, INode<T> root, IWatcherLogger logger) => root.GetValueNodeId(blackboard, logger));
	}

	public bool GetValueNodeIdsLayered(IBlackboard bb, HashSet<Guid> outPayloads)
	{
		return LayeredLookup(bb, outPayloads, (_watcherLogger, _tmpLayerMap), delegate(IBlackboard blackboard, INode<T> root, HashSet<Guid> hashSet, (IWatcherLogger Logger, HashSet<string> Layers) dataBlob)
		{
			root.GetValueNodeIdsLayered(blackboard, dataBlob.Logger, hashSet, dataBlob.Layers);
		});
	}

	private O SingleLookup<O>(IBlackboard bb, Func<IBlackboard, INode<T>, IWatcherLogger, O> performLookup)
	{
		_watcherLogger.BeginRequestBookmark(this, bb);
		if (Root == null)
		{
			_watcherLogger.EndRequestBookmark(this, RequestResult.NoRoot);
			return default(O);
		}
		O val = performLookup(bb, Root, _watcherLogger);
		if (val == null)
		{
			_watcherLogger.EndRequestBookmark(this, RequestResult.NoResult);
			return default(O);
		}
		_watcherLogger.EndRequestBookmark(this, RequestResult.Success);
		return val;
	}

	private bool LayeredLookup<O, D>(IBlackboard bb, HashSet<O> lookupOutput, D dataBlob, Action<IBlackboard, INode<T>, HashSet<O>, D> preformLookup)
	{
		if (!IsLayered)
		{
			throw new InvalidOperationException("Type " + PayloadType.Name + " is not an ILayeredPayload. GetLayeredPayload() cannot be called.");
		}
		lookupOutput.Clear();
		_tmpLayerMap.Clear();
		_watcherLogger.BeginRequestBookmark(this, bb);
		if (Root == null)
		{
			_watcherLogger.EndRequestBookmark(this, RequestResult.NoRoot);
			return false;
		}
		preformLookup?.Invoke(bb, Root, lookupOutput, dataBlob);
		_tmpLayerMap.Clear();
		if (lookupOutput.Count == 0)
		{
			_watcherLogger.EndRequestBookmark(this, RequestResult.NoResult);
			return false;
		}
		_watcherLogger.EndRequestBookmark(this, RequestResult.Success);
		return true;
	}

	public IEnumerable<INode<T>> EnumerateNodes()
	{
		if (Root == null)
		{
			yield break;
		}
		foreach (INode<T> item in Root.EnumerateNodes())
		{
			yield return item;
		}
	}

	public Type GetPayloadType()
	{
		return typeof(T);
	}
}
