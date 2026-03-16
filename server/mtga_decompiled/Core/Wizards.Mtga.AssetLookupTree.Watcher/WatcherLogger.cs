using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Nodes;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace Wizards.Mtga.AssetLookupTree.Watcher;

public class WatcherLogger : IWatcherLogger
{
	public bool RecordNewRequests;

	private readonly List<AltRequestLog> _requestLogs = new List<AltRequestLog>(100);

	private AltRequestLog _activeLog;

	public IReadOnlyList<AltRequestLog> RequestLogs => _requestLogs;

	public void ClearAllLogs()
	{
		_requestLogs.Clear();
	}

	public void ClearLog(AltRequestLog log)
	{
		_requestLogs.Remove(log);
	}

	void IWatcherLogger.BeginRequestBookmark<T>(AssetLookupTree<T> tree, IBlackboard bb)
	{
		if (RecordNewRequests && _activeLog == null)
		{
			_activeLog = AltRequestLog.CreateLog(tree, bb);
		}
	}

	void IWatcherLogger.EndRequestBookmark<T>(AssetLookupTree<T> tree, RequestResult result)
	{
		if (RecordNewRequests && _activeLog != null && _activeLog.Tree == tree)
		{
			_activeLog.Result = result;
			_requestLogs.Add(_activeLog);
			_activeLog = null;
		}
	}

	void IWatcherLogger.RegisterNode<T>(ValueNode<T> node, ValueNodeResult result)
	{
		if (RecordNewRequests && _activeLog != null)
		{
			_activeLog.Entries.Add(ValueLogEntry.CreateEntry(node, result));
		}
	}

	void IWatcherLogger.RegisterNode<T>(ConditionNode<T> node, ConditionNodeResult result)
	{
		if (RecordNewRequests && _activeLog != null)
		{
			_activeLog.Entries.Add(ConditionLogEntry.CreateEntry(node, result));
		}
	}

	void IWatcherLogger.RegisterNode<T, U>(BucketNode<T, U> node, BucketNodeResult result)
	{
		if (RecordNewRequests && _activeLog != null)
		{
			_activeLog.Entries.Add(BucketLogEntry.CreateEntry(node, result));
		}
	}

	void IWatcherLogger.RegisterNode<T>(IndirectionNode<T> node, IndirectionNodeResult result)
	{
		if (RecordNewRequests && _activeLog != null)
		{
			_activeLog.Entries.Add(IndirectionLogEntry.CreateEntry(node, result));
		}
	}

	void IWatcherLogger.RegisterNode<T>(PriorityNode<T> node, PriorityNodeResult result)
	{
		if (RecordNewRequests && _activeLog != null)
		{
			_activeLog.Entries.Add(PriorityLogEntry.CreateEntry(node, result));
		}
	}
}
