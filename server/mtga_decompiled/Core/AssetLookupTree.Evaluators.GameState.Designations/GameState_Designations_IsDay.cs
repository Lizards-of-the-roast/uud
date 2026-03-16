using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.Designations;

public class GameState_Designations_IsDay : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GameState != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.GameState.Designations.Exists((DesignationData x) => x.AffectorId == 0 && x.AffectedId == 0 && x.Type == Designation.Day));
		}
		return false;
	}
}
