using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class NamedCardFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _localizationManager;

	private readonly DeckFormat _currentEventFormat;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public NamedCardFaceInfoGenerator(ICardDatabaseAdapter cardDatabase, IClientLocProvider locManager, DeckFormat currentEventFormat)
	{
		_cardDatabase = cardDatabase;
		_localizationManager = locManager ?? NullLocProvider.Default;
		_currentEventFormat = currentEventFormat;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (cardData.Zone == null)
		{
			return _data;
		}
		if (cardData.Instance == null)
		{
			return _data;
		}
		string localizedText = _localizationManager.GetLocalizedText("DuelScene/FaceHanger/NamedCard");
		foreach (uint linkedInfoTitleLocId in cardData.LinkedInfoTitleLocIds)
		{
			IReadOnlyList<CardPrintingData> printingsByTitleId = _cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(linkedInfoTitleLocId);
			if (printingsByTitleId != null && printingsByTitleId.Count != 0)
			{
				CardPrintingData cardPrintingData = printingsByTitleId[0];
				if (cardPrintingData.RebalancedCardLink != 0 && _currentEventFormat != null && _currentEventFormat.UseRebalancedCards != cardPrintingData.IsRebalanced)
				{
					cardPrintingData = _cardDatabase.CardDataProvider.GetCardPrintingById(cardPrintingData.RebalancedCardLink);
				}
				MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance();
				mtgCardInstance.ObjectType = GameObjectType.Card;
				CardData cardData2 = new CardData(mtgCardInstance, cardPrintingData);
				_data.Add(new FaceHanger.FaceCardInfo(cardData2, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), FaceHanger.HangerType.NamedCard));
			}
		}
		return _data;
	}
}
