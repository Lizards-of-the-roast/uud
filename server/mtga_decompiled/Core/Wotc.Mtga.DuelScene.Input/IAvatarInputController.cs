using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Emotes;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Input;

public interface IAvatarInputController
{
	void ApplyInput(DuelScene_AvatarView avatar);

	static IAvatarInputController Create(IEmoteControllerProvider emotes, ICardHolderProvider cardHolderProvider, IAvatarHoverController avatarHoverController, IClickableWorkflowProvider workflowProvider, FullControl fullControl)
	{
		if (PlatformUtils.IsHandheld())
		{
			AvatarInteractions lifeTotalInteraction = new AvatarInteractions(new LifeTotalClicked(workflowProvider), new LifeTotalTapped(cardHolderProvider), new NullEntityInputEvent<IAvatarView>(), new NullEntityInputEvent<IAvatarView>());
			IEntityInputEvent<IAvatarView> primary;
			if (!PlatformUtils.IsAspectRatio4x3())
			{
				IEntityInputEvent<IAvatarView> entityInputEvent = new PortraitClicked(emotes, fullControl);
				primary = entityInputEvent;
			}
			else
			{
				IEntityInputEvent<IAvatarView> entityInputEvent = new AvatarClicked(workflowProvider, emotes);
				primary = entityInputEvent;
			}
			IEntityInputEvent<IAvatarView> entityInputEvent2;
			if (!(fullControl != null))
			{
				IEntityInputEvent<IAvatarView> entityInputEvent = new NullEntityInputEvent<IAvatarView>();
				entityInputEvent2 = entityInputEvent;
			}
			else
			{
				IEntityInputEvent<IAvatarView> entityInputEvent = new PortraitLongTapped(emotes, fullControl);
				entityInputEvent2 = entityInputEvent;
			}
			IEntityInputEvent<IAvatarView> secondary = entityInputEvent2;
			AvatarInteractions portraitInteraction = new AvatarInteractions(primary, secondary, new NullEntityInputEvent<IAvatarView>(), new NullEntityInputEvent<IAvatarView>());
			return new HandheldAvatarInputController(lifeTotalInteraction, portraitInteraction);
		}
		return new AvatarInputController(new AvatarInteractions(new AvatarClicked(workflowProvider, emotes), new NullEntityInputEvent<IAvatarView>(), new AvatarPointerEnter(avatarHoverController, workflowProvider, emotes), new AvatarPointerExit(avatarHoverController, emotes)));
	}
}
