using GreClient.Rules;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class SourceCardNameParameterProvider : IPromptParameterProvider
{
	public string GetKey()
	{
		return "CardName";
	}

	public bool TryGetValue(BaseUserRequest request, GameManager gameManager, out string paramValue)
	{
		paramValue = string.Empty;
		if (gameManager.LatestGameState.TryGetCard(request.SourceId, out var card))
		{
			paramValue = gameManager.CardDatabase.GreLocProvider.GetLocalizedText(card.TitleId);
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
