using UnityEngine;

public struct RectTransformCopyInfo
{
	private Vector3 position;

	private Quaternion rotation;

	private Vector3 scale;

	private Vector2 anchorMin;

	private Vector2 anchorMax;

	private Vector2 anchoredPosition;

	private Vector2 sizeDelta;

	private Vector2 pivot;

	public static RectTransformCopyInfo FromTransform(RectTransform transform)
	{
		return new RectTransformCopyInfo
		{
			position = transform.localPosition,
			rotation = transform.localRotation,
			scale = transform.localScale,
			anchorMin = transform.anchorMin,
			anchorMax = transform.anchorMax,
			anchoredPosition = transform.anchoredPosition,
			sizeDelta = transform.sizeDelta,
			pivot = transform.pivot
		};
	}

	public void ApplyToTransform(RectTransform transform)
	{
		transform.localPosition = position;
		transform.localRotation = rotation;
		transform.localScale = scale;
		transform.anchorMin = anchorMin;
		transform.anchorMax = anchorMax;
		transform.anchoredPosition = anchoredPosition;
		transform.sizeDelta = sizeDelta;
		transform.pivot = pivot;
	}
}
