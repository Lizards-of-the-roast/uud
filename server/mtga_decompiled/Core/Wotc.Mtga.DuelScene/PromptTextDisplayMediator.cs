using System;
using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene;

public class PromptTextDisplayMediator : IDisposable
{
	private readonly View_UserPrompt _promptDisplay;

	private readonly BrowserManager _browserManager;

	private readonly ISignalListen<PromptTextUpdatedSignalArgs> _signal;

	public PromptTextDisplayMediator(View_UserPrompt promptDisplay, BrowserManager browserManager, ISignalListen<PromptTextUpdatedSignalArgs> signal)
	{
		_promptDisplay = promptDisplay;
		_browserManager = browserManager;
		_signal = signal;
		_signal.Listeners += OnPromptTextUpdated;
		_browserManager.BrowserShown += OnBrowserShown;
		_browserManager.BrowserHidden += OnBrowserHidden;
	}

	private void OnBrowserShown(BrowserBase browser)
	{
		_promptDisplay.SetVisibility(visible: false);
	}

	private void OnBrowserHidden(BrowserBase browser)
	{
		_promptDisplay.SetVisibility(visible: true);
	}

	private void OnPromptTextUpdated(PromptTextUpdatedSignalArgs signalArgs)
	{
		_promptDisplay.SetPromptText(signalArgs.PromptText);
	}

	public void Dispose()
	{
		_signal.Listeners += OnPromptTextUpdated;
		_browserManager.BrowserShown -= OnBrowserShown;
		_browserManager.BrowserHidden -= OnBrowserHidden;
	}
}
