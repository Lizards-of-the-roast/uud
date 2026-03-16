using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.GameState;

public class GameState_Actions_Opponent : GameState_Actions
{
	public override IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState == null)
		{
			yield break;
		}
		foreach (ActionInfo action in bb.GameState.Actions)
		{
			if (action.SeatId == bb.GameState.Opponent.InstanceId)
			{
				bb.GreAction = action.Action;
				bb.GreActionType = action.Action.ActionType;
				yield return bb;
			}
		}
	}
}
