using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class View_NPE_TextWidget : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TMP_Text _text;

	private CanvasGroup _canvasGroup;

	private GameManager _gameManager;

	public virtual string Text => _text.text;

	public bool Active => _canvasGroup.interactable;

	public event Action<string> ClickCallback;

	public void Init(GameManager gameManager)
	{
		_gameManager = gameManager;
	}

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnDestroy()
	{
		_gameManager = null;
		_canvasGroup = null;
	}

	public virtual void Show(string text)
	{
		if (_canvasGroup == null)
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}
		_canvasGroup.DOKill();
		_canvasGroup.DOFade(1f, 0.6f);
		if (!string.IsNullOrEmpty(text))
		{
			_text.text = text;
		}
		_canvasGroup.interactable = true;
		_canvasGroup.blocksRaycasts = true;
	}

	public virtual void Hide(bool immediate = false)
	{
		if (_canvasGroup == null)
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}
		float duration = (immediate ? 0f : 0.4f);
		_canvasGroup.DOKill();
		_canvasGroup.DOFade(0f, duration);
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Hide();
		this.ClickCallback?.Invoke(Text);
		_gameManager?.NpeDirector?.ClearNPEUXPrompts();
	}
}
