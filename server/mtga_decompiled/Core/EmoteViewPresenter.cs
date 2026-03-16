using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Arena.Enums.Cosmetic;
using Wotc.Mtga.Extensions;

public class EmoteViewPresenter : MonoBehaviour
{
	private enum TailLocation
	{
		None,
		Top,
		Bottom
	}

	private EmoteView _displayedEmote;

	private Queue<EmoteView> _emoteViewQueue = new Queue<EmoteView>();

	private Coroutine _emotePresenterCoroutine;

	[SerializeField]
	private float _durationPresentingEmotes = 3f;

	[SerializeField]
	private float _durationBetweenEmotes = 0.5f;

	[SerializeField]
	private Transform _phraseEmoteViewParent;

	[SerializeField]
	private Transform _stickerEmoteViewParent;

	[SerializeField]
	private TailLocation _tailLocation;

	public event Action EmotePresented;

	public void Awake()
	{
		if (_phraseEmoteViewParent == null)
		{
			_phraseEmoteViewParent = base.gameObject.transform;
		}
	}

	public void PresentQueuedEmote(EmoteView emoteView, EmoteData emoteData)
	{
		ParentEmote(emoteView, emoteData);
		emoteView.gameObject.SetActive(value: false);
		_emoteViewQueue.Enqueue(emoteView);
		if (_emotePresenterCoroutine == null)
		{
			_emotePresenterCoroutine = StartCoroutine(Coroutine_PresentEmoteViewQueue());
		}
	}

	public void PresentQueuedEmoteNoData(EmoteView emoteView)
	{
		emoteView.gameObject.SetActive(value: false);
		_emoteViewQueue.Enqueue(emoteView);
		if (_emotePresenterCoroutine == null)
		{
			_emotePresenterCoroutine = StartCoroutine(Coroutine_PresentEmoteViewQueue());
		}
	}

	public void PresentQueuedDialog(EmoteView emoteView, bool pushToFrontOfQueue)
	{
		emoteView.transform.SetParent(_phraseEmoteViewParent);
		emoteView.transform.ZeroOut();
		emoteView.gameObject.SetActive(value: false);
		if (pushToFrontOfQueue)
		{
			Queue<EmoteView> queue = new Queue<EmoteView>();
			queue.Enqueue(emoteView);
			while (_emoteViewQueue.Count > 0)
			{
				queue.Enqueue(_emoteViewQueue.Dequeue());
			}
			_emoteViewQueue = queue;
		}
		else
		{
			_emoteViewQueue.Enqueue(emoteView);
		}
		if (_emotePresenterCoroutine == null)
		{
			_emotePresenterCoroutine = StartCoroutine(Coroutine_PresentEmoteViewQueue());
		}
	}

	public void PresentPermanentEmote(EmoteView emoteView)
	{
		ClearQueue();
		emoteView.transform.SetParent(_phraseEmoteViewParent);
		emoteView.transform.ZeroOut();
		SetDisplayedEmote(emoteView);
	}

	public void SetDisplayedEmoteText(string text)
	{
		_displayedEmote?.SetLocalizedText(text);
	}

	public void ClearDisplayedEmote()
	{
		if (!(_displayedEmote == null))
		{
			UnityEngine.Object.Destroy(_displayedEmote.gameObject);
			_displayedEmote = null;
		}
	}

	private void ParentEmote(EmoteView emoteView, EmoteData emoteData)
	{
		Transform parent = ((emoteData.Entry.Page == EmotePage.Sticker) ? _stickerEmoteViewParent : _phraseEmoteViewParent);
		emoteView.transform.SetParent(parent);
		emoteView.transform.ZeroOut();
	}

	private IEnumerator Coroutine_PresentEmoteViewQueue()
	{
		while (_emoteViewQueue.Count > 0)
		{
			SetDisplayedEmote(_emoteViewQueue.Dequeue());
			this.EmotePresented?.Invoke();
			yield return new WaitForSeconds(_durationPresentingEmotes);
			_displayedEmote.FadeOut();
			yield return new WaitForSeconds(_durationBetweenEmotes);
			ClearDisplayedEmote();
		}
		_emotePresenterCoroutine = null;
	}

	private void SetDisplayedEmote(EmoteView emoteView)
	{
		_displayedEmote = emoteView;
		emoteView.gameObject.SetActive(value: true);
		emoteView.SetDisplayOnly(isDisplayOnly: true, _tailLocation == TailLocation.Top);
		emoteView.FadeIn();
	}

	public void ClearQueue()
	{
		_emoteViewQueue.Clear();
		if (_emotePresenterCoroutine != null)
		{
			StopCoroutine(_emotePresenterCoroutine);
			_emotePresenterCoroutine = null;
			if (!(_displayedEmote == null))
			{
				UnityEngine.Object.Destroy(_displayedEmote.gameObject);
				_displayedEmote = null;
			}
		}
	}
}
