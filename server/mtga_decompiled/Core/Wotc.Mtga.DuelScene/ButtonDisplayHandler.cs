using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene;

public class ButtonDisplayHandler : IUpdate
{
	private readonly UIManager _uiManager;

	private readonly ManaColorSelector _manaColorSelector;

	private readonly IBrowserProvider _browserProvider;

	public ButtonDisplayHandler(UIManager uiManager, IBrowserProvider browserProvider, ManaColorSelector manaColorSelector)
	{
		_uiManager = uiManager;
		_browserProvider = browserProvider;
		_manaColorSelector = manaColorSelector;
	}

	public void OnUpdate(float time)
	{
		_uiManager.ShowButtons(!_manaColorSelector.IsOpen && !_browserProvider.IsAnyBrowserOpen);
	}
}
