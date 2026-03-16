using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Indirectors;

public abstract class IndirectorBase_Enum<T> : IIndirector
{
	public T ExpectedValue { get; set; }

	public virtual void SetCache(IBlackboard bb)
	{
		throw new NotImplementedException();
	}

	public virtual void ClearCache(IBlackboard bb)
	{
		throw new NotImplementedException();
	}

	public virtual IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		throw new NotImplementedException();
	}
}
