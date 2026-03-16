using UnityEngine;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_WaitGame : AutoPlayAction
{
	private GameManager _gameManager;

	protected override void OnUpdate()
	{
		_gameManager = ((_gameManager != null) ? _gameManager : Object.FindObjectOfType<GameManager>());
		if (!base.IsComplete && _gameManager != null && !_gameManager.UXEventQueue.IsRunning)
		{
			Complete($"Waited {GetRunTime()} for EventQueue to finish");
		}
	}
}
