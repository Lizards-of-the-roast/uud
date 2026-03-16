using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ScrollFade : MonoBehaviour
{
	public enum FadeDirection
	{
		FadeOut,
		FadeIn
	}

	[SerializeField]
	[FormerlySerializedAs("_scrollView")]
	public ScrollRect ScrollView;

	[SerializeField]
	[MinMaxSlider(0f, 1f)]
	private Vector2 _fadeRange = new Vector2(0.5f, 1f);

	[SerializeField]
	private FadeDirection _fadeDirection;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Raycast/interactable snaps off as long as alpha is this value or lower.")]
	private float _raycastThreshold;

	[SerializeField]
	[Tooltip("Looks for a Graphic and/or CanvasGroup here. If null, looks on this object instead.")]
	private GameObject _fadeObject;

	private Graphic _fadeGraphic;

	private CanvasGroup _fadeCanvasGroup;

	private void Update()
	{
		if (_fadeGraphic == null && _fadeCanvasGroup == null)
		{
			if (_fadeObject == null)
			{
				_fadeObject = base.gameObject;
			}
			_fadeGraphic = _fadeObject.GetComponent<Graphic>();
			_fadeCanvasGroup = _fadeObject.GetComponent<CanvasGroup>();
			if (_fadeGraphic == null && _fadeCanvasGroup == null)
			{
				return;
			}
		}
		if (ScrollView == null)
		{
			return;
		}
		float num;
		if (ScrollView.vertical)
		{
			num = ((ScrollView.viewport.rect.height > 0f) ? (ScrollView.content.localPosition.y / ScrollView.viewport.rect.height) : 0f);
		}
		else
		{
			if (!ScrollView.horizontal)
			{
				return;
			}
			num = ((ScrollView.viewport.rect.width > 0f) ? (ScrollView.content.localPosition.x / ScrollView.viewport.rect.width) : 0f);
		}
		float num2 = ((num >= _fadeRange.y) ? 1f : ((!(num >= _fadeRange.x)) ? 0f : ((num - _fadeRange.x) / (_fadeRange.y - _fadeRange.x))));
		if (_fadeDirection == FadeDirection.FadeOut)
		{
			num2 = 1f - num2;
		}
		bool flag = ((_raycastThreshold >= 1f) ? (num2 >= _raycastThreshold) : (num2 > _raycastThreshold));
		if (_fadeGraphic != null)
		{
			Color color = _fadeGraphic.color;
			color.a = num2;
			_fadeGraphic.color = color;
			_fadeGraphic.raycastTarget = flag;
		}
		if (_fadeCanvasGroup != null)
		{
			_fadeCanvasGroup.alpha = num2;
			_fadeCanvasGroup.blocksRaycasts = flag;
			_fadeCanvasGroup.interactable = flag;
		}
	}
}
