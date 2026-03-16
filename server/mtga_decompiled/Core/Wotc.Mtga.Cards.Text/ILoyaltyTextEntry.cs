namespace Wotc.Mtga.Cards.Text;

internal interface ILoyaltyTextEntry : ICardTextEntry
{
	string GetCost();

	LoyaltyValence GetValence();
}
