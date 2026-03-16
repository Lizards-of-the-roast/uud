using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsPromptParam_CardId : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Prompt != null && bb.Prompt.Parameters != null && bb.CardData != null)
		{
			foreach (PromptParameter parameter in bb.Prompt.Parameters)
			{
				if (parameter.ParameterName == "CardId" && parameter.Type == ParameterType.Number)
				{
					int numberValue = parameter.NumberValue;
					if (bb.CardData.InstanceId == numberValue)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
