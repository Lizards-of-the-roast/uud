using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code;
using DG.Tweening;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftPackHolder : MetaCardHolder
{
	[Header("Draft Pack Holder Parameters")]
	[SerializeField]
	private DraftPackCardView _cardPrefab;

	[SerializeField]
	private Transform _cardViewPoolParent;

	[SerializeField]
	private RectTransform _columnCardViewParent;

	[SerializeField]
	private RectTransform _listCardViewParent;

	[SerializeField]
	private PackAnimationData _packAnimationData;

	[SerializeField]
	private bool _canDragCards = true;

	[Header("List View Layout Data")]
	[SerializeField]
	private DraftPackLayoutData[] _listLayoutDatas;

	[Header("List View Layout Data")]
	[SerializeField]
	private DraftPackLayoutData[] _columnLayoutDatas;

	private bool _isInitialized;

	private Camera _camera;

	private RectTransform _currentCardViewParent;

	private DraftPackLayoutData _currentPackLayoutData;

	private readonly List<DraftPackCardView> _allCardViews = new List<DraftPackCardView>();

	private readonly Queue<DraftPackCardView> _cardViewPool = new Queue<DraftPackCardView>();

	private CardCollection _packCollection;

	private CardCollection _pendingPackCollection;

	private DraftPackCardView _lastCardClicked;

	private bool _SortPackCollection;

	public bool AnimateClockwise = true;

	private IEnumerator _setCardsInternalCoroutine;

	private GlobalCoroutineExecutor _globalCoroutineExecutor;

	public new Action<MetaCardView> OnCardClicked;

	public new Action<MetaCardView> OnCardDragged;

	public new Action<MetaCardView> OnEndCardDragged;

	[SerializeField]
	private float _constantScreenWidth;

	public Action OnAnimatingCardsInStarted;

	public Action OnAnimatingCardsOutFinished;

	public List<DraftPackCardView> CardViews => _allCardViews;

	public bool IsAnimating { get; private set; }

	private void Update()
	{
		if (_isInitialized)
		{
			AspectRatio aspectRatio = _camera.GetAspectRatio();
			if (_currentPackLayoutData.AspectRatio != aspectRatio)
			{
				SetCardLayout(isColumnView: false, aspectRatio);
			}
		}
	}

	public void Init(Camera camera, ICardRolloverZoom rolloverZoom, DraftContentController draftController, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_isInitialized = true;
		_camera = camera;
		_rolloverZoomView = rolloverZoom;
		_SortPackCollection = true;
		EnsureInit(cardDatabase, cardViewBuilder);
		base.CanDragCards = delegate(MetaCardView cardView)
		{
			if (_canDragCards)
			{
				DraftPackCardView obj = cardView as DraftPackCardView;
				if ((object)obj == null)
				{
					return true;
				}
				return !obj.UseButtonOverlay;
			}
			return false;
		};
		base.CanSingleClickCards = (MetaCardView cardView) => true;
		base.CanDoubleClickCards = (MetaCardView cardView) => false;
		base.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(base.OnCardClicked, new Action<MetaCardView>(HandleOnCardClicked));
		base.OnCardDragged = (Action<MetaCardView>)Delegate.Combine(base.OnCardDragged, new Action<MetaCardView>(HandleOnCardDragged));
		base.OnEndCardDragged = (Action<MetaCardView>)Delegate.Combine(base.OnEndCardDragged, new Action<MetaCardView>(HandleOnEndCardDragged));
		draftController.OnCardPicked = (Action<List<DraftPackCardView>>)Delegate.Combine(draftController.OnCardPicked, new Action<List<DraftPackCardView>>(HandleOnCardsPicked));
	}

	public IEnumerator Coroutine_SetCards(CardCollection packCollection, bool sortPackCollection, Action onComplete)
	{
		_pendingPackCollection = packCollection;
		_SortPackCollection = sortPackCollection;
		yield return _setCardsInternalCoroutine ?? (_setCardsInternalCoroutine = SetCards_internal());
		onComplete?.Invoke();
	}

	private void Awake()
	{
		_globalCoroutineExecutor = Pantry.Get<GlobalCoroutineExecutor>();
	}

	private void OnEnable()
	{
		if (_pendingPackCollection != null)
		{
			_globalCoroutineExecutor.StartGlobalCoroutine(_setCardsInternalCoroutine ?? (_setCardsInternalCoroutine = SetCards_internal()));
		}
	}

	private IEnumerator SetCards_internal()
	{
		yield return WaitUntilDoneAnimating();
		ReleaseAllDraggingCards();
		if (_packCollection != null)
		{
			AnimatePackOut();
			yield return WaitUntilDoneAnimating();
		}
		_packCollection = _pendingPackCollection;
		CardCollection cardCollection = _packCollection;
		if (_SortPackCollection)
		{
			cardCollection = CardSorter.Sort(_packCollection, base.CardDatabase, SortTypeFilters.DraftPack);
		}
		foreach (ICardCollectionItem item in cardCollection)
		{
			for (int i = 0; i < item.Quantity; i++)
			{
				CreateCard(item);
			}
		}
		_pendingPackCollection = null;
		LayoutCards(_currentPackLayoutData.UpperLeft, _currentPackLayoutData.Scale, _currentPackLayoutData.Offset, _currentPackLayoutData.ColumnCount, _currentPackLayoutData.RowCount);
		AnimatePackIn();
		yield return WaitUntilDoneAnimating();
		_setCardsInternalCoroutine = null;
	}

	public override void ClearCards()
	{
		foreach (DraftPackCardView allCardView in _allCardViews)
		{
			allCardView.IgnoreRepositionNextEndDrag = true;
			allCardView.transform.SetParent(_cardViewPoolParent, worldPositionStays: false);
			_cardViewPool.Enqueue(allCardView);
			allCardView.ActivateUndoButton(activate: false);
		}
		_allCardViews.Clear();
		_packCollection = null;
	}

	public List<DraftPackCardView> GetAllCardViews()
	{
		return new List<DraftPackCardView>(_allCardViews);
	}

	private DraftPackCardView CreateCard(ICardCollectionItem collectionItem)
	{
		Transform parent = ((_currentCardViewParent != null) ? _currentCardViewParent : base.transform);
		DraftPackCardView draftPackCardView = ((_cardViewPool.Count > 0) ? _cardViewPool.Dequeue() : UnityEngine.Object.Instantiate(_cardPrefab, parent));
		draftPackCardView.Holder = this;
		draftPackCardView.transform.SetParent(parent);
		draftPackCardView.transform.ZeroOut();
		draftPackCardView.Init(base.CardDatabase, base.CardViewBuilder);
		draftPackCardView.SetDataIncludingBans(collectionItem);
		_allCardViews.Add(draftPackCardView);
		return draftPackCardView;
	}

	private void HandleOnCardsPicked(IReadOnlyList<MetaCardView> cardViews)
	{
		AnimatePackOut();
		CDCMetaCardView cDCMetaCardView = cardViews.OfType<CDCMetaCardView>().FirstOrDefault();
		if (cDCMetaCardView != null)
		{
			AudioManager.Instance.PlayAudio_BoosterETB(cDCMetaCardView.CardView);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_add_card, base.gameObject);
	}

	private void HandleOnCardClicked(MetaCardView cardView)
	{
		_lastCardClicked = cardView as DraftPackCardView;
		OnCardClicked?.Invoke(_lastCardClicked);
	}

	private void HandleOnCardDragged(MetaCardView cardView)
	{
		if (_lastCardClicked != null)
		{
			_lastCardClicked.ActivateFirstTag(isFirst: false);
		}
		OnCardDragged?.Invoke(cardView);
	}

	private void HandleOnEndCardDragged(MetaCardView cardView)
	{
		OnEndCardDragged?.Invoke(cardView);
	}

	public void SetCardLayout(bool isColumnView, AspectRatio layoutData = AspectRatio.Invalid)
	{
		if (layoutData == AspectRatio.Invalid)
		{
			layoutData = ((_currentPackLayoutData.AspectRatio == AspectRatio.Invalid) ? _camera.GetAspectRatio() : _currentPackLayoutData.AspectRatio);
		}
		if (isColumnView)
		{
			for (int i = 0; i < _columnLayoutDatas.Length; i++)
			{
				if (_columnLayoutDatas[i].AspectRatio == layoutData)
				{
					_currentPackLayoutData = _columnLayoutDatas[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < _listLayoutDatas.Length; j++)
			{
				if (_listLayoutDatas[j].AspectRatio == layoutData)
				{
					_currentPackLayoutData = _listLayoutDatas[j];
				}
			}
		}
		if ((bool)_columnCardViewParent && (bool)_listCardViewParent)
		{
			if (isColumnView)
			{
				_currentCardViewParent = _columnCardViewParent;
				_columnCardViewParent.transform.parent.gameObject.SetActive(value: true);
				_listCardViewParent.transform.parent.gameObject.SetActive(value: false);
			}
			else
			{
				_currentCardViewParent = _listCardViewParent;
				_columnCardViewParent.transform.parent.gameObject.SetActive(value: false);
				_listCardViewParent.transform.parent.gameObject.SetActive(value: true);
			}
		}
		LayoutCards(_currentPackLayoutData.UpperLeft, _currentPackLayoutData.Scale, _currentPackLayoutData.Offset, _currentPackLayoutData.ColumnCount, _currentPackLayoutData.RowCount);
	}

	private void LayoutCards(Vector3 upperLeft, float scale, Vector3 offset, int columnCount, int rowCount)
	{
		if (rowCount != 0 && (bool)_currentCardViewParent)
		{
			columnCount = (_allCardViews.Count + rowCount - 1) / rowCount;
			RectTransform component = _currentCardViewParent.parent.GetComponent<RectTransform>();
			if (component != null)
			{
				columnCount = Math.Max(Mathf.CeilToInt((Math.Abs(component.rect.width) - Math.Abs(upperLeft.x * 2f)) / offset.x), columnCount);
			}
			_currentCardViewParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, offset.x * (float)(columnCount - 1) + upperLeft.x * 2f);
			_currentCardViewParent.anchoredPosition = _currentCardViewParent.sizeDelta;
		}
		for (int i = 0; i < _allCardViews.Count; i++)
		{
			int num = i % columnCount;
			int num2 = i / columnCount;
			Vector3 vector = upperLeft;
			vector.x += offset.x * (float)num;
			vector.y += offset.y * (float)num2;
			DraftPackCardView draftPackCardView = _allCardViews[i];
			RectTransform component2 = draftPackCardView.GetComponent<RectTransform>();
			if ((bool)_currentCardViewParent)
			{
				draftPackCardView.transform.SetParent(_currentCardViewParent);
			}
			if ((bool)component2)
			{
				component2.anchoredPosition3D = vector;
			}
			else
			{
				draftPackCardView.transform.localPosition = vector;
			}
			draftPackCardView.transform.localScale = Vector3.one * scale;
		}
	}

	private void AnimatePackIn()
	{
		IsAnimating = true;
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_flip_a_loop_speed2_start, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_flip_a_loop_speed2_stop, base.gameObject, 0.25f);
		Sequence sequence = DOTween.Sequence();
		int num = (AnimateClockwise ? 1 : (-1));
		float num2 = Math.Max((_constantScreenWidth != 0f) ? _constantScreenWidth : ((float)Screen.width), _currentCardViewParent ? _currentCardViewParent.sizeDelta.x : 0f) * (float)num * 2f;
		for (int i = 0; i < _allCardViews.Count; i++)
		{
			DraftPackCardView draftPackCardView = _allCardViews[i];
			sequence.Insert(0f, draftPackCardView.transform.DOLocalMoveX(draftPackCardView.transform.localPosition.x + num2, _packAnimationData.CardDuration).SetEase(_packAnimationData.CardEase).SetDelay(_packAnimationData.StaggerDuration * (float)i)
				.From());
			draftPackCardView.Collider.enabled = false;
		}
		OnAnimatingCardsInStarted?.Invoke();
		sequence.OnComplete(AnimatePackIn_OnComplete);
	}

	private void AnimatePackIn_OnComplete()
	{
		IsAnimating = false;
		foreach (DraftPackCardView allCardView in _allCardViews)
		{
			allCardView.Collider.enabled = true;
		}
	}

	private void AnimatePackOut()
	{
		IsAnimating = true;
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_place_whoosh, base.gameObject, 0.25f);
		Sequence sequence = DOTween.Sequence();
		int num = (AnimateClockwise ? 1 : (-1));
		float num2 = Math.Max((_constantScreenWidth != 0f) ? _constantScreenWidth : ((float)Screen.width), _currentCardViewParent ? _currentCardViewParent.sizeDelta.x : 0f) * (float)num * 2f;
		for (int i = 0; i < _allCardViews.Count; i++)
		{
			DraftPackCardView draftPackCardView = _allCardViews[i];
			sequence.Insert(0f, draftPackCardView.transform.DOLocalMoveX(draftPackCardView.transform.localPosition.x - num2, _packAnimationData.CardDuration).SetEase(_packAnimationData.CardEase).SetDelay(_packAnimationData.StaggerDuration * (float)i));
			draftPackCardView.Collider.enabled = false;
		}
		sequence.OnComplete(AnimatePackOut_OnComplete);
	}

	private void AnimatePackOut_OnComplete()
	{
		IsAnimating = false;
		ClearCards();
		OnAnimatingCardsOutFinished?.Invoke();
	}

	private IEnumerator WaitUntilDoneAnimating()
	{
		while (IsAnimating)
		{
			yield return null;
		}
	}
}
