using UnityEngine;

public class AccessoryCooldownTimerManager : TickableBase
{
	private float _clickTimerLocal_Time;

	private float _clickTimerLocal;

	private float _clickTimerOpponent_Time;

	private float _clickTimerOpponent;

	private float _reactionTimer;

	private float _reactionTimer_Time = 3f;

	public bool ReadyToReact;

	public bool ReadyLocalClick;

	public bool ReadyOpponentClick;

	public AccessoryCooldownTimerManager(AccessoryController accessoryController, Animator animator, float clickTimerLocal_Time, float clickTimerOpponent_Time)
		: base(accessoryController, animator)
	{
		_clickTimerLocal_Time = clickTimerLocal_Time;
		_clickTimerOpponent_Time = clickTimerOpponent_Time;
	}

	public void SetTimer_LocalClick()
	{
		_clickTimerLocal = _clickTimerLocal_Time;
		ReadyLocalClick = false;
	}

	public void SetTimer_OpponentClick()
	{
		_clickTimerOpponent = _clickTimerOpponent_Time;
		ReadyOpponentClick = false;
	}

	public void SetTimer_Reaction()
	{
		_reactionTimer = _reactionTimer_Time;
		ReadyToReact = false;
	}

	public override void Update(float deltaTime)
	{
		TrackParameter(TickableBase._LocalClick_Cooldown_p, _clickTimerLocal);
		TrackParameter(TickableBase._OppClick_Cooldown_p, _clickTimerOpponent);
		TrackParameter(TickableBase._Reaction_Cooldown_p, _reactionTimer);
		TrackParameter(TickableBase._ReadyToReact_p, ReadyToReact ? 1f : 0f);
		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !_animator.GetCurrentAnimatorStateInfo(0).IsName("idle") && _reactionTimer <= 0f && !ReadyToReact)
		{
			ReadyToReact = true;
		}
		if (_clickTimerLocal > 0f)
		{
			_clickTimerLocal -= deltaTime;
		}
		else
		{
			ReadyLocalClick = true;
		}
		if (_clickTimerOpponent > 0f)
		{
			_clickTimerOpponent -= deltaTime;
		}
		else
		{
			ReadyOpponentClick = true;
		}
		if (_reactionTimer > 0f)
		{
			_reactionTimer -= deltaTime;
		}
	}
}
