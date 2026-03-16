using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace AssetLookupTree.Payloads.Helpers;

public class ClientOrGreLocKey
{
	public string Key;

	public MTGALocalizedString GetText(IClientLocProvider clientLocManager, IGreLocProvider greLocManager, Dictionary<string, string> parameters = null)
	{
		if (uint.TryParse(Key, out var result) && greLocManager != null)
		{
			return new GRELocalizedString(greLocManager)
			{
				Key = Key,
				locId = result
			};
		}
		if (clientLocManager != null)
		{
			return new MTGALocalizedString(clientLocManager)
			{
				Key = Key,
				Parameters = parameters
			};
		}
		return new UnlocalizedMTGAString();
	}
}
