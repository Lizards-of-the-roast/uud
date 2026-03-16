using Wotc.Mtga.DuelScene.Emotes;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Input;

public class AvatarPointerEnter : IEntityInputEvent<IAvatarView>
{
	private readonly IAvatarHoverController _hoverController;

	private readonly IClickableWorkflowProvider _clickableWorkflowProvider;

	private readonly IEmoteControllerProvider _emoteManager;

	public AvatarPointerEnter(IAvatarHoverController hoverController, IClickableWorkflowProvider clickableWorkflowProvider, IEmoteControllerProvider emoteManager)
	{
		_hoverController = hoverController ?? new NullAvatarHoverController();
		_clickableWorkflowProvider = clickableWorkflowProvider ?? NullClickableWorkflowProvider.Default;
		_emoteManager = emoteManager ?? NullEmoteControllerProvider.Default;
	}

	public void Execute(IAvatarView avatar)
	{
		_hoverController.BeginAvatarHover(avatar);
		if (!_clickableWorkflowProvider.CanClick(avatar, SimpleInteractionType.Primary) && _emoteManager.TryEmoteControllerById(avatar.InstanceId, out var dialogController))
		{
			dialogController.Hovered = true;
		}
	}
}
