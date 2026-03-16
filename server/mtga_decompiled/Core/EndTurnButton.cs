using System;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class EndTurnButton : MonoBehaviour
{
	[SerializeField]
	private CustomButton _button;

	[SerializeField]
	private GameObject _spinner;

	[SerializeField]
	private Image _buttonOff;

	[SerializeField]
	private Image _buttonOn;

	[SerializeField]
	private Sprite _buttonOffPlayer;

	[SerializeField]
	private Sprite _buttonOffOpponent;

	[SerializeField]
	private Sprite _buttonOnPlayer;

	[SerializeField]
	private Sprite _buttonOnOpponent;

	private GREPlayerNum _activePlayer;

	private bool _autoPassEnabled;

	private bool _showing;

	private Animator _animator;

	private Localize _text;

	public event System.Action Clicked;

	private void Start()
	{
		_animator = GetComponentInChildren<Animator>();
		_text = GetComponentInChildren<Localize>(includeInactive: true);
		_button.OnClick.AddListener(OnPointerClick);
		_button.OnMouseover.AddListener(OnPointerEnter);
		_button.OnMouseoff.AddListener(OnPointerExit);
		UpdateText();
	}

	public void SetEnabled(bool enabled)
	{
		_button.Interactable = enabled;
	}

	public void OnSettingsUpdated(SettingsMessage settingsMessage)
	{
		bool flag = settingsMessage.AutoPassEnabled();
		if (_autoPassEnabled != flag)
		{
			_autoPassEnabled = flag;
			UpdateButton();
			UpdateText();
		}
	}

	private void OnPointerClick()
	{
		this.Clicked?.Invoke();
		AudioManager.PlayAudio(WwiseEvents.sfx_UI_phasebutton_endyourturn.EventName, AudioManager.Default);
	}

	private void OnPointerExit()
	{
		SetShowing(showing: false);
	}

	private void OnPointerEnter()
	{
		SetShowing(showing: true);
	}

	private void SetShowing(bool showing)
	{
		if (_showing != showing)
		{
			_showing = showing;
			UpdateText();
		}
	}

	public void UpdateActivePlayer(GREPlayerNum player)
	{
		_activePlayer = player;
		UpdateButton();
	}

	private void UpdateButton()
	{
		_buttonOn.gameObject.SetActive(_autoPassEnabled);
		_buttonOff.gameObject.SetActive(!_autoPassEnabled);
		_buttonOn.sprite = ((_activePlayer != GREPlayerNum.Opponent) ? _buttonOnPlayer : _buttonOnOpponent);
		_buttonOff.sprite = ((_activePlayer != GREPlayerNum.Opponent) ? _buttonOffPlayer : _buttonOffOpponent);
	}

	private void UpdateText()
	{
		_spinner.gameObject.SetActive(_autoPassEnabled && !_showing);
		if (_showing || _autoPassEnabled)
		{
			if (_showing && _autoPassEnabled)
			{
				_text.SetText("DuelScene/SettingsMenu/Gameplay/EndTurnStopPass");
			}
			else if (_autoPassEnabled)
			{
				_text.SetText("DuelScene/SettingsMenu/Gameplay/EndTurnPassing");
			}
			else
			{
				_text.SetText("DuelScene/SettingsMenu/Gameplay/EndTurnHardPass");
			}
			if (!_text.gameObject.activeSelf)
			{
				_text.gameObject.SetActive(value: true);
				_animator.SetTrigger("TextIntro");
			}
		}
		else
		{
			_text.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_button)
		{
			_button.OnClick.RemoveListener(OnPointerClick);
			_button.OnMouseover.RemoveListener(OnPointerEnter);
			_button.OnMouseoff.RemoveListener(OnPointerExit);
		}
	}
}
