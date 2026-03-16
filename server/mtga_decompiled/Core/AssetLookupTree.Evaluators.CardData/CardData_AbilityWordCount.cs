using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AbilityWordCount : EvaluatorBase_Int
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
		int? num = null;
		foreach (AbilityWordData activeAbilityWord in bb.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord.Equals(AbilityWord))
			{
				if (!string.IsNullOrWhiteSpace(activeAbilityWord.AdditionalDetail) && int.TryParse(activeAbilityWord.AdditionalDetail, out var result))
				{
					num = result;
				}
				else if (!num.HasValue)
				{
					num = 0;
				}
			}
		}
		if (num.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, num.Value);
		}
		return false;
	}
}
