using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Loc;

public class ChatBubble : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Localize _locText;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Animator _animator;

	private Coroutine followCoroutine;

	public bool Showing { get; private set; }

	public event Action Clicked;

	public void Show(MTGALocalizedString text, bool followCard = false, DuelScene_CDC speakingCard = null, Camera mainCamera = null)
	{
		_locText.SetText(text);
		if (!Showing)
		{
			Showing = true;
			_canvasGroup.interactable = true;
			_canvasGroup.blocksRaycasts = true;
			base.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		}
		if (followCard && (object)mainCamera != null && (object)speakingCard != null)
		{
			if (followCoroutine != null)
			{
				StopCoroutine(followCoroutine);
			}
			followCoroutine = StartCoroutine(FollowCard(speakingCard, mainCamera));
		}
	}

	public void Hide()
	{
		StopAllCoroutines();
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
		if (base.gameObject.activeInHierarchy)
		{
			_animator.SetTrigger("Outro");
		}
		Showing = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Hide();
		this.Clicked?.Invoke();
	}

	private void OnDisable()
	{
		base.gameObject.SetActive(value: false);
		Showing = false;
	}

	private IEnumerator FollowCard(DuelScene_CDC speakingCard, Camera mainCamera)
	{
		while (speakingCard != null)
		{
			base.transform.position = mainCamera.WorldToScreenPoint(speakingCard.transform.position);
			yield return null;
		}
	}
}
