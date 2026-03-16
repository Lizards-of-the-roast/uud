using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_ActiveAbilityWords : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in bb.CardData.ActiveAbilityWords
				where x.IsActive
				select x.AbilityWord, MinCount, MaxCount);
		}
		return false;
	}
}
