using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class SourceCardBackFaceNameParameterProvider : IPromptParameterProvider
{
	public string GetKey()
	{
		return "BackFaceName";
	}

	public bool TryGetValue(BaseUserRequest request, GameManager gameManager, out string paramValue)
	{
		paramValue = string.Empty;
		if (gameManager.LatestGameState.TryGetCard(request.SourceId, out var card))
		{
			CardPrintingData cardPrintingData = ((card.ObjectType != GameObjectType.Ability) ? gameManager.CardDatabase.CardDataProvider.GetCardPrintingById(card.GrpId) : gameManager.CardDatabase.CardDataProvider.GetCardPrintingById(card.Parent?.GrpId ?? 0));
			if (cardPrintingData != null && cardPrintingData.LinkedFacePrintings.Count == 1)
			{
				paramValue = gameManager.CardDatabase.GreLocProvider.GetLocalizedText(cardPrintingData.LinkedFacePrintings[0].TitleId);
			}
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
