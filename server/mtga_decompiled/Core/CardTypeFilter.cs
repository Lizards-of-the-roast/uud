using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wizards.Arena.Enums.Card;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardTypeFilter : CardPropertyFilter
{
	private readonly Wotc.Mtgo.Gre.External.Messaging.CardType _cardType;

	public CardTypeFilter(Wizards.Arena.Enums.Card.CardType cardType)
	{
		_cardType = (Wotc.Mtgo.Gre.External.Messaging.CardType)cardType;
	}

	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			foreach (Wotc.Mtgo.Gre.External.Messaging.CardType type in cards.Cards[num].Card.Types)
			{
				if (type == _cardType)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				foreach (CardPrintingData linkedFacePrinting in cards.Cards[num].Card.LinkedFacePrintings)
				{
					foreach (Wotc.Mtgo.Gre.External.Messaging.CardType type2 in linkedFacePrinting.Types)
					{
						if (type2 == _cardType)
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.CardTypeFilter);
			}
		}
		return cards;
	}
}
