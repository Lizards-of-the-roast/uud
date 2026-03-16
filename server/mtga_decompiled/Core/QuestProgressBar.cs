using UnityEngine;

public class QuestProgressBar : MonoBehaviour
{
	[Range(0f, 1f)]
	public float Percent;

	public RectTransform ProgressVisual;

	[Tooltip("Number of pixels the bar will be visually filled to when its value is 0.")]
	public float VisualZeroPoint = 12f;

	private float _lastPercent = float.MinValue;

	private void Update()
	{
		Percent = Mathf.Clamp01(Percent);
		if (_lastPercent != Percent)
		{
			_lastPercent = Percent;
			float b = ProgressVisual.rect.width - VisualZeroPoint;
			ProgressVisual.anchoredPosition = new Vector2((Percent - 1f) * Mathf.Max(0f, b), 0f);
		}
	}
}
