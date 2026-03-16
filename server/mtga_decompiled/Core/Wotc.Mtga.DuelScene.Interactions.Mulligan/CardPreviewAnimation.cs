using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.Mulligan;

public class CardPreviewAnimation
{
	private const float PREVIEW_DELAY_SECONDS = 0.75f;

	private const float PREVIEW_DURATION_SECONDS = 0.75f;

	private readonly ICardDataProvider _cardDataProvider;

	private readonly ICardViewController _cardViewBuilder;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IDuelSceneStateProvider _dsStateProvider;

	private readonly List<DuelScene_CDC> _createdCdcs = new List<DuelScene_CDC>(7);

	private readonly List<(DuelScene_CDC cardView, MtgCardInstance linkedInstance, CardPrintingData linkedPrinting)> _pendingPreviews = new List<(DuelScene_CDC, MtgCardInstance, CardPrintingData)>(7);

	private readonly List<DuelScene_CDC> _activePreviews = new List<DuelScene_CDC>(7);

	private readonly List<CDCPart_AnimatedCardback> _animatedCardBacks = new List<CDCPart_AnimatedCardback>(7);

	private float _previewDelay = 0.75f;

	private float _previewDuration;

	private bool _isDone = true;

	public CardPreviewAnimation(ISplineMovementSystem splineMovementSystem, ICardViewController cardBuilder, ICardDataProvider cardProvider, IGameStateProvider gameStateProvider, ICardHolderProvider cardHolderProvider, IDuelSceneStateProvider stateProvider)
	{
		_splineMovementSystem = splineMovementSystem;
		_cardViewBuilder = cardBuilder ?? NullCardViewController.Default;
		_cardDataProvider = cardProvider ?? NullCardDataProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_dsStateProvider = stateProvider ?? NullDuelSceneStateProvider.Default;
	}

	public void Play(IEnumerable<DuelScene_CDC> openingHandCdcs)
	{
		_isDone = false;
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		foreach (DuelScene_CDC openingHandCdc in openingHandCdcs)
		{
			if (!openingHandCdc)
			{
				continue;
			}
			MtgCardInstance cardById = mtgGameState.GetCardById(openingHandCdc.InstanceId);
			if (cardById == null || cardById.LinkedFaceInstances.Count != 1)
			{
				continue;
			}
			CardPrintingData cardPrintingById = _cardDataProvider.GetCardPrintingById(cardById.GrpId);
			if (cardPrintingById == null || cardPrintingById.LinkedFacePrintings.Count != 1)
			{
				continue;
			}
			MtgCardInstance mtgCardInstance = cardById.LinkedFaceInstances[0];
			if (mtgCardInstance != null)
			{
				CardPrintingData cardPrintingData = cardPrintingById.LinkedFacePrintings[0];
				if (cardPrintingData != null && cardPrintingById.LinkedFaceType == LinkedFace.MdfcBack)
				{
					_pendingPreviews.Add((openingHandCdc, mtgCardInstance, cardPrintingData));
				}
			}
		}
		_pendingPreviews.Sort(((DuelScene_CDC cardView, MtgCardInstance linkedInstance, CardPrintingData linkedPrinting) lhs, (DuelScene_CDC cardView, MtgCardInstance linkedInstance, CardPrintingData linkedPrinting) rhs) => rhs.cardView.Root.position.x.CompareTo(lhs.cardView.Root.position.x));
	}

