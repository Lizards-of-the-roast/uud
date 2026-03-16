using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Wotc.Mtga.Loc;

[RequireComponent(typeof(Animator))]
public class KeyboardToggleButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private static int ANIMPROPERTY_HIDDEN = Animator.StringToHash("Hidden");

	private static int ANIMPROPERTY_HIGHLIGHT = Animator.StringToHash("Highlight");

	private bool _isOn;

	[Header("Configuration")]
	public bool HiddenWhenOff;

	public bool HiddenOnAwake;

	[Tooltip("Requires outside component to respond to OnToggled and then SetToggle() appropriately")]
	public bool SetToggleManually;

	public bool SetToggleOnShow = true;

	public bool SetToggleOnHide = true;

	[Header("Assets")]
	[SerializeField]
	private Localize _labelText;

	[SerializeField]
	private Localize _keyText;

	private Animator _animator;

	public UnityEvent OnToggled;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		if (HiddenOnAwake)
		{
			HideToggle();
		}
	}

	private void OnEnable()
	{
		_isOn = !_isOn;
		SetToggled(!_isOn);
	}

	public void SetKeyText(MTGALocalizedString key)
	{
		_keyText?.SetText(key);
	}

	public void SetLabelText(MTGALocalizedString label)
	{
		_labelText?.SetText(label);
	}

	public void Toggle()
	{
		if (!SetToggleManually)
		{
			SetToggled(!_isOn);
		}
		OnToggled.Invoke();
	}

	public void ShowToggle()
	{
		if (SetToggleOnShow)
		{
			SetToggled(on: true);
		}
		_animator.SetBool(ANIMPROPERTY_HIDDEN, value: false);
	}

	public void HideToggle()
	{
		if (SetToggleOnHide)
		{
			SetToggled(on: false);
		}
		_animator.SetBool(ANIMPROPERTY_HIDDEN, value: true);
	}

	public void SetToggled(bool on)
	{
		if (_isOn == on)
		{
			return;
		}
		_isOn = on;
		if (!(_animator == null) && _animator.isActiveAndEnabled)
		{
			if (HiddenWhenOff)
			{
				_animator.SetBool(ANIMPROPERTY_HIDDEN, !_isOn);
			}
			_animator.SetBool(ANIMPROPERTY_HIGHLIGHT, _isOn);
			_labelText?.DoLocalize();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Toggle();
	}
}
