using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

public class SuperTypeFilter : CardPropertyFilter
{
	private readonly SuperType _superType;

	public SuperTypeFilter(SuperType superType)
	{
		_superType = superType;
	}

	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			foreach (SuperType supertype in cards.Cards[num].Card.Supertypes)
			{
				if (supertype == _superType)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				foreach (CardPrintingData linkedFacePrinting in cards.Cards[num].Card.LinkedFacePrintings)
				{
					foreach (SuperType supertype2 in linkedFacePrinting.Supertypes)
					{
						if (supertype2 == _superType)
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.CardSuperTypeFilter);
			}
		}
		return cards;
	}
}
