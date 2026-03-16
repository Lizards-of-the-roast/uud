using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public class XChoiceNumeralProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "numeral";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.CardData != null && filledBB.CardData.Instance != null && filledBB.CardData.Instance.ChooseXResult.HasValue)
		{
			paramValue = filledBB.CardData.Instance.ChooseXResult.Value.ToString();
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
