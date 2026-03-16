using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.Zone;

public class GameState_Zone_Size : EvaluatorBase_Int
{
	public ZoneType Zone;

	public GREPlayerNum Player;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.GameState != null)
		{
			MtgZone zone = bb.GameState.GetZone(Zone, Player);
			if (zone != null)
			{
				return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, zone.VisibleCards.Count);
			}
		}
		return false;
	}
}