	private bool TryGetFlipPoint((DuelScene_CDC cardView, MtgCardInstance linkedInstance, CardPrintingData linkedPrinting) tuple, out IdealPoint flipPoint)
	{
		flipPoint = default(IdealPoint);
		if (!tuple.cardView || !tuple.cardView.Root)
		{
			return false;
		}
		if (!_cardHolderProvider.TryGetCardHolder(GREPlayerNum.Invalid, CardHolderType.CardBrowserDefault, out var cardHolder))
		{
			return false;
		}
		CDCPart_AnimatedCardback componentInChildren = tuple.cardView.GetComponentInChildren<CDCPart_AnimatedCardback>();
		if ((bool)componentInChildren)
		{
			_animatedCardBacks.Add(componentInChildren);
			componentInChildren.gameObject.SetActive(value: false);
		}
		IdealPoint layoutEndpoint = cardHolder.GetLayoutEndpoint(tuple.cardView);
		CardData cardData = new CardData(tuple.linkedInstance, tuple.linkedPrinting);
		DuelScene_CDC duelScene_CDC = _cardViewBuilder.CreateCardView(cardData);
		CDCPart_AnimatedCardback componentInChildren2 = duelScene_CDC.GetComponentInChildren<CDCPart_AnimatedCardback>();
		if ((bool)componentInChildren2)
		{
			componentInChildren2.gameObject.SetActive(value: false);
		}
		duelScene_CDC.UpdateVisibility(shouldBeVisible: true);
		duelScene_CDC.gameObject.SetLayer(cardHolder.Layer);
		duelScene_CDC.Root.SetParent(tuple.cardView.PartsRoot);
		duelScene_CDC.Root.ZeroOut();
		duelScene_CDC.Root.localPosition = Vector3.forward * 0.1f;
		duelScene_CDC.Root.localRotation = Quaternion.Euler(0f, 180f, 0f);
		_createdCdcs.Add(duelScene_CDC);
		flipPoint = new IdealPoint(layoutEndpoint.Position - cardHolder.CardRoot.forward * 2f, layoutEndpoint.Rotation * Quaternion.Euler(0f, 180f, 0f), layoutEndpoint.Scale);
		return true;
	}

	public void Update(float dt)
	{
		if (!_dsStateProvider.AllowInput || _isDone)
		{
			return;
		}
		if (_activePreviews.Count > 0)
		{
			_previewDuration += dt;
			while (_previewDuration >= 0.75f)
			{
				_previewDuration -= 0.75f;
				DuelScene_CDC duelScene_CDC = _activePreviews[0];
				_activePreviews.RemoveAt(0);
				if ((bool)duelScene_CDC && (bool)duelScene_CDC.Root)
				{
					_splineMovementSystem.RemoveTemporaryGoal(duelScene_CDC.Root);
				}
			}
		}
		if (_pendingPreviews.Count > 0)
		{
			_previewDelay += dt;
			while (_previewDelay >= 0.75f)
			{
				_previewDelay -= 0.75f;
				(DuelScene_CDC, MtgCardInstance, CardPrintingData) tuple = _pendingPreviews[0];
				_pendingPreviews.RemoveAt(0);
				_activePreviews.Add(tuple.Item1);
				if (TryGetFlipPoint(tuple, out var flipPoint))
				{
					_splineMovementSystem.AddTemporaryGoal(tuple.Item1.Root, flipPoint, AllowInteractionType.Never);
				}
			}
		}
		if (_pendingPreviews.Count == 0 && _activePreviews.Count == 0)
		{
			_previewDelay += dt;
			if (_previewDelay >= 0.75f)
			{
				_previewDelay = 0f;
				CleanUp();
			}
		}
	}

	public void CleanUp()
	{
		if (_isDone)
		{
			return;
		}
		_isDone = true;
		foreach (DuelScene_CDC createdCdc in _createdCdcs)
		{
			if ((bool)createdCdc)
			{
				_cardViewBuilder.DeleteCard(createdCdc.InstanceId);
			}
		}
		_createdCdcs.Clear();
		foreach (var pendingPreview in _pendingPreviews)
		{
			if ((bool)pendingPreview.cardView && (bool)pendingPreview.cardView.Root)
			{
				_splineMovementSystem.RemoveTemporaryGoal(pendingPreview.cardView.Root);
			}
		}
		_pendingPreviews.Clear();
		foreach (DuelScene_CDC activePreview in _activePreviews)
		{
			if ((bool)activePreview)
			{
				_splineMovementSystem.RemoveTemporaryGoal(activePreview.Root);
			}
		}
		_activePreviews.Clear();
		foreach (CDCPart_AnimatedCardback animatedCardBack in _animatedCardBacks)
		{
			animatedCardBack.gameObject.SetActive(value: true);
		}
		_animatedCardBacks.Clear();
	}
}
