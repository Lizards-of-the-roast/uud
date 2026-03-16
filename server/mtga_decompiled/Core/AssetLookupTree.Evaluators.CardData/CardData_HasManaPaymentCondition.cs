using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasManaPaymentCondition : EvaluatorBase_Boolean
{
	private static readonly ManaPaymentConditionAbilities ManaPaymentAbilities = new ManaPaymentConditionAbilities();

	public ManaPaymentConditionAbilities.AbilityGroup AbilityGroup;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		bool inValue = ManaPaymentAbilities.HasManaConditionAbility(AbilityGroup, bb.CardData);
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
	}
}
