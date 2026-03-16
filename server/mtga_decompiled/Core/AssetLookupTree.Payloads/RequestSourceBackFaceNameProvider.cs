using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads;

public class RequestSourceBackFaceNameProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "BackFaceName";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.Request != null && filledBB.GameState != null && filledBB.CardDatabase != null && filledBB.Request.SourceId != 0 && filledBB.GameState.TryGetCard(filledBB.Request.SourceId, out var card))
		{
			CardPrintingData cardPrintingData = ((card.ObjectType != GameObjectType.Ability) ? filledBB.CardDataProvider.GetCardPrintingById(card.GrpId) : filledBB.CardDataProvider.GetCardPrintingById(card.Parent?.GrpId ?? 0));
			if (cardPrintingData != null && cardPrintingData.LinkedFacePrintings.Count == 1)
			{
				paramValue = filledBB.CardDatabase.GreLocProvider.GetLocalizedText(cardPrintingData.LinkedFacePrintings[0].TitleId);
			}
		}
		return !string.IsNullOrEmpty(paramValue);
	}
}
