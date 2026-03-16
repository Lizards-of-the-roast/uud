using Wotc.Mtga.DuelScene.Emotes;

namespace Wotc.Mtga.DuelScene.Input;

public class PortraitClicked : IEntityInputEvent<IAvatarView>
{
	private readonly IEmoteControllerProvider _emoteManager;

	private readonly FullControlToggle _fullControl;

	public PortraitClicked(IEmoteControllerProvider emoteManager, FullControl fullControl)
	{
		_emoteManager = emoteManager ?? NullEmoteControllerProvider.Default;
		if (fullControl is FullControlToggle fullControl2)
		{
			_fullControl = fullControl2;
		}
	}

	public void Execute(IAvatarView avatar)
	{
		if (!_emoteManager.TryEmoteControllerById(avatar.InstanceId, out var dialogController))
		{
			return;
		}
		if (avatar.IsLocalPlayer)
		{
			bool flag = false;
			if ((bool)_fullControl)
			{
				flag = _fullControl.Visible;
				_fullControl.HideToggle();
			}
			if (avatar.ShowingPlayerName)
			{
				if (flag)
				{
					dialogController.Open();
					return;
				}
				dialogController.Close();
				avatar.ShowPlayerNames(visible: false);
			}
			else
			{
				dialogController.Open();
				avatar.ShowPlayerNames(visible: true);
			}
		}
		else if (avatar.ShowingPlayerName)
		{
			dialogController.Close();
			avatar.ShowPlayerNames(visible: false);
		}
		else
		{
			dialogController.Open();
			avatar.ShowPlayerNames(visible: true);
		}
	}
}
