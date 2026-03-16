using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public class CardNameProvider : KeyCaseLocParameterProvider
{
	protected override string GetParamKey()
	{
		return "CardName";
	}

	public override bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.CardData != null && filledBB.CardDatabase != null)
		{
			paramValue = filledBB.CardDatabase.GreLocProvider.GetLocalizedText(filledBB.CardData.TitleId);
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
