using UnityEngine;
using UnityEngine.UI;

public class ImageAnimationHandler : MonoBehaviour, ICustomButtonAnimationHandler
{
	[SerializeField]
	private Image _target;

	[SerializeField]
	private Sprite _disabled;

	[SerializeField]
	private Sprite _mouseOff;

	[SerializeField]
	private Sprite _mouseOver;

	[SerializeField]
	private Sprite _pressedOver;

	[SerializeField]
	private Sprite _pressedOff;

	private void Awake()
	{
		if (_target == null)
		{
			_target = GetComponentInChildren<Image>();
		}
		if (_target == null)
		{
			Debug.LogError("Image Animation Handler missing an image reference", base.gameObject);
		}
	}

	public void BeginDisabled()
	{
		if (_target != null)
		{
			_target.sprite = _disabled;
		}
	}

	public void BeginDisabled(float duration)
	{
		if (_target != null)
		{
			_target.sprite = _disabled;
		}
	}

	public void BeginMouseOff()
	{
		if (_target != null)
		{
			_target.sprite = _mouseOff;
		}
	}

	public void BeginMouseOff(float duration)
	{
		if (_target != null)
		{
			_target.sprite = _mouseOff;
		}
	}

	public void BeginMouseOver()
	{
		if (_target != null)
		{
			_target.sprite = _mouseOver;
		}
	}

	public void BeginMouseOver(float duration)
	{
		if (_target != null)
		{
			_target.sprite = _mouseOver;
		}
	}

	public void BeginPressedOver()
	{
		if (_target != null)
		{
			_target.sprite = _pressedOver;
		}
	}

	public void BeginPressedOver(float duration)
	{
		if (_target != null)
		{
			_target.sprite = _pressedOver;
		}
	}

	public void BeginPressedOff()
	{
		if (_target != null)
		{
			_target.sprite = _pressedOff;
		}
	}

	public void BeginPressedOff(float duration)
	{
		if (_target != null)
		{
			_target.sprite = _pressedOff;
		}
	}
}
