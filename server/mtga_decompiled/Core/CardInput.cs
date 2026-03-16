using InteractionSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Universal;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardInput : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
{
	private GameManager _gameManagerCache;

	private HandCardHolder _localHandCardHolderCache;

	private DuelScene_CDC _cardViewCache;

	private IBattlefieldCardHolder _battlefield;

	private GameManager GameManager => _gameManagerCache ?? (_gameManagerCache = Object.FindObjectOfType<GameManager>());

	private HandCardHolder LocalHandCardHolder
	{
		get
		{
			if (_localHandCardHolderCache != null)
			{
				return _localHandCardHolderCache;
			}
			GameManager gameManager = GameManager;
			if (gameManager == null)
			{
				return null;
			}
			ICardHolderProvider cardHolderManager = gameManager.CardHolderManager;
			if (cardHolderManager == null)
			{
				return null;
			}
			_localHandCardHolderCache = cardHolderManager.GetCardHolder<HandCardHolder>(GREPlayerNum.LocalPlayer, CardHolderType.Hand);
			return _localHandCardHolderCache;
		}
	}

	private IBattlefieldCardHolder Battlefield
	{
		get
		{
			if (_battlefield != null)
			{
				return _battlefield;
			}
			if (GameManager != null && GameManager.CardHolderManager != null && GameManager.CardHolderManager.TryGetCardHolder(GREPlayerNum.Invalid, CardHolderType.Battlefield, out IBattlefieldCardHolder result))
			{
				_battlefield = result;
				return _battlefield;
			}
			return null;
		}
	}

	private static Rect ScreenRect => new Rect(0f, 0f, Screen.width, Screen.height);

	public bool IsVisible => _cardViewCache?.IsVisible ?? false;

	private void OnEnable()
	{
		_cardViewCache = base.gameObject.GetComponent<DuelScene_CDC>();
	}

	private void Start()
	{
		if (!GameManager)
		{
			base.enabled = false;
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (base.enabled)
		{
			GameManager?.InteractionSystem?.ExecuteEndDrag(_cardViewCache, null);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!_cardViewCache || !_cardViewCache.IsVisible || !ScreenRect.Contains(eventData.position))
		{
			return;
		}
		BaseHandCardHolder componentInParent = base.gameObject.GetComponentInParent<BaseHandCardHolder>();
		if (!(componentInParent != null) || componentInParent.CardHandlesInput())
		{
			GameInteractionSystem gameInteractionSystem = GameManager?.InteractionSystem;
			if (eventData.button == PointerEventData.InputButton.Left && gameInteractionSystem != null && gameInteractionSystem.CanBeginDrag(_cardViewCache))
			{
				gameInteractionSystem.ExecuteBeginDrag(_cardViewCache);
			}
			if (Battlefield is UniversalBattlefieldCardHolder universalBattlefieldCardHolder)
			{
				universalBattlefieldCardHolder.HandleCardDragBegin(_cardViewCache, eventData.position);
			}
			eventData.Use();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if ((bool)_cardViewCache && _cardViewCache.IsVisible)
		{
			if (Battlefield is UniversalBattlefieldCardHolder universalBattlefieldCardHolder)
			{
				universalBattlefieldCardHolder.HandleCardDragSustain(_cardViewCache, eventData.position);
			}
			eventData.Use();
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if ((bool)_cardViewCache && _cardViewCache.IsVisible && eventData.button == PointerEventData.InputButton.Left)
		{
			bool num = eventData.hovered.Contains(base.gameObject);
			IEntityView endEntityView = eventData.pointerEnter?.GetComponentInParent<IEntityView>();
			GameManager?.InteractionSystem?.ExecuteEndDrag(_cardViewCache, endEntityView);
			if (!num)
			{
				GameManager?.InteractionSystem?.HandleHoverEnd(_cardViewCache);
			}
			if (Battlefield is UniversalBattlefieldCardHolder universalBattlefieldCardHolder)
			{
				universalBattlefieldCardHolder.HandleCardDragEnd(_cardViewCache);
			}
			eventData.Use();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!IsVisible)
		{
			return;
		}
		if (!ScreenRect.Contains(eventData.position))
		{
			Debug.LogWarning("We got a pointer event for a card that's not on the screen. Seems pretty weird.");
			return;
		}
		if (PlatformUtils.IsHandheld())
		{
			eventData.pointerPress = base.gameObject;
		}
		if (TryBeingConsidered(eventData.pointerPress))
		{
			eventData.Use();
		}
	}

	public bool TryBeingConsidered(GameObject pointerTarget)
	{
		BaseHandCardHolder componentInParent = base.gameObject.GetComponentInParent<BaseHandCardHolder>();
		if (componentInParent != null && !componentInParent.CardHandlesInput())
		{
			return false;
		}
		GameManager?.InteractionSystem?.HandleHover(_cardViewCache, pointerTarget);
		if (_cardViewCache.Model.Visibility == Visibility.Public || ((bool)LocalHandCardHolder && LocalHandCardHolder.CardViews.Contains(_cardViewCache)))
		{
			GameManager?.UIMessageHandler?.TrySendHoverMessage(_cardViewCache.Model.InstanceId);
		}
		return true;
	}

	public void StopBeingConsidered()
	{
		GameManager?.InteractionSystem?.HandleHoverEnd(_cardViewCache);
		GameManager?.UIMessageHandler?.TrySendHoverMessage(0u);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (IsVisible && !eventData.dragging)
		{
			StopBeingConsidered();
			eventData.Use();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (PlatformUtils.IsHandheld())
		{
			GameManager?.InteractionSystem?.OnCardDown(_cardViewCache, eventData);
			if (_cardViewCache.CurrentCardHolder is HandCardHolder handCardHolder)
			{
				handCardHolder.OnPointerEnter(eventData);
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (PlatformUtils.IsHandheld())
		{
			GameManager?.InteractionSystem?.OnCardUp();
			if (_cardViewCache.CurrentCardHolder is HandCardHolder handCardHolder)
			{
				handCardHolder.OnPointerExit();
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		BaseHandCardHolder componentInParent = base.gameObject.GetComponentInParent<BaseHandCardHolder>();
		if (componentInParent != null)
		{
			componentInParent.HandleClick(eventData, this);
		}
		else
		{
			HandleClick(eventData);
		}
	}

	public void HandleClick(PointerEventData eventData)
	{
		if (!_cardViewCache || !_cardViewCache.IsVisible || !ScreenRect.Contains(eventData.position))
		{
			return;
		}
		if (!eventData.dragging)
		{
			SimpleInteractionType interactionType = SimpleInteractionType.None;
			switch (eventData.button)
			{
			case PointerEventData.InputButton.Left:
				interactionType = ((eventData.clickCount <= 1) ? SimpleInteractionType.Primary : SimpleInteractionType.DoublePrimary);
				break;
			case PointerEventData.InputButton.Right:
				interactionType = SimpleInteractionType.Secondary;
				break;
			}
			ProcessInteraction(interactionType);
		}
		eventData.Use();
	}

	public void ProcessInteraction(SimpleInteractionType interactionType)
	{
		GameManager.InteractionSystem.OnCardClicked(_cardViewCache, interactionType);
	}

	public void OnScroll(PointerEventData eventData)
	{
		GameManager?.InteractionSystem?.HandleScroll(_cardViewCache, eventData);
	}

	public void Teardown()
	{
		_gameManagerCache = null;
		_localHandCardHolderCache = null;
		_cardViewCache = null;
		_battlefield = null;
	}
}
