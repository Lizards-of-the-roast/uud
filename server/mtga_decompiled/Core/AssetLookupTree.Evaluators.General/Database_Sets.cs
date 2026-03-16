using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class Database_Sets : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardDatabase != null)
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardDatabase.DatabaseUtilities.SetsInDatabase(), MinCount, MaxCount);
		}
		return false;
	}
}
