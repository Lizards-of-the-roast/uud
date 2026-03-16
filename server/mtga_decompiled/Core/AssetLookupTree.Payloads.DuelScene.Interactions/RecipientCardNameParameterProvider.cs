using GreClient.Rules;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class RecipientCardNameParameterProvider : IPromptParameterProvider
{
	public string GetKey()
	{
		return "RecipientCardName";
	}

	public bool TryGetValue(BaseUserRequest request, GameManager gameManager, out string paramValue)
	{
		paramValue = string.Empty;
		if (request is OptionalActionMessageRequest { RecipientIds: not null } optionalActionMessageRequest && optionalActionMessageRequest.RecipientIds.Count > 0 && gameManager.LatestGameState.TryGetCard(optionalActionMessageRequest.RecipientIds[0], out var card))
		{
			paramValue = gameManager.CardDatabase.GreLocProvider.GetLocalizedText(card.TitleId);
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
