using System;

namespace Wotc.Mtga.DuelScene;

public class FullControlMediator : IDisposable
{
	private readonly FullControl _fullControl;

	private readonly AutoResponseManager _autoResponseManager;

	public FullControlMediator(FullControl fullControl, AutoResponseManager autoResponseManager)
	{
		_fullControl = fullControl;
		_autoResponseManager = autoResponseManager;
		if (!(_fullControl == null))
		{
			autoResponseManager.SettingsUpdated += fullControl.OnSettingsUpdated;
			if (fullControl.forceLocked)
			{
				fullControl.Clicked += autoResponseManager.ToggleForceLockedFullControl;
			}
			else
			{
				fullControl.Clicked += autoResponseManager.ToggleFullControl;
			}
		}
	}

	public void Dispose()
	{
		if (!(_fullControl == null))
		{
			_autoResponseManager.SettingsUpdated -= _fullControl.OnSettingsUpdated;
			if (_fullControl.forceLocked)
			{
				_fullControl.Clicked -= _autoResponseManager.ToggleForceLockedFullControl;
			}
			else
			{
				_fullControl.Clicked -= _autoResponseManager.ToggleFullControl;
			}
		}
	}
}
