using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.Actions;

public class GameState_Actions_Count : EvaluatorBase_Int
{
	public GREPlayerNum Player;

	public ActionType Type;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.GameState != null)
		{
			MtgPlayer player = bb.GameState.GetPlayerByEnum(Player);
			if (player != null)
			{
				int inValue = bb.GameState.Actions.Count((ActionInfo x) => x.IsActionType(Type) && x.SeatId == player.InstanceId);
				return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, inValue);
			}
		}
		return false;
	}
}
