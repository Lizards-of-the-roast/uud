using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Wizards.Mtga.Platforms;

public class CustomTouchButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
{
	public enum ClickType
	{
		None,
		Click,
		Hold,
		Drag
	}

	[FormerlySerializedAs("_startInteractable")]
	[SerializeField]
	private bool _interactable = true;

	[SerializeField]
	private Transform _stateHandlerContainer;

	public float ClickAndHoldThresholdSeconds = 0.42f;

	public float _doubleClickMaxTimeBetweenTaps = 0.4f;

	private ClickType _clickType;

	private Coroutine _clickAndHoldCoroutine;

	private bool _mouseOver;

	private bool _mouseDown;

	private ICustomButtonAnimationHandler[] _allStateHandlers;

	private Animator _interactableAnimator;

	private TextMeshProUGUI _textObject;

	private float _previousSingleClickTime;

	[SerializeField]
	private UnityEvent _onClickDown;

	[SerializeField]
	private UnityEvent _onClickUp;

	[SerializeField]
	private UnityEvent _onClick;

	[SerializeField]
	private UnityEvent _onDoubleClick;

	[SerializeField]
	private UnityEvent _onClickAndHold;

	[SerializeField]
	private UnityEvent _onClickAndHoldEnd;

	[SerializeField]
	private UnityEvent _onRightClick;

	[SerializeField]
	private UnityEvent _onMouseover;

	[SerializeField]
	private UnityEvent _onMouseoff;

	[SerializeField]
	private UnityEvent _onEnable;

	[SerializeField]
	private UnityEvent _onDisable;

	public Action<GameObject> OnClickedReturnSource;

	public bool Interactable
	{
		get
		{
			return _interactable;
		}
		set
		{
			if (_interactable != value)
			{
				SetInteractable(value);
			}
		}
	}

	public UnityEvent OnClickDown => _onClickDown;

	public UnityEvent OnClickUp => _onClickUp;

	public UnityEvent OnClick => _onClick;

	public UnityEvent OnDoubleClick => _onDoubleClick;

	public UnityEvent OnClickAndHold => _onClickAndHold;

	public UnityEvent OnClickAndHoldEnd => _onClickAndHoldEnd;

	public UnityEvent OnRightClick => _onRightClick;

	public UnityEvent OnMouseOver => _onMouseover;

	public UnityEvent OnMouseOff => _onMouseoff;

	public UnityEvent OnEnabled => _onEnable;

	public UnityEvent OnDisabled => _onDisable;

	private bool IsDoubleClick(PointerEventData eventData)
	{
		if (PlatformUtils.IsHandheld())
		{
			return Time.time - _previousSingleClickTime < _doubleClickMaxTimeBetweenTaps;
		}
		if (eventData.clickCount > 0)
		{
			return eventData.clickCount % 2 == 0;
		}
		return false;
	}

	public virtual void Awake()
	{
		_interactableAnimator = GetComponent<Animator>();
		if (_interactableAnimator != null && Array.FindIndex(_interactableAnimator.parameters, (AnimatorControllerParameter p) => p.name == "Disabled") == -1)
		{
			_interactableAnimator = null;
		}
	}

	private void OnEnable()
	{
		SetInteractable(_interactable);
	}

	private void OnDestroy()
	{
		_clickType = ClickType.None;
		CancelClickAndHold();
		OnClickedReturnSource = null;
	}

	public void SetInitialMouseState()
	{
		_clickType = ClickType.None;
		_mouseDown = false;
		_mouseOver = false;
		UpdateState();
	}

	public void SetText(string text)
	{
		if (_textObject == null)
		{
			_textObject = GetComponentInChildren<TextMeshProUGUI>();
		}
		if (_textObject != null)
		{
			_textObject.text = text;
		}
		else
		{
			Debug.LogWarning("Trying to set text and it doesn't exist");
		}
	}

	public void SimulateClick(PointerEventData.InputButton button = PointerEventData.InputButton.Left)
	{
		if (_interactable)
		{
			OnPointerDown(new PointerEventData(null)
			{
				button = button
			});
			_onClick?.Invoke();
		}
	}

	private void SetInteractable(bool value)
	{
		_interactable = value;
		if (base.isActiveAndEnabled)
		{
			UpdateState();
			if (_interactable)
			{
				_onEnable?.Invoke();
			}
			else
			{
				CancelClickAndHold();
				_clickType = ClickType.None;
				_onDisable?.Invoke();
			}
			if (_interactableAnimator != null)
			{
				_interactableAnimator.SetBool("Disabled", !_interactable);
			}
		}
	}

	protected void UpdateState()
	{
		if (_allStateHandlers == null)
		{
			_allStateHandlers = ((_stateHandlerContainer != null) ? _stateHandlerContainer.GetComponentsInChildren<ICustomButtonAnimationHandler>(includeInactive: true) : new ICustomButtonAnimationHandler[0]);
		}
		ICustomButtonAnimationHandler[] allStateHandlers = _allStateHandlers;
		foreach (ICustomButtonAnimationHandler customButtonAnimationHandler in allStateHandlers)
		{
			if (!_interactable)
			{
				customButtonAnimationHandler.BeginDisabled();
			}
			else if (_mouseOver && _mouseDown)
			{
				customButtonAnimationHandler.BeginPressedOver();
			}
			else if (_mouseOver)
			{
				customButtonAnimationHandler.BeginMouseOver();
			}
			else if (_mouseDown)
			{
				customButtonAnimationHandler.BeginPressedOff();
			}
			else
			{
				customButtonAnimationHandler.BeginMouseOff();
			}
		}
	}

	private IEnumerator CheckClickAndHoldYield()
	{
		yield return new WaitForSecondsRealtime(ClickAndHoldThresholdSeconds);
		_clickType = ClickType.Hold;
		_onClickAndHold?.Invoke();
	}

	private void CancelClickAndHold()
	{
		if (_clickAndHoldCoroutine != null)
		{
			StopCoroutine(_clickAndHoldCoroutine);
			_clickAndHoldCoroutine = null;
		}
		if (_clickType == ClickType.Hold || _clickType == ClickType.Drag)
		{
			_onClickAndHoldEnd?.Invoke();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_mouseOver = true;
		if (_interactable)
		{
			UpdateState();
			_onMouseover?.Invoke();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_mouseOver = false;
		CancelClickAndHold();
		if (_interactable)
		{
			UpdateState();
			_onMouseoff?.Invoke();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_mouseDown = true;
		if (_interactable && eventData.button == PointerEventData.InputButton.Left)
		{
			_clickType = ClickType.Click;
			_onClickDown?.Invoke();
			UpdateState();
			_clickAndHoldCoroutine = StartCoroutine(CheckClickAndHoldYield());
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_mouseDown = false;
		if (!_interactable)
		{
			return;
		}
		UpdateState();
		_onClickUp?.Invoke();
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			if (_clickType == ClickType.Click)
			{
				if (IsDoubleClick(eventData))
				{
					_onDoubleClick?.Invoke();
				}
				else
				{
					_previousSingleClickTime = Time.time;
					_onClick?.Invoke();
					OnClickedReturnSource?.Invoke(base.gameObject);
				}
			}
			CancelClickAndHold();
			_clickType = ClickType.None;
			break;
		case PointerEventData.InputButton.Right:
			_onRightClick?.Invoke();
			break;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_clickType = ClickType.Drag;
	}
}
