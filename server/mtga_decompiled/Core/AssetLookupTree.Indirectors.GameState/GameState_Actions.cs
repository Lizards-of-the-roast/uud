using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.GameState;

public class GameState_Actions : IIndirector
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
		_cacheType = ActionType.None;
		_cacheAction = null;
	}

	public virtual IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState == null)
		{
			yield break;
		}
		foreach (ActionInfo action in bb.GameState.Actions)
		{
			bb.GreAction = action.Action;
			bb.GreActionType = action.Action.ActionType;
			yield return bb;
		}
	}
}
