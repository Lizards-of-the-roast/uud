using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_DesignationValue_Intensity : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null)
		{
			foreach (DesignationData designation in bb.CardData.Instance.Designations)
			{
				if (designation.Type == Designation.Intensity && designation.Value.HasValue)
				{
					return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)designation.Value.Value);
				}
			}
		}
		return false;
	}
}
