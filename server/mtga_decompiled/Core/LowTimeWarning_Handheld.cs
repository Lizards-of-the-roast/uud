using System.Collections;
using UnityEngine;

public class LowTimeWarning_Handheld : LowTimeWarning
{
	[SerializeField]
	private float _expandMoveDistance = 0.5f;

	[SerializeField]
	private float _expandMoveDuration = 0.5f;

	[SerializeField]
	private CanvasGroup _hourGlassCanvasGroup;

	[SerializeField]
	private bool _isLocalPlayer;

	private RectTransform _rootObjRect;

	private RectTransform _hourglassRect;

	private CanvasGroup _timeoutPipsCanvasGroup;

	private Vector2 _collapsedRootPosition = Vector2.zero;

	private Vector2 _collapsedHourGlassPosition = Vector2.zero;

	private Vector2 _expandedRootPosition = Vector2.zero;

	private Vector2 _expandedHourGlassPosition = Vector2.zero;

	protected override void Awake()
	{
		base.Awake();
		_rootObjRect = _rootObj.GetComponent<RectTransform>();
		_hourglassRect = _hourGlassCanvasGroup.GetComponent<RectTransform>();
		_timeoutPipsCanvasGroup = _timeoutPipRoot.GetComponent<CanvasGroup>();
		_collapsedRootPosition = _rootObjRect.anchoredPosition;
		_collapsedHourGlassPosition = _hourglassRect.anchoredPosition;
		float num = (_isLocalPlayer ? _expandMoveDistance : (-1f * _expandMoveDistance));
		_expandedRootPosition = _collapsedRootPosition + new Vector2(0f, num);
		_expandedHourGlassPosition = _collapsedHourGlassPosition + new Vector2(0f, num / _rootObjRect.localScale.y);
	}

	public IEnumerator AdjustPosition(bool isExpanding)
	{
		float elapsedTime = 0f;
		Vector2 startingPosition = _rootObjRect.anchoredPosition;
		Vector2 targetPosition = (isExpanding ? _expandedRootPosition : _collapsedRootPosition);
		Vector2 startingPositionHourglass = _hourglassRect.anchoredPosition;
		Vector2 targetPositionHourglass = (isExpanding ? _expandedHourGlassPosition : _collapsedHourGlassPosition);
		while (elapsedTime < _expandMoveDuration)
		{
			_rootObjRect.anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, elapsedTime / _expandMoveDuration);
			_hourglassRect.anchoredPosition = Vector2.Lerp(startingPositionHourglass, targetPositionHourglass, elapsedTime / _expandMoveDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		_rootObjRect.anchoredPosition = targetPosition;
		_hourglassRect.anchoredPosition = targetPositionHourglass;
	}

	public void ToggleHourGlass(bool isEnabled)
	{
		_hourGlassCanvasGroup.alpha = (isEnabled ? 1 : 0);
		_timeoutPipsCanvasGroup.alpha = (isEnabled ? 1 : 0);
	}
}
