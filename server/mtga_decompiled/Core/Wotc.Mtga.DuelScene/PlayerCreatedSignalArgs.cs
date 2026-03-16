namespace Wotc.Mtga.DuelScene;

public class PlayerCreatedSignalArgs : SignalArgs
{
	public readonly DuelScene_AvatarView Player;

	public PlayerCreatedSignalArgs(object dispatcher, DuelScene_AvatarView player)
		: base(dispatcher)
	{
		Player = player;
	}
}
