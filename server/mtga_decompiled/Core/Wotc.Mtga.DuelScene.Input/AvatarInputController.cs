using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Input;

public class AvatarInputController : IAvatarInputController
{
	private readonly AvatarInteractions _interactions;

	public AvatarInputController(AvatarInteractions interactions)
	{
		_interactions = interactions;
	}

	public void ApplyInput(DuelScene_AvatarView avatar)
	{
		ApplyInput(avatar.PortraitInput);
	}

	private void ApplyInput(AvatarInput input)
	{
		input.Clicked += _interactions.PrimaryInteraction;
		input.PointerEnter += _interactions.PointerEnter;
		input.PointerExit += _interactions.PointerExit;
	}
}
