using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Loc;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[FormerlySerializedAs("_startInteractable")]
	[SerializeField]
	private bool _interactable = true;

	[SerializeField]
	private Transform _stateHandlerContainer;

	[SerializeField]
	private UnityEvent _onClickDown;

	[SerializeField]
	private UnityEvent _onClick;

	[SerializeField]
	private UnityEvent _onRightClick;

	public Action<GameObject> OnClickedReturnSource;

	[SerializeField]
	private UnityEvent _onMouseover;

	[SerializeField]
	private UnityEvent _onMouseoff;

	[SerializeField]
	private UnityEvent _onEnable;

	[SerializeField]
	private UnityEvent _onDisable;

	private float previousTapTime;

	[SerializeField]
	private float _timeBetweenTaps = 0.4f;

	private static bool _isDoubleTap = false;

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	private bool _mouseOver;

	private bool _mouseDown;

	private ICustomButtonAnimationHandler[] _allStateHandlers;

	private Animator _interactableAnimator;

	private TextMeshProUGUI _textObject;

	private Localize _localizeObject;

	public UnityEvent OnClickDown => _onClickDown;

	public UnityEvent OnClick => _onClick;

	public UnityEvent OnRightClick => _onRightClick;

	public UnityEvent OnMouseover => _onMouseover;

	public UnityEvent OnMouseoff => _onMouseoff;

	public UnityEvent OnEnabled => _onEnable;

	public UnityEvent OnDisabled => _onDisable;

	public float TimeBetweenTaps => _timeBetweenTaps;

	public static int ClickCount { get; private set; }

	public static bool IsDoubleClick
	{
		get
		{
			if (PlatformUtils.IsHandheld())
			{
				return _isDoubleTap;
			}
			if (ClickCount > 0)
			{
				return ClickCount % 2 == 0;
			}
			return false;
		}
	}

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

	public void SetText(string text, bool warnOnMissingTextComponent = true)
	{
		if (_textObject == null)
		{
			_textObject = GetComponentInChildren<TextMeshProUGUI>();
		}
		if (_textObject != null)
		{
			_textObject.text = text;
		}
		else if (warnOnMissingTextComponent)
		{
			Debug.LogWarning("Trying to set text and it doesn't exist");
		}
	}

	public void SetLocText(string text, bool warnOnMissingTextComponent = true)
	{
		if (_localizeObject == null)
		{
			_localizeObject = GetComponentInChildren<Localize>();
		}
		if (_localizeObject != null)
		{
			_localizeObject.SetText(text);
		}
		else if (warnOnMissingTextComponent)
		{
			Debug.LogWarning("Trying to set text and it doesn't exist");
		}
	}

	private void OnDestroy()
	{
		_onClickDown.RemoveAllListeners();
		_onClick.RemoveAllListeners();
		_onRightClick.RemoveAllListeners();
		_onMouseover.RemoveAllListeners();
		_onMouseoff.RemoveAllListeners();
		_onEnable.RemoveAllListeners();
		_onDisable.RemoveAllListeners();
		OnClickedReturnSource = null;
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

	private void SetInteractable(bool value)
	{
		_interactable = value;
		if (base.isActiveAndEnabled)
		{
			UpdateState();
			if (_interactable && _onEnable != null)
			{
				_onEnable.Invoke();
			}
			else if (!_interactable && _onDisable != null)
			{
				_onDisable.Invoke();
			}
			if (_interactableAnimator != null)
			{
				_interactableAnimator.SetBool(Disabled, !_interactable);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_mouseOver = true;
		if (_interactable)
		{
			UpdateState();
			if (_onMouseover != null)
			{
				_onMouseover.Invoke();
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_mouseOver = false;
		if (_interactable)
		{
			UpdateState();
			if (_onMouseoff != null)
			{
				_onMouseoff.Invoke();
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_mouseDown = true;
		if (_interactable && eventData.button == PointerEventData.InputButton.Left)
		{
			_onClickDown.Invoke();
			UpdateState();
		}
		if (_interactable && PlatformUtils.IsHandheld())
		{
			CheckDoubleTap();
		}
	}

	public void CheckDoubleTap()
	{
		if (Time.time - previousTapTime < _timeBetweenTaps)
		{
			_isDoubleTap = true;
			return;
		}
		_isDoubleTap = false;
		previousTapTime = Time.time;
	}

	public void Click()
	{
		if (_interactable)
		{
			OnPointerDown(new PointerEventData(null)
			{
				button = PointerEventData.InputButton.Left
			});
			_onClick.Invoke();
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
		if (!CustomInputModule.PointerIsHeldDown() && _mouseOver)
		{
			switch (eventData.button)
			{
			case PointerEventData.InputButton.Left:
				ClickCount = eventData.clickCount;
				_onClick.Invoke();
				OnClickedReturnSource?.Invoke(base.gameObject);
				break;
			case PointerEventData.InputButton.Right:
				_onRightClick.Invoke();
				break;
			}
		}
	}

	protected void UpdateState()
	{
		if (_allStateHandlers == null)
		{
			_allStateHandlers = ((_stateHandlerContainer != null) ? _stateHandlerContainer.GetComponentsInChildren<ICustomButtonAnimationHandler>(includeInactive: true) : Array.Empty<ICustomButtonAnimationHandler>());
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

	public void SetInitialMouseState()
	{
		_mouseDown = false;
		_mouseOver = false;
		UpdateState();
	}
}
