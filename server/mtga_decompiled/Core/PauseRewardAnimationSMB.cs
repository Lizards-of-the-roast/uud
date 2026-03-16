using System;
using UnityEngine;

public class PauseRewardAnimationSMB : SMBehaviour
{
	[SerializeField]
	private bool _unpause;

	[SerializeField]
	private bool _revertOnExit = true;

	[SerializeField]
	private bool _revertOnStateMachineClose = true;

	private RewardTreeController _rewardTree;

	protected override void OnEnter()
	{
		SceneLoader.GetSceneLoader().SetRewardTreeAnimationPaused(!_unpause);
		if (_revertOnStateMachineClose)
		{
			StateMachineEvents stateMachineEvents = GetStateMachineEvents();
			stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Combine(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
		}
	}

	protected override void OnExit()
	{
		if (_revertOnExit)
		{
			Revert();
		}
	}

	private void Revert()
	{
		StateMachineEvents stateMachineEvents = GetStateMachineEvents();
		stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Remove(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
		SceneLoader.GetSceneLoader().SetRewardTreeAnimationPaused(_unpause);
	}
}
