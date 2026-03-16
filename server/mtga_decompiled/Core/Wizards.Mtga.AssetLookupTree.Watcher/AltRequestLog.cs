using System;
using System.Collections.Generic;
using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using Newtonsoft.Json.Linq;
using Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

namespace Wizards.Mtga.AssetLookupTree.Watcher;

public class AltRequestLog
{
	private static (ulong version, JObject snapshot) _cachedBbSnapshot;

	private static ulong _nextId;

	public readonly ulong Id;

	public readonly StackTrace StackTrace;

	public readonly Type PayloadType;

	public readonly object Tree;

	public readonly JObject BlackboardSnapshot;

	public readonly string BlackboardSnapshotText;

	public readonly List<IAltRequestLogEntry> Entries = new List<IAltRequestLogEntry>(10);

	public readonly DateTime RequestStart;

	private RequestResult _result;

	public DateTime RequestEnd { get; private set; }

	public double RequestDuration => (RequestEnd - RequestStart).TotalMilliseconds;

	public RequestResult Result
	{
		get
		{
			return _result;
		}
		set
		{
			_result = value;
			RequestEnd = DateTime.Now;
		}
	}

	public static AltRequestLog CreateLog<T>(AssetLookupTree<T> tree, IBlackboard bb) where T : class, IPayload
	{
		if (_cachedBbSnapshot.version != bb.ContentVersion)
		{
			_cachedBbSnapshot = (version: bb.ContentVersion, snapshot: BlackboardUtils.CreateBlackboardSnapshot(bb));
		}
		return new AltRequestLog(typeof(T), tree, _cachedBbSnapshot.snapshot);
	}

	public AltRequestLog(Type payloadType, object tree, JObject blackboardSnapshot)
	{
		Id = _nextId++;
		RequestStart = DateTime.Now;
		PayloadType = payloadType;
		Tree = tree;
		BlackboardSnapshot = blackboardSnapshot;
		BlackboardSnapshotText = blackboardSnapshot.ToString();
		StackTrace = new StackTrace(4, fNeedFileInfo: true);
	}
}
