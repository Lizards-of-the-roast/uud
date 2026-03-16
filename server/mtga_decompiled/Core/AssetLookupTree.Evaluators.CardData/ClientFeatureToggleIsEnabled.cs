using AssetLookupTree.Blackboard;
using Core.Code.ClientFeatureToggle;
using Wizards.Mtga;

namespace AssetLookupTree.Evaluators.CardData;

public class ClientFeatureToggleIsEnabled : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (string.IsNullOrWhiteSpace(ExpectedValue))
		{
			return false;
		}
		ClientFeatureToggleDataProvider clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		if (!clientFeatureToggleDataProvider.IsInitialized())
		{
			return false;
		}
		bool toggleValueById = clientFeatureToggleDataProvider.GetToggleValueById(ExpectedValue);
		return EvaluatorBase_String.GetResult(true.ToString(), Operation, ExpectedResult, toggleValueById.ToString());
	}
}
