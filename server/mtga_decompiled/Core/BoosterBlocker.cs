using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;

public class BoosterBlocker : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private GameObject _collider;

	public Action OnClick { get; set; }

	private void Awake()
	{
		UpdateCanvasGroupVisibility(visible: false, updateRaycastsImmediately: true, 0f);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (OnClick != null)
		{
			OnClick();
		}
	}

	public void UpdateCanvasGroupVisibility(bool visible, bool updateRaycastsImmediately, float duration)
	{
		DOTween.Kill(_canvasGroup);
		Tweener t = _canvasGroup.DOFade(visible ? 1f : 0f, duration).SetEase(Ease.InOutSine).SetTarget(_canvasGroup);
		if (updateRaycastsImmediately)
		{
			CanvasGroup canvasGroup = _canvasGroup;
			bool blocksRaycasts = (_canvasGroup.interactable = visible);
			canvasGroup.blocksRaycasts = blocksRaycasts;
			_collider.UpdateActive(visible);
			return;
		}
		t.OnComplete(delegate
		{
			CanvasGroup canvasGroup2 = _canvasGroup;
			bool blocksRaycasts2 = (_canvasGroup.interactable = visible);
			canvasGroup2.blocksRaycasts = blocksRaycasts2;
			_collider.UpdateActive(visible);
		});
	}
}
