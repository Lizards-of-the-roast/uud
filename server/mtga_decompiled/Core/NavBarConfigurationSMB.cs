using System;
using UnityEngine;

public class NavBarConfigurationSMB : SMBehaviour
{
	[SerializeField]
	private NavBarController.OnboardingState _onboardingState;

	[SerializeField]
	private bool _revertOnStateMachineClose;

	private NavBarController _navBar;

	private NavBarController.OnboardingState _currentState;

	protected override void OnEnter()
	{
		_navBar = WrapperController.Instance.NavBarController;
		if (_navBar != null)
		{
			_currentState = _onboardingState;
			_navBar.SetOnboardingState(_currentState);
		}
		if (_revertOnStateMachineClose)
		{
			StateMachineEvents stateMachineEvents = GetStateMachineEvents();
			stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Combine(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
		}
	}

	protected override void OnUpdate()
	{
		if (_currentState != _onboardingState)
		{
			_currentState = _onboardingState;
			_navBar.SetOnboardingState(_currentState);
		}
	}

	protected void Revert()
	{
		StateMachineEvents stateMachineEvents = GetStateMachineEvents();
		stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Remove(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
		_navBar.SetOnboardingState(NavBarController.OnboardingState.None);
	}
}
