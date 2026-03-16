using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.Interaction.ActionsAvailable;

public abstract class ActionIndirector : IIndirector
{
	private ActionType _cacheType;

	private Wotc.Mtgo.Gre.External.Messaging.Action _cacheAction;

	public void SetCache(IBlackboard bb)
	{
		_cacheType = bb.GreActionType;
		_cacheAction = bb.GreAction;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.GreActionType = _cacheType;
		bb.GreAction = _cacheAction;
	}

	public abstract IEnumerable<IBlackboard> Execute(IBlackboard bb);
}
