namespace Wotc.Mtga.DuelScene.Input;

public class AvatarInteractions
{
	private readonly IEntityInputEvent<IAvatarView> _primary;

	private readonly IEntityInputEvent<IAvatarView> _secondary;

	private readonly IEntityInputEvent<IAvatarView> _pointerEnter;

	private readonly IEntityInputEvent<IAvatarView> _pointerExit;

	public AvatarInteractions(IEntityInputEvent<IAvatarView> primary, IEntityInputEvent<IAvatarView> secondary, IEntityInputEvent<IAvatarView> pointerEnter, IEntityInputEvent<IAvatarView> pointerExit)
	{
		_primary = primary;
		_secondary = secondary;
		_pointerEnter = pointerEnter;
		_pointerExit = pointerExit;
	}

	public void PrimaryInteraction(IAvatarView avatar)
	{
		_primary?.Execute(avatar);
	}

	public void SecondaryInteraction(IAvatarView avatar)
	{
		_secondary?.Execute(avatar);
	}

	public void PointerEnter(IAvatarView avatar)
	{
		_pointerEnter?.Execute(avatar);
	}

	public void PointerExit(IAvatarView avatar)
	{
		_pointerExit?.Execute(avatar);
	}
}
