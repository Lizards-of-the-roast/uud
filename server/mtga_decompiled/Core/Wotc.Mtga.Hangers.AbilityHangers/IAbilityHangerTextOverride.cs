using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public interface IAbilityHangerTextOverride
{
	bool CanUse(ICardDataAdapter model, AbilityPrintingData ability, IReadOnlyCollection<string> layers);

	(string Header, string BodyText) GetText(ICardDataAdapter model);
}
