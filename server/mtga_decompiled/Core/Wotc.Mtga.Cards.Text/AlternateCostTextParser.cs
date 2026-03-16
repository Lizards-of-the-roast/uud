using System.Collections.Generic;
using System.Text.RegularExpressions;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class AlternateCostTextParser : ITextEntryParser
{
	private static HashSet<uint> _studentCostAbilityIds = new HashSet<uint> { 142031u, 141991u, 141942u, 141919u, 141890u };

	private const string LOC_PARAM_COST = "cost";

	private readonly IClientLocProvider _clientLocManager;

	private readonly IGreLocProvider _greLocManager;

	public AlternateCostTextParser(IClientLocProvider clientLocManager, IGreLocProvider greLocManager)
	{
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_greLocManager = greLocManager ?? NullGreLocManager.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (TryGetStudentCostAbility(card, out var studentCostAbility))
		{
			Match match = GreClient.CardData.Constants.MANA_COST_BRACES_MATCHER.Match(_greLocManager.GetLocalizedText(studentCostAbility.TextId, overrideLang, formatted: false));
			if (match.Success)
			{
				yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, ManaUtilities.ConvertManaSymbols(_clientLocManager.GetLocalizedTextForLanguage("DuelScene/RuleText/CastingTimeOption_Student", overrideLang, ("cost", match.Value)))));
			}
			else
			{
				yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, _clientLocManager.GetLocalizedTextForLanguage("DuelScene/RuleText/CastingTimeOption_Student_NoCost", overrideLang)));
			}
		}
	}

	private bool TryGetStudentCostAbility(ICardDataAdapter card, out AbilityPrintingData studentCostAbility)
	{
		foreach (CastingTimeOption castingTimeOption in card.CastingTimeOptions)
		{
			if (!castingTimeOption.IsCastThroughAbility || !castingTimeOption.AbilityId.HasValue || !_studentCostAbilityIds.Contains(castingTimeOption.AbilityId.Value))
			{
				continue;
			}
			foreach (AbilityPrintingData ability in card.Abilities)
			{
				if (ability.Category == AbilityCategory.AlternativeCost)
				{
					studentCostAbility = ability;
					return true;
				}
			}
		}
		studentCostAbility = null;
		return false;
	}
}
