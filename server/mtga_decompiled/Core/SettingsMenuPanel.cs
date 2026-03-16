using UnityEngine;

public abstract class SettingsMenuPanel : MonoBehaviour
{
	protected SettingsMenu _settingsMenu;

	public virtual void Init(SettingsMenu settingsMenu)
	{
		_settingsMenu = settingsMenu;
	}

	public virtual void ShowPanel()
	{
	}

	public virtual void HidePanel()
	{
	}
}
