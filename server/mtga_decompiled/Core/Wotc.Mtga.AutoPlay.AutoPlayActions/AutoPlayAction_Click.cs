using System;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Click : AutoPlayAction
{
	private enum ButtonAction
	{
		Click,
		ClickDown
	}

	private string _targetTag;

	private ButtonAction _action;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		string text = AutoPlayAction.FromParameter(in parameters, index);
		if (text == null || !Enum.TryParse<ButtonAction>(text, out _action))
		{
			_action = ButtonAction.Click;
		}
		_targetTag = AutoPlayAction.FromParameter(in parameters, index + 1);
	}

	protected override void OnExecute()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[2]
		{
			typeof(CustomButton),
			typeof(Button)
		});
		if (autoplayHookFromTag != null)
		{
			if (!(autoplayHookFromTag is CustomButton customButton))
			{
				if (autoplayHookFromTag is Button button)
				{
					if (_action == ButtonAction.Click)
					{
						button.onClick?.Invoke();
						Complete($"Clicked on {autoplayHookFromTag}");
					}
					else
					{
						Fail($"Button {autoplayHookFromTag} doesn't have a click down");
					}
				}
				else
				{
					Fail($"Failed to find a supported targetType for {autoplayHookFromTag}");
				}
			}
			else
			{
				if (_action == ButtonAction.Click)
				{
					customButton.OnClick?.Invoke();
				}
				else if (_action == ButtonAction.ClickDown)
				{
					customButton.OnClickDown?.Invoke();
				}
				Complete($"Clicked on {autoplayHookFromTag}");
			}
		}
		else
		{
			Fail("Failed to find " + _targetTag);
		}
	}
}
