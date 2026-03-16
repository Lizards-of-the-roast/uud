using GreClient.Rules;
using UnityEngine;

public class LowTimeFrameGlow : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[Header("Approximately the time it will take to reach the target. A smaller value will reach the target faster")]
	[SerializeField]
	private float _fadeInDuration;

	[SerializeField]
	private float _fadeOutDuration;

	private const float TIME_REMAINING_SHOW_THRESHOLD = 30f;

	private MtgTimer _activeTimer;

	private float _timeRunning;

	private float _current;

	private float _target;

	private float _velocity;

	public void UpdateTimer(MtgTimer activeTimer)
	{
		_timeRunning = 0f;
		_activeTimer = activeTimer;
	}

	private void Update()
	{
		if (_activeTimer != null)
		{
			float num = _activeTimer.RemainingTime - _timeRunning;
			_target = 1f - num / 30f;
			if (_activeTimer.Running)
			{
				_timeRunning += Time.smoothDeltaTime;
			}
		}
		else
		{
			_velocity = 0f;
			_target = 0f;
		}
		if (_current != _target)
		{
			float smoothTime = ((_current < _target) ? _fadeInDuration : _fadeOutDuration);
			_current = Mathf.SmoothDamp(_current, _target, ref _velocity, smoothTime);
			_animator.Play("Intensity", 0, _current);
		}
	}
}
