using AssetLookupTree.Blackboard;
using UnityEngine;
using Wizards.Mtga;

namespace AssetLookupTree.Evaluators.CardData;

public class IsDebugBuild : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		bool inValue = Debug.isDebugBuild || HasRole(Pantry.Get<IAccountClient>());
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
	}

	private bool HasRole(IAccountClient accountClient)
	{
		if (accountClient == null)
		{
			return false;
		}
		return accountClient.AccountInformation?.HasRole_Debugging() ?? false;
	}
}
