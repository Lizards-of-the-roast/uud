using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Spinner_OptionSelector : MonoBehaviour
{
	public class SpinnerValueChangeEvent : UnityEvent<int, string>
	{
	}

	[SerializeField]
	private TextMeshProUGUI _valueLabel;

	[SerializeField]
	private CustomButton _buttonNextValue;

	[SerializeField]
	private CustomButton _buttonPreviousValue;

	public SpinnerValueChangeEvent onValueChanged = new SpinnerValueChangeEvent();

	private List<string> _options = new List<string>();

	private int _valueIndex;

	public string Value
	{
		get
		{
			return _options[ValueIndex];
		}
		set
		{
			SelectOption(value);
		}
	}

	public int ValueIndex
	{
		get
		{
			return _valueIndex;
		}
		set
		{
			if (_options.Count > 0)
			{
				int num = (value + _options.Count) % _options.Count;
				if (_valueIndex != num)
				{
					_valueIndex = num;
					onValueChanged.Invoke(num, Value);
				}
				RefreshView();
			}
		}
	}

	public void AddOption(string option)
	{
		_options.Add(option);
	}

	public void AddOptions(List<string> options)
	{
		_options.AddRange(options);
	}

	public void ClearOptions()
	{
		_options = new List<string>();
		ValueIndex = 0;
	}

	public void SelectOption(Enum enumValue)
	{
		SelectOption(Convert.ToInt32(enumValue));
	}

	public void SelectOption(int index)
	{
		ValueIndex = index;
	}

	public void SelectOption(string option)
	{
		if (_options.Contains(option))
		{
			ValueIndex = _options.FindIndex((string item) => item == option);
		}
	}

	private void RefreshView()
	{
		_valueLabel.text = ((_options.Count > 0) ? _options[ValueIndex] : "N/A");
	}

	private void Awake()
	{
		_buttonNextValue.OnClick.AddListener(OnNextValue);
		_buttonPreviousValue.OnClick.AddListener(OnPreviousValue);
	}

	private void Start()
	{
		RefreshView();
	}

	private void OnDestroy()
	{
		_buttonNextValue.OnClick.RemoveListener(OnNextValue);
		_buttonPreviousValue.OnClick.RemoveListener(OnPreviousValue);
	}

	private void OnNextValue()
	{
		int valueIndex = ValueIndex + 1;
		ValueIndex = valueIndex;
	}

	private void OnPreviousValue()
	{
		int valueIndex = ValueIndex - 1;
		ValueIndex = valueIndex;
	}
}
