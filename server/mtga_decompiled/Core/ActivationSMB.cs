using System;
using UnityEngine;

public class ActivationSMB : SMBehaviour
{
	[SerializeField]
	private bool _deactivate;

	[SerializeField]
	private bool _revertOnExit = true;

	[SerializeField]
	protected bool _revertOnStateMachineClose = true;

	[SerializeField]
	private bool _keepAnimatorControllerStateOnDisable;

	[SerializeField]
	[SceneObjectReference(null)]
	protected string _targetPath;

	protected GameObject _target;

	protected override void OnEnter()
	{
		_target = SceneObjectReference.GetSceneObject(_targetPath);
		if (_target == null && Application.isEditor)
		{
			Debug.LogWarning("Cannot find activation target at " + _targetPath + " on " + Animator.name, Animator);
		}
		UpdateActive();
		if (_revertOnStateMachineClose)
		{
			StateMachineEvents stateMachineEvents = GetStateMachineEvents();
			stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Combine(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
		}
	}

	protected void UpdateActive()
	{
		if (!(_target != null))
		{
			return;
		}
		if (_keepAnimatorControllerStateOnDisable)
		{
			Animator component = _target.GetComponent<Animator>();
			if (component != null)
			{
				component.keepAnimatorStateOnDisable = _keepAnimatorControllerStateOnDisable;
			}
		}
		_target.SetActive(!_deactivate);
	}

	protected override void OnExit()
	{
		if (_revertOnExit)
		{
			Revert();
		}
	}

	protected void Revert()
	{
		StateMachineEvents stateMachineEvents = GetStateMachineEvents();
		stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Remove(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
		if (!(_target != null))
		{
			return;
		}
		if (_keepAnimatorControllerStateOnDisable)
		{
			Animator component = _target.GetComponent<Animator>();
			if (component != null)
			{
				component.keepAnimatorStateOnDisable = !_keepAnimatorControllerStateOnDisable;
			}
		}
		_target.SetActive(_deactivate);
	}
}
