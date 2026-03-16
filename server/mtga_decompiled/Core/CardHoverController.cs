using System;
using System.Collections.Generic;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardHoverController : IDisposable, IUpdate, ICardHoverController, IAvatarHoverController
{
	private static readonly Dictionary<uint, HighlightType> _relatedUserHighlights = new Dictionary<uint, HighlightType>();

	private readonly IUnityObjectPool _objectPool;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private readonly Camera _camera;

	private readonly CardViewBuilder _cardViewBuilder;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IHighlightController _highlightController;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly BrowserManager _browserManager;

	private readonly HangerController _hangerController;

	private readonly IRelatedCardIdProvider _relatedCardIdProvider;

	private readonly NPEDirector _npeDirector;

	private readonly SplineMovementData _handHoverSpline;

	private readonly SplineMovementData _handReturnSpline;

	private float _hoverDelayRemaining;

	private const float DefaultHoverOffsetMultiplier = 5f;

	private DuelScene_CDC _hoveredCard;

	private IBattlefieldCardHolder _battlefieldCahce;

	private StackCardHolder _stackCardHolderCache;

	private HandCardHolder _localHandCache;

	private CardBrowserCardHolder _defaultBrowserCache;

	private ExamineViewCardHolder _examineCardHolderCache;

	private const float HORZ_SCALE_NORMAL = 1f;

	private const float HORZ_SCALE_WIDE = 2f;

	public static DuelScene_CDC HoveredCard { get; private set; }

	public bool IsHovering => HoveredCard;

	public bool IsHoveringBatStack { get; private set; }

	public DuelScene_CDC HoverCardCopy { get; private set; }

	public IAvatarView HoveredAvatar { get; private set; }

	private IBattlefieldCardHolder _battlefield => _battlefieldCahce ?? (_battlefieldCahce = _cardHolderProvider.GetCardHolder<IBattlefieldCardHolder>(GREPlayerNum.Invalid, CardHolderType.Battlefield));

	private StackCardHolder _stackCardHolder => _stackCardHolderCache ?? (_stackCardHolderCache = _cardHolderProvider.GetCardHolder<StackCardHolder>(GREPlayerNum.Invalid, CardHolderType.Stack));

	private HandCardHolder _localHand => _localHandCache ?? (_localHandCache = _cardHolderProvider.GetCardHolder<HandCardHolder>(GREPlayerNum.LocalPlayer, CardHolderType.Hand));

	private CardBrowserCardHolder _defaultBrowser => _defaultBrowserCache ?? (_defaultBrowserCache = _cardHolderProvider.GetCardHolder<CardBrowserCardHolder>(GREPlayerNum.Invalid, CardHolderType.CardBrowserDefault));

	private ExamineViewCardHolder _examineCardHolder => _examineCardHolderCache ?? (_examineCardHolderCache = _cardHolderProvider.GetCardHolder<ExamineViewCardHolder>(GREPlayerNum.Invalid, CardHolderType.Examine));

	public static event Action<IAvatarView> HoveredAvatarChangedHandlers;

	public static event Action<DuelScene_CDC> OnHoveredCardUpdated;

	public DuelScene_CDC GetHoveredCard()
	{
		return _hoveredCard;
	}

	public CardHoverController(IUnityObjectPool unityPool, ISplineMovementSystem splineMovementSystem, Camera camera, CardViewBuilder cardViewBuilder, ICardBuilder<DuelScene_CDC> cardBuilder, ICardViewProvider cardViewProvider, IHighlightController highlightController, ICardHolderProvider cardHolderProvider, BrowserManager browserManager, HangerController hangerController, IRelatedCardIdProvider relatedCardIdProvider, NPEDirector npeDirector)
	{
		_objectPool = unityPool ?? NullUnityObjectPool.Default;
		_splineMovementSystem = splineMovementSystem;
		_camera = camera;
		_cardViewBuilder = cardViewBuilder;
		_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_highlightController = highlightController ?? NullHighlightController.Default;
		_cardHolderProvider = cardHolderProvider;
		_browserManager = browserManager;
		_hangerController = hangerController;
		_relatedCardIdProvider = relatedCardIdProvider ?? NullRelatedCardIdProvider.Default;
		_npeDirector = npeDirector;
		_cardViewBuilder.preCardDestroyEvent += HandleCardDestory;
		_handHoverSpline = Resources.Load<SplineMovementData>("HandHoverSpline");
		_handReturnSpline = Resources.Load<SplineMovementData>("HandReturnSpline");
	}

	public void Dispose()
	{
		_cardViewBuilder.preCardDestroyEvent -= HandleCardDestory;
		CardHoverController.HoveredAvatarChangedHandlers = null;
		CardHoverController.OnHoveredCardUpdated = null;
		_battlefieldCahce = null;
		_stackCardHolderCache = null;
		_localHandCache = null;
		_examineCardHolderCache = null;
		_defaultBrowserCache = null;
		Resources.UnloadAsset(_handHoverSpline);
		Resources.UnloadAsset(_handReturnSpline);
	}

	public void OnUpdate(float deltaTime)
	{
		if (!IsHovering || _hoverDelayRemaining < 0f)
		{
			return;
		}
		_hoverDelayRemaining -= deltaTime;
		if (!(_hoverDelayRemaining < 0f))
		{
			return;
		}
		if (_splineMovementSystem.InteractionsAreAllowed(HoveredCard.Root))
		{
			HandleHover(HoveredCard);
			if (!LayoutNow(forceFlipLeft: false))
			{
				LayoutNow(forceFlipLeft: true);
			}
		}
		else
		{
			Reset();
		}
	}

	public void BeginAvatarHover(IAvatarView avatarView)
	{
		if (HoveredAvatar != avatarView)
		{
			HoveredAvatar = avatarView;
			CardHoverController.HoveredAvatarChangedHandlers?.Invoke(avatarView);
		}
	}

	public void EndAvatarHover(IAvatarView avatarView)
	{
		if (HoveredAvatar == avatarView)
		{
			HoveredAvatar = null;
			CardHoverController.HoveredAvatarChangedHandlers?.Invoke(null);
		}
	}

	public void BeginHover(DuelScene_CDC cardView)
	{
		if (!(HoveredCard == cardView))
		{
			if (HoveredCard != null)
			{
				EndHover();
			}
			ClearHoverCopyCard(HoverCardCopy);
			HoverCardCopy = null;
			HoveredCard = (_hoveredCard = cardView);
			_hoverDelayRemaining = GetHoverDelay();
			SetRelatedUserHighlights();
		}
	}

	public void OnCardBeginDrag(DuelScene_CDC card)
	{
		switch (card.CurrentCardHolder.CardHolderType)
		{
		case CardHolderType.CardBrowserDefault:
			EndHover();
			break;
		case CardHolderType.Hand:
			if (HoveredCard != card)
			{
				BeginHover(card);
			}
			break;
		}
	}

	public void EndHover()
	{
		if (IsHovering)
		{
			Reset();
		}
	}

	public void UpdateVFX()
	{
		HoveredCard.PlayPersistVFX<PersistVFX>();
	}

	private void Reset()
	{
		bool num = HoveredCard != null;
		if (HoverCardCopy == null && HoveredCard != null)
		{
			if ((HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Library && HoveredCard.CurrentCardHolder.PlayerNum == GREPlayerNum.Opponent) || (HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Hand && HoveredCard.CurrentCardHolder.PlayerNum == GREPlayerNum.Opponent) || HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield || HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Graveyard || HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Command)
			{
				if (HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield && HoveredCard.CurrentCardHolder is IBattlefieldCardHolder battlefieldCardHolder)
				{
					battlefieldCardHolder.RefreshAbilitiesForCardsStack(HoveredCard);
					if (IsHoveringBatStack)
					{
						battlefieldCardHolder.LayoutNow();
					}
				}
			}
			else
			{
				if (HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Hand)
				{
					UpdateVFX();
				}
				if (HoveredCard != _examineCardHolder.ClonedCardView)
				{
					HoveredCard.ClearOverrides();
				}
				_splineMovementSystem.RemoveTemporaryGoal(HoveredCard.Root);
				if (HoveredCard.CurrentCardHolder.CardHolderType == CardHolderType.Hand)
				{
					IdealPoint goal = _splineMovementSystem.GetGoal(HoveredCard.Root);
					_splineMovementSystem.AddPermanentGoal(HoveredCard.Root, goal, allowInteractions: true, _handReturnSpline);
				}
			}
			HoveredCard.UpdateVisuals();
			if (HoveredCard == _examineCardHolder.ClonedCardView)
			{
				HoveredCard.Root.gameObject.SetLayer(_examineCardHolder.gameObject.layer);
			}
			else
			{
				HoveredCard.Root.gameObject.SetLayer(HoveredCard.CurrentCardHolder.Layer);
			}
		}
		ClearHoverCopyCard(HoverCardCopy);
		_hoverDelayRemaining = 0f;
		HoverCardCopy = null;
		HoveredCard = (_hoveredCard = null);
		IsHoveringBatStack = false;
		_hangerController.ClearHangers();
		SetRelatedUserHighlights();
		_examineCardHolder.LimboFaceHanger.DeactivateHanger();
		if (num)
		{
			CardHoverController.OnHoveredCardUpdated?.Invoke(null);
		}
	}

	private float GetHoverDelay()
	{
		switch (HoveredCard.CurrentCardHolder.CardHolderType)
		{
		case CardHolderType.Battlefield:
			return 0.1f;
		case CardHolderType.Hand:
			if (HoveredCard.CurrentCardHolder is HandCardHolder_Handheld { LayoutComplete: false })
			{
				return 0.1f;
			}
			return 0f;
		default:
			return 0f;
		}
	}

	private void HandleHover(DuelScene_CDC hoveredCard)
	{
		if (hoveredCard.VisualModel.IsDisplayedFaceDown && !hoveredCard.VisualModel.Instance.FaceDownState.IsFaceDown)
		{
			HandleFaceDownHover(hoveredCard);
		}
		else
		{
			switch (hoveredCard.CurrentCardHolder.CardHolderType)
			{
			case CardHolderType.Battlefield:
				HandleBattlefieldHover(hoveredCard);
				break;
			case CardHolderType.Hand:
				if (hoveredCard.CurrentCardHolder.PlayerNum == GREPlayerNum.LocalPlayer)
				{
					HandleHandHover(hoveredCard);
				}
				else
				{
					HandleOpponentHandHover(hoveredCard);
				}
				break;
			case CardHolderType.Library:
				HandleLibraryHover(hoveredCard);
				break;
			case CardHolderType.CardBrowserDefault:
			case CardHolderType.CardBrowserViewDismiss:
				if (_browserManager.IsBrowserVisible && _browserManager.CurrentBrowser is CardBrowserBase { AllowsHoverInteractions: not false })
				{
					HandleBrowserHover(hoveredCard);
				}
				break;
			case CardHolderType.Command:
				HandleBattlefieldHover(hoveredCard);
				break;
			case CardHolderType.Graveyard:
				HandleGraveyardHover(hoveredCard);
				break;
			case CardHolderType.Stack:
				HandleStackHover(hoveredCard);
				break;
			}
			hoveredCard.PlayPersistVFX<PersistVFXOnHoveredAndExaminedCards>(hoveredCard.CurrentCardHolder.CardHolderType != CardHolderType.Battlefield);
		}
		CardHoverController.OnHoveredCardUpdated?.Invoke(HoveredCard);
	}

	public void HandleMobileTap(DuelScene_CDC tappedCard)
	{
		if (_splineMovementSystem.InteractionsAreAllowed(tappedCard.Root) && tappedCard.VisualModel.IsDisplayedFaceDown)
		{
			HandleFaceDownHover(tappedCard);
		}
	}

	private void HandleFaceDownHover(DuelScene_CDC hoveredCard)
	{
		if (!(hoveredCard == null) && hoveredCard.VisualModel != null && hoveredCard.VisualModel.Controller != null)
		{
			hoveredCard.GetSleeveFXPayload(hoveredCard.VisualModel, hoveredCard.CurrentCardHolder.CardHolderType, out var sleeveFXPayload, out var prefabFilePath);
			if (sleeveFXPayload != null && !string.IsNullOrWhiteSpace(prefabFilePath) && (hoveredCard.EffectsRoot.childCount <= 0 || sleeveFXPayload.AllowDuplicates))
			{
				GameObject gameObject = _objectPool.PopObject(prefabFilePath);
				gameObject.transform.SetParent(hoveredCard.EffectsRoot);
				gameObject.transform.localPosition = sleeveFXPayload.OffsetData.PositionOffset;
				gameObject.transform.localEulerAngles = sleeveFXPayload.OffsetData.RotationOffset;
				gameObject.transform.localScale = sleeveFXPayload.OffsetData.ScaleMultiplier;
				gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(sleeveFXPayload.CleanUpAfterSeconds);
				AudioManager.PlayAudio(sleeveFXPayload.AudioEvent, gameObject);
			}
		}
	}

	private void HandleStackHover(DuelScene_CDC hoveredCard)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, hoveredCard.Root.gameObject);
		_examineCardHolder.LimboFaceHanger.ActivateHanger(hoveredCard, hoveredCard.Model, new HangerSituation
		{
			DelayActivation = true,
			ShouldCycleFaces = true
		});
	}

	private void HandleGraveyardHover(DuelScene_CDC hoveredCard)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, hoveredCard.Root.gameObject);
		CreateHoverCopyCard(hoveredCard);
	}

	private void HandleBattlefieldHover(DuelScene_CDC hoveredCard)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, hoveredCard.Root.gameObject);
		if (((_battlefield != null) ? _battlefield.GetStackForCard(hoveredCard) : null) != null)
		{
			IsHoveringBatStack = true;
			_battlefield.LayoutNow();
		}
		CreateHoverCopyCard(hoveredCard);
	}

	private void HandleOpponentHandHover(DuelScene_CDC hoveredCard)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, hoveredCard.Root.gameObject);
		CreateHoverCopyCard(hoveredCard);
	}

	private void HandleHandHover(DuelScene_CDC hoveredCard)
	{
		Transform transform = _camera.transform;
		Transform parent = hoveredCard.Root.parent;
		Vector3 position = transform.position;
		Vector3 position2 = hoveredCard.Root.position;
		float magnitude = (position - position2).magnitude;
		Vector3 normalized = (position - position2).normalized;
		Vector3 vector = parent.position - parent.forward * 2f;
		Vector3 vector2 = parent.TransformDirection(Vector3.up);
		float num = hoveredCard.ActiveScaffold.GetColliderBounds.size.y * 0.5f;
		Vector3 one = Vector3.one;
		Vector3 intersectionPoint;
		Vector3 cameraCenterPoint;
		Rect rect = _camera.FrustumAtPoint(vector, vector2, out intersectionPoint, out cameraCenterPoint);
		Vector3 pos;
		if (PlatformUtils.IsDesktop() || !PlatformUtils.TouchSupported())
		{
			Vector3 vector3 = cameraCenterPoint + -_camera.transform.up * (rect.height * 0.5f);
			Debug.DrawRay(vector, parent.TransformDirection(Vector3.up) * 5f, UnityEngine.Color.cyan, 5f);
			Debug.DrawRay(intersectionPoint, normalized * 10f, UnityEngine.Color.black, 5f);
			Debug.DrawLine(intersectionPoint, vector3, UnityEngine.Color.blue, 5f);
			Debug.DrawRay(vector3, _camera.transform.right * 5f, UnityEngine.Color.green, 5f);
			num *= 1.5f;
			pos = vector3 + vector2 * num;
			pos.x = hoveredCard.Root.position.x;
			one *= 1.5f;
		}
		else
		{
			float num2 = ((!PlatformUtils.IsAspectRatio4x3()) ? 2.2f : 1.5f);
			Vector3 vector4 = ((Input.touchCount > 0) ? ((Vector3)Input.GetTouch(0).position) : Input.mousePosition);
			Vector3 vector5 = _camera.ScreenToWorldPoint(vector4 + Vector3.forward * magnitude);
			Vector3 normalized2 = (_camera.transform.position - vector5).normalized;
			float num3 = 5f;
			num *= num2;
			pos = vector5 + normalized2 * num3 + vector2 * (num + 1f);
			pos = GetClampedScreenPosition(hoveredCard, pos, hoveredCard.transform.rotation, num2);
			one *= num2;
		}
		IdealPoint endPoint = new IdealPoint(pos, parent.rotation, one, 2f);
		_splineMovementSystem.AddTemporaryGoal(hoveredCard.Root, endPoint, allowInteractions: true, _handHoverSpline);
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, hoveredCard.Root.gameObject);
		hoveredCard.IsMousedOver = true;
		hoveredCard.UpdateVisuals();
		if (HoveredCard == hoveredCard && hoveredCard.CurrentCardHolder.PlayerNum == GREPlayerNum.LocalPlayer)
		{
			bool delayActivation = hoveredCard.VisualModel.LinkedFaceType != LinkedFace.MdfcBack;
			_hangerController.ShowHangersForCard(hoveredCard, hoveredCard.VisualModel, new HangerSituation
			{
				DelayActivation = delayActivation
			});
		}
		if (_npeDirector != null)
		{
			_npeDirector.NPEController.KillEmphasisAnimationOnHoverCard(hoveredCard);
		}
	}

	private void HandleLibraryHover(DuelScene_CDC hoveredCard)
	{
		if (hoveredCard.CurrentCardHolder.PlayerNum == GREPlayerNum.Opponent)
		{
			CreateHoverCopyCard(hoveredCard);
			return;
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, hoveredCard.Root.gameObject);
		hoveredCard.IsMousedOver = true;
		hoveredCard.UpdateVisuals();
		Vector3 position = new Vector3(14f, 12f, 11.75f);
		Quaternion quaternion = Quaternion.Euler(105f, 0f, -180f);
		Vector3 scale = Vector3.one * 1.5f;
		position = GetClampedScreenPosition(hoveredCard, position, quaternion, 1.5f);
		IdealPoint endPoint = new IdealPoint(position, quaternion, scale, 2f);
		_splineMovementSystem.AddTemporaryGoal(hoveredCard.Root, endPoint, allowInteractions: true);
		if (HoveredCard == hoveredCard && hoveredCard.CurrentCardHolder.PlayerNum == GREPlayerNum.LocalPlayer)
		{
			_hangerController.ShowHangersForCard(hoveredCard, hoveredCard.VisualModel, new HangerSituation
			{
				DelayActivation = true
			});
		}
		hoveredCard.Root.gameObject.SetLayer(_localHand.Layer);
	}

	private void HandleBrowserHover(DuelScene_CDC hoveredCard)
	{
		if (_browserManager.CurrentBrowser is CardBrowserBase { IsVisible: not false } cardBrowserBase)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_touch.EventName, AudioManager.Default);
			hoveredCard.IsMousedOver = true;
			hoveredCard.UpdateVisuals();
			BrowserHoverData browserHoverData = cardBrowserBase.GetBrowserHoverData();
			Transform parent = hoveredCard.Root.parent;
			CardBrowserCardHolder defaultBrowser = _defaultBrowser;
			Transform transform = defaultBrowser.transform;
			Vector3 position = _splineMovementSystem.GetGoal(hoveredCard.Root).Position;
			Vector3 vector = transform.TransformDirection(Vector3.up);
			Vector3 position2 = position - parent.forward * browserHoverData.ForwardOffset;
			position2 += vector * browserHoverData.YOffset;
			position2 += _camera.transform.right * browserHoverData.XOffset;
			float num = defaultBrowser.CardScale * browserHoverData.Scale;
			Quaternion rotation = transform.rotation;
			Vector3 scale = Vector3.one * num;
			position2 = GetClampedScreenPosition(hoveredCard, position2, rotation, num);
			IdealPoint endPoint = new IdealPoint(position2, rotation, scale, 2f);
			_splineMovementSystem.AddTemporaryGoal(hoveredCard.Root, endPoint, allowInteractions: true, _handHoverSpline);
			if (HoveredCard == hoveredCard)
			{
				_hangerController.ShowHangersForCard(hoveredCard, hoveredCard.VisualModel, new HangerSituation
				{
					DelayActivation = true
				});
			}
		}
	}

	private Vector3 GetClampedScreenPosition(DuelScene_CDC cardView, Vector3 position, Quaternion rotation, float scale)
	{
		Vector3 pointUp = rotation * Vector3.up;
		Vector3 intersectionPoint;
		Vector3 cameraCenterPoint;
		Rect rect = _camera.FrustumAtPoint(position, pointUp, out intersectionPoint, out cameraCenterPoint);
		float num = cardView.ActiveScaffold.GetColliderBounds.size.y * scale * 0.5f;
		float num2 = cardView.ActiveScaffold.GetColliderBounds.size.x * scale * HorizontalScale(cardView) * 0.5f;
		Transform transform = _camera.transform;
		Vector3 up = transform.up;
		Vector3 right = transform.right;
		Vector3 vector = cameraCenterPoint + up * (rect.height * 0.49f - num);
		Vector3 vector2 = cameraCenterPoint + -up * (rect.height * 0.49f - num);
		Vector3 vector3 = cameraCenterPoint + right * (rect.width * 0.49f - num2);
		Vector3 vector4 = cameraCenterPoint + -right * (rect.width * 0.49f - num2);
		if (Vector3.Dot(up, vector - position) < 0f)
		{
			position.y = vector.y;
			position.z = vector.z;
		}
		if (Vector3.Dot(-up, vector2 - position) < 0f)
		{
			position.y = vector2.y;
			position.z = vector2.z;
		}
		if (Vector3.Dot(right, vector3 - position) < 0f)
		{
			position.x = vector3.x;
		}
		if (Vector3.Dot(-right, vector4 - position) < 0f)
		{
			position.x = vector4.x;
		}
		return position;
	}

	private static float HorizontalScale(DuelScene_CDC cardView)
	{
		if (!UsesHorizontalLayout(cardView))
		{
			return 1f;
		}
		return 2f;
	}

	private static bool UsesHorizontalLayout(DuelScene_CDC cardView)
	{
		if (cardView != null)
		{
			return UsesHorizontalLayout(cardView.Model);
		}
		return false;
	}

	private static bool UsesHorizontalLayout(ICardDataAdapter model)
	{
		if (!model.CardTypes.Contains(CardType.Battle) && !model.IsRoomParent())
		{
			return model.IsSplitCard();
		}
		return true;
	}

	private bool LayoutNow(bool forceFlipLeft)
	{
		if (!HoverCardCopy)
		{
			return true;
		}
		Vector3 position = _camera.transform.position;
		Vector3 normalized = (position - HoveredCard.Root.position).normalized;
		Vector3 vector = position - normalized * 21f;
		HoverCardCopy.Root.position = vector;
		HoverCardCopy.Root.rotation = _localHandCache.transform.rotation;
		float num = HoverCardCopy.ActiveScaffold.GetColliderBounds.extents.y * 2f * 1f;
		float num2 = HoverCardCopy.ActiveScaffold.GetColliderBounds.extents.x * 2f * 1f;
		Vector3 intersectionPoint;
		Vector3 cameraCenterPoint;
		Rect rect = _camera.FrustumAtPoint(HoverCardCopy.Root, out intersectionPoint, out cameraCenterPoint);
		bool flag;
		if (_stackCardHolder.TryGetTopCardOnStack(out var _))
		{
			flag = vector.x <= 0f;
		}
		else
		{
			Vector3 vector2 = cameraCenterPoint + Vector3.left * rect.width * 0.48f;
			flag = vector.x - num2 * 0.5f - 4f <= vector2.x;
		}
		vector.x += (forceFlipLeft ? 4f : (-4f));
		Vector3 vector3 = cameraCenterPoint + _camera.transform.up * rect.height * 0.46f;
		Vector3 vector4 = HoverCardCopy.Root.position + HoverCardCopy.Root.up * num * 0.5f;
		vector3.x = (vector4.x = 0f);
		if (Vector3.Distance(cameraCenterPoint, vector3) < Vector3.Distance(cameraCenterPoint, vector4))
		{
			vector += vector3 - vector4;
			if (vector4.y + 1.2f > vector3.y)
			{
				vector.y -= vector4.y + 1.2f - vector3.y;
			}
		}
		Vector3 vector5 = cameraCenterPoint - _camera.transform.up * rect.height * 0.49f;
		Vector3 vector6 = HoverCardCopy.Root.position - HoverCardCopy.Root.up * num * 0.5f;
		vector5.x = (vector6.x = 0f);
		if (Vector3.Distance(cameraCenterPoint, vector5) < Vector3.Distance(cameraCenterPoint, vector6))
		{
			vector += vector5 - vector6;
		}
		IdealPoint endPoint = new IdealPoint(vector, _localHand.transform.rotation, Vector3.one * 1f, 5f);
		HoverCardCopy.Root.position = endPoint.Position;
		HoverCardCopy.Root.rotation = endPoint.Rotation;
		_splineMovementSystem.RemoveTemporaryGoal(HoverCardCopy.Root);
		_splineMovementSystem.AddTemporaryGoal(HoverCardCopy.Root, endPoint);
		flag |= !_hangerController.LayoutNow();
		return !flag;
	}

	private void CreateHoverCopyCard(DuelScene_CDC hoveredCard)
	{
		ICardDataAdapter cardData = new CardData(hoveredCard.VisualModel.Instance.GetExamineCopy(), hoveredCard.VisualModel.Printing)
		{
			RulesTextOverride = hoveredCard.VisualModel.RulesTextOverride
		};
		HoverCardCopy = _cardBuilder.CreateCDC(cardData, isVisible: true);
		HoverCardCopy.IsHoverCopy = true;
		HoverCardCopy.IsMousedOver = true;
		HoverCardCopy.Collider.enabled = false;
		_examineCardHolder.AddCard(HoverCardCopy);
		HoverCardCopy.Root.gameObject.SetLayer(_localHand.Layer);
		HoverCardCopy.UpdateHighlight(HighlightType.None);
		HoverCardCopy.SetOpponentHoverState(isMousedOver: false);
		HoverCardCopy.SetDimmedState(isDimmed: false);
		Vector3 position = HoveredCard.Root.position;
		Vector3 position2 = _camera.transform.position;
		_camera.FrustumAtPoint(HoverCardCopy.Root, out var _, out var cameraCenterPoint);
		Plane plane = new Plane(_camera.transform.forward, cameraCenterPoint);
		Bounds getColliderBounds = HoveredCard.ActiveScaffold.GetColliderBounds;
		Vector3 vector = ProjectOntoPlane(position - Vector3.forward * getColliderBounds.extents.y, plane, position2);
		Vector3 vector2 = ProjectOntoPlane(position + Vector3.forward * getColliderBounds.extents.y, plane, position2);
		Vector3 vector3 = ProjectOntoPlane(position - Vector3.right * getColliderBounds.extents.x, plane, position2);
		Vector3 vector4 = ProjectOntoPlane(position + Vector3.right * getColliderBounds.extents.x, plane, position2);
		Vector3 center = (vector + vector2 + vector3 + vector4) * 0.25f;
		float x = vector4.x - vector3.x;
		float y = vector2.z - vector.z;
		Bounds hoveredCardBounds = new Bounds(center, new Vector3(x, y, 0f));
		_hangerController.ShowHangersForCard(HoverCardCopy, hoveredCard.Model, new HangerSituation
		{
			DelayActivation = true,
			HoveredCardBounds = hoveredCardBounds
		});
		HoverCardCopy.PlayPersistVFX<PersistVFXOnHoveredAndExaminedCards>();
		if (_npeDirector != null && _npeDirector.GameSpecificConfiguration.ShowPowerToughnessIcon && HoverCardCopy.Model.CardTypes.Contains(CardType.Creature))
		{
			_npeDirector.NPEController.ShowPTIconsOnHoverCard(HoverCardCopy);
		}
		static Vector3 ProjectOntoPlane(Vector3 pt, Plane plane2, Vector3 cameraPos)
		{
			plane2.Raycast(new Ray(cameraPos, pt - cameraPos), out var enter);
			return cameraPos + (pt - cameraPos).normalized * enter;
		}
	}

	private void ClearHoverCopyCard(DuelScene_CDC copy)
	{
		if (_npeDirector != null)
		{
			_npeDirector.NPEController.ClearPTIconsOnHoverCard();
		}
		if (copy != null)
		{
			_examineCardHolder.RemoveCard(copy);
			copy.ClearOverrides();
			_splineMovementSystem.RemovePermanentGoal(copy.Root);
			_cardViewBuilder.DestroyCDC(copy);
		}
	}

	private void HandleCardDestory(BASE_CDC baseCDC)
	{
		if ((bool)baseCDC && baseCDC.Model != null)
		{
			GameObjectType objectType = baseCDC.Model.ObjectType;
			if (objectType != GameObjectType.SplitLeft && objectType != GameObjectType.SplitRight && !(HoveredCard == null) && !(baseCDC != HoveredCard))
			{
				EndHover();
			}
		}
	}

	public static Dictionary<uint, HighlightType> GetRelatedUserHighlights()
	{
		return _relatedUserHighlights;
	}

	private void SetRelatedUserHighlights()
	{
		CalculateRelatedUserHighlights();
		_highlightController.SetUserHighlights(_relatedUserHighlights);
	}

	private void CalculateRelatedUserHighlights()
	{
		_relatedUserHighlights.Clear();
		if (!HoveredCard)
		{
			return;
		}
		foreach (uint relatedId in _relatedCardIdProvider.GetRelatedIds(HoveredCard))
		{
			if (_cardViewProvider.TryGetCardView(relatedId, out var cardView) && cardView.HolderType != CardHolderType.Graveyard && cardView.HolderType != CardHolderType.Exile)
			{
				_relatedUserHighlights[relatedId] = HighlightType.Hover;
			}
		}
	}
}
