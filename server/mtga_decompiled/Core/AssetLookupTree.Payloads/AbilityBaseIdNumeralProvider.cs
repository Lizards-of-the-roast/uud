using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public class AbilityBaseIdNumeralProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "numeral";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.Ability != null && filledBB.Ability.BaseIdNumeral.HasValue)
		{
			paramValue = filledBB.Ability.BaseIdNumeral.Value.ToString();
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
