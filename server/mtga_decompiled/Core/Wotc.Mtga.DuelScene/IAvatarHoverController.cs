namespace Wotc.Mtga.DuelScene;

public interface IAvatarHoverController
{
	void BeginAvatarHover(IAvatarView avatarView);

	void EndAvatarHover(IAvatarView avatarView);
}
