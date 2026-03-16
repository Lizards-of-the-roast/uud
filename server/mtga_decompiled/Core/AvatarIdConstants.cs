public static class AvatarIdConstants
{
	public const string Sparky = "Avatar_Basic_Sparky";

	public const string NPE_Player = "Avatar_Basic_NPE";

	private const string NPE_Opponent_Game1 = "NPE_ELF";

	private const string NPE_Opponent_Game2 = "NPE_GOBBO";

	private const string NPE_Opponent_Game3 = "NPE_WET_ELF";

	private const string NPE_Opponent_Game4 = "NPE_GOTH";

	private const string NPE_Opponent_Game5 = "NPE_BOLAS";

	public static string GetNPEOpponentByGameNumber(int gameNumber)
	{
		return gameNumber switch
		{
			0 => "NPE_ELF", 
			1 => "NPE_GOBBO", 
			2 => "NPE_WET_ELF", 
			3 => "NPE_GOTH", 
			4 => "NPE_BOLAS", 
			_ => "Avatar_Basic_NPE", 
		};
	}
}
