namespace Wotc.Mtga.DuelScene.Input;

public class NullAvatarInputController : IAvatarInputController
{
	public static readonly IAvatarInputController Default = new NullAvatarInputController();

	public void ApplyInput(DuelScene_AvatarView avatar)
	{
	}
}
