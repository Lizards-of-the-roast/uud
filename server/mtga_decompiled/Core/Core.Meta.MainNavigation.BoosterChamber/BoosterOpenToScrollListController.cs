using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.BoosterChamber;

public class BoosterOpenToScrollListController : MonoBehaviour
{
	public BoosterCardSpawner cardSpawner;

	public RewardScrollList scrollList;

	[SerializeField]
	private BoosterMetaCardView _cardPrefab;

	private int _currentInitialTransformIndex;

	private int _currentCardIndex;

	[SerializeField]
	private List<TimingCurvesByCardCount> _timingCurvesByCardCount = new List<TimingCurvesByCardCount>();

	[SerializeField]
	private BoosterMetaCardHolder _cardInputController;

	[Tooltip("This is the starting position from 0 - 1. 0 will start content fully scrolled left, 1 will start content fully scrolled right.")]
	[SerializeField]
	private float _startingNormalizedX;

	[SerializeField]
	private Toggle _alwaysAutoRevealToggle;

	[SerializeField]
	private Toggle _alwaysSkipAnimationToggle;

	[SerializeField]
	private MainButton _dismissCardsButton;

	[SerializeField]
	private CustomButton _skipButton;

	[SerializeField]
	private List<GameObject> _placeholderCards;

	private Action _onOpenAnimationSequenceComplete;

	private List<CardDataAndRevealStatus> _cardsToOpen;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private Coroutine _nextCardRoutine;

	private string _userAccountID;

	private bool _alwaysAutoReveal;

	private bool _alwaysSkipAnimation;

	private bool _animationSequenceActiveField;

	private List<TransformAndIndex> _initialTransforms;

	private float _elapsedTime;

	private DateTime _lastDisplayTime;

	private int _delayBackup;

	private ICardRolloverZoom _cardRolloverZoomBase;

	private Dictionary<int, BoosterCardHolder> _onScreenboosterCardHoldersWithIndex = new Dictionary<int, BoosterCardHolder>();

	private Action _dismissCardsOffScreen;

	private float _totalCardCount;

	private TimingCurvesByCardCount _timingCurves;

	private BoosterMetaCardViewPool _boosterMetaCardViewPool;

	private Coroutine _revealAllCoroutine;

	private bool _animationSequenceActive
	{
		get
		{
			return _animationSequenceActiveField;
		}
		set
		{
			_animationSequenceActiveField = value;
			scrollList.ManualScrollDragAllowed = !value;
		}
	}

