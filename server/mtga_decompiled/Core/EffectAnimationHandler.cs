using UnityEngine;

public class EffectAnimationHandler : MonoBehaviour, ICustomButtonAnimationHandler
{
	[SerializeField]
	private ParticleSystem _notPressed;

	[SerializeField]
	private ParticleSystem _pressed;

	public void BeginDisabled()
	{
		if (_notPressed != null && _notPressed.gameObject.activeSelf)
		{
			_notPressed.gameObject.SetActive(value: false);
		}
		if (_pressed != null && _pressed.gameObject.activeSelf)
		{
			_pressed.gameObject.SetActive(value: false);
		}
	}

	public void BeginDisabled(float duration)
	{
		BeginDisabled();
	}

	public void BeginMouseOff()
	{
		if (_notPressed != null && !_notPressed.gameObject.activeSelf)
		{
			_notPressed.gameObject.SetActive(value: true);
		}
		if (_pressed != null && _pressed.gameObject.activeSelf)
		{
			_pressed.gameObject.SetActive(value: false);
		}
	}

	public void BeginMouseOff(float duration)
	{
		BeginMouseOff();
	}

	public void BeginMouseOver()
	{
	}

	public void BeginMouseOver(float duration)
	{
	}

	public void BeginPressedOver()
	{
		if (_notPressed != null && _notPressed.gameObject.activeSelf)
		{
			_notPressed.gameObject.SetActive(value: false);
		}
		if (_pressed != null && !_pressed.gameObject.activeSelf)
		{
			_pressed.gameObject.SetActive(value: true);
		}
	}

	public void BeginPressedOver(float duration)
	{
		BeginPressedOver();
	}

	public void BeginPressedOff()
	{
		BeginMouseOff();
	}

	public void BeginPressedOff(float duration)
	{
		BeginMouseOff();
	}
}
