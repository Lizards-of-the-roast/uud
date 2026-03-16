using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public static class CardHolderTypeExtensions
{
	public static ZoneType ToZoneType(this CardHolderType cardHolderType)
	{
		return cardHolderType switch
		{
			CardHolderType.Command => ZoneType.Command, 
			CardHolderType.Stack => ZoneType.Stack, 
			CardHolderType.Battlefield => ZoneType.Battlefield, 
			CardHolderType.Exile => ZoneType.Exile, 
			CardHolderType.Hand => ZoneType.Hand, 
			CardHolderType.Library => ZoneType.Library, 
			CardHolderType.Graveyard => ZoneType.Graveyard, 
			CardHolderType.Reveal => ZoneType.Revealed, 
			_ => ZoneType.None, 
		};
	}
}
