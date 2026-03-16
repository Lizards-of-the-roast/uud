namespace Wotc.Mtga.DuelScene;

public class PlayerDeletedSignalArgs : SignalArgs
{
	public readonly DuelScene_AvatarView Player;

	public PlayerDeletedSignalArgs(object dispatcher, DuelScene_AvatarView player)
		: base(dispatcher)
	{
		Player = player;
	}
}
