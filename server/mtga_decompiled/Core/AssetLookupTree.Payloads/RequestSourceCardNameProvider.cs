using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Payloads;

public class RequestSourceCardNameProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "CardName";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.Request != null && filledBB.GameState != null && filledBB.CardDatabase != null && filledBB.Request.SourceId != 0 && filledBB.GameState.TryGetCard(filledBB.Request.SourceId, out var card))
		{
			paramValue = filledBB.CardDatabase.GreLocProvider.GetLocalizedText(card.TitleId);
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
