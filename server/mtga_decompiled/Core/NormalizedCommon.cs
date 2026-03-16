using Core.Shared.Code.CardFilters;

public class NormalizedCommon : CardPropertyFilter
{
	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = cards.Cards[num].Card.Rarity == CardRarity.Common || (cards.Cards[num].Card.Rarity == CardRarity.Land && !cards.Cards[num].Card.IsBasicLand);
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.NormalizedCommon);
			}
		}
		return cards;
	}
}
