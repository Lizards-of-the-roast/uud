using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ActionProcessor : IActionProcessor
{
	private readonly Action<GreInteraction> _submitActionCallback;

	private readonly GameManager _gameManager;

	public ActionProcessor(Action<GreInteraction> submitActionCallback, GameManager gameManager)
	{
		_submitActionCallback = submitActionCallback;
		_gameManager = gameManager;
	}

	public void HandleActions(IEntityView entity, List<GreInteraction> actions)
	{
		ClientSideInteraction.HandleActions(entity, actions, _submitActionCallback, _gameManager, MDNPlayerPrefs.GameplayWarningsEnabled);
	}
}
