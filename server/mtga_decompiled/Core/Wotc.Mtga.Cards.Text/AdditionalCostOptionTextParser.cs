using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class AdditionalCostOptionTextParser : ITextEntryParser
{
	private readonly HashSet<uint> _cardsWithAdditionalCostChoice = new HashSet<uint> { 760258u };

	private readonly IClientLocProvider _clientLocManager;

	private readonly IPromptEngine _promptEngine;

	public AdditionalCostOptionTextParser(IClientLocProvider locProvider, ICardDatabaseAdapter cardDatabaseAdapter)
	{
		_clientLocManager = locProvider;
		_promptEngine = cardDatabaseAdapter.PromptEngine;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (!_cardsWithAdditionalCostChoice.Contains(card.TitleId))
		{
			yield break;
		}
		foreach (CastingTimeOption castingTimeOption in card.CastingTimeOptions)
		{
			if (castingTimeOption.Type == CastingTimeOptionType.ChooseOrCost && castingTimeOption.PromptId.HasValue)
			{
				string localizedTextForLanguage = _clientLocManager.GetLocalizedTextForLanguage("DuelScene/RuleText/CastingTimeOption/AdditionalCostWasChosen", overrideLang, ("0", _promptEngine.GetPromptText((int)castingTimeOption.PromptId.Value)));
				yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, localizedTextForLanguage));
			}
		}
	}
}
