using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Input;

public class HandheldAvatarInputController : IAvatarInputController
{
	private readonly AvatarInteractions _lifeTotalInteraction;

	private readonly AvatarInteractions _portraitInteraction;

	public HandheldAvatarInputController(AvatarInteractions lifeTotalInteraction, AvatarInteractions portraitInteraction)
	{
		_lifeTotalInteraction = lifeTotalInteraction;
		_portraitInteraction = portraitInteraction;
	}

	public void ApplyInput(DuelScene_AvatarView avatar)
	{
		ApplyLifeInputInteractions(avatar.LifeInput);
		ApplyPortraitInput(avatar.PortraitInput);
	}

	private void ApplyLifeInputInteractions(AvatarInput lifeInput)
	{
		lifeInput.Clicked += _lifeTotalInteraction.PrimaryInteraction;
		lifeInput.PointerDown += _lifeTotalInteraction.SecondaryInteraction;
	}

	private void ApplyPortraitInput(AvatarInput portraitInput)
	{
		portraitInput.Clicked += _portraitInteraction.PrimaryInteraction;
		portraitInput.LongClicked += _portraitInteraction.SecondaryInteraction;
	}
}
