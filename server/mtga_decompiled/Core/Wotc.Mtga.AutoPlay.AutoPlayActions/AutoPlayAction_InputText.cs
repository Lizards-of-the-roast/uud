using System;
using TMPro;
using UnityEngine;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_InputText : AutoPlayAction
{
	private string _targetTag;

	private string _text;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_targetTag = AutoPlayAction.FromParameter(in parameters, index + 1);
		_text = AutoPlayAction.FromParameter(in parameters, index + 2) ?? string.Empty;
	}

	protected override void OnExecute()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[1] { typeof(TMP_InputField) });
		if (autoplayHookFromTag != null)
		{
			if (autoplayHookFromTag is TMP_InputField tMP_InputField)
			{
				tMP_InputField.text = _text;
				Complete($"Text Input {_text} on {autoplayHookFromTag}");
			}
			else
			{
				Fail($"Failed to find a supported targetType to input text on for {autoplayHookFromTag}");
			}
		}
		else
		{
			Fail("Failed to find " + _targetTag + " on to input text");
		}
	}
}
