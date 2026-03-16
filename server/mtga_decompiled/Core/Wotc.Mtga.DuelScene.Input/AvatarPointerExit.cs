using Wotc.Mtga.DuelScene.Emotes;

namespace Wotc.Mtga.DuelScene.Input;

public class AvatarPointerExit : IEntityInputEvent<IAvatarView>
{
	private readonly IAvatarHoverController _avatarHoverController;

	private readonly IEmoteControllerProvider _emoteManager;

	public AvatarPointerExit(IAvatarHoverController hoverController, IEmoteControllerProvider emoteManager)
	{
		_avatarHoverController = hoverController ?? new NullAvatarHoverController();
		_emoteManager = emoteManager ?? NullEmoteControllerProvider.Default;
	}

	public void Execute(IAvatarView avatar)
	{
		_avatarHoverController.EndAvatarHover(avatar);
		if (_emoteManager.TryEmoteControllerById(avatar.InstanceId, out var dialogController))
		{
			dialogController.Hovered = false;
		}
	}
}
