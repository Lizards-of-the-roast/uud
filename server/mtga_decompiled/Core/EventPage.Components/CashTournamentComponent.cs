using System;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class CashTournamentComponent : EventComponent
{
	[SerializeField]
	private Localize _buttonText;

	[SerializeField]
	private CustomButton _button;

	public Action OnClick;

	public void Initialize(string locKey)
	{
		_buttonText.SetText(locKey);
		_button.OnClick.RemoveAllListeners();
		_button.OnClick.AddListener(ButtonOnClicked);
	}

	private void ButtonOnClicked()
	{
		OnClick?.Invoke();
	}
}
