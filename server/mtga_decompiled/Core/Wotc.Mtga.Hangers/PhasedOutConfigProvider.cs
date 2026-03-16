using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class PhasedOutConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _locProvider;

	public PhasedOutConfigProvider(IClientLocProvider locProvider)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (IsPhasedOut(model))
		{
			string localizedText = _locProvider.GetLocalizedText("AbilityHanger/Keyword/PhasedOut_Title");
			string localizedText2 = _locProvider.GetLocalizedText("AbilityHanger/Keyword/PhasedOut");
			yield return new HangerConfig(localizedText, localizedText2, null, null, convertSymbols: false);
		}
	}

	private static bool IsPhasedOut(ICardDataAdapter cardData)
	{
		if (cardData != null)
		{
			return cardData.ZoneType == ZoneType.PhasedOut;
		}
		return false;
	}
}
