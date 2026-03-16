using AssetLookupTree.Blackboard;
using Wotc.Mtga.Hangers;

namespace AssetLookupTree.Payloads;

public class AbilityBaseNumeralCardNameProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "cardName";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.Ability != null && filledBB.CardDatabase != null && filledBB.Ability.BaseIdNumeral.HasValue && InjectNumeralCardName.TryGetCardNameLoc(filledBB.Ability, filledBB.CardDatabase.GreLocProvider, out var locText))
		{
			paramValue = locText;
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
