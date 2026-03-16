using System.Text;
using Wizards.Models;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Events;

public static class PlayerEventExtensionMethods
{
	public static string GetDeckName(this IPlayerEvent playerEvent, IClientLocProvider locMan)
	{
		Client_Deck courseDeck = playerEvent.CourseData.CourseDeck;
		if (playerEvent.PacketsChosen != null && playerEvent.PacketsChosen.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (DTO_JumpStartSelection item in playerEvent.PacketsChosen)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(locMan.GetLocalizedText("Events/Packets/" + item.packetName));
			}
			return stringBuilder.ToString();
		}
		return Utils.GetLocalizedDeckName(courseDeck.Summary.Name);
	}
}
