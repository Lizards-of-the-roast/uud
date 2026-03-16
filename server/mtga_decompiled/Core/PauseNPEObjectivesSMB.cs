using System;
using UnityEngine;

public class PauseNPEObjectivesSMB : SMBehaviour
{
	[SerializeField]
	private bool _unpause;

	[SerializeField]
	private bool _revertOnExit = true;

	[SerializeField]
	private bool _revertOnStateMachineClose;

	private NPEObjectivesController _objectives;

	protected override void OnEnter()
	{
		_objectives = Animator.GetComponentInChildren<NPEObjectivesController>(includeInactive: true);
		_objectives.PauseProgess = !_unpause;
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
		if (_objectives != null)
		{
			_objectives.PauseProgess = _unpause;
		}
	}
}
