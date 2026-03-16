using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Core.Meta.MainNavigation.BoosterChamber;

public class BoosterCardSpawner : MonoBehaviour
{
	[SerializeField]
	private BoosterCardHolder _boosterCardHolder;

	private BoosterMetaCardHolder _cardInputController;

	private List<Transform> _activeTweens;

	private Queue<BoosterCardHolder> _boosterCardHolderQueue = new Queue<BoosterCardHolder>();

	public void Init(BoosterMetaCardHolder cardInputController)
	{
		_cardInputController = cardInputController;
	}

	public BoosterCardHolder GetBoosterCardHolder(Transform targetTransform, bool InFinalPosition)
	{
		BoosterCardHolder boosterCardHolder = _getBoosterCardHolderFromPool(targetTransform);
		boosterCardHolder.transform.localScale = new Vector3(1f, 1f, 1f);
		if (!InFinalPosition)
		{
			boosterCardHolder.transform.localPosition = targetTransform.InverseTransformPoint(base.transform.position);
			boosterCardHolder.transform.localRotation = base.transform.rotation;
			boosterCardHolder.transform.Rotate(0f, 180f, 0f);
		}
		else
		{
			boosterCardHolder.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		return boosterCardHolder;
	}

	private BoosterCardHolder _getBoosterCardHolderFromPool(Transform targetTransform)
	{
		BoosterCardHolder boosterCardHolder;
		if (_boosterCardHolderQueue.Count > 0)
		{
			boosterCardHolder = _boosterCardHolderQueue.Dequeue();
			boosterCardHolder.gameObject.transform.SetParent(targetTransform, worldPositionStays: false);
			boosterCardHolder.gameObject.SetActive(value: true);
		}
		else
		{
			boosterCardHolder = Object.Instantiate(_boosterCardHolder, targetTransform).GetComponent<BoosterCardHolder>();
		}
		boosterCardHolder.InteractionsAllowed = true;
		return boosterCardHolder;
	}

	public void ReturnBoosterCardHolderToPool(BoosterCardHolder boosterCardHolder)
	{
		boosterCardHolder.transform.DOKill();
		boosterCardHolder.ResetCardViewsAndHiddenStatus();
		boosterCardHolder.gameObject.SetActive(value: false);
		boosterCardHolder.gameObject.transform.SetParent(null);
		_boosterCardHolderQueue.Enqueue(boosterCardHolder);
	}

	public BoosterCardHolder SpawnCard(CardDataAndRevealStatus cardData, Transform targetTransform, BoosterMetaCardView emptyCardView, float movementTiming, AnimationCurve movementEase, float flipTiming, AnimationCurve flipEase, BoosterMetaCardView rebalancedCardView = null)
	{
		BoosterCardHolder boosterCardHolder = SetupCardPrefab(cardData, GetBoosterCardHolder(targetTransform, cardData.InFinalPosition), emptyCardView, rebalancedCardView);
		if (!cardData.Revealed)
		{
			boosterCardHolder.HideCard(cardData.NeedsAnticipation);
			if (cardData.AutoReveal)
			{
				boosterCardHolder.RevealCard(cardData.NeedsAnticipation, flipTiming, flipEase);
			}
		}
		else
		{
			boosterCardHolder.RevealCard(anticipation: false, 0f);
		}
		if (!cardData.InFinalPosition)
		{
			boosterCardHolder.transform.DOLocalMove(Vector3.zero, movementTiming).SetEase(movementEase).OnComplete(delegate
			{
				OnTweenComplete(boosterCardHolder.transform);
			});
			boosterCardHolder.transform.DOLocalRotate(Vector3.zero, movementTiming).SetEase(movementEase);
			_activeTweens.Add(boosterCardHolder.transform);
			cardData.InFinalPosition = true;
		}
		return boosterCardHolder;
	}

	private BoosterCardHolder SetupCardPrefab(CardDataAndRevealStatus data, BoosterCardHolder parent, BoosterMetaCardView cardView, BoosterMetaCardView rebalancedCardView = null)
	{
		parent.AddCard(cardView);
		parent.SetCardDataAndRevealStatus(data);
		cardView.SetData(data.CardData);
		cardView.Holder = _cardInputController;
		if (data.RebalancedCardData != null)
		{
			parent.AddCard(rebalancedCardView);
			rebalancedCardView.SetData(data.RebalancedCardData);
			rebalancedCardView.Holder = _cardInputController;
		}
		return parent;
	}

	private void Start()
	{
		_activeTweens = new List<Transform>();
	}

	public void StopActiveTweens()
	{
		if (_activeTweens != null)
		{
			for (int num = _activeTweens.Count - 1; num > -1; num--)
			{
				if (DOTween.IsTweening(_activeTweens[num]))
				{
					_activeTweens[num].DOComplete();
				}
			}
		}
		_activeTweens = new List<Transform>();
	}

	private void OnTweenComplete(Transform tweenTransform)
	{
		if (_activeTweens != null && _activeTweens.Contains(tweenTransform))
		{
			_activeTweens.Remove(tweenTransform);
		}
	}

	public void Cleanup()
	{
		while (_boosterCardHolderQueue.Count > 0)
		{
			Object.Destroy(_boosterCardHolderQueue.Dequeue().gameObject);
		}
	}

	private void OnDestroy()
	{
		Cleanup();
		_activeTweens = null;
	}
}
