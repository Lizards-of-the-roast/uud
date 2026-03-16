using System;
using UnityEngine.EventSystems;

namespace Wotc.Mtga.DuelScene.Interactions;

public class AvatarInput : IDisposable
{
	private readonly IAvatarView _avatar;

	private readonly ClickAndHoldButton _input;

	public event Action<IAvatarView> Clicked;

	public event Action<IAvatarView> PointerDown;

	public event Action<IAvatarView> PointerEnter;

	public event Action<IAvatarView> PointerExit;

	public event Action<IAvatarView> LongClicked;

	public AvatarInput(IAvatarView avatar, ClickAndHoldButton input)
	{
		_avatar = avatar;
		_input = input;
		_input.Clicked += OnClick;
		_input.PointerDown += OnPointerDown;
		_input.PointerEnter += OnPointerEnter;
		_input.PointerExit += OnPointerExit;
		_input.ClickAndHold += OnLongClick;
	}

	private void OnClick(PointerEventData eventData)
	{
		this.Clicked?.Invoke(_avatar);
	}

	private void OnPointerDown(PointerEventData eventData)
	{
		this.PointerDown?.Invoke(_avatar);
	}

	private void OnPointerEnter(PointerEventData eventData)
	{
		this.PointerEnter?.Invoke(_avatar);
	}

	private void OnPointerExit(PointerEventData eventData)
	{
		this.PointerExit?.Invoke(_avatar);
	}

	private void OnLongClick()
	{
		this.LongClicked?.Invoke(_avatar);
	}

	public void Dispose()
	{
		if ((bool)_input)
		{
			_input.Clicked -= OnClick;
			_input.PointerDown -= OnPointerDown;
			_input.PointerEnter -= OnPointerEnter;
			_input.PointerExit -= OnPointerExit;
			_input.ClickAndHold -= OnLongClick;
		}
		this.Clicked = null;
		this.PointerDown = null;
		this.PointerEnter = null;
		this.PointerExit = null;
		this.LongClicked = null;
	}
}
