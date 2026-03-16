using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AbilityWordDataCount : EvaluatorBase_Int
{
	public string AbilityWord;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.ActiveAbilityWords.Count == 0)
		{
			return false;
		}
		int num = 0;
		foreach (AbilityWordData activeAbilityWord in bb.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord.Equals(AbilityWord))
			{
				num++;
			}
		}
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, num);
	}
}
