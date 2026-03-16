using Core.Shared.Code.CardFilters;

public class ColorFilter : CardPropertyFilter
{
	public enum ColorFilterType
	{
		Flags,
		Gold,
		Colorless
	}

	public PropertyType Property;

	public TokenType Operator;

	public ColorFilterType Type;

	public CardColorFlags ColorFlags;

	public ColorFilter()
	{
	}

	public ColorFilter(string str)
	{
		switch (str)
		{
		case "WHITE":
			ColorFlags = CardColorFlags.White;
			return;
		case "BLUE":
			ColorFlags = CardColorFlags.Blue;
			return;
		case "BLACK":
			ColorFlags = CardColorFlags.Black;
			return;
		case "RED":
			ColorFlags = CardColorFlags.Red;
			return;
		case "GREEN":
			ColorFlags = CardColorFlags.Green;
			return;
		case "C":
		case "COLORLESS":
			Type = ColorFilterType.Colorless;
			return;
		case "M":
		case "GOLD":
		case "MULTICOLOR":
			Type = ColorFilterType.Gold;
			return;
		}
		for (int i = 0; i < str.Length; i++)
		{
			switch (str[i])
			{
			case 'W':
				ColorFlags |= CardColorFlags.White;
				break;
			case 'U':
				ColorFlags |= CardColorFlags.Blue;
				break;
			case 'B':
				ColorFlags |= CardColorFlags.Black;
				break;
			case 'R':
				ColorFlags |= CardColorFlags.Red;
				break;
			case 'G':
				ColorFlags |= CardColorFlags.Green;
				break;
			}
		}
	}

	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = true;
			CardColorFlags cardColorFlags = ((Property == PropertyType.Color) ? cards.Cards[num].Card.ColorFlags : cards.Cards[num].Card.ColorIdentityFlags);
			if (Type == ColorFilterType.Colorless)
			{
				flag = cardColorFlags == CardColorFlags.None;
			}
			else if (Type == ColorFilterType.Gold)
			{
				flag = ((cardColorFlags - 1) & cardColorFlags) != 0;
			}
			else
			{
				switch (Operator)
				{
				case TokenType.Colon:
				case TokenType.Equals:
					flag = cardColorFlags == ColorFlags;
					break;
				case TokenType.NotEqual:
					flag = cardColorFlags != ColorFlags;
					break;
				case TokenType.GreaterThan:
					flag = (cardColorFlags & ColorFlags) == ColorFlags && cardColorFlags - ColorFlags > 0;
					break;
				case TokenType.GreaterThanOrEqual:
					flag = (cardColorFlags & ColorFlags) == ColorFlags;
					break;
				case TokenType.LessThan:
					flag = (cardColorFlags & ~ColorFlags) == 0 && cardColorFlags != ColorFlags;
					break;
				case TokenType.LessThanOrEqual:
					flag = (cardColorFlags & ~ColorFlags) == 0;
					break;
				}
			}
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.ColorFilter);
			}
		}
		return cards;
	}
}
