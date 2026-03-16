using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventTileParameter : MonoBehaviour
{
	public TextMeshProUGUI ParameterValueText;

	private List<string> _possibleValues;

	private int _valueIndex;

	public string ParameterName { get; private set; }

	public void SetValues(string paramName, List<string> possibleValues)
	{
		ParameterName = paramName;
		_possibleValues = possibleValues;
		UpdateText();
	}

	public void OnNextValue()
	{
		_valueIndex++;
		_valueIndex %= _possibleValues.Count;
		UpdateText();
	}

	public void OnPreviousValue()
	{
		_valueIndex--;
		if (_valueIndex < 0)
		{
			_valueIndex = _possibleValues.Count - 1;
		}
		UpdateText();
	}

	private void UpdateText()
	{
		ParameterValueText.text = _possibleValues[_valueIndex];
	}

	public void SetEnabled(bool enabled)
	{
		GetComponent<Animator>().SetTrigger(enabled ? "On" : "Off");
		CustomButton[] componentsInChildren = GetComponentsInChildren<CustomButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Interactable = enabled;
		}
	}
}
