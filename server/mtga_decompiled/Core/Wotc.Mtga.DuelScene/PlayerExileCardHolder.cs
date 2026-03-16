using System;
using System.Collections.Generic;
using GreClient.Rules;
using InteractionSystem;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene;

public class PlayerExileCardHolder : ZoneCardHolderBase, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IHoverableZone
{
	[SerializeField]
	private ParticleSystem _newCardAddedEffectSystem;

	[SerializeField]
	private ParticleSystem _persistentParticleSystem;

	[SerializeField]
	private Collider _interactionCollider;

	private GameInteractionSystem _gameInteractionSystem;

	private IClientLocProvider _locProvider = NullLocProvider.Default;

	private IBrowserManager _browserManager = NullBrowserManager.Default;

	private uint _playerId;

	private List<DuelScene_CDC> _playerOwnedCards = new List<DuelScene_CDC>();

	public event Action<MtgZone> Hovered;

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		_gameInteractionSystem = gameManager.InteractionSystem;
		IContext context = gameManager.Context;
		_locProvider = context.Get<IClientLocProvider>() ?? NullLocProvider.Default;
		_browserManager = context.Get<IBrowserManager>() ?? NullBrowserManager.Default;
		CardLayout_Horizontal cardLayout_Horizontal = new CardLayout_Horizontal();
		cardLayout_Horizontal.Spacing = Vector3.zero;
		base.Layout = cardLayout_Horizontal;
	}

	public void SetPlayerId(uint playerId)
	{
		_playerId = playerId;
	}

	protected override void OnDestroy()
	{
		_playerOwnedCards.Clear();
		_locProvider = NullLocProvider.Default;
		_browserManager = NullBrowserManager.Default;
		_gameInteractionSystem = null;
		_playerId = 0u;
		this.Hovered = null;
		base.OnDestroy();
	}

	private void UpdateExileCount()
	{
		uint num = 0u;
		foreach (uint associatedId in GetAssociatedIds(_zoneModel, _playerId))
		{
			_ = associatedId;
			num++;
		}
		if (num != 0 && !_persistentParticleSystem.isPlaying)
		{
			_persistentParticleSystem.Play(withChildren: true);
		}
		else if (num == 0)
		{
			_persistentParticleSystem.Stop(withChildren: true);
		}
		bool flag = num != 0;
		_interactionCollider.enabled = flag && _visibility;
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (_browserManager.IsBrowserVisible)
		{
			return;
		}
		_playerOwnedCards.Clear();
		if (_zoneModel != null)
		{
			_playerOwnedCards.AddRange(GetAssociatedCards(_zoneModel, _playerId, _cardViewProvider));
			_playerOwnedCards.Sort((DuelScene_CDC a, DuelScene_CDC b) => b.InstanceId.CompareTo(a.InstanceId));
			if (_playerOwnedCards.Count != 0)
			{
				ViewDismissBrowserProvider viewDismissBrowserProvider = new ViewDismissBrowserProvider(_playerOwnedCards, null, _locProvider.GetLocalizedText("Enum/ZoneType/ZoneType_Exile"), _gameInteractionSystem.HandleViewDismissCardClick, null, base.PlayerNum);
				viewDismissBrowserProvider.SetOpenedBrowser(_browserManager.OpenBrowser(viewDismissBrowserProvider));
			}
		}
	}

	protected override void OnPostLayout()
	{
		base.OnPostLayout();
		UpdateExileCount();
	}

	public override IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		Vector3 scale = (_overrideCdcSize ? (Vector3.one * _cdcSize) : data.Scale);
		if (_secondaryLayoutData.Contains(data))
		{
			scale = data.Scale;
		}
		return new IdealPoint(data.Card.Root.parent.TransformPoint(data.Position), data.Card.Root.parent.rotation * data.Rotation, scale);
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		layoutSplineEvents.Events.Add(new SplineEventCallback(0f, ExileEffectCallback));
		layoutSplineEvents.Events.Add(new SplineEventAudio(0.5f, new List<AudioEvent>
		{
			new AudioEvent(WwiseEvents.sfx_basicloc_exile.EventName)
		}));
		return layoutSplineEvents;
	}

	protected override bool CalcCardVisibility(CardLayoutData data, int indexInList)
	{
		if (!_visibility)
		{
			return false;
		}
		if (ThisCardIsNotInExile(data.Card.InstanceId))
		{
			return base.CalcCardVisibility(data, indexInList);
		}
		return _secondaryLayoutData.Contains(data);
	}

	public override void SetVisibility(bool visibility)
	{
		_interactionCollider.enabled = visibility && _cardViews.Count > 0;
		base.SetVisibility(visibility);
	}

	private bool ThisCardIsNotInExile(uint instanceId)
	{
		return !_zoneModel.CardIds.Contains(instanceId);
	}

	private void ExileEffectCallback(float value)
	{
		_newCardAddedEffectSystem.Play(withChildren: true);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		this.Hovered?.Invoke(_zoneModel);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		this.Hovered?.Invoke(null);
	}

	public static IEnumerable<uint> GetAssociatedIds(MtgZone exile, uint playerId)
	{
		if (exile == null)
		{
			yield break;
		}
		foreach (MtgCardInstance visibleCard in exile.VisibleCards)
		{
			MtgPlayer controller = visibleCard.Controller;
			if (controller != null && controller.InstanceId == playerId)
			{
				yield return visibleCard.InstanceId;
			}
		}
	}

	private static IEnumerable<DuelScene_CDC> GetAssociatedCards(MtgZone exile, uint playerId, ICardViewProvider cardViewProvider)
	{
		foreach (uint associatedId in GetAssociatedIds(exile, playerId))
		{
			if (cardViewProvider.TryGetCardView(associatedId, out var cardView))
			{
				yield return cardView;
			}
		}
	}
}
