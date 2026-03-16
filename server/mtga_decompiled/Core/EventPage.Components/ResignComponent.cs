using System;
using UnityEngine;

namespace EventPage.Components;

public class ResignComponent : EventComponent
{
	[SerializeField]
	private CustomButton _resignButton;

	public Action OnClick;

	private void Awake()
	{
		_resignButton.OnClick.AddListener(ResignButtonClicked);
	}

	private void ResignButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
		OnClick?.Invoke();
	}
}
