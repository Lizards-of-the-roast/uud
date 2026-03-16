using System;
using UnityEngine;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Hover : AutoPlayAction
{
	private string _targetTag;

	private const float HoverDelay = 5f;

	private float _hoverStart;

	private bool _didHover;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_targetTag = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	private void MouseOver()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[1] { typeof(CustomButton) });
		if (autoplayHookFromTag != null && autoplayHookFromTag is CustomButton customButton)
		{
			customButton.OnMouseover?.Invoke();
			_didHover = true;
			_hoverStart = Time.realtimeSinceStartup;
		}
	}

	private void MouseOff()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[1] { typeof(CustomButton) });
		if (autoplayHookFromTag != null && autoplayHookFromTag is CustomButton customButton)
		{
			customButton.OnMouseover?.Invoke();
			Complete($"Hover on {autoplayHookFromTag}");
		}
	}

	protected override void OnUpdate()
	{
		if (!base.IsComplete)
		{
			if (!_didHover)
			{
				MouseOver();
			}
			else if (Time.realtimeSinceStartup - _hoverStart >= 5f)
			{
				MouseOff();
			}
		}
	}
}
