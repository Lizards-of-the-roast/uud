using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public static class ZoneTypeExtensions
{
	public static CardHolderType ToCardHolderType(this ZoneType zoneType)
	{
		return zoneType switch
		{
			ZoneType.Pending => CardHolderType.Invalid, 
			ZoneType.Limbo => CardHolderType.Invalid, 
			ZoneType.Sideboard => CardHolderType.Invalid, 
			ZoneType.Command => CardHolderType.Command, 
			ZoneType.Revealed => CardHolderType.Reveal, 
			ZoneType.Stack => CardHolderType.Stack, 
			ZoneType.Battlefield => CardHolderType.Battlefield, 
			ZoneType.Exile => CardHolderType.Exile, 
			ZoneType.Hand => CardHolderType.Hand, 
			ZoneType.Library => CardHolderType.Library, 
			ZoneType.Graveyard => CardHolderType.Graveyard, 
			_ => CardHolderType.Invalid, 
		};
	}
}
