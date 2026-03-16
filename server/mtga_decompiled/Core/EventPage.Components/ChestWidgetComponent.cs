using System;
using UnityEngine;

namespace EventPage.Components;

public class ChestWidgetComponent : EventComponent
{
	[Header("UI")]
	[SerializeField]
	private ChestWidget _widget;

	public Action OnClick;

	private void Awake()
	{
		_widget.Clicked += OnWidgetClicked;
	}

	public void Init()
	{
		_widget.Init();
		_widget.Activate(activate: true);
	}

	private void OnWidgetClicked()
	{
		OnClick?.Invoke();
	}
}
