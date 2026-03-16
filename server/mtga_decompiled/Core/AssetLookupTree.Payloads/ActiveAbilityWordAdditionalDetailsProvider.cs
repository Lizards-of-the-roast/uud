using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Payloads;

public class ActiveAbilityWordAdditionalDetailsProvider : ILocParameterProvider
{
	public string LocParameterKey = "AdditionalDetail";

	public string AbilityWord = "";

	public string GetKey()
	{
		return LocParameterKey;
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.CardData != null && filledBB.Ability != null)
		{
			paramValue = FindAbilityWordAdditionalDetails(filledBB);
		}
		return !string.IsNullOrEmpty(paramValue);
	}

	private string FindAbilityWordAdditionalDetails(IBlackboard filledBB)
	{
		foreach (AbilityWordData activeAbilityWord in filledBB.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == AbilityWord)
			{
				return activeAbilityWord.AdditionalDetail;
			}
		}
		return string.Empty;
	}
}
