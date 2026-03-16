namespace Wotc.Mtga.Cards.Text;

public class LoyaltyTextEntry : ILoyaltyTextEntry, ICardTextEntry
{
	private readonly string _loyaltyText;

	private readonly string _abilityText;

	private readonly LoyaltyValence _loyaltyValence;

	public LoyaltyTextEntry(string loyaltyText, string abilityText)
	{
		_loyaltyText = loyaltyText;
		_abilityText = abilityText;
		_loyaltyValence = CalcValence(loyaltyText);
	}

	public string GetText()
	{
		return _abilityText;
	}

	public string GetCost()
	{
		return _loyaltyText;
	}

	public LoyaltyValence GetValence()
	{
		return _loyaltyValence;
	}

	public static LoyaltyValence CalcValence(string loyaltyText)
	{
		if (string.IsNullOrWhiteSpace(loyaltyText))
		{
			return LoyaltyValence.Invalid;
		}
		if (loyaltyText.IndexOfAny(Constants.PLUS_CHARS) >= 0)
		{
			return LoyaltyValence.Positive;
		}
		if (loyaltyText.IndexOfAny(Constants.MINUS_CHARS) >= 0)
		{
			return LoyaltyValence.Negative;
		}
		return LoyaltyValence.Neutral;
	}
}
