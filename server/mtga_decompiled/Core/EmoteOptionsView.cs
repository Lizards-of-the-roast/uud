using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class EmoteOptionsView : MonoBehaviour
{
	[SerializeField]
	private Animator _emoteOptionsContainerAnimator;

	[SerializeField]
	private Animator _emoteOptionsButtonAnimator;

	[Header("Button References")]
	[SerializeField]
	private Button _nextPageButton;

	[SerializeField]
	private Button _previousPageButton;

	[SerializeField]
	private Button _closeButton;

	[Header("Transform References")]
	[SerializeField]
	private Transform[] _phraseEmoteViewParentsOdd;

	[SerializeField]
	private Transform[] _stickerEmoteViewParentsOdd;

	[SerializeField]
	private Transform[] _phraseEmoteViewParentsEven;

	[SerializeField]
	private Transform[] _stickerEmoteViewParentsEven;

	private readonly int SHOW_OPTIONS_ANIM_FLAG = Animator.StringToHash("shouldBeShowing");

	private readonly int ISMOUSEOVER_ANIM_FLAG = Animator.StringToHash("IsMouseOver");

	private bool _isOpen;

	public int PhraseEmoteViewParentCount => Math.Max(_phraseEmoteViewParentsOdd.Length, _phraseEmoteViewParentsEven.Length);

	public int StickerEmoteViewParentCount => Math.Max(_stickerEmoteViewParentsOdd.Length, _stickerEmoteViewParentsEven.Length);

	public event Action OnNextEmotePageClicked;

	public event Action OnPreviousEmotePageClicked;

	private void Awake()
	{
		if ((bool)_nextPageButton)
		{
			_nextPageButton.onClick.AddListener(_onNextEmotePageClicked);
		}
		if ((bool)_previousPageButton)
		{
			_previousPageButton.onClick.AddListener(_onPreviousEmotePageClicked);
		}
		if ((bool)_closeButton)
		{
			_closeButton.onClick.AddListener(OnCloseClicked);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_nextPageButton)
		{
			_nextPageButton.onClick.RemoveAllListeners();
		}
		if ((bool)_previousPageButton)
		{
			_previousPageButton.onClick.RemoveAllListeners();
		}
		if ((bool)_closeButton)
		{
			_closeButton.onClick.RemoveAllListeners();
		}
	}

	private void _onNextEmotePageClicked()
	{
		this.OnNextEmotePageClicked?.Invoke();
	}

	private void _onPreviousEmotePageClicked()
	{
		this.OnPreviousEmotePageClicked?.Invoke();
	}

	private void OnCloseClicked()
	{
		EventSystem current = EventSystem.current;
		if ((bool)current)
		{
			current.SetSelectedGameObject(null);
		}
	}

	public void SetIsHovered(bool isHovered)
	{
		bool value = !_isOpen && isHovered;
		_emoteOptionsButtonAnimator.SetBool(ISMOUSEOVER_ANIM_FLAG, value);
	}

	public void SetPagesEnabled(bool enabled)
	{
		_nextPageButton.gameObject.SetActive(enabled);
		_previousPageButton.gameObject.SetActive(enabled);
	}

	private static bool IsEven(int n)
	{
		return n % 2 == 0;
	}

	private void SetEmoteViewPositions(List<EmoteView> emoteViews, Transform[] evenParents, Transform[] oddParents)
	{
		Transform[] array = (IsEven(emoteViews.Count) ? evenParents : oddParents);
		for (int i = 0; i < emoteViews.Count && i < array.Length; i++)
		{
			Transform obj = emoteViews[i].transform;
			obj.SetParent(array[i]);
			obj.ZeroOut();
		}
	}

	public void SetPhraseEmoteViewSelections(List<EmoteView> emoteViews)
	{
		SetEmoteViewPositions(emoteViews, _phraseEmoteViewParentsEven, _phraseEmoteViewParentsOdd);
	}

	public void SetStickerEmoteViewSelections(List<EmoteView> emoteViews)
	{
		SetEmoteViewPositions(emoteViews, _stickerEmoteViewParentsEven, _stickerEmoteViewParentsOdd);
	}

	public void Open()
	{
		_isOpen = true;
		_emoteOptionsContainerAnimator.SetBool(SHOW_OPTIONS_ANIM_FLAG, value: true);
		_emoteOptionsButtonAnimator.SetBool(ISMOUSEOVER_ANIM_FLAG, value: false);
	}

	public void Close()
	{
		_isOpen = false;
		_emoteOptionsContainerAnimator.SetBool(SHOW_OPTIONS_ANIM_FLAG, value: false);
		_emoteOptionsButtonAnimator.SetBool(ISMOUSEOVER_ANIM_FLAG, value: false);
	}

	public Transform[] returnStickerTransforms()
	{
		return _stickerEmoteViewParentsOdd;
	}
}
