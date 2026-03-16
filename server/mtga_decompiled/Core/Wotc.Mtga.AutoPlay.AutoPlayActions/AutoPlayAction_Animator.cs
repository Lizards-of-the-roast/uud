using System;
using UnityEngine;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Animator : AutoPlayAction
{
	private string _targetTag;

	private string _parameter;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_targetTag = AutoPlayAction.FromParameter(in parameters, index + 1);
		_parameter = AutoPlayAction.FromParameter(in parameters, index + 2);
	}

	protected override void OnExecute()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[1] { typeof(Animator) });
		Animator animator = autoplayHookFromTag as Animator;
		if (autoplayHookFromTag == null || animator == null)
		{
			if (autoplayHookFromTag == null)
			{
				Fail("Could not find " + _targetTag);
			}
			else
			{
				Fail($"{autoplayHookFromTag} doesn't have an animator");
			}
		}
		else
		{
			animator.SetTrigger(_parameter);
			Complete("Set trigger " + _parameter + " on " + _targetTag);
		}
	}
}
