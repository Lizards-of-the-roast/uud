using AssetLookupTree.Blackboard;
using GreClient.CardData;
using ReferenceMap;

namespace AssetLookupTree.Payloads;

public class TriggeredByRequestSourceNameProvider : ILocParameterProvider
{
	public string GetKey()
	{
		return "CardName";
	}

	public bool TryGetValue(IBlackboard filledBB, out string paramValue)
	{
		paramValue = string.Empty;
		if (filledBB.Request != null && filledBB.GameState != null && filledBB.CardDatabase != null && filledBB.Request.SourceId != 0 && filledBB.GameState.TryGetCard(filledBB.Request.SourceId, out var _))
		{
			uint triggeredById = filledBB.GameState.ReferenceMap.GetTriggeredById(filledBB.Request.SourceId);
			if (filledBB.GameState.TryGetCard(triggeredById, out var card2))
			{
				if (card2.TitleId == 0)
				{
					CardPrintingData cardPrintingById = filledBB.CardDataProvider.GetCardPrintingById(card2.GrpId);
					if (cardPrintingById != null && cardPrintingById.TitleId != 0)
					{
						paramValue = filledBB.CardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId);
						goto IL_00de;
					}
				}
				paramValue = filledBB.CardDatabase.GreLocProvider.GetLocalizedText(card2.TitleId);
			}
		}
		goto IL_00de;
		IL_00de:
		return !string.IsNullOrEmpty(paramValue);
	}
}
