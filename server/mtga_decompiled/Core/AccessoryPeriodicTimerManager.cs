using System.Collections.Generic;
using UnityEngine;

public class AccessoryPeriodicTimerManager : TickableBase
{
	public enum Delay
	{
		Small = 2,
		Medium = 3,
		Large = 5
	}

	private float _boredomTimerMin;

	private float _boredomTimerMax;

	private float _restlessTimerMin;

	private float _restlessTimerMax;

	private float _boredom;

	private float _restlessness;

	private float _delayTimer;

	private AnimatorStateInfo _stateInfo;

	private int _stateHash;

	private static readonly HashSet<int> _idleStateHashes = new HashSet<int>
	{
		Animator.StringToHash("Base Layer.idle"),
		Animator.StringToHash("Base Layer.Idle"),
		Animator.StringToHash("Base Layer.Idle.Idle"),
		Animator.StringToHash("Base Layer.Idle.Idle_A"),
		Animator.StringToHash("Base Layer.Idle.Idle_B"),
		Animator.StringToHash("Base Layer.Idle_A"),
		Animator.StringToHash("Base Layer.Idle_B"),
		Animator.StringToHash("Base Layer.Idle_C"),
		Animator.StringToHash("Base Layer.Idle_Flying"),
		Animator.StringToHash("Base Layer.Idle_Roost"),
		Animator.StringToHash("Base Layer.Idles.IdleA"),
		Animator.StringToHash("Base Layer.Idles.IdleB"),
		Animator.StringToHash("Base Layer.Idles.IdleC"),
		Animator.StringToHash("BaseLayer.idle")
	};

	public AccessoryPeriodicTimerManager(AccessoryController accessoryController, Animator animator, float boredomTimerMin, float boredomTimerMax, float restlessTimerMin, float restlessTimerMax)
		: base(accessoryController, animator)
	{
		_boredomTimerMin = boredomTimerMin;
		_boredomTimerMax = boredomTimerMax;
		_restlessTimerMin = restlessTimerMin;
		_restlessTimerMax = restlessTimerMax;
		SetTimer_Boredom();
		SetTimer_Restlessness();
	}

	public void SetTimer_Boredom()
	{
		_boredom = Random.Range(_boredomTimerMin, _boredomTimerMax);
		SetDelay_Medium();
	}

	public void SetTimer_Restlessness()
	{
		_restlessness = Random.Range(_restlessTimerMin, _restlessTimerMax);
		SetDelay_Short();
	}

	private void SetTimer_Delay(float t)
	{
		if (_delayTimer < 0f)
		{
			_delayTimer = 0f;
		}
		_delayTimer += t;
	}

	public void SetDelay_Long()
	{
		SetTimer_Delay(5f);
	}

	public void SetDelay_Medium()
	{
		SetTimer_Delay(3f);
	}

	public void SetDelay_Short()
	{
		SetTimer_Delay(2f);
	}

	public override void Update(float deltaTime)
	{
		TrackParameter(TickableBase._Bordom_Timer_p, _boredom);
		TrackParameter(TickableBase._Restless_Timer_p, _restlessness);
		_stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
		_stateHash = _stateInfo.fullPathHash;
		if (_idleStateHashes.Contains(_stateHash))
		{
			ResumeTimers();
		}
		else
		{
			PauseTimers();
		}
		if ((ParamExists[TickableBase._Pause_Timers_p] && _animator.GetBool(TickableBase._Pause_Timers_p)) || isPaused)
		{
			return;
		}
		if (_delayTimer > 0f)
		{
			_delayTimer -= deltaTime;
			return;
		}
		_boredom -= deltaTime;
		_restlessness -= deltaTime;
		if (_boredom <= 0f)
		{
			_accessoryController.HandleFidget();
			SetTimer_Boredom();
		}
		if (_restlessness <= 0f)
		{
			_accessoryController.HandleIdleFidget();
			SetTimer_Restlessness();
		}
	}
}
