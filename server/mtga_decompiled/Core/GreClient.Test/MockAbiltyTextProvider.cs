using System.Collections.Generic;
using System.Linq;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Test;

public class MockAbiltyTextProvider : IAbilityTextProvider
{
	private readonly Dictionary<uint, string> _abilityText;

	public MockAbiltyTextProvider(Dictionary<uint, string> abilityText = null)
	{
		_abilityText = abilityText ?? new Dictionary<uint, string>();
	}

	public string GetAbilityTextByCardAbilityGrpId(uint cardGrpId, uint abilityGrpId, IEnumerable<uint> abilityIds, uint cardTitleId = 0u, string overrideLanguageCode = null, bool formatted = true)
	{
		if (!_abilityText.TryGetValue(abilityGrpId, out var value))
		{
			return null;
		}
		return value;
	}

	public string GetAbilityTextByCardAbilityGrpId(IReadOnlyCollection<uint> cardGrpIds, uint abilityGrpId, IEnumerable<uint> abilityIds, uint cardTitleId = 0u, string overrideLanguageCode = null, bool formatted = true)
	{
		return GetAbilityTextByCardAbilityGrpId(cardGrpIds.FirstOrDefault(), abilityGrpId, abilityIds, cardTitleId, overrideLanguageCode, formatted);
	}
}
