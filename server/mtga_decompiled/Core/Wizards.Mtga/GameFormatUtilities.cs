namespace Wizards.Mtga;

public static class GameFormatUtilities
{
	public static GameFormat FormatStringToGameFormat(string formatName)
	{
		if (string.IsNullOrEmpty(formatName))
		{
			return GameFormat.Unknown;
		}
		return formatName switch
		{
			"Alchemy" => GameFormat.Alchemy, 
			"Standard" => GameFormat.Standard, 
			"Historic" => GameFormat.Historic, 
			"Brawl" => GameFormat.Brawl, 
			"Explorer" => GameFormat.Explorer, 
			"Timeless" => GameFormat.Timeless, 
			_ => GameFormat.Unknown, 
		};
	}
}
