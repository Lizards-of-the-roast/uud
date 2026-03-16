using Wotc.Mtga.DuelScene.Emotes;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Input;

public class AvatarClicked : IEntityInputEvent<IAvatarView>
{
	private readonly IClickableWorkflowProvider _clickableWorkflowProvider;

	private readonly IEmoteControllerProvider _emoteManager;

	public AvatarClicked(IClickableWorkflowProvider clickableWorkflowProvider, IEmoteControllerProvider emoteManager)
	{
		_clickableWorkflowProvider = clickableWorkflowProvider ?? NullClickableWorkflowProvider.Default;
		_emoteManager = emoteManager ?? NullEmoteControllerProvider.Default;
	}

	public void Execute(IAvatarView avatar)
	{
		IEmoteController dialogController;
		if (_clickableWorkflowProvider.CanClick(avatar, SimpleInteractionType.Primary))
		{
			_clickableWorkflowProvider.OnClick(avatar, SimpleInteractionType.Primary);
		}
		else if (_emoteManager.TryEmoteControllerById(avatar.InstanceId, out dialogController))
		{
			dialogController.Toggle();
		}
	}
}
