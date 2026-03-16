using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Cards.Text;

public class TokenFakeAbilityTextParser : ITextEntryParser
{
	private readonly IGreLocProvider _locProvider;

	public TokenFakeAbilityTextParser(IGreLocProvider locProvider)
	{
		_locProvider = locProvider ?? NullGreLocManager.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (card.Printing != null && card.Printing.Abilities.ContainsId(121977u) && !card.RemovedAbilities.ContainsId(121977u) && !card.Abilities.ContainsId(121977u))
		{
			string localizedText = _locProvider.GetLocalizedText(card.Printing.AbilityIds.FirstOrDefault(((uint Id, uint TextId) x) => x.Id == 121977).TextId);
			yield return new BasicTextEntry(string.Format(colorSettings.DefaultFormat, localizedText));
		}
	}
}
