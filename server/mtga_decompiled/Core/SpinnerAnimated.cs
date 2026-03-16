using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class SpinnerAnimated : MonoBehaviour, ISpinner
{
	[SerializeField]
	private TextMeshProUGUI _transitionText;

	[SerializeField]
	private float _transitionDuration = 0.1f;

	[SerializeField]
	private Ease _transitionEase = Ease.InOutQuad;

	[SerializeField]
	private float _bumpOutDuration = 0.05f;

	[SerializeField]
	private Ease _bumpOutEase = Ease.OutCirc;

	[SerializeField]
	private float _bumpInDuration = 0.05f;

	[SerializeField]
	private Ease _bumpInEase = Ease.InCirc;

	[SerializeField]
	private float _bumpPercent = 0.2f;

	private int _displayingValue;

	private Coroutine _animationCoroutine;

	private Coroutine _bumpCoroutine;

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

	private bool _squelchEvents;

	public uint InstanceId { get; set; }

	public uint ShiftMultiplier { get; set; }

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
				ValueChangedEventArgs eventArgs = new ValueChangedEventArgs
				{
					OldValue = _value,
					NewValue = num,
					AssociatedInstanceId = InstanceId
				};
				_value = num;
				OnValueChanged(eventArgs);
			}
			_displayingValue = num;
			_valueText.text = num.ToString();
			DOTween.Kill(this, complete: true);
			if (_animationCoroutine != null)
			{
				StopCoroutine(_animationCoroutine);
				_animationCoroutine = null;
			}
			_valueText.transform.localPosition = Vector3.zero;
			_transitionText.transform.localPosition = new Vector3(0f, _valueText.rectTransform.sizeDelta.y, 0f);
			_downButton.interactable = !UseMin || Value != MinValue;
			_upButton.interactable = !UseMax || Value != MaxValue;
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

	public event EventHandler<ValueChangedEventArgs> ValueChanged;

	private void OnValueChanged(ValueChangedEventArgs eventArgs)
	{
		EventHandler<ValueChangedEventArgs> eventHandler = this.ValueChanged;
		if (eventHandler != null && !_squelchEvents)
		{
			eventHandler(this, eventArgs);
		}
	}

	public void InitValue(int value)
	{
		_squelchEvents = true;
		_value = value;
		Value = value;
		_squelchEvents = false;
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
		_displayingValue = Value;
		_valueText.text = Value.ToString();
		_valueText.transform.localPosition = Vector3.zero;
		_transitionText.transform.localPosition = new Vector3(0f, _valueText.rectTransform.sizeDelta.y, 0f);
	}

	private void OnDestroy()
	{
		_upButton.onClick.RemoveListener(OnUpButtonClicked);
		_downButton.onClick.RemoveListener(OnDownButtonClicked);
	}

	private void OnUpButtonClicked()
	{
		int num = 1;
		if (ShiftMultiplier != 0 && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			num *= (int)ShiftMultiplier;
		}
		int num2 = ClampNewValue(_value + num);
		if (num2 > _value)
		{
			Value = num2;
			if (_animationCoroutine == null)
			{
				_animationCoroutine = StartCoroutine(Coroutine_AnimateValue());
			}
		}
		else if (_bumpCoroutine == null)
		{
			_bumpCoroutine = StartCoroutine(Coroutine_Bump(1f));
		}
	}

	private void OnDownButtonClicked()
	{
		int num = 1;
		if (ShiftMultiplier != 0 && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			num *= (int)ShiftMultiplier;
		}
		int num2 = ClampNewValue(_value - num);
		if (num2 < _value)
		{
			Value = num2;
			if (_animationCoroutine == null)
			{
				_animationCoroutine = StartCoroutine(Coroutine_AnimateValue());
			}
		}
		else if (_bumpCoroutine == null)
		{
			_bumpCoroutine = StartCoroutine(Coroutine_Bump(-1f));
		}
	}

	private IEnumerator Coroutine_AnimateValue()
	{
		while (_displayingValue != _value)
		{
			_transitionText.text = _valueText.text;
			_valueText.text = _value.ToString();
			float y = _transitionText.rectTransform.sizeDelta.y;
			float num = ((_displayingValue > _value) ? (-1f) : 1f);
			_transitionText.transform.localPosition = Vector3.zero;
			_valueText.transform.localPosition = new Vector3(0f, num * y, 0f);
			Sequence sequence = DOTween.Sequence();
			sequence.Insert(0f, _transitionText.transform.DOLocalMoveY((0f - num) * y, _transitionDuration));
			sequence.Insert(0f, _valueText.transform.DOLocalMoveY(0f, _transitionDuration));
			sequence.SetEase(_transitionEase).SetTarget(this);
			_displayingValue = _value;
			yield return sequence.WaitForCompletion();
		}
		_animationCoroutine = null;
	}

	private IEnumerator Coroutine_Bump(float direction)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(_valueText.transform.DOLocalMoveY(direction * _bumpPercent * _valueText.rectTransform.sizeDelta.y, _bumpOutDuration).SetEase(_bumpOutEase));
		sequence.Append(_valueText.transform.DOLocalMoveY(0f, _bumpInDuration).SetEase(_bumpInEase));
		yield return sequence.WaitForCompletion();
		_bumpCoroutine = null;
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

	public void SetTextColor(Color32 color)
	{
		_valueText.color = color;
	}

	public void SetButtonsActive(bool active)
	{
		_upButton.gameObject.UpdateActive(active);
		_downButton.gameObject.UpdateActive(active);
	}
}
