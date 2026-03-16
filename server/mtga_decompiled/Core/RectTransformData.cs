using UnityEngine;

public class RectTransformData
{
	private Vector3 localPosition;

	private Quaternion localRotation;

	private Vector3 localScale;

	private Vector2 anchoredPosition;

	private Vector2 anchorMax;

	private Vector2 anchorMin;

	private Vector2 offsetMin;

	private Vector2 offsetMax;

	private Vector2 pivot;

	private Vector2 sizeDelta;

	public RectTransformData(RectTransform source)
	{
		localPosition = source.localPosition;
		localRotation = source.localRotation;
		localScale = source.localScale;
		anchorMax = source.anchorMax;
		anchorMin = source.anchorMin;
		offsetMin = source.offsetMin;
		offsetMax = source.offsetMax;
		pivot = source.pivot;
		sizeDelta = source.sizeDelta;
	}

	public void CopyTo(RectTransform target)
	{
		target.localPosition = localPosition;
		target.localRotation = localRotation;
		target.localScale = localScale;
		target.anchorMax = anchorMax;
		target.anchorMin = anchorMin;
		target.offsetMin = offsetMin;
		target.offsetMax = offsetMax;
		target.pivot = pivot;
		target.sizeDelta = sizeDelta;
	}
}
