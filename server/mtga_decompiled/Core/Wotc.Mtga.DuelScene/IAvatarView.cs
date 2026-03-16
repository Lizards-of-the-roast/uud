using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IAvatarView : IEntityView
{
	MtgPlayer Model { get; }

	bool IsLocalPlayer { get; }

	bool ShowingPlayerName { get; }

	void ShowPlayerNames(bool visible);
}
