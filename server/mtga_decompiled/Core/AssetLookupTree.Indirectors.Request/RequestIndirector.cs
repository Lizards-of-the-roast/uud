using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.Request;

public abstract class RequestIndirector : IIndirector
{
	private BaseUserRequest _cachedRequest;

	public void SetCache(IBlackboard bb)
	{
		_cachedRequest = bb.Request;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Request = _cachedRequest;
	}

	public abstract IEnumerable<IBlackboard> Execute(IBlackboard bb);
}
