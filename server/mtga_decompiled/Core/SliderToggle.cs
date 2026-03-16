using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class SliderToggle : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public class ValueChangedEvent : UnityEvent
	{
	}

	[SerializeField]
	private string _value;

	[SerializeField]
	private float _slideDuration = 0.1f;

	[SerializeField]
	private Ease _slideEase = Ease.OutQuad;

	[SerializeField]
	private float _dragDistanceThreshold = 15f;

	[SerializeField]
	private float _dragDurationThreshold = 0.25f;

	[SerializeField]
	private bool _buttonClickCyclesTarget;

	private ValueChangedEvent _onValueChanged = new ValueChangedEvent();

	private Button _button;

	private TextMeshProUGUI _buttonLabel;

	private bool _isInitialized;

	private bool _isDragging;

	private Vector3 _dragPosition;

	private Vector3 _dragOffset;

	private float _dragTime;

	private Canvas _canvas;

	private RectTransform _canvasTransform;

	private Vector2 _mousePoint;

	private SliderToggleTarget[] _allTargets;

	public ValueChangedEvent OnValueChanged => _onValueChanged;

	private void Awake()
	{
		_button = GetComponentInChildren<Button>();
		_button.onClick.AddListener(OnButtonClick);
		_buttonLabel = _button.GetComponentInChildren<TextMeshProUGUI>();
		_allTargets = (from t in GetComponentsInChildren<SliderToggleTarget>()
			orderby t.Position.x
			select t).ToArray();
		for (int num = 0; num < _allTargets.Length; num++)
		{
			SliderToggleTarget obj = _allTargets[num];
			obj.Index = num;
			obj.ParentToggle = this;
		}
		if (_allTargets.Length < 2)
		{
			Debug.LogErrorFormat("SliderToggle({0}): {1} found, requires at least 2", base.gameObject.name, _allTargets.Length.ToQuantity("target"));
		}
	}

	private void Start()
	{
		SliderToggleTarget target = GetTarget(_value);
		if (target == null)
		{
			Debug.LogErrorFormat("SliderToggle({0}): starting value ({1}) not found", _value);
		}
		else
		{
			_buttonLabel.text = target.Text;
			_button.transform.position = target.Position;
			_isInitialized = true;
		}
	}

	private void OnDestroy()
	{
		_button.onClick.RemoveListener(OnButtonClick);
	}

	public string GetValue()
	{
		return _value;
	}

	public void SetValue(string newValue)
	{
		if (_isInitialized)
		{
			SliderToggleTarget target = GetTarget(newValue);
			if (target == null)
			{
				Debug.LogErrorFormat("SliderToggle({0}): SetValue called with value ({1}) not found", newValue);
				return;
			}
			_buttonLabel.text = target.Text;
			DOTween.Kill(this);
			_button.transform.DOMove(target.Position, _slideDuration).SetEase(_slideEase).SetTarget(this);
			if (_value != newValue)
			{
				_value = newValue;
				_onValueChanged.Invoke();
			}
		}
		else
		{
			_value = newValue;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_isDragging = eventData.rawPointerPress == _button.gameObject;
		if (_isDragging)
		{
			_dragPosition = _button.transform.position;
			_dragOffset = _dragPosition - GetMouseWorldPosition(eventData);
			_dragTime = Time.time;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (_isDragging)
		{
			_dragPosition.x = Mathf.Clamp((GetMouseWorldPosition(eventData) + _dragOffset).x, _allTargets.First().Position.x, _allTargets.Last().Position.x);
			_button.transform.position = _dragPosition;
			float x = _button.transform.position.x;
			SliderToggleTarget nearestTarget = GetNearestTarget(x);
			_buttonLabel.text = nearestTarget.Text;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!_isDragging)
		{
			return;
		}
		float x = _button.transform.position.x;
		SliderToggleTarget target = GetTarget(_value);
		SliderToggleTarget sliderToggleTarget = GetNearestTarget(x);
		if (target == sliderToggleTarget)
		{
			float num = x - target.Position.x;
			float num2 = Time.time - _dragTime;
			bool num3 = Mathf.Abs(num) >= _dragDistanceThreshold;
			bool flag = num2 <= _dragDurationThreshold;
			if (num3 && flag)
			{
				int index = target.Index;
				int num4 = ((num > 0f) ? GetLoopingIndex(index + 1) : GetLoopingIndex(index - 1));
				sliderToggleTarget = _allTargets[num4];
			}
		}
		SetValue(sliderToggleTarget.Value);
		_isDragging = false;
	}

	private void OnButtonClick()
	{
		if (!_isDragging)
		{
			if (_buttonClickCyclesTarget)
			{
				int index = GetTarget(_value).Index;
				int loopingIndex = GetLoopingIndex(index + 1);
				SetValue(_allTargets[loopingIndex].Value);
			}
			else
			{
				_onValueChanged.Invoke();
			}
		}
	}

	private Vector3 GetMouseWorldPosition(PointerEventData eventData)
	{
		if (_canvas == null)
		{
			_canvas = GetComponentInParent<Canvas>();
			_canvasTransform = _canvas.transform as RectTransform;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, eventData.position, _canvas.worldCamera, out _mousePoint);
		return _canvasTransform.TransformPoint(_mousePoint);
	}

	private SliderToggleTarget GetTarget(string value)
	{
		return _allTargets.FirstOrDefault((SliderToggleTarget t) => string.Compare(t.Value, value) == 0);
	}

	private SliderToggleTarget GetNearestTarget(float x)
	{
		int num = 0;
		float num2 = Mathf.Abs(x - _allTargets[0].Position.x);
		for (int i = 1; i < _allTargets.Length; i++)
		{
			float num3 = Mathf.Abs(x - _allTargets[i].Position.x);
			if (num3 < num2)
			{
				num2 = num3;
				num = i;
			}
		}
		return _allTargets[num];
	}

	private int GetLoopingIndex(int testIndex)
	{
		int num = testIndex % _allTargets.Length;
		if (num < 0)
		{
			num += _allTargets.Length;
		}
		return num;
	}
}
