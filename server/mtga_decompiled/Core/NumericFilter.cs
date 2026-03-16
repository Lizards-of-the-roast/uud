using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class NumericFilter : CardPropertyFilter
{
	public PropertyType Property;

	public TokenType Operator;

	public int Value;

	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			int value = 0;
			bool flag = false;
			switch (Property)
			{
			case PropertyType.CMC:
				if (cards.Cards[num].Card.LinkedFaceType == LinkedFace.SplitHalf && cards.Cards[num].Card.LinkedFacePrintings.Exists((CardPrintingData p) => EvaluateSingleCard(p, metadata)))
				{
					flag = true;
				}
				value = (int)cards.Cards[num].Card.ConvertedManaCost;
				break;
			case PropertyType.Loyalty:
				value = (cards.Cards[num].Card.Types.Contains(CardType.Planeswalker) ? cards.Cards[num].Card.Toughness.Value : 0);
				break;
			case PropertyType.Power:
				value = cards.Cards[num].Card.Power.Value;
				break;
			case PropertyType.Toughness:
				value = cards.Cards[num].Card.Toughness.Value;
				break;
			case PropertyType.Rarity:
				value = GetNormalizedRarityValue(cards.Cards[num].Card);
				break;
			case PropertyType.Owned:
				if (metadata != null && metadata.TitleIdsToNumberOwned != null)
				{
					metadata.TitleIdsToNumberOwned.TryGetValue(cards.Cards[num].Card.TitleId, out value);
				}
				break;
			}
			if (!flag)
			{
				switch (Operator)
				{
				case TokenType.GreaterThan:
					flag = value > Value;
					break;
				case TokenType.GreaterThanOrEqual:
					flag = value >= Value;
					break;
				case TokenType.LessThan:
					flag = value < Value;
					break;
				case TokenType.LessThanOrEqual:
					flag = value <= Value;
					break;
				case TokenType.Colon:
				case TokenType.Equals:
					flag = value == Value;
					break;
				case TokenType.NotEqual:
					flag = value != Value;
					break;
				}
				flag = (Negate ? (!flag) : flag);
			}
			if (!flag)
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.NumericFilter);
			}
		}
		return cards;
	}

	private bool EvaluateSingleCard(CardPrintingData card, CardMatcher.CardMatcherMetadata metadata)
	{
		bool flag = false;
		int value = 0;
		switch (Property)
		{
		case PropertyType.CMC:
			if (card.LinkedFaceType == LinkedFace.SplitHalf && card.LinkedFacePrintings.Exists((CardPrintingData p) => EvaluateSingleCard(p, metadata)))
			{
				return true;
			}
			value = (int)card.ConvertedManaCost;
			break;
		case PropertyType.Loyalty:
			value = (card.Types.Contains(CardType.Planeswalker) ? card.Toughness.Value : 0);
			break;
		case PropertyType.Power:
			value = card.Power.Value;
			break;
		case PropertyType.Toughness:
			value = card.Toughness.Value;
			break;
		case PropertyType.Rarity:
			value = GetNormalizedRarityValue(card);
			break;
		case PropertyType.Owned:
			if (metadata != null && metadata.TitleIdsToNumberOwned != null)
			{
				metadata.TitleIdsToNumberOwned.TryGetValue(card.TitleId, out value);
			}
			break;
		}
		switch (Operator)
		{
		case TokenType.GreaterThan:
			flag = value > Value;
			break;
		case TokenType.GreaterThanOrEqual:
			flag = value >= Value;
			break;
		case TokenType.LessThan:
			flag = value < Value;
			break;
		case TokenType.LessThanOrEqual:
			flag = value <= Value;
			break;
		case TokenType.Colon:
		case TokenType.Equals:
			flag = value == Value;
			break;
		case TokenType.NotEqual:
			flag = value != Value;
			break;
		}
		return Negate ? (!flag) : flag;
	}

	private static int GetNormalizedRarityValue(CardPrintingData card)
	{
		if (card.Rarity == CardRarity.Land && !card.IsBasicLand)
		{
			return 2;
		}
		return (int)card.Rarity;
	}
}
