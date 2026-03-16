using System.Collections.Generic;
using UnityEngine;

public abstract class TickableBase : ITickable
{
	public bool isPaused;

	private protected AccessoryController _accessoryController;

	private protected Animator _animator;

	private protected Dictionary<string, bool> ParamExists;

	private protected static string _Bordom_Timer_p = "Boredom Timer";

	private protected static string _Restless_Timer_p = "Restless Timer";

	private protected static string _Pause_Timers_p = "Pause Timers";

	private protected static string _LocalClick_Cooldown_p = "LocalClick Cooldown";

	private protected static string _OppClick_Cooldown_p = "OppClick Cooldown";

	private protected static string _Reaction_Cooldown_p = "Reaction Cooldown";

	private protected static string _ReadyToReact_p = "Ready To React";

	protected TickableBase(AccessoryController accessoryController, Animator animator)
	{
		_accessoryController = accessoryController;
		_animator = animator;
		ParamExists = new Dictionary<string, bool>
		{
			{
				_Bordom_Timer_p,
				HasParam(_Bordom_Timer_p)
			},
			{
				_Restless_Timer_p,
				HasParam(_Restless_Timer_p)
			},
			{
				_Pause_Timers_p,
				HasParam(_Pause_Timers_p)
			},
			{
				_LocalClick_Cooldown_p,
				HasParam(_LocalClick_Cooldown_p)
			},
			{
				_OppClick_Cooldown_p,
				HasParam(_OppClick_Cooldown_p)
			},
			{
				_Reaction_Cooldown_p,
				HasParam(_Reaction_Cooldown_p)
			},
			{
				_ReadyToReact_p,
				HasParam(_ReadyToReact_p)
			}
		};
	}

	public abstract void Update(float deltaTime);

	public void PauseTimers()
	{
		isPaused = true;
	}

	public void ResumeTimers()
	{
		isPaused = false;
	}

	private bool HasParam(string paramName)
	{
		AnimatorControllerParameter[] parameters = _animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == paramName)
			{
				return true;
			}
		}
		return false;
	}

	private protected void TrackParameter(string key, float param)
	{
		if (ParamExists[key])
		{
			_animator.SetFloat(key, param);
		}
	}
}
