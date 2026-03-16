using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.Input;
using Core.Meta.MainNavigation.Store;
using GreClient.CardData;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class NPEContentControllerRewards : ContentControllerRewards
{
	[Serializable]
	public struct InspectionCard
	{
		public uint GrpID;

		public uint unlockOnStage;
	}

	[Header("Reward Chest")]
	[SerializeField]
	private bool _ignoreManaAnimations;

	[SerializeField]
	private GameObject[] _keyPrefabs;

	[SerializeField]
	private Transform[] _keyAnchors;

	[SerializeField]
	private GameObject[] _keyUnlockedEffects;

	[SerializeField]
	private Animator _chestAnimator;

	[SerializeField]
	private GameObject _deckPreviewPopup;

	[SerializeField]
	private Animator _deckPreviewPopupAnimator;

	[Header("Deck Inspector")]
	[SerializeField]
	private RewardDisplayCard _individualCardRewardPrefab;

	[SerializeField]
	private Animator _deckBoxAnimator;

	[SerializeField]
	private GameObject _deckInspectionContainer;

	[SerializeField]
	private Transform _deckInspectionParent;

	[SerializeField]
	private BoosterMetaCardHolder _deckInspectionCardHolder;

	[SerializeField]
	private InspectionCard[] _deckInspectionCards;

	[SerializeField]
	private GameObject _lockedCardPrefab;

	private int _currentKeyIndex;

	private GameObject _currentKey;

	[HideInInspector]
	public bool isPuttingAwayRewards;

	[SerializeField]
	private Animator _NPEObjectivesStateMachineAnimator;

	private static readonly int IsOpen = Animator.StringToHash("IsOpen");

	private static readonly int IsHovering = Animator.StringToHash("isHovering");

	private static readonly int IsRevealed = Animator.StringToHash("IsRevealed");

	private static readonly int Unlock = Animator.StringToHash("Unlock");

	private static readonly int Steal = Animator.StringToHash("Steal");

	private static readonly int FinishedSteal = Animator.StringToHash("FinishedSteal");

	private static readonly int MouseOver = Animator.StringToHash("MouseOver");

	private static readonly int Outro = Animator.StringToHash("Outro");

	private static readonly int FinishedMove = Animator.StringToHash("FinishedMove");

	private static readonly int ExitNpeTutorial = Animator.StringToHash("ExitNPETutorial");

	public override PriorityLevelEnum Priority => PriorityLevelEnum.DuelScene_PopUps;

	public void NPEInit(CardDatabase cardDatabase, AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoom, CardViewBuilder cardViewBuilder, KeyboardManager keyboardManager, IActionSystem actionSystem)
	{
		Init(null, assetLookupSystem, zoom, keyboardManager, actionSystem, cardDatabase, cardViewBuilder);
		_deckInspectionCardHolder.EnsureInit(base.CardDatabase, cardViewBuilder);
		_deckInspectionCardHolder.RolloverZoomView = base.ZoomHandler;
		_deckInspectionCardHolder.ShowHighlight = (MetaCardView cardView) => false;
	}

	public void SetUp(int setUpTo)
	{
		for (int i = 0; i < setUpTo; i++)
		{
			UnityEngine.Object.Instantiate(_keyPrefabs[i], _keyAnchors[i]);
		}
	}

	public void CleanUp()
	{
		if (_currentKey != null)
		{
			UnityEngine.Object.Destroy(_currentKey);
		}
		GameObject[] keyUnlockedEffects = _keyUnlockedEffects;
		for (int i = 0; i < keyUnlockedEffects.Length; i++)
		{
			keyUnlockedEffects[i].SetActive(value: false);
		}
	}

	public void SetUpUnlockedCards(int unlockUpTo)
	{
		_deckInspectionParent.DestroyChildren();
		_deckInspectionCardHolder.ClearCards();
		CardCollection cardCollection = new CardCollection(_cardHolder.CardDatabase);
		InspectionCard[] deckInspectionCards = _deckInspectionCards;
		for (int i = 0; i < deckInspectionCards.Length; i++)
		{
			InspectionCard inspectionCard = deckInspectionCards[i];
			if (inspectionCard.unlockOnStage <= unlockUpTo)
			{
				CardData card = AddCardToDeckInspector(inspectionCard.GrpID);
				cardCollection.Add(card, 1);
				continue;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(_lockedCardPrefab, _deckInspectionParent);
			gameObject.transform.localScale = new Vector3(3.45f, 3.45f, 3.45f);
			switch (inspectionCard.unlockOnStage)
			{
			case 1u:
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "I";
				break;
			case 2u:
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "II";
				break;
			case 3u:
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "III";
				break;
			case 4u:
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "IV";
				break;
			case 5u:
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "V";
				break;
			}
		}
		_deckInspectionCardHolder.SetCards(cardCollection);
	}

	public void PutAwayRewards()
	{
		if ((!HaveAnyRewardsToShow() || SubPageDisplayed) && !isPuttingAwayRewards)
		{
			isPuttingAwayRewards = true;
			StartCoroutine(Coroutine_PutAwayRewards());
		}
	}

	private CardData AddCardToDeckInspector(uint grpID, int siblingIndex = -1)
	{
		RewardDisplayCard rewardDisplayCard = UnityEngine.Object.Instantiate(_individualCardRewardPrefab, _deckInspectionParent);
		if (siblingIndex >= 0)
		{
			rewardDisplayCard.transform.SetSiblingIndex(siblingIndex);
		}
		CardData cardData = new CardData(null, base.CardDatabase.CardDataProvider.GetCardPrintingById(grpID));
		CDCMetaCardView cDCMetaCardView = UnityEngine.Object.Instantiate(_cardPrefab, rewardDisplayCard.transform);
		cDCMetaCardView.InitWithData(cardData, base.CardDatabase, base.CardViewBuilder);
		Meta_CDC cardView = cDCMetaCardView.CardView;
		Transform obj = cardView.transform;
		obj.SetParent(rewardDisplayCard.CardParent1.transform);
		obj.localPosition = Vector3.zero;
		obj.localScale = new Vector3(0.925f, 0.925f, 0.925f);
		obj.localRotation = Quaternion.Euler(Vector3.zero);
		rewardDisplayCard.card = cardView;
		rewardDisplayCard.SetRarity(cardView.Model.Rarity, cardData.Rarity);
		cDCMetaCardView.Holder = _deckInspectionCardHolder;
		CardRolloverZoomHandler component = rewardDisplayCard.GetComponent<CardRolloverZoomHandler>();
		component.ZoomView = base.ZoomHandler;
		component.Card = cDCMetaCardView.Card;
		component.CardCollider = cardView.GetComponentInChildren<Collider>();
		return cDCMetaCardView.Card;
	}

	public void DisableDeckPreviewPopup()
	{
		_deckPreviewPopup.SetActive(value: false);
	}

	public void EnableDeckPreviewPopup()
	{
		_deckPreviewPopup.SetActive(value: true);
	}

	public void RewardsChestStartHover()
	{
		_chestAnimator.SetBool(IsHovering, value: true);
	}

	public void RewardsChestEndHover()
	{
		_chestAnimator.SetBool(IsHovering, value: false);
	}

	public void RewardsChestToggleHover()
	{
		bool flag = _chestAnimator.GetBool(IsHovering);
		_chestAnimator.SetBool(IsHovering, !flag);
	}

	public void ShowDeckPreviewPopup()
	{
		_deckPreviewPopupAnimator.SetBool(IsRevealed, value: true);
		_NPEObjectivesStateMachineAnimator.enabled = false;
	}

	public void HideDeckPreviewPopup()
	{
		_deckPreviewPopupAnimator.SetBool(IsRevealed, value: false);
		_NPEObjectivesStateMachineAnimator.enabled = true;
	}

	public void ShowHideDeckPreviewPopup()
	{
		bool flag = _deckPreviewPopupAnimator.GetBool(IsRevealed);
		_deckPreviewPopupAnimator.SetBool(IsRevealed, !flag);
		_NPEObjectivesStateMachineAnimator.enabled = flag;
	}

	public IEnumerator Coroutine_UnlockAnimation()
	{
		yield return new WaitUntil(() => !isPuttingAwayRewards);
		_chestAnimator.SetTrigger(Unlock);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_chest_open, _chestAnimator.gameObject);
		yield return new WaitUntil(() => _chestAnimator.GetCurrentAnimatorStateInfo(1).IsName("Unlock") && _chestAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.999f);
	}

	public IEnumerator Coroutine_CreateKey(int keyIndex, Vector3 position, Quaternion rotation)
	{
		if (!_ignoreManaAnimations)
		{
			if (_currentKey != null)
			{
				UnityEngine.Object.Destroy(_currentKey);
			}
			_currentKey = UnityEngine.Object.Instantiate(_keyPrefabs[keyIndex], position, rotation);
			_currentKey.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			yield return new WaitForEndOfFrame();
		}
	}

	public void AwardAllKeys()
	{
		for (int i = 0; i < _keyAnchors.Length; i++)
		{
			if (_keyAnchors[i].childCount == 0)
			{
				UnityEngine.Object.Instantiate(_keyPrefabs[i], _keyAnchors[i]).transform.localPosition = default(Vector3);
			}
		}
	}

	public IEnumerator Coroutine_AwardKey(int keyIndex)
	{
		if (_ignoreManaAnimations)
		{
			UnityEngine.Object.Instantiate(_keyPrefabs[keyIndex], _keyAnchors[keyIndex]).transform.localPosition = default(Vector3);
			_keyUnlockedEffects[keyIndex].SetActive(value: true);
		}
		else if (!(_currentKey == null))
		{
			_rewardsTitleText.gameObject.SetActive(value: false);
			_currentKeyIndex = keyIndex;
			Animator keyAnimator = _currentKey.GetComponent<Animator>();
			base.Visible = true;
			TravelSMB[] behaviours = keyAnimator.GetBehaviours<TravelSMB>();
			for (int i = 0; i < behaviours.Length; i++)
			{
				behaviours[i].transformDestination = _rewardParent;
			}
			keyAnimator.SetTrigger(Steal);
			yield return new WaitUntil(() => keyAnimator.GetBool(FinishedSteal));
			yield return new WaitUntil(() => isPuttingAwayRewards);
			yield return new WaitUntil(() => !isPuttingAwayRewards);
			_rewardsTitleText.gameObject.SetActive(value: true);
		}
	}

	public void HideDeckInspector(Animator npeAnimator)
	{
		_deckInspectionContainer.gameObject.SetActive(value: false);
		npeAnimator.enabled = true;
	}

	private IEnumerator Coroutine_PutAwayRewards()
	{
		bool hasCards = false;
		bool hasDecks = false;
		bool hasKeys = false;
		List<GameObject> rewards = new List<GameObject>();
		foreach (Transform item in _rewardParent)
		{
			rewards.Add(item.gameObject);
		}
		bool flag = false;
		foreach (GameObject item2 in rewards)
		{
			if (item2.name.Contains("Deck") && item2.GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("RewardInteract_DeckBoxLid_Close"))
			{
				flag = true;
				item2.GetComponent<Animator>().SetTrigger(MouseOver);
				item2.GetComponent<MetaDeckView>().TriggerOpenEffect();
			}
		}
		if (flag)
		{
			isPuttingAwayRewards = false;
			yield break;
		}
		if (SubPageDisplayed)
		{
			SubPageDisplayed = false;
			isPuttingAwayRewards = false;
			yield break;
		}
		foreach (GameObject item3 in rewards)
		{
			if (!item3.name.Contains("Deck"))
			{
				item3.GetComponent<Animator>().SetTrigger(Outro);
			}
			if (item3.name.Contains("Card"))
			{
				hasCards = true;
				Vector3 position = item3.transform.position;
				item3.transform.SetParent(null);
				item3.transform.position = new Vector3(position.x, position.y, 0f);
				if (!_deckBoxAnimator.GetBool(IsOpen))
				{
					_deckBoxAnimator.SetBool(IsOpen, value: true);
				}
			}
		}
		GetComponent<Animator>().SetTrigger(Outro);
		if (_currentKey != null)
		{
			hasKeys = true;
			Animator component = _currentKey.GetComponent<Animator>();
			TravelSMB[] behaviours = component.GetBehaviours<TravelSMB>();
			for (int i = 0; i < behaviours.Length; i++)
			{
				behaviours[i].transformDestination = _keyAnchors[_currentKeyIndex];
			}
			component.SetTrigger(Outro);
			_currentKey.transform.parent = _keyAnchors[_currentKeyIndex];
		}
		yield return new WaitUntil(delegate
		{
			bool flag2 = true;
			foreach (GameObject item4 in rewards)
			{
				if (item4 != null && item4 != null && !item4.GetComponent<Animator>().GetBool(FinishedMove))
				{
					if (item4.name.Contains("Card"))
					{
						CustomButton component2 = item4.GetComponent<CustomButton>();
						if (component2 != null)
						{
							component2.enabled = false;
						}
						item4.GetComponent<Animator>().SetTrigger(Outro);
					}
					flag2 = false;
				}
			}
			bool flag3 = true;
			if (_rewardParent.childCount > 0 && _rewardParent.GetChild(0).name.Contains("Deck"))
			{
				hasDecks = true;
				if (!_NPEObjectivesStateMachineAnimator.GetCurrentAnimatorStateInfo(0).IsName("ExitNPETutorial"))
				{
					flag3 = false;
					_NPEObjectivesStateMachineAnimator.SetTrigger(ExitNpeTutorial);
				}
				else if (_NPEObjectivesStateMachineAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
				{
					flag3 = false;
				}
			}
			bool flag4 = _currentKey == null || _currentKey.GetComponent<Animator>().GetBool(FinishedMove);
			return (!hasCards || flag2) && (!hasKeys || flag4) && (!hasDecks || flag3);
		});
		if (_currentKey != null)
		{
			_currentKey.transform.localPosition = default(Vector3);
			_keyUnlockedEffects[_currentKeyIndex].SetActive(value: true);
			_currentKey = null;
		}
		Clear();
		isPuttingAwayRewards = false;
		rewards.Clear();
		_deckBoxAnimator.SetBool(IsOpen, value: false);
	}

	public override bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			PutAwayRewards();
			return true;
		}
		return false;
	}

	public override void OnBack(ActionContext context)
	{
		PutAwayRewards();
	}

	protected override IEnumerator DisplayRewards(string title, string claimButtonText, bool fromEndOfSeasonRewards = false)
	{
		_cardReward.SetPrefab(_individualCardRewardPrefab);
		yield return base.DisplayRewards(title, claimButtonText, fromEndOfSeasonRewards);
	}
}
