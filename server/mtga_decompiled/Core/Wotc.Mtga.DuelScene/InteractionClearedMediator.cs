using System;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class InteractionClearedMediator : IDisposable
{
	private readonly WorkflowController _workflowController;

	private readonly BrowserManager _browserManager;

	public InteractionClearedMediator(WorkflowController workflowController, BrowserManager browserManager)
	{
		_workflowController = workflowController;
		_browserManager = browserManager;
		_workflowController.InteractionCleared += _browserManager.OnInteractionCleared;
	}

	public void Dispose()
	{
		_workflowController.InteractionCleared -= _browserManager.OnInteractionCleared;
	}
}
