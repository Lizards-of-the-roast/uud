using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class CardNameProvider : IEntityNameProvider<MtgCardInstance>
{
	private readonly ICardTitleProvider _cardTitleProvider;

	public CardNameProvider(ICardTitleProvider cardTitleProvider)
	{
		_cardTitleProvider = cardTitleProvider ?? NullCardTitleProvider.Default;
	}

	public string GetName(MtgCardInstance card, bool formatted = true)
	{
		if (!formatted)
		{
			return _cardTitleProvider.GetCardTitle(card.GrpId);
		}
		return CardUtilities.FormatComplexTitle(_cardTitleProvider.GetCardTitle(card.GrpId));
	}
}
