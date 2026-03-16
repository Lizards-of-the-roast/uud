using System;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Loc;

public class PromptButton : MonoBehaviour
{
	[SerializeField]
	private Localize _primaryText;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private GameObject _buttonEffect;

	private string buttonSFX = "sfx_ui_accept";

	private CustomButton _customButtonScript;

	public bool IsActive => base.gameObject.activeSelf;

	public bool HasInteraction { get; private set; }

	public void Awake()
	{
		_customButtonScript = GetComponent<CustomButton>();
	}

	public void SetAction(MTGALocalizedString key, Action action, string sfx = "sfx_ui_accept")
	{
		SetButtonActive(active: true);
		_primaryText.SetText(key);
		buttonSFX = sfx;
		if (_button != null)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(delegate
			{
				action();
				AudioManager.PlayAudio(buttonSFX, AudioManager.Default);
			});
		}
		else if (_customButtonScript != null)
		{
			_customButtonScript.OnClick.RemoveAllListeners();
			_customButtonScript.OnClick.AddListener(delegate
			{
				action();
				AudioManager.PlayAudio(buttonSFX, AudioManager.Default);
			});
		}
	}

	public void SetEnabledState(bool isEnabled)
	{
		if (_button != null)
		{
			_button.enabled = isEnabled;
		}
		if (_customButtonScript == null)
		{
			_customButtonScript = GetComponent<CustomButton>();
		}
		_customButtonScript.Interactable = isEnabled;
		if (_buttonEffect != null)
		{
			_buttonEffect.SetActive(isEnabled);
		}
		HasInteraction = isEnabled;
	}

	public void SetDisabled()
	{
		_primaryText.SetText("MainNav/General/Empty_String");
		if (_button != null)
		{
			_button.onClick.RemoveAllListeners();
		}
		SetButtonActive(active: false);
	}

	private void SetButtonActive(bool active)
	{
		HasInteraction = active;
		if (_buttonEffect != null)
		{
			_buttonEffect.SetActive(active);
		}
		SetActive(active);
	}

	public void SetActive(bool active)
	{
		if (base.gameObject.activeSelf != active)
		{
			base.gameObject.SetActive(active);
		}
	}
}
