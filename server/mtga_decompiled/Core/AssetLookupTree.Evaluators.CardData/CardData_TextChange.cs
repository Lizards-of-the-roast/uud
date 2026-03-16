using AssetLookupTree.Blackboard;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_TextChange : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.AbilityIds.Contains(bb.CardData.Printing.TextChangeData.ChangedAbilityId))
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)bb.CardData.Printing.TextChangeData.ChangeSourceId);
		}
		return false;
	}
}
