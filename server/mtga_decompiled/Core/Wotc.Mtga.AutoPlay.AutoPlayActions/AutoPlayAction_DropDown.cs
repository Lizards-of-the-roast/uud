using System;
using TMPro;
using UnityEngine;
using cTMP;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_DropDown : AutoPlayAction
{
	private string _targetTag;

	private string _selection;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_targetTag = AutoPlayAction.FromParameter(in parameters, index + 1);
		_selection = AutoPlayAction.FromParameter(in parameters, index + 2);
	}

	protected override void OnExecute()
	{
		Component autoplayHookFromTag = ComponentGetters.GetAutoplayHookFromTag(_targetTag, new Type[2]
		{
			typeof(cTMP_Dropdown),
			typeof(TMP_Dropdown)
		});
		if (autoplayHookFromTag == null)
		{
			Fail("Could not find " + _targetTag);
			return;
		}
		if (!(autoplayHookFromTag is cTMP_Dropdown cTMP_Dropdown))
		{
			if (!(autoplayHookFromTag is TMP_Dropdown tMP_Dropdown))
			{
				return;
			}
			if (!int.TryParse(_selection, out var result))
			{
				result = tMP_Dropdown.options.FindIndex((TMP_Dropdown.OptionData option) => option.text == _selection);
				if (result == -1)
				{
					Fail("Could not select " + _selection + " from dropdown " + _targetTag);
					return;
				}
			}
			if (result == tMP_Dropdown.value)
			{
				Complete("Dropdown " + _targetTag + " was already set to " + _selection);
				return;
			}
			tMP_Dropdown.value = result;
			Complete($"Set dropdown {_targetTag} to index {result} based on parameter {_selection}. Component: {tMP_Dropdown}");
			return;
		}
		if (!int.TryParse(_selection, out var result2))
		{
			result2 = cTMP_Dropdown.options.FindIndex((cTMP_Dropdown.OptionData option) => option.text == _selection);
			if (result2 == -1)
			{
				Fail("Could not select " + _selection + " from dropdown " + _targetTag);
				return;
			}
		}
		if (result2 == cTMP_Dropdown.value)
		{
			Complete("Dropdown " + _targetTag + " was already set to " + _selection);
			return;
		}
		cTMP_Dropdown.value = result2;
		Complete($"Set dropdown {_targetTag} to index {result2} based on parameter {_selection}. Component: {cTMP_Dropdown}");
	}
}
