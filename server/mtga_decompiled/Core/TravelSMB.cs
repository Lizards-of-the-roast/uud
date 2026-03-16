using System;
using DG.Tweening;
using UnityEngine;

public class TravelSMB : SMBehaviour
{
	[Header("Destination Options")]
	[SceneObjectReference(typeof(Transform))]
	[SerializeField]
	private string _anchorDestination;

	[Tooltip("Destination if destination anchor is not set or found.")]
	[SerializeField]
	private Vector3 fallbackDestination;

	[Header("Duration Options")]
	[SerializeField]
	private bool _followAnimationDuration;

	[Tooltip("Duration if not following animation duration or if no animation is on the state.")]
	[SerializeField]
	private float _fallbackDuration = 5f;

	[Header("Movement Options")]
	[SerializeField]
	private float _jumpStrength;

	[Tooltip("Speed ease in and out. Go to easings.net for ease information.")]
	[SerializeField]
	private Ease _easeType = Ease.Linear;

	[Header("Animator Parameters On Complete")]
	[SerializeField]
	private string _parameterName;

	[SerializeField]
	private AnimatorControllerParameterType _parameterType = AnimatorControllerParameterType.Trigger;

	[SerializeField]
	private bool _boolValue;

	[SerializeField]
	private int _intValue;

	[SerializeField]
	private float _floatValue;

	[Header("Debug")]
	public bool Pause;

	private bool _pause;

	private bool _startedMoving;

	[HideInInspector]
	public Transform transformDestination;

	protected override void OnEnter()
	{
		if (transformDestination == null)
		{
			transformDestination = SceneObjectReference.GetSceneObject<Transform>(_anchorDestination);
		}
		Vector3 endValue = ((!(transformDestination != null)) ? fallbackDestination : transformDestination.position);
		float duration = (_followAnimationDuration ? StateInfo.length : _fallbackDuration);
		Sequence sequence = Animator.transform.DOJump(endValue, _jumpStrength, 1, duration);
		sequence.SetEase(_easeType);
		sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, new TweenCallback(SetParameter));
		sequence.Play();
		_startedMoving = true;
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit()
	{
		UpdatePause(pause: false);
	}

	private void SetParameter()
	{
		if (!(_parameterName == ""))
		{
			switch (_parameterType)
			{
			case AnimatorControllerParameterType.Float:
				Animator.SetFloat(_parameterName, _floatValue);
				break;
			case AnimatorControllerParameterType.Int:
				Animator.SetInteger(_parameterName, _intValue);
				break;
			case AnimatorControllerParameterType.Bool:
				Animator.SetBool(_parameterName, _boolValue);
				break;
			case AnimatorControllerParameterType.Trigger:
				Animator.SetTrigger(_parameterName);
				break;
			}
		}
	}

	private void UpdatePause(bool pause)
	{
		if (_pause != pause)
		{
			_pause = pause;
		}
	}
}
