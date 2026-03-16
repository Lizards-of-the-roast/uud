using UnityEngine;
using UnityEngine.UI;

public class ToggleImageAnimationHandler : MonoBehaviour, ICustomToggleAnimationHandler
{
	[SerializeField]
	private Image _target;

	[SerializeField]
	private Sprite _false;

	[SerializeField]
	private Sprite _true;

	public void BeginFalse()
	{
		_target.sprite = _false;
	}

	public void BeginFalse(float duration)
	{
		_target.sprite = _false;
	}

	public void BeginTrue()
	{
		_target.sprite = _true;
	}

	public void BeginTrue(float duration)
	{
		_target.sprite = _true;
	}
}
