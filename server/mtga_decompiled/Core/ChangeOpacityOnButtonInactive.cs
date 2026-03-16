using UnityEngine;
using UnityEngine.UI;

public class ChangeOpacityOnButtonInactive : MonoBehaviour
{
	public Button AttachedButton;

	public float OpacityOnInactive = 0.5f;

	private float _oldOpacity = 1f;

	private bool _wasInteractable;

	public void Awake()
	{
		if (!AttachedButton.interactable)
		{
			SetToOpacity();
		}
		_wasInteractable = AttachedButton.interactable;
	}

	private void SetToOpacity()
	{
		Image componentInChildren = AttachedButton.GetComponentInChildren<Image>();
		Color color = componentInChildren.color;
		_oldOpacity = color.a;
		color.a = OpacityOnInactive;
		componentInChildren.color = color;
	}

	private void ResetOpacity()
	{
		Image componentInChildren = AttachedButton.GetComponentInChildren<Image>();
		Color color = componentInChildren.color;
		color.a = _oldOpacity;
		componentInChildren.color = color;
	}

	public void Update()
	{
		if (_wasInteractable != AttachedButton.interactable)
		{
			if (_wasInteractable)
			{
				SetToOpacity();
			}
			else
			{
				ResetOpacity();
			}
			_wasInteractable = AttachedButton.interactable;
		}
	}
}
