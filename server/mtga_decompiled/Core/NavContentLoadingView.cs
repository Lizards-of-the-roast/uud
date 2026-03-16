using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class NavContentLoadingView : MonoBehaviour
{
	public Graphic LoadingGraphic;

	public CanvasGroup ContentCanvasGroup;

	public void ShowLoading()
	{
		DOTween.Kill(this);
		ContentCanvasGroup.UpdateActive(active: false);
		LoadingGraphic.color = new Color(1f, 1f, 1f, 0f);
		LoadingGraphic.DOFade(1f, 2f).SetEase(Ease.InOutSine).SetDelay(0.5f)
			.SetTarget(this)
			.OnComplete(delegate
			{
				Sequence sequence = DOTween.Sequence();
				sequence.Append(LoadingGraphic.DOFade(0.5f, 1f).SetEase(Ease.InCubic));
				sequence.Append(LoadingGraphic.DOFade(1f, 1f).SetEase(Ease.OutCubic));
				sequence.SetTarget(this);
				sequence.SetLoops(-1);
			});
	}

	public void ShowContent()
	{
		DOTween.Kill(this);
		ContentCanvasGroup.DOFade(1f, 0.1f).SetEase(Ease.InOutSine).SetTarget(this)
			.OnComplete(delegate
			{
				CanvasGroup contentCanvasGroup = ContentCanvasGroup;
				bool interactable = (ContentCanvasGroup.blocksRaycasts = true);
				contentCanvasGroup.interactable = interactable;
			});
		LoadingGraphic.DOFade(0f, 0.1f).SetEase(Ease.InOutSine).SetTarget(this);
	}
}
