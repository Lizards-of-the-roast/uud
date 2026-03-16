using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CustomButton))]
public class CustomToggle : MonoBehaviour
{
	public bool Value;

	[SerializeField]
	private Transform _stateHandlerContainer;

	[SerializeField]
	private UnityEvent _onValueChanged;

	private CustomButton _button;

	private ICustomToggleAnimationHandler[] _allStateHandlers;

	private bool? _lastValue;

	public UnityEvent OnValueChanged => _onValueChanged;

	public CustomButton Button
	{
		get
		{
			if (_button == null)
			{
				_button = GetComponent<CustomButton>();
				_button.OnClick.AddListener(Button_OnClick);
			}
			return _button;
		}
	}

	private void Awake()
	{
		_ = Button;
	}

	private void Update()
	{
		if (_allStateHandlers == null)
		{
			_allStateHandlers = ((_stateHandlerContainer != null) ? _stateHandlerContainer.GetComponentsInChildren<ICustomToggleAnimationHandler>() : new ICustomToggleAnimationHandler[0]);
		}
		if (!_lastValue.HasValue)
		{
			_lastValue = Value;
			ICustomToggleAnimationHandler[] allStateHandlers = _allStateHandlers;
			foreach (ICustomToggleAnimationHandler customToggleAnimationHandler in allStateHandlers)
			{
				if (Value)
				{
					customToggleAnimationHandler.BeginTrue(0f);
				}
				else
				{
					customToggleAnimationHandler.BeginFalse(0f);
				}
			}
		}
		else
		{
			if (_lastValue == Value)
			{
				return;
			}
			_lastValue = Value;
			ICustomToggleAnimationHandler[] allStateHandlers = _allStateHandlers;
			foreach (ICustomToggleAnimationHandler customToggleAnimationHandler2 in allStateHandlers)
			{
				if (Value)
				{
					customToggleAnimationHandler2.BeginTrue();
				}
				else
				{
					customToggleAnimationHandler2.BeginFalse();
				}
			}
			OnValueChanged.Invoke();
		}
	}

	private void Button_OnClick()
	{
		Value = !Value;
	}
}
