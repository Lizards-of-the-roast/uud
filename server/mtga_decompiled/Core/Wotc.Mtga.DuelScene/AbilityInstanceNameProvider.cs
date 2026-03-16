using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class AbilityInstanceNameProvider : IEntityNameProvider<MtgCardInstance>
{
	private readonly IAbilityTextProvider _abilityTextProvider;

	public AbilityInstanceNameProvider(IAbilityTextProvider abilityTextProvider)
	{
		_abilityTextProvider = abilityTextProvider ?? NullAbilityTextProvider.Default;
	}

	public string GetName(MtgCardInstance card, bool formatted = true)
	{
		return _abilityTextProvider.GetAbilityTextByCardAbilityGrpId(card.ObjectSourceGrpId, card.GrpId, card.Abilities.Select((AbilityPrintingData o) => o.Id), 0u, null, formatted);
	}
}
