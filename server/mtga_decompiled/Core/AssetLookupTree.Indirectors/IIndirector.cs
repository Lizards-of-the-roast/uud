using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Indirectors;

public interface IIndirector
{
	void SetCache(IBlackboard bb);

	void ClearCache(IBlackboard bb);

	IEnumerable<IBlackboard> Execute(IBlackboard bb);
}
