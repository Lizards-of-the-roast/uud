using System;
using UnityEngine;

namespace Wotc.Mtga.Wrapper.PacketSelect;

[RequireComponent(typeof(JumpStartPacket))]
[RequireComponent(typeof(CustomTouchButton))]
public class PacketInput : MonoBehaviour
{
	private JumpStartPacket _pack;

	private CustomTouchButton _touchButton;

	public Action<JumpStartPacket> Clicked { get; set; }

	public Action<JumpStartPacket> DoubleClicked { get; set; }

	public Action<JumpStartPacket> BeginClickAndHold { get; set; }

	public Action<JumpStartPacket> EndClickAndHold { get; set; }

	public Action<JumpStartPacket> MouseEntered { get; set; }

	public Action<JumpStartPacket> MouseExit { get; set; }

	private void Awake()
	{
		_pack = GetComponent<JumpStartPacket>();
		_touchButton = GetComponent<CustomTouchButton>();
		_touchButton.OnClick.AddListener(OnClick);
		_touchButton.OnDoubleClick.AddListener(OnDoubleClick);
		_touchButton.OnClickAndHold.AddListener(OnBeginClickAndHold);
		_touchButton.OnClickAndHoldEnd.AddListener(OnEndClickAndHold);
		_touchButton.OnMouseOver.AddListener(OnMouseEnter);
		_touchButton.OnMouseOff.AddListener(OnMouseExit);
	}

	private void OnClick()
	{
		Clicked?.Invoke(_pack);
	}

	private void OnDoubleClick()
	{
		DoubleClicked?.Invoke(_pack);
	}

	private void OnBeginClickAndHold()
	{
		BeginClickAndHold?.Invoke(_pack);
	}

	private void OnEndClickAndHold()
	{
		EndClickAndHold?.Invoke(_pack);
	}

	private void OnMouseEnter()
	{
		MouseEntered?.Invoke(_pack);
	}

	private void OnMouseExit()
	{
		MouseExit?.Invoke(_pack);
	}

	public void ResetInput()
	{
		Clicked = null;
		DoubleClicked = null;
		BeginClickAndHold = null;
		EndClickAndHold = null;
		MouseEntered = null;
		MouseExit = null;
	}

	private void OnDestroy()
	{
		ResetInput();
		_touchButton.OnClick.RemoveListener(OnClick);
		_touchButton.OnDoubleClick.RemoveListener(OnDoubleClick);
		_touchButton.OnClickAndHold.RemoveListener(OnBeginClickAndHold);
		_touchButton.OnClickAndHoldEnd.RemoveListener(OnEndClickAndHold);
		_touchButton.OnMouseOver.RemoveListener(OnMouseEnter);
		_touchButton.OnMouseOff.RemoveListener(OnMouseExit);
		_pack = null;
		_touchButton = null;
	}
}
