using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AbilityWordChoice : EvaluatorBase_String
{
	public string AbilityWord;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		AbilityWordData abilityWordData = bb.CardData.ActiveAbilityWords.Find(AbilityWord, (AbilityWordData x, string t) => x.AbilityWord == t);
		if (abilityWordData.Equals(default(AbilityWordData)))
		{
			return false;
		}
		return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, abilityWordData.Choice);
	}
}
