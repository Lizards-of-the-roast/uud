using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Spinner : MonoBehaviour, ISpinner
{
	[SerializeField]
	private TextMeshProUGUI _valueText;

	[SerializeField]
	private Button _upButton;

	[SerializeField]
	private Button _downButton;

	[SerializeField]
	private int _value;

	[SerializeField]
	private bool _useMin = true;

	[SerializeField]
	private int _minValue;

	[SerializeField]
	private bool _useMax;

	[SerializeField]
	private int _maxValue;

	private SpinnerGroup _group;

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			int num = ClampNewValue(value);
			if (_value != num)
			{
				_value = num;
				_valueText.text = _value.ToString();
			}
		}
	}

	public bool UseMin
	{
		get
		{
			return _useMin;
		}
		set
		{
			_useMin = value;
		}
	}

	public int MinValue
	{
		get
		{
			return _minValue;
		}
		set
		{
			_minValue = value;
		}
	}

	public bool UseMax
	{
		get
		{
			return _useMax;
		}
		set
		{
			_useMax = value;
		}
	}

	public int MaxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			_maxValue = value;
		}
	}

	private void Awake()
	{
		_upButton.onClick.AddListener(OnUpButtonClicked);
		_downButton.onClick.AddListener(OnDownButtonClicked);
		_group = GetComponentInParent<SpinnerGroup>();
		if (_group != null)
		{
			_group.RegisterSpinner(this);
		}
	}

	private void Start()
	{
		_valueText.text = Value.ToString();
	}

	private void OnDestroy()
	{
		_upButton.onClick.RemoveListener(OnUpButtonClicked);
		_downButton.onClick.RemoveListener(OnDownButtonClicked);
	}

	private void OnUpButtonClicked()
	{
		int value = Value + 1;
		Value = value;
	}

	private void OnDownButtonClicked()
	{
		int value = Value - 1;
		Value = value;
	}

	private int ClampNewValue(int newValue)
	{
		if (_useMin && newValue < _minValue)
		{
			newValue = _minValue;
		}
		if (_useMax && newValue > _maxValue)
		{
			newValue = _maxValue;
		}
		if (_group != null)
		{
			newValue = _group.ClampNewValue(newValue, this);
		}
		return newValue;
	}
}