	public void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, ICardRolloverZoom cardRolloverZoomBase, Action dismissCards, BoosterMetaCardViewPool cardViewPool)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardRolloverZoomBase = cardRolloverZoomBase;
		_cardInputController.EnsureInit(_cardDatabase, _cardViewBuilder);
		_cardInputController.SendDragEventsUp = true;
		_cardInputController.RolloverZoomView = _cardRolloverZoomBase;
		cardSpawner.Init(_cardInputController);
		_alwaysAutoReveal = MDNPlayerPrefs.GetBoosterPackOpenAutoReveal();
		_alwaysAutoRevealToggle.onValueChanged.AddListener(OnAutoRevealToggle);
		_alwaysAutoRevealToggle.isOn = _alwaysAutoReveal;
		_alwaysAutoRevealToggle.interactable = true;
		_alwaysSkipAnimation = MDNPlayerPrefs.GetBoosterPackOpenSkipAnimation();
		_alwaysSkipAnimationToggle.onValueChanged.AddListener(OnSkipAnimationToggle);
		_alwaysSkipAnimationToggle.isOn = _alwaysSkipAnimation;
		_alwaysSkipAnimationToggle.interactable = true;
		_dismissCardsOffScreen = dismissCards;
		_boosterMetaCardViewPool = cardViewPool;
	}

	public void SetCardsToDisplay(List<CardDataAndRevealStatus> cardsToOpen)
	{
		Reset();
		CleanupCardsToOpen();
		_cardsToOpen = cardsToOpen;
		_totalCardCount = cardsToOpen.Count;
		_currentCardIndex = cardsToOpen.Count;
		_timingCurves = GetSpeedCurveForCardCount(cardsToOpen.Count);
	}

	private TimingCurvesByCardCount GetSpeedCurveForCardCount(int count)
	{
		TimingCurvesByCardCount timingCurvesByCardCount = null;
		foreach (TimingCurvesByCardCount item in _timingCurvesByCardCount)
		{
			if (item.CardCount <= count)
			{
				timingCurvesByCardCount = item;
			}
		}
		if (timingCurvesByCardCount == null)
		{
			Debug.LogWarning("BoosterOpenToScrollListController.cs is using default animation curves.");
			AnimationCurve animationCurve = new AnimationCurve();
			timingCurvesByCardCount = new TimingCurvesByCardCount
			{
				CardCount = 0,
				CardSpawnTimingCurve = animationCurve,
				defaultTimingCurves = new TimingCurvesByRarity
				{
					CardFlipEase = animationCurve,
					CardFlipTiming = animationCurve,
					CardMovementEase = animationCurve,
					CardMovementTiming = animationCurve
				}
			};
		}
		return timingCurvesByCardCount;
	}

	public void StartBoosterOpenAnimationSequence()
	{
		StartBoosterOpenAnimationSequence(null);
	}

	public void StartBoosterOpenAnimationSequence(Action onComplete = null)
	{
		Reset();
		scrollList.gameObject.SetActive(value: true);
		_onOpenAnimationSequenceComplete = (Action)Delegate.Combine(_onOpenAnimationSequenceComplete, onComplete);
		RewardScrollList rewardScrollList = scrollList;
		rewardScrollList.layoutUpdateComplete = (Action)Delegate.Combine(rewardScrollList.layoutUpdateComplete, new Action(OnScrollLayoutUpdated));
		scrollList.SetTotalRewardCount(_cardsToOpen.Count);
		_initialTransforms = new List<TransformAndIndex>();
		if (_alwaysSkipAnimation)
		{
			RevealSequenceComplete();
			SnapToNormalizedX(0f);
		}
		else
		{
			_animationSequenceActive = true;
			SetSkipActive(active: true);
		}
		_onScreenboosterCardHoldersWithIndex = new Dictionary<int, BoosterCardHolder>();
	}

	public void StopBoosterOpenAnimationSequence()
	{
		if (!_animationSequenceActive)
		{
			return;
		}
		foreach (CardDataAndRevealStatus item in _cardsToOpen)
		{
			item.InFinalPosition = true;
			if (item.AutoReveal)
			{
				item.Revealed = true;
			}
		}
		RevealSequenceComplete();
		SnapToNormalizedX(0f);
	}

	public void SnapToNormalizedX(float normalizedX)
	{
		Reset();
		RewardScrollList rewardScrollList = scrollList;
		rewardScrollList.transformOnScreen = (Action<TransformAndIndex>)Delegate.Combine(rewardScrollList.transformOnScreen, new Action<TransformAndIndex>(OnTransformOnScreen));
		RewardScrollList rewardScrollList2 = scrollList;
		rewardScrollList2.transformOffScreen = (Action<TransformAndIndex>)Delegate.Combine(rewardScrollList2.transformOffScreen, new Action<TransformAndIndex>(OnTransformOffScreen));
		scrollList.SnapToNormalizedX(normalizedX);
	}

	public void ClearCardsOffScreen()
	{
		Reset();
		CleanupCardsToOpen();
		scrollList.gameObject.SetActive(value: false);
		scrollList.ClearAllTransforms();
	}

	public int GetUnrevealedCardCount()
	{
		return _cardsToOpen.Count((CardDataAndRevealStatus cardData) => !cardData.Revealed);
	}

	public void RevealAllCards()
	{
		if (_revealAllCoroutine == null)
		{
			_revealAllCoroutine = StartCoroutine(RevealAllCoroutine());
		}
	}

	private IEnumerator RevealAllCoroutine()
	{
		for (int i = 0; i < _cardsToOpen.Count; i++)
		{
			CardDataAndRevealStatus cardDataAndRevealStatus = _cardsToOpen[i];
			if (!cardDataAndRevealStatus.Revealed)
			{
				cardDataAndRevealStatus.Revealed = true;
				if (_onScreenboosterCardHoldersWithIndex.TryGetValue(i, out var value))
				{
					value.RevealCard(cardDataAndRevealStatus.NeedsAnticipation);
					yield return new WaitForSeconds(0.05f);
				}
			}
		}
		_revealAllCoroutine = null;
	}

	private void SetSkipActive(bool active)
	{
		if (_skipButton != null)
		{
			_skipButton.gameObject.SetActive(active);
			if (active)
			{
				_skipButton.OnClick.AddListener(StopBoosterOpenAnimationSequence);
			}
			else
			{
				_skipButton.OnClick.RemoveListener(StopBoosterOpenAnimationSequence);
			}
		}
	}

	private void OnAutoRevealToggle(bool newValue)
	{
		MDNPlayerPrefs.SetBoosterPackOpenAutoReveal(newValue);
		_alwaysAutoReveal = newValue;
	}

	private void OnSkipAnimationToggle(bool newValue)
	{
		MDNPlayerPrefs.SetBoosterPackOpenSkipAnimation(newValue);
		_alwaysSkipAnimation = newValue;
		if (_animationSequenceActive && newValue)
		{
			StopBoosterOpenAnimationSequence();
		}
	}

	private void OnScrollLayoutUpdated()
	{
		RewardScrollList rewardScrollList = scrollList;
		rewardScrollList.layoutUpdateComplete = (Action)Delegate.Remove(rewardScrollList.layoutUpdateComplete, new Action(OnScrollLayoutUpdated));
		scrollList.SnapToNormalizedX(_startingNormalizedX);
		_initialTransforms = scrollList.GetOnScreenTransforms();
		_currentInitialTransformIndex = _initialTransforms.Count - 1;
		_elapsedTime = 0f;
	}

	private void Update()
	{
		if (_animationSequenceActive && _currentInitialTransformIndex >= 0)
		{
			_elapsedTime += Time.deltaTime;
			if (_elapsedTime >= GetCurrentSecondsBetweenCards())
			{
				DisplayNextInitialCard();
				_elapsedTime = 0f;
			}
		}
	}

	private void _returnCardViewAndHolderToPool(BoosterCardHolder cardHolder)
	{
		foreach (BoosterMetaCardView cardView in cardHolder.CardViews)
		{
			cardView.gameObject.SetActive(value: false);
			_boosterMetaCardViewPool.ReturnCardView(cardView);
		}
		cardSpawner.ReturnBoosterCardHolderToPool(cardHolder);
	}

	private void DisplayNextInitialCard()
	{
		DisplayCard(_initialTransforms[_currentInitialTransformIndex], _animationSequenceActive);
		_currentInitialTransformIndex--;
		if (_currentInitialTransformIndex < 0)
		{
			StartAutoScroll();
		}
	}

	private void StartAutoScroll()
	{
		RewardScrollList rewardScrollList = scrollList;
		rewardScrollList.transformOnScreen = (Action<TransformAndIndex>)Delegate.Combine(rewardScrollList.transformOnScreen, new Action<TransformAndIndex>(OnTransformOnScreen));
		RewardScrollList rewardScrollList2 = scrollList;
		rewardScrollList2.transformOffScreen = (Action<TransformAndIndex>)Delegate.Combine(rewardScrollList2.transformOffScreen, new Action<TransformAndIndex>(OnTransformOffScreen));
		scrollList.AutoScroll(GetCurrentSecondsBetweenCards, RevealSequenceComplete);
	}

	private float GetCurrentSecondsBetweenCards()
	{
		float a = Math.Abs((float)_currentCardIndex - _totalCardCount) / _totalCardCount;
		float num = _timingCurves.CardSpawnTimingCurve.Evaluate(Mathf.Min(a, 1f));
		if (!(num > 0f))
		{
			return 0.001f;
		}
		return num;
	}

	private float GetCurrentMovementSpeed()
	{
		float a = Math.Abs((float)_currentCardIndex - _totalCardCount) / _totalCardCount;
		return _timingCurves.defaultTimingCurves.CardMovementTiming.Evaluate(Mathf.Min(a, 1f));
	}

	private AnimationCurve GetCurrentMovementEase()
	{
		return _timingCurves.defaultTimingCurves.CardMovementEase;
	}

	private float GetCurrentFlipSpeed()
	{
		float a = Math.Abs((float)_currentCardIndex - _totalCardCount) / _totalCardCount;
		return _timingCurves.defaultTimingCurves.CardFlipTiming.Evaluate(Mathf.Min(a, 1f));
	}

	private AnimationCurve GetCurrentFlipEase()
	{
		return _timingCurves.defaultTimingCurves.CardFlipEase;
	}

	private void RevealSequenceComplete()
	{
		SetSkipActive(active: false);
		EnableAllCardInteractions();
		if (_onOpenAnimationSequenceComplete != null)
		{
			_onOpenAnimationSequenceComplete();
			_onOpenAnimationSequenceComplete = null;
		}
		CustomButton component = _dismissCardsButton.GetComponent<CustomButton>();
		component.OnClick.RemoveListener(DismissCards);
		component.OnClick.AddListener(DismissCards);
		_animationSequenceActive = false;
	}

	private void OnTransformOnScreen(TransformAndIndex transformIndex)
	{
		if (_animationSequenceActive && !_alwaysSkipAnimation)
		{
			double totalSeconds = DateTime.Now.Subtract(_lastDisplayTime).TotalSeconds;
			float currentSecondsBetweenCards = GetCurrentSecondsBetweenCards();
			if (totalSeconds >= (double)currentSecondsBetweenCards)
			{
				DisplayCard(transformIndex, _animationSequenceActive);
				return;
			}
			_delayBackup++;
			double num = (double)(currentSecondsBetweenCards * (float)_delayBackup) - totalSeconds;
			StartCoroutine(DelayedCardDisplay(transformIndex, (float)num, _animationSequenceActive));
		}
		else
		{
			DisplayCard(transformIndex, _animationSequenceActive);
		}
	}

	private void OnTransformOffScreen(TransformAndIndex transformIndex)
	{
		Debug.Log("Index Reveal complete: " + transformIndex.index);
		if (_onScreenboosterCardHoldersWithIndex.TryGetValue(transformIndex.index, out var value))
		{
			_returnCardViewAndHolderToPool(value);
			value.InteractionsAllowed = true;
			_onScreenboosterCardHoldersWithIndex.Remove(transformIndex.index);
			return;
		}
		foreach (Transform item in transformIndex.transform)
		{
			_returnCardViewAndHolderToPool(item.gameObject.GetComponent<BoosterCardHolder>());
		}
	}

	private void DisplayCard(TransformAndIndex transformIndex, bool animate)
	{
		_currentCardIndex = transformIndex.index;
		_lastDisplayTime = DateTime.Now;
		CardDataAndRevealStatus cardDataAndRevealStatus = _cardsToOpen[transformIndex.index];
		BoosterMetaCardView rebalancedCardView = null;
		if (cardDataAndRevealStatus.RebalancedCardData != null)
		{
			rebalancedCardView = _boosterMetaCardViewPool.GetCardView();
		}
		BoosterCardHolder boosterCardHolder = cardSpawner.SpawnCard(cardDataAndRevealStatus, transformIndex.transform, _boosterMetaCardViewPool.GetCardView(), GetCurrentMovementSpeed(), GetCurrentMovementEase(), GetCurrentFlipSpeed(), GetCurrentFlipEase(), rebalancedCardView);
		_onScreenboosterCardHoldersWithIndex.Add(transformIndex.index, boosterCardHolder);
		boosterCardHolder.InteractionsAllowed = !_animationSequenceActive;
		UpdatePlaceholderCardVisibility(transformIndex.index);
	}

	private IEnumerator DelayedCardDisplay(TransformAndIndex transformAndIndex, float time, bool animationSequenceActive)
	{
		yield return new WaitForSeconds(time);
		if (_delayBackup > 0)
		{
			_delayBackup--;
			DisplayCard(transformAndIndex, animationSequenceActive);
		}
	}

	private void UpdatePlaceholderCardVisibility(int currentIndex)
	{
		if (currentIndex < _placeholderCards.Count)
		{
			_placeholderCards[currentIndex].SetActive(value: false);
		}
	}

	public void UpdateAllPlaceholderCardVisibity(bool visible)
	{
		foreach (GameObject placeholderCard in _placeholderCards)
		{
			placeholderCard.SetActive(visible);
		}
	}

	private void EnableAllCardInteractions()
	{
		foreach (KeyValuePair<int, BoosterCardHolder> item in _onScreenboosterCardHoldersWithIndex)
		{
			item.Value.InteractionsAllowed = true;
		}
	}

	private void ReturnOnScreenCardsHoldersToPool()
	{
		foreach (KeyValuePair<int, BoosterCardHolder> item in _onScreenboosterCardHoldersWithIndex)
		{
			_returnCardViewAndHolderToPool(item.Value);
		}
		_onScreenboosterCardHoldersWithIndex = new Dictionary<int, BoosterCardHolder>();
	}

	private void CleanupCardsToOpen()
	{
		if (_cardsToOpen == null)
		{
			return;
		}
		foreach (CardDataAndRevealStatus item in _cardsToOpen)
		{
			item.OnRevealed = null;
		}
		_cardsToOpen = null;
	}

	private void DismissCards()
	{
		if (_dismissCardsOffScreen != null)
		{
			_dismissCardsOffScreen();
		}
	}

	private void Reset()
	{
		if (_revealAllCoroutine != null)
		{
			StopCoroutine(_revealAllCoroutine);
		}
		_animationSequenceActive = false;
		if (cardSpawner != null)
		{
			cardSpawner.StopActiveTweens();
		}
		ReturnOnScreenCardsHoldersToPool();
		if (scrollList != null)
		{
			scrollList.StopScroll();
			RewardScrollList rewardScrollList = scrollList;
			rewardScrollList.layoutUpdateComplete = (Action)Delegate.Remove(rewardScrollList.layoutUpdateComplete, new Action(OnScrollLayoutUpdated));
			RewardScrollList rewardScrollList2 = scrollList;
			rewardScrollList2.transformOnScreen = (Action<TransformAndIndex>)Delegate.Remove(rewardScrollList2.transformOnScreen, new Action<TransformAndIndex>(OnTransformOnScreen));
			RewardScrollList rewardScrollList3 = scrollList;
			rewardScrollList3.transformOffScreen = (Action<TransformAndIndex>)Delegate.Remove(rewardScrollList3.transformOffScreen, new Action<TransformAndIndex>(OnTransformOffScreen));
		}
		_delayBackup = 0;
		_currentInitialTransformIndex = -1;
		_onOpenAnimationSequenceComplete = null;
	}

	public void Cleanup()
	{
		Reset();
		if (cardSpawner != null)
		{
			cardSpawner.Cleanup();
		}
		if (_boosterMetaCardViewPool != null)
		{
			_boosterMetaCardViewPool.Clear();
		}
		if (_skipButton != null)
		{
			_skipButton.OnClick.RemoveListener(StopBoosterOpenAnimationSequence);
		}
		_dismissCardsButton.GetComponent<CustomButton>().OnClick.RemoveListener(DismissCards);
		_alwaysAutoRevealToggle.onValueChanged.RemoveListener(OnAutoRevealToggle);
		_alwaysSkipAnimationToggle.onValueChanged.RemoveListener(OnSkipAnimationToggle);
	}

	private void OnDestroy()
	{
		Cleanup();
	}
}
