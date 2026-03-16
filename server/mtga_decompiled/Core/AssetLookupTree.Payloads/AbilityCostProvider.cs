using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Payloads;

public class AbilityCostProvider : ILocParameterProvider
{
	private const string KEY = "abilityCost";

	public string GetKey()
	{
		return "abilityCost";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		if (filledBB == null || filledBB.Ability == null)
		{
			paramValue = string.Empty;
			return false;
		}
		paramValue = ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaText(filledBB.Ability.ManaCost));
		return true;
	}
}
