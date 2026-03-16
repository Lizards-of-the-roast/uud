using System;
using UnityEngine;
using UnityEngine.Events;

public class WaitUntilAction : CustomYieldInstruction
{
	private Action _action;

	private UnityEvent _unityEvent;

	private bool _called;

	public override bool keepWaiting => !_called;

	public WaitUntilAction(Action action)
	{
		_action = action;
		_action = (Action)Delegate.Combine(_action, new Action(OnAction));
	}

	public WaitUntilAction(UnityEvent unityEvent)
	{
		_unityEvent = unityEvent;
		_unityEvent.AddListener(OnEvent);
	}

	private void OnAction()
	{
		_action = (Action)Delegate.Remove(_action, new Action(OnAction));
		_called = true;
	}

	private void OnEvent()
	{
		_unityEvent.RemoveListener(OnEvent);
		_called = true;
	}
}
