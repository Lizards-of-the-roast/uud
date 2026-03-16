using Wotc.Mtga.DuelScene.Emotes;

namespace Wotc.Mtga.DuelScene.Input;

public class PortraitLongTapped : IEntityInputEvent<IAvatarView>
{
	private readonly IEmoteControllerProvider _emoteManager;

	private readonly FullControlToggle _fullControl;

	public PortraitLongTapped(IEmoteControllerProvider emoteManager, FullControl fullControl)
	{
		_emoteManager = emoteManager ?? NullEmoteControllerProvider.Default;
		if (fullControl is FullControlToggle fullControl2)
		{
			_fullControl = fullControl2;
		}
	}

	public void Execute(IAvatarView avatar)
	{
		if (avatar.IsLocalPlayer)
		{
			if ((bool)_fullControl)
			{
				_fullControl.ShowToggle();
			}
			if (_emoteManager.TryEmoteControllerById(avatar.InstanceId, out var dialogController))
			{
				avatar.ShowPlayerNames(visible: true);
				dialogController.Close();
			}
		}
	}
}
