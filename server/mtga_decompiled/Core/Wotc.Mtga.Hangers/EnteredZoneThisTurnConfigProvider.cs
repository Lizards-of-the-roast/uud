using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class EnteredZoneThisTurnConfigProvider : IHangerConfigProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	public EnteredZoneThisTurnConfigProvider(IClientLocProvider clientLocProvider)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		MtgCardInstance instance = model.Instance;
		if (instance != null)
		{
			string text = ZoneTypeToKey(instance.EnteredZoneThisTurn);
			if (!string.IsNullOrEmpty(text))
			{
				yield return new HangerConfig(string.Empty, _clientLocProvider.GetLocalizedText(text), null, null, convertSymbols: false);
			}
		}
	}

	private static string ZoneTypeToKey(ZoneType zoneType)
	{
		return zoneType switch
		{
			ZoneType.Battlefield => "AbilityHanger/SpecialHangers/EnteredBattlefieldThisTurn", 
			ZoneType.Graveyard => "AbilityHanger/SpecialHangers/EnteredGraveyardThisTurn", 
			ZoneType.Exile => "AbilityHanger/SpecialHangers/EnteredExileThisTurn", 
			ZoneType.Library => "AbilityHanger/SpecialHangers/EnteredLibraryThisTurn", 
			_ => string.Empty, 
		};
	}
}
