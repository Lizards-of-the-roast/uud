using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AbilityWordValue : EvaluatorBase_List<int>
{
	public string AbilityWord;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.CardData.ActiveAbilityWords == null)
		{
			return false;
		}
		AbilityWordData abilityWordData = bb.CardData.ActiveAbilityWords.Find(AbilityWord, (AbilityWordData x, string t) => x.AbilityWord == t);
		if (abilityWordData.Equals(default(AbilityWordData)) || abilityWordData.Values == null)
		{
			return false;
		}
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, abilityWordData.Values, MinCount, MaxCount);
	}
}
