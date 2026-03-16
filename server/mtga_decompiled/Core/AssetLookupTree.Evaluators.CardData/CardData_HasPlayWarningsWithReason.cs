using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasPlayWarningsWithReason : EvaluatorBase_List<ShouldntPlayData.ReasonType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null && bb.CardData.Instance.PlayWarnings != null)
		{
			return EvaluatorBase_List<ShouldntPlayData.ReasonType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Instance.PlayWarnings.SelectMany((ShouldntPlayData x) => x.Reasons), MinCount, MaxCount);
		}
		return false;
	}
}
