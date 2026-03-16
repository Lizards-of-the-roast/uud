using System;
using UnityEngine;

namespace EventPage.Components;

public class ViewCardPoolComponent : EventComponent
{
	[SerializeField]
	private CustomButton _button;

	public Action OnClicked;

	private void Awake()
	{
		_button.OnClick.AddListener(delegate
		{
			OnClicked?.Invoke();
		});
	}
}
