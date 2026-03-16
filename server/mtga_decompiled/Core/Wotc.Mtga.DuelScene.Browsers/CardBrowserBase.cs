using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Browser;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public abstract class CardBrowserBase : BrowserBase, ICardBrowser, IBrowser
{
	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	protected readonly ICardBrowserProvider cardBrowserProvider;

	protected readonly CardHolderManager cardHolderManager;

	protected readonly EntityViewManager entityViewManager;

	protected readonly IUnityObjectPool _unityObjectPool;

	protected readonly ISplineMovementSystem _splineMovementSystem;

	protected CardBrowserCardHolder cardHolder;

	protected List<DuelScene_CDC> cardViews = new List<DuelScene_CDC>();

	protected readonly Dictionary<DuelScene_CDC, BrowserCardHeader> cardInfoObjects = new Dictionary<DuelScene_CDC, BrowserCardHeader>();

	private readonly List<GameObject> _instantiatedCardVFX = new List<GameObject>();

	protected CardLayout_ScrollableBrowser scrollableLayout;

	private string _cachedHoverDataKey;

	private BrowserHoverData _cachedHoverdata;

	private Coroutine updateCardHeadersCoroutine;

	protected Dictionary<DuelScene_CDC, CardVFX> _cardVFX;

	public ICardHolder CardHolder => cardHolder;

	public bool ReleasingCards { get; protected set; }

	public bool AllowsHoverInteractions { get; protected set; }

	public DuelSceneBrowserType BrowserType => cardBrowserProvider?.GetBrowserType() ?? DuelSceneBrowserType.Invalid;

	public string CardHolderLayoutKey => cardBrowserProvider?.GetCardHolderLayoutKey() ?? string.Empty;

	public event Action<DuelScene_CDC> CardViewSelectedHandlers;

	public event System.Action PreReleaseCardViewsHandlers;

	public event Action<DuelScene_CDC> ReleaseCardViewHandlers;

	public event Action<DuelScene_CDC> CardViewRemovedHandlers;

	public virtual bool AllowsDragInteractions(DuelScene_CDC cardView)
	{
		return false;
	}

	public CardBrowserBase(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		cardBrowserProvider = provider as ICardBrowserProvider;
		entityViewManager = _gameManager.ViewManager;
		cardHolderManager = _gameManager.CardHolderManager;
		_unityObjectPool = _gameManager.UnityPool;
		_cardBuilder = gameManager.Context.Get<ICardBuilder<DuelScene_CDC>>() ?? NullCardBuilder<DuelScene_CDC>.Default;
		cardHolder = cardHolderManager.DefaultBrowser;
		_splineMovementSystem = _gameManager.SplineMovementSystem;
	}

	public override void Init()
	{
		InitScrollableLayout();
		InitCardHolder();
		SetupCards();
		SetupCardFX();
		cardHolder.LayoutNow();
		base.Init();
	}

	protected virtual void InitScrollableLayout()
	{
		if (cardBrowserProvider != null)
		{
			scrollableLayout = new CardLayout_ScrollableBrowser(GetCardHolderLayoutData(cardBrowserProvider.GetCardHolderLayoutKey()));
		}
	}

	protected virtual void InitCardHolder()
	{
		cardHolder.ApplyControllerOffset = cardBrowserProvider.ApplyControllerOffset;
		cardHolder.ApplyTargetOffset = cardBrowserProvider.ApplyTargetOffset;
		cardHolder.ApplySourceOffset = cardBrowserProvider.ApplySourceOffset;
		cardHolder.Layout = GetCardHolderLayout();
		if (cardHolder.Layout is CardLayout_ScrollableBrowser && scrollableLayout != null && scrollableLayout.IsReversedDisplay)
		{
			scrollableLayout.ScrollPosition = 1f;
		}
		cardHolder.CardRemoved += CardHolder_OnCardRemoved;
		updateCardHeadersCoroutine = cardHolder.StartCoroutine(UpdateCardHeadersCoroutine());
	}

	public override void Close()
	{
		if ((bool)cardHolder)
		{
			cardHolder.CardRemoved -= CardHolder_OnCardRemoved;
			if (updateCardHeadersCoroutine != null)
			{
				cardHolder.StopCoroutine(updateCardHeadersCoroutine);
			}
		}
		if (!base.IsClosed)
		{
			ReleaseCards();
			if ((bool)cardHolder)
			{
				cardHolder.LockCardDetails = false;
			}
			base.Close();
		}
	}

	protected virtual void OnScroll(float scrollValue)
	{
		if (GetCardHolderLayout() is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	protected virtual void ReleaseCards()
	{
		this.PreReleaseCardViewsHandlers?.Invoke();
		ReleasingCards = true;
		if (CardHoverController.HoveredCard != null && (bool)cardHolder && cardHolder.CardViews.Contains(CardHoverController.HoveredCard))
		{
			_gameManager.InteractionSystem.HandleHoverEnd(CardHoverController.HoveredCard);
		}
		ClearCardVFX();
		if ((bool)cardHolder && cardHolder.CardViews.Count > 0)
		{
			List<DuelScene_CDC> list = new List<DuelScene_CDC>(cardHolder.CardViews);
			for (int i = 0; i < list.Count; i++)
			{
				DuelScene_CDC duelScene_CDC = list[i];
				if ((bool)duelScene_CDC && (bool)duelScene_CDC.Root)
				{
					duelScene_CDC.gameObject.UpdateActive(active: true);
					this.ReleaseCardViewHandlers?.Invoke(duelScene_CDC);
					if (duelScene_CDC.Model == null || duelScene_CDC.Model.Zone == null || duelScene_CDC.Model.InstanceId == 0 || (!((ICardLock)cardHolder).LockCardDetails && !entityViewManager.TryGetCardView(duelScene_CDC.InstanceId, out var _)))
					{
						cardHolder.RemoveCard(duelScene_CDC);
						_cardBuilder.DestroyCDC(duelScene_CDC);
					}
					else
					{
						_cardMovementController.MoveCard(duelScene_CDC, duelScene_CDC.Model.Zone);
					}
				}
			}
			list.Clear();
		}
		ReleasingCards = false;
	}

	private void ClearCardVFX()
	{
		while (_instantiatedCardVFX.Count > 0)
		{
			_unityObjectPool.PushObject(_instantiatedCardVFX[0]);
			_instantiatedCardVFX.RemoveAt(0);
		}
		if (_cardVFX != null)
		{
			_cardVFX.Clear();
		}
	}

	public override void SetVisibility(bool visible)
	{
		base.SetVisibility(visible);
		if (!cardHolder)
		{
			return;
		}
		DuelScene_CDC cardView;
		if (!base.IsVisible)
		{
			cardViews = new List<DuelScene_CDC>(cardHolder.CardViews);
			{
				foreach (DuelScene_CDC cardView2 in cardViews)
				{
					if (!(cardView2 == null) && !(cardView2.Root == null))
					{
						if (cardView2.Model == null || cardView2.Model.Zone == null || cardView2.Model.InstanceId == 0 || !entityViewManager.TryGetCardView(cardView2.InstanceId, out cardView))
						{
							cardView2.gameObject.SetActive(value: false);
						}
						else
						{
							_cardMovementController.MoveCard(cardView2, cardView2.Model.Zone);
						}
					}
				}
				return;
			}
		}
		foreach (DuelScene_CDC cardView3 in cardViews)
		{
			if (!(cardView3 == null) && !(cardView3.Root == null))
			{
				if (cardView3.Model == null || cardView3.Model.Zone == null || cardView3.Model.InstanceId == 0 || (!((ICardLock)cardHolder).LockCardDetails && !entityViewManager.TryGetCardView(cardView3.InstanceId, out cardView)))
				{
					cardView3.gameObject.SetActive(value: true);
					cardHolder.RemoveCard(cardView3);
				}
				_cardMovementController.MoveCard(cardView3, cardHolder);
				if (_cardVFX.TryGetValue(cardView3, out var value))
				{
					PlayCardVFX(cardView3, value);
				}
			}
		}
		cardHolder.Layout = GetCardHolderLayout();
		cardHolder.LayoutNow();
	}

	protected abstract void SetupCards();

	protected void SetupCardFX()
	{
		ClearCardVFX();
		AssetLookupTree<CardVFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<CardVFX>();
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.GameState = _gameManager.CurrentGameState;
		blackboard.Interaction = _gameManager.CurrentInteraction;
		blackboard.ActiveResolution = _gameManager.ActiveResolutionEffect;
		blackboard.CardBrowserCardCount = (uint)cardViews.Count;
		blackboard.CardBrowserLayoutID = cardBrowserProvider.GetCardHolderLayoutKey();
		blackboard.CardBrowserType = cardBrowserProvider.GetBrowserType();
		blackboard.CardHolder = cardHolder;
		blackboard.CardHolderType = cardHolder.CardHolderType;
		cardBrowserProvider.SetFxBlackboardData(blackboard);
		_cardVFX = new Dictionary<DuelScene_CDC, CardVFX>();
		foreach (DuelScene_CDC cardView in cardViews)
		{
			blackboard.SetCardDataExtensive(cardView.Model);
			blackboard.SetCdcViewMetadata(new CDCViewMetadata(cardView));
			blackboard.HighlightType = cardView.CurrentHighlight();
			cardBrowserProvider.SetFxBlackboardDataForCard(cardView, blackboard);
			CardVFX payload = assetLookupTree.GetPayload(blackboard);
			if (payload != null && !string.IsNullOrWhiteSpace(payload.PrefabRef?.RelativePath))
			{
				_cardVFX.Add(cardView, payload);
			}
		}
		blackboard.Clear();
	}

	protected void PlayCardVFX(DuelScene_CDC cardView, CardVFX vfxPayload)
	{
		GameObject item = _vfxProvider.PlayVFX(new VfxData
		{
			ActivationType = VfxActivationType.OneShot,
			IgnoreDedupe = false,
			ParentToSpace = true,
			Offset = vfxPayload.Offset,
			PrefabData = new VfxPrefabData
			{
				AllPrefabs = { vfxPayload.PrefabRef },
				CleanupAfterTime = 0f,
				SkipSelfCleanup = true
			},
			SpaceData = 
			{
				Space = RelativeSpace.Local
			}
		}, cardView.Model, cardView.Model.Instance, cardView.EffectsRoot);
		if (!_instantiatedCardVFX.Contains(item))
		{
			_instantiatedCardVFX.Add(item);
		}
	}

	protected virtual ICardLayout GetCardHolderLayout()
	{
		return scrollableLayout;
	}

	public virtual void HandleDrag(DuelScene_CDC draggedCard)
	{
	}

	public virtual void OnDragRelease(DuelScene_CDC draggedCard)
	{
		cardViews = new List<DuelScene_CDC>(cardHolder.CardViews);
	}

	protected void MoveCardViewsToBrowser(IEnumerable<DuelScene_CDC> cardsToMove)
	{
		foreach (DuelScene_CDC item in cardsToMove)
		{
			MoveCardToBrowser(item);
		}
	}

	protected void MoveCardToBrowser(DuelScene_CDC cardToMove)
	{
		_cardMovementController.MoveCard(cardToMove, cardHolder);
		if (!cardInfoObjects.ContainsKey(cardToMove))
		{
			BrowserCardHeader.BrowserCardHeaderData cardHeaderData = cardBrowserProvider.GetCardHeaderData(cardToMove);
			if (cardHeaderData != null)
			{
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.CardBrowserElementID = "CardInfoHeader";
				BrowserElementPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<BrowserElementPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
				BrowserCardHeader component = _unityObjectPool.PopObject(payload.PrefabPath).GetComponent<BrowserCardHeader>();
				component.SetText(cardHeaderData);
				Transform transform = component.transform;
				transform.SetParent(cardHolder.transform);
				PositionBrowserCardInfo(transform, cardToMove);
				component.gameObject.SetActive(value: false);
				cardInfoObjects[cardToMove] = component;
			}
		}
	}

	public virtual List<DuelScene_CDC> GetCardViews()
	{
		return cardViews;
	}

	public virtual void OnCardViewSelected(DuelScene_CDC cardView)
	{
		if (TryGetCardClickVFX(cardView, out var payload))
		{
			_vfxProvider.PlayVFX(new VfxData
			{
				SpaceData = 
				{
					Space = RelativeSpace.Local
				},
				PrefabData = 
				{
					AllPrefabs = { payload.PrefabRef }
				},
				Offset = payload.Offset,
				ParentToSpace = false
			}, cardView.Model, cardView.Model.Instance, cardView.EffectsRoot);
		}
		PlayButtonFX(cardView);
		if (_backgroundSfx != null)
		{
			AudioManager.PlayAudio(_backgroundSfx.SelectionEvent.AudioEvents, cardView.gameObject);
		}
		this.CardViewSelectedHandlers?.Invoke(cardView);
	}

	protected virtual void CardHolder_OnCardRemoved(DuelScene_CDC cardView)
	{
		this.CardViewRemovedHandlers?.Invoke(cardView);
	}

	public BrowserHoverData GetBrowserHoverData()
	{
		string cardHolderLayoutKey = cardBrowserProvider.GetCardHolderLayoutKey();
		if (cardHolderLayoutKey != _cachedHoverDataKey)
		{
			_gameManager.AssetLookupSystem.Blackboard.Clear();
			_gameManager.AssetLookupSystem.Blackboard.CardBrowserLayoutID = cardHolderLayoutKey;
			BrowserCardHolderHoverPrefab payload = _gameManager.AssetLookupSystem.TreeLoader.LoadTree<BrowserCardHolderHoverPrefab>(returnNewTree: false).GetPayload(_gameManager.AssetLookupSystem.Blackboard);
			if (payload == null)
			{
				Debug.LogErrorFormat("no BrowserCardHolderHoverPrefab payload found for CardBrowserLayoutID with key {0}", cardHolderLayoutKey);
				return null;
			}
			_cachedHoverDataKey = cardHolderLayoutKey;
			_cachedHoverdata = AssetLoader.GetObjectData<BrowserHoverData>(payload.PrefabPath);
		}
		return _cachedHoverdata;
	}

	public CardLayout_ScrollableBrowser GetCardHolderLayoutData(string layoutKey, uint? cardCount = null)
	{
		_gameManager.AssetLookupSystem.Blackboard.Clear();
		_gameManager.AssetLookupSystem.Blackboard.CardBrowserLayoutID = layoutKey;
		_gameManager.AssetLookupSystem.Blackboard.CardBrowserCardCount = cardCount;
		BrowserCardHolderLayoutPrefab payload = _gameManager.AssetLookupSystem.TreeLoader.LoadTree<BrowserCardHolderLayoutPrefab>(returnNewTree: false).GetPayload(_gameManager.AssetLookupSystem.Blackboard);
		if (payload == null || payload.PrefabPath == null)
		{
			Debug.LogErrorFormat("no BrowserCardHolderLayoutPrefab payload found for CardBrowserLayoutID with key {0}", layoutKey);
			return null;
		}
		BrowserCardHolderLayoutData objectData = AssetLoader.GetObjectData<BrowserCardHolderLayoutData>(payload.PrefabPath);
		if (objectData == null)
		{
			Debug.LogErrorFormat("null CardHolderLayoutData found in payload with layoutKey {0}", layoutKey);
			return null;
		}
		return objectData.CardHolderLayout;
	}

	private IEnumerator UpdateCardHeadersCoroutine()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser = cardHolder.Layout as CardLayout_ScrollableBrowser;
			if (cardLayout_ScrollableBrowser == null && cardHolder.Layout is CardLayout_MultiLayout)
			{
				cardLayout_ScrollableBrowser = (cardHolder.Layout as CardLayout_MultiLayout).GetLayout(0) as CardLayout_ScrollableBrowser;
			}
			foreach (DuelScene_CDC key in cardInfoObjects.Keys)
			{
				Vector3 position = _splineMovementSystem.GetGoal(key.Root).Position;
				bool flag = base.IsVisible && CardHoverController.HoveredCard != key && key.Root.position == position && (CardBrowserCardHolder)key.CurrentCardHolder == cardHolder && key.IsVisible && key.gameObject.activeSelf;
				if (cardLayout_ScrollableBrowser != null && flag)
				{
					int indexForCard = cardHolder.GetIndexForCard(key);
					flag = flag && indexForCard >= cardLayout_ScrollableBrowser.PiledLeft && indexForCard < cardLayout_ScrollableBrowser.FrontCount + cardLayout_ScrollableBrowser.PiledLeft;
				}
				if (flag)
				{
					float canvasWidth = key.Collider.size.x;
					float num = cardLayout_ScrollableBrowser?.FrontSpacing ?? 0f;
					if (cardViews.Count > 1)
					{
						canvasWidth = (num - key.Collider.size.x * 0.5f * key.transform.localScale.x) * 2f;
						canvasWidth = ((key.transform.localScale.x == 0f) ? 0f : (canvasWidth * (1f / key.transform.localScale.x)));
						canvasWidth = Mathf.Min(canvasWidth, key.Collider.size.x);
					}
					cardInfoObjects[key].SetCanvasWidth(canvasWidth);
				}
				cardInfoObjects[key].gameObject.SetActive(flag);
				PositionBrowserCardInfo(cardInfoObjects[key].transform, key);
			}
		}
	}

	private void PositionBrowserCardInfo(Transform cardInfoTransform, DuelScene_CDC cardView)
	{
		cardInfoTransform.position = cardView.PartsRoot.position;
		cardInfoTransform.rotation = cardView.Root.rotation;
		cardInfoTransform.localScale = cardView.Root.localScale;
	}

	protected override void ReleaseUIElements()
	{
		base.ReleaseUIElements();
		foreach (BrowserCardHeader value in cardInfoObjects.Values)
		{
			if (_unityObjectPool != null)
			{
				value.gameObject.SetActive(value: true);
				value.RestoreOriginalCanvasWidth();
				_unityObjectPool.PushObject(value.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
		}
	}

	protected bool TryGetCardClickVFX(DuelScene_CDC cardView, out CardClickVFX payload)
	{
		payload = null;
		if (_gameManager != null && _assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<CardClickVFX> loadedTree))
		{
			IBlackboard blackboard = _assetLookupSystem.Blackboard;
			blackboard.Clear();
			_duelSceneBrowserProvider.SetFxBlackboardData(blackboard);
			cardBrowserProvider.SetFxBlackboardDataForCard(cardView, blackboard);
			blackboard.CardBrowserType = _duelSceneBrowserProvider.GetBrowserType();
			blackboard.CardHolderType = ((cardHolder != null) ? cardHolder.CardHolderType : CardHolderType.None);
			blackboard.CardHolder = cardHolder;
			blackboard.ActiveResolution = _gameManager.ActiveResolutionEffect;
			blackboard.HighlightType = cardView.CurrentHighlight();
			payload = loadedTree.GetPayload(blackboard);
			blackboard.Clear();
		}
		return payload != null;
	}

	protected void PlayButtonFX(DuelScene_CDC cardView)
	{
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ButtonVFX> loadedTree))
		{
			IBlackboard blackboard = _assetLookupSystem.Blackboard;
			_duelSceneBrowserProvider.SetFxBlackboardData(blackboard);
			cardBrowserProvider.SetFxBlackboardDataForCard(cardView, blackboard);
			blackboard.CardBrowserType = _duelSceneBrowserProvider.GetBrowserType();
			blackboard.CardHolderType = ((cardHolder != null) ? cardHolder.CardHolderType : CardHolderType.None);
			blackboard.CardHolder = cardHolder;
			blackboard.ActiveResolution = _gameManager.ActiveResolutionEffect;
			ButtonVFX payload = loadedTree.GetPayload(blackboard);
			if (payload != null && uiElementData.TryGetValue(payload.ButtonName, out var value))
			{
				_vfxProvider.PlayVFX(payload.VfxData, null, null, value.GameObject.transform);
			}
			blackboard.Clear();
		}
	}

	protected static CardData CreateLibraryPlaceHolderCardData(MtgPlayer owner, string additionalFrameDetails = "Library")
	{
		MtgCardInstance instance = new MtgCardInstance
		{
			Zone = new MtgZone
			{
				Type = ZoneType.Library
			},
			Controller = new MtgPlayer(GREPlayerNum.Opponent),
			Owner = owner
		};
		IReadOnlyCollection<string> additionalFrameDetails2 = (IReadOnlyCollection<string>)(object)((!string.IsNullOrEmpty(additionalFrameDetails)) ? additionalFrameDetails.Split(',') : null);
		return new CardData(instance, new CardPrintingData(new CardPrintingRecord(0u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, null, null, null, null, null, null, null, null, null, null, additionalFrameDetails2), NullCardDataProvider.Default, NullAbilityDataProvider.Default));
	}

	protected override void OnClickViewBattlefield()
	{
		ClearCardVFX();
		base.OnClickViewBattlefield();
	}
}
