using System.Collections.Generic;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class RevealCardHolder : CardHolderBase, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private const float SECONDS_DEFAULT = 10f;

	[SerializeField]
	private float _fadeSpeed = 2f;

	[SerializeField]
	private float _cardSpacing = 0.5f;

	[SerializeField]
	private int _maxVisibleCards = 3;

	[SerializeField]
	private CanvasGroup _panelGroup;

	[SerializeField]
	private Image _panelBackground;

	[SerializeField]
	private Button _viewButton;

	[SerializeField]
	private CanvasGroup _viewButtonGroup;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private CanvasGroup _closeButtonGroup;

	[SerializeField]
	private Transform _cardRoot;

	private bool _shouldButtonsBeVisible;

	private float _remainingTimer;

	private float _startingTimer;

	private ViewDismissBrowserProvider _currentBrowserProvider;

	private ICardMovementController _cardMovementController = NullCardMovementController.Default;

	public override Transform CardRoot => _cardRoot;

	public override Transform EffectsRoot => _cardRoot;

	private void Awake()
	{
		base.Layout = new CardLayout_HorizontalAligned
		{
			Spacing = new Vector3(0f - _cardSpacing, 0f, 0.1f)
		};
	}

	private void Start()
	{
		_panelGroup.alpha = 0f;
		_viewButtonGroup.alpha = 0f;
		_closeButtonGroup.alpha = 1f;
		_viewButton.onClick.AddListener(SwitchToBrowser);
		_closeButton.onClick.AddListener(CloseWindow);
		base.gameObject.UpdateActive(active: false);
	}

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		IContext context = gameManager.Context;
		_cardMovementController = context.Get<ICardMovementController>() ?? NullCardMovementController.Default;
	}

	private void Update()
	{
		if (_remainingTimer > 0f)
		{
			_remainingTimer -= Time.deltaTime;
			if (_remainingTimer <= 0f)
			{
				CloseWindow();
				return;
			}
		}
		if ((bool)_panelBackground && _startingTimer > 0f)
		{
			_panelBackground.fillAmount = Mathf.Clamp01(_remainingTimer / _startingTimer);
		}
		float num = Time.deltaTime * _fadeSpeed;
		if (_currentBrowserProvider != null)
		{
			if (_panelGroup.alpha > 0f)
			{
				_panelGroup.alpha -= num;
			}
		}
		else if (_panelGroup.alpha < 1f)
		{
			_panelGroup.alpha += num;
		}
		if (_shouldButtonsBeVisible)
		{
			_viewButtonGroup.alpha = 1f;
			_closeButtonGroup.alpha = 1f;
		}
		else
		{
			_viewButtonGroup.alpha = Mathf.Max(0f, _viewButtonGroup.alpha - num);
			_closeButtonGroup.alpha = Mathf.Max(0f, _closeButtonGroup.alpha - num);
		}
	}

	protected override void OnDestroy()
	{
		_cardMovementController = NullCardMovementController.Default;
		if ((bool)_viewButton)
		{
			_viewButton.onClick?.RemoveAllListeners();
		}
		if ((bool)_closeButton)
		{
			_closeButton.onClick?.RemoveAllListeners();
		}
		base.OnDestroy();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_shouldButtonsBeVisible = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_shouldButtonsBeVisible = false;
	}

	protected override bool CalcCardVisibility(CardLayoutData data, int indexInList)
	{
		return indexInList < _maxVisibleCards;
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		base.HandleAddedCard(cardView);
		if (cardView.PreviousCardHolder != null && cardView.PreviousCardHolder.CardHolderType != CardHolderType.CardBrowserViewDismiss)
		{
			_cardViews.Remove(cardView);
			_cardViews.Insert(0, cardView);
			_remainingTimer = (_startingTimer = 10f);
		}
		OpenWindow();
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		cardView.Collider.enabled = true;
		cardView.Collider.gameObject.UpdateActive(active: true);
		base.RemoveCard(cardView);
	}

	protected override void OnPreLayout()
	{
		base.OnPreLayout();
		Vector3 localPosition = _cardRoot.localPosition;
		localPosition.x = _cardSpacing * (float)(Mathf.Min(_maxVisibleCards, _cardViews.Count) - 1) * 0.5f;
		_cardRoot.localPosition = localPosition;
		for (int i = 0; i < _cardViews.Count; i++)
		{
			_cardViews[i].Collider.gameObject.UpdateActive(i < _maxVisibleCards);
		}
	}

	protected override void ApplyLayoutData(CardLayoutData data, bool added, bool shouldBeVisible, bool moveInstantly = false)
	{
		moveInstantly = moveInstantly || !shouldBeVisible;
		ICardHolder currentCardHolder = data.Card.CurrentCardHolder;
		if (currentCardHolder == null || currentCardHolder.CardHolderType != CardHolderType.CardBrowserViewDismiss)
		{
			data.Card.Root.ZeroOut();
			data.Card.Root.localScale = Vector3.one * 0.1f;
		}
		base.ApplyLayoutData(data, added, shouldBeVisible, moveInstantly);
	}

	public void SwitchToBrowser()
	{
		_currentBrowserProvider = new ViewDismissBrowserProvider(new List<DuelScene_CDC>(_cardViews), null, Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Revealed_Card_Title"), null, OnBrowserPreDismiss);
		IBrowser openedBrowser = _gameManager.BrowserManager.OpenBrowser(_currentBrowserProvider);
		_currentBrowserProvider.SetOpenedBrowser(openedBrowser);
		_cardViews.Clear();
	}

	private void OnBrowserPreDismiss()
	{
		if (_currentBrowserProvider?.OpenedBrowser != null && base.gameObject.activeSelf)
		{
			foreach (DuelScene_CDC cardView in _currentBrowserProvider.OpenedBrowser.GetCardViews())
			{
				_cardMovementController.MoveCard(cardView, this);
			}
			_currentBrowserProvider.OpenedBrowser.GetCardViews().Clear();
		}
		_currentBrowserProvider = null;
	}

	public void OpenWindow()
	{
		base.gameObject.UpdateActive(active: true);
	}

	public void CloseWindow()
	{
		while (_cardViews.Count > 0)
		{
			DuelScene_CDC duelScene_CDC = _cardViews[0];
			RemoveCard(duelScene_CDC);
			_cardViewBuilder.DestroyCDC(duelScene_CDC);
		}
		base.gameObject.UpdateActive(active: false);
	}
}
