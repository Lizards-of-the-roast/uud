using System;
using UnityEngine;

namespace EventPage.Components;

public class EventButtonComponent : EventComponent
{
	[SerializeField]
	private CustomButton _button;

	public Action OnClick;

	private void Awake()
	{
		_button.OnClick.AddListener(delegate
		{
			OnClick?.Invoke();
		});
	}
}
