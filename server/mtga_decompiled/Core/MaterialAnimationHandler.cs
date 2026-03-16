using UnityEngine;
using UnityEngine.UI;

public class MaterialAnimationHandler : MonoBehaviour, ICustomButtonAnimationHandler
{
	[SerializeField]
	private Graphic _target;

	[SerializeField]
	private Material _disabled;

	[SerializeField]
	private Material _mouseOff;

	[SerializeField]
	private Material _mouseOver;

	[SerializeField]
	private Material _pressedOver;

	[SerializeField]
	private Material _pressedOff;

	public void BeginDisabled()
	{
		_target.material = _disabled;
	}

	public void BeginDisabled(float duration)
	{
		_target.material = _disabled;
	}

	public void BeginMouseOff()
	{
		_target.material = _mouseOff;
	}

	public void BeginMouseOff(float duration)
	{
		_target.material = _mouseOff;
	}

	public void BeginMouseOver()
	{
		_target.material = _mouseOver;
	}

	public void BeginMouseOver(float duration)
	{
		_target.material = _mouseOver;
	}

	public void BeginPressedOver()
	{
		_target.material = _pressedOver;
	}

	public void BeginPressedOver(float duration)
	{
		_target.material = _pressedOver;
	}

	public void BeginPressedOff()
	{
		_target.material = _pressedOff;
	}

	public void BeginPressedOff(float duration)
	{
		_target.material = _pressedOff;
	}
}
