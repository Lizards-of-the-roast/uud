using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollWheelInputForScrollbar : MonoBehaviour
{
	[SerializeField]
	private float deltaMultiplier = 0.25f;

	[SerializeField]
	private Scrollbar scrollbar;

	public int ElementCount;

	public int ElementsFeatured;

	private void Update()
	{
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > float.Epsilon || axis < -1E-45f)
		{
			axis = ((ElementCount >= 1 && ElementsFeatured >= 1 && ElementCount - ElementsFeatured >= 1) ? ((float)Math.Sign(axis) / (float)(ElementCount - ElementsFeatured)) : (axis * deltaMultiplier));
			scrollbar.value = Mathf.Clamp01(scrollbar.value + axis);
		}
	}
}
