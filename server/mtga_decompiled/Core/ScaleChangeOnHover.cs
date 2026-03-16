using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScaleChangeOnHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private float EnterSize = 1.05f;

	[SerializeField]
	private float EnterDuration = 0.2f;

	[SerializeField]
	private Ease EnterEase = Ease.InOutSine;

	[SerializeField]
	private float ExitDuration = 0.2f;

	[SerializeField]
	private Ease ExitEase = Ease.InOutSine;

	private Tweener _currentTweener;

	private void Start()
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!base.enabled)
		{
			return;
		}
		Toggle componentInChildren = GetComponentInChildren<Toggle>();
		if (!(componentInChildren != null) || componentInChildren.interactable)
		{
			if (_currentTweener != null)
			{
				_currentTweener.Kill();
			}
			_currentTweener = base.transform.DOScale(EnterSize, EnterDuration).SetEase(EnterEase);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.enabled)
		{
			if (_currentTweener != null)
			{
				_currentTweener.Kill();
			}
			_currentTweener = base.transform.DOScale(1f, ExitDuration).SetEase(ExitEase);
		}
	}
}
