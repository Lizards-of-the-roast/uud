using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class TransformScaling : MonoBehaviour
{
	public enum FitMode
	{
		None,
		Fill,
		Contain,
		Cover
	}

	public bool UpdateAtRuntime;

	public FitMode Fit = FitMode.Fill;

	private RectTransform _rectTransform;

	private RectTransform _parentTransform;

	private void OnEnable()
	{
		_rectTransform = (RectTransform)base.transform;
		_parentTransform = _rectTransform.parent as RectTransform;
		if (_parentTransform == null)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (UpdateAtRuntime || !Application.isPlaying)
		{
			Rect rect = _rectTransform.rect;
			Rect rect2 = _parentTransform.rect;
			float num = rect2.width / rect.width;
			float num2 = rect2.height / rect.height;
			switch (Fit)
			{
			case FitMode.Fill:
				_rectTransform.localScale = new Vector3(num, num2, 1f);
				break;
			case FitMode.Contain:
			{
				float num3 = ((num > num2) ? num2 : num);
				_rectTransform.localScale = new Vector3(num3, num3, num3);
				break;
			}
			case FitMode.Cover:
			{
				float num3 = ((num < num2) ? num2 : num);
				_rectTransform.localScale = new Vector3(num3, num3, num3);
				break;
			}
			}
		}
	}
}
