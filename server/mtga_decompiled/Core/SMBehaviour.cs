using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SMBehaviour : StateMachineBehaviour
{
	protected class StateMachineEvents
	{
		public Action OnStateMachineDisable;
	}

	protected Animator Animator;

	protected AnimatorStateInfo StateInfo;

	protected int LayerIndex;

	private string _stateName;

	private static Dictionary<Animator, List<StateMachineBehaviour>> _activeSMBs = new Dictionary<Animator, List<StateMachineBehaviour>>();

	private static Dictionary<Animator, StateMachineEvents> _stateMachineEvents = new Dictionary<Animator, StateMachineEvents>();

	public bool Active { get; private set; }

	public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Animator = animator;
		StateInfo = stateInfo;
		LayerIndex = layerIndex;
		if (!Active)
		{
			Active = true;
			ActivateState(this, animator);
			OnEnter();
			OnUpdate();
		}
	}

	public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!Active)
		{
			OnStateEnter(animator, stateInfo, layerIndex);
		}
		else
		{
			OnUpdate();
		}
	}

	public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!Active)
		{
			OnStateEnter(animator, stateInfo, layerIndex);
		}
		DeactivateState(this, animator);
		OnExit();
		Active = false;
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnExit()
	{
	}

	protected StateMachineEvents GetStateMachineEvents()
	{
		if (!_stateMachineEvents.ContainsKey(Animator))
		{
			_stateMachineEvents.Add(Animator, new StateMachineEvents());
		}
		return _stateMachineEvents[Animator];
	}

	public string GetStateName()
	{
		if (!string.IsNullOrEmpty(_stateName))
		{
			return _stateName;
		}
		return "Unknown State";
	}

	[ContextMenu("Copy Object")]
	private void CopyObject()
	{
		ScriptableObjectClipboard.CopyObject(this);
	}

	[ContextMenu("Paste Object Values")]
	private void PasteValues()
	{
		ScriptableObjectClipboard.PasteValues(this);
	}

	public static void DeactivateStateMachine(Animator stateMachine)
	{
		if (stateMachine == null)
		{
			return;
		}
		if (_activeSMBs.ContainsKey(stateMachine))
		{
			List<StateMachineBehaviour> list = _activeSMBs[stateMachine];
			while (list.Count > 0)
			{
				StateMachineBehaviour stateMachineBehaviour = list[0];
				stateMachineBehaviour.OnStateExit(stateMachine, default(AnimatorStateInfo), 0);
				list.Remove(stateMachineBehaviour);
			}
		}
		if (_stateMachineEvents.ContainsKey(stateMachine))
		{
			_stateMachineEvents[stateMachine].OnStateMachineDisable?.Invoke();
			_stateMachineEvents.Remove(stateMachine);
		}
	}

	public static void DeactivateStateMachine(GameObject stateMachine)
	{
		DeactivateStateMachine(stateMachine.GetComponent<Animator>());
	}

	public static void DeactivateStateMachine(Component stateMachine)
	{
		DeactivateStateMachine(stateMachine.GetComponent<Animator>());
	}

	public static void ActivateState(StateMachineBehaviour smb, Animator stateMachine)
	{
		if (!_activeSMBs.ContainsKey(stateMachine))
		{
			_activeSMBs.Add(stateMachine, new List<StateMachineBehaviour>());
		}
		if (!_activeSMBs[stateMachine].Contains(smb))
		{
			_activeSMBs[stateMachine].Add(smb);
		}
	}

	public static void DeactivateState(StateMachineBehaviour smb, Animator stateMachine)
	{
		if (_activeSMBs.ContainsKey(stateMachine))
		{
			_activeSMBs[stateMachine].Remove(smb);
		}
	}
}
