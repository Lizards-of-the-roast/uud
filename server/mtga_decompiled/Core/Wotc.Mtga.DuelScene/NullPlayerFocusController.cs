namespace Wotc.Mtga.DuelScene;

public class NullPlayerFocusController : IPlayerFocusController
{
	public static readonly IPlayerFocusController Default = new NullPlayerFocusController();

	public void FocusPlayer(uint playerId)
	{
	}
}
