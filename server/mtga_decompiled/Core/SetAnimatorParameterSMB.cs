using System;
using UnityEngine;

public class SetAnimatorParameterSMB : SMBehaviour
{
	public enum ParameterType
	{
		Trigger,
		Bool,
		Int,
		Float
	}

	[SceneObjectReference(typeof(Animator))]
	[SerializeField]
	private string _targetPath;

	[SerializeField]
	private bool _revertParameter = true;

	[SerializeField]
	private bool _revertState = true;

	[SerializeField]
	private bool _revertOnExit;

	[SerializeField]
	private bool _revertOnStateMachineClose;

	[SerializeField]
	private string _parameterName;

	[SerializeField]
	private ParameterType _parameterType;

	[Header("Value")]
	[SerializeField]
	private bool _boolValue;

	[SerializeField]
	private int _intValue;

	[SerializeField]
	private float _floatValue;

	private Animator _target;

	private AnimatorStateInfo[] _revertStateInfo;

	private bool _revertBool;

	private int _revertInt;

	private float _revertFloat;

	protected override void OnEnter()
	{
		_target = SceneObjectReference.GetSceneObject<Animator>(_targetPath);
		if (_target == null)
		{
			return;
		}
		if (_revertOnExit || _revertOnStateMachineClose)
		{
			_revertStateInfo = new AnimatorStateInfo[_target.layerCount];
			for (int i = 0; i < _target.layerCount; i++)
			{
				_revertStateInfo[i] = _target.GetCurrentAnimatorStateInfo(i);
				_target.Play(_revertStateInfo[i].shortNameHash, i, _revertStateInfo[i].normalizedTime);
			}
			if (_revertOnStateMachineClose)
			{
				StateMachineEvents stateMachineEvents = GetStateMachineEvents();
				stateMachineEvents.OnStateMachineDisable = (Action)Delegate.Combine(stateMachineEvents.OnStateMachineDisable, new Action(Revert));
			}
		}
		switch (_parameterType)
		{
		case ParameterType.Trigger:
			_target.SetTrigger(_parameterName);
			break;
		case ParameterType.Bool:
			_revertBool = _target.GetBool(_parameterName);
			_target.SetBool(_parameterName, _boolValue);
			break;
		case ParameterType.Int:
			_revertInt = _target.GetInteger(_parameterName);
			_target.SetInteger(_parameterName, _intValue);
			break;
		case ParameterType.Float:
			_revertFloat = _target.GetFloat(_parameterName);
			_target.SetFloat(_parameterName, _floatValue);
			break;
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
		if (!(_target != null))
		{
			return;
		}
		if (_revertState && _revertStateInfo != null)
		{
			for (int i = 0; i < _revertStateInfo.Length; i++)
			{
				_target.Play(_revertStateInfo[i].shortNameHash, i, _revertStateInfo[i].normalizedTime);
			}
		}
		if (_revertParameter)
		{
			switch (_parameterType)
			{
			case ParameterType.Trigger:
				_target.ResetTrigger(_parameterName);
				break;
			case ParameterType.Bool:
				_target.SetBool(_parameterName, _revertBool);
				break;
			case ParameterType.Int:
				_target.SetInteger(_parameterName, _revertInt);
				break;
			case ParameterType.Float:
				_target.SetFloat(_parameterName, _revertFloat);
				break;
			}
		}
	}
}
