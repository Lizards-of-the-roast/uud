using UnityEngine;
using UnityEngine.UI;

public class ToggleAnimator : MonoBehaviour
{
	[SerializeField]
	[Tooltip("If null, will be found with GetComponent")]
	private Toggle _toggle;

	[SerializeField]
	[Tooltip("If null, will be found with GetComponent")]
	private Animator _animator;

	[SerializeField]
	private bool _resetOnEnable;

	public string[] OnBoolNames;

	public string[] OnTriggerNames;

	private bool _initialized;

	private bool _initialToggleSetting;

	private bool _isOn;

	public bool IsOn
	{
		get
		{
			return _toggle.isOn;
		}
		set
		{
			_toggle.isOn = value;
		}
	}

	private void Awake()
	{
		if (_toggle == null)
		{
			_toggle = GetComponent<Toggle>();
			_initialToggleSetting = _toggle.isOn;
		}
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
	}

	private void OnEnable()
	{
		if (_resetOnEnable)
		{
			_toggle.isOn = _initialToggleSetting;
			_initialized = false;
		}
	}

	private void LateUpdate()
	{
		if (_initialized && _isOn == _toggle.isOn)
		{
			return;
		}
		_initialized = true;
		_isOn = _toggle.isOn;
		string[] onBoolNames = OnBoolNames;
		foreach (string text in onBoolNames)
		{
			_animator.SetBool(text, _isOn);
		}
		onBoolNames = OnTriggerNames;
		foreach (string trigger in onBoolNames)
		{
			if (_isOn)
			{
				_animator.SetTrigger(trigger);
			}
			else
			{
				_animator.ResetTrigger(trigger);
			}
		}
	}
}
