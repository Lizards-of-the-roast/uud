using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.MainNavigation.RewardTrack;
using GreClient.CardData;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class RenewalPopup : PopupBase, IKeyDownSubscriber, IKeySubscriber
{
	[SerializeField]
	private CustomButton _openButton;

	[SerializeField]
	private CustomButton _moreButton;

	[SerializeField]
	private Transform _rewardsContainer;

	[Header("Prefabs for displaying Renewal Rewards")]
	private GameObject _cardRewardPrefab;

	[SerializeField]
	private RewardDisplayCard _cardReward;

	[SerializeField]
	private CDCMetaCardView _cardPrefab;

	[SerializeField]
	private BoosterMetaCardHolder _cardHolder;

	[SerializeField]
	private float _cardScale = 0.45f;

	[SerializeField]
	private float _sequenceDelay = 0.15f;

	[SerializeField]
	private int _cardsPerScreen = 7;

	[SerializeField]
	private float _initialRevealDelay = 3.2f;

	private ICardRolloverZoom _zoomHandler;

	private CardCollection _cardHolderCollection;

	private readonly Queue<RewardDisplayCard> revealed = new Queue<RewardDisplayCard>();

	private ClientInventoryUpdateReportItem FAKEDATA;

	private KeyboardManager _keyboardManager;

	private AssetLookupSystem _assetLookupSystem;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper_PopUps;

	public void Init(AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, ClientInventoryUpdateReportItem fakeData = null, KeyboardManager keyboardManager = null)
	{
		_cardHolderCollection = new CardCollection(cardDatabase);
		FAKEDATA = fakeData;
		_rewardsContainer.DestroyChildren();
		Activate(activate: true);
		_keyboardManager = keyboardManager;
		_keyboardManager?.Subscribe(this);
		_assetLookupSystem = assetLookupSystem;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_zoomHandler = SceneLoader.GetSceneLoader().GetCardZoomView();
		_cardHolder.RolloverZoomView = _zoomHandler;
		_cardHolder.ShowHighlight = (MetaCardView cardView) => false;
		_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_openButton.OnClick.AddListener(OnOpen);
		_moreButton.OnClick.AddListener(OnMore);
	}

	private void OnDestroy()
	{
		_openButton.OnClick.RemoveListener(OnOpen);
		_moreButton.OnClick.RemoveListener(OnMore);
		_keyboardManager?.Unsubscribe(this);
	}

	private void OnOpen()
	{
		if (FAKEDATA == null)
		{
			WrapperController.Instance.RenewalManager.RedeemRenewalRewards(ProcessClientInventoryUpdate);
		}
		else
		{
			ProcessClientInventoryUpdate(new List<ClientInventoryUpdateReportItem> { FAKEDATA });
		}
		AudioManager.PlayAudio("sfx_ui_renewal_rotation_egg_crackopen", base.gameObject);
		_keyboardManager?.Subscribe(this);
	}

	private void OnMore()
	{
		if (revealed.Count > 0)
		{
			StartCoroutine(RevealCardsCoroutine(_cardsPerScreen, _sequenceDelay));
			return;
		}
		ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(Pantry.Get<SetMasteryDataProvider>().CurrentBpName, NavContentType.Home, NavContentType.Home, playIntro: true);
		SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From Renewal");
		Activate(activate: false);
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		if (_openButton.gameObject.activeInHierarchy)
		{
			_openButton.Click();
		}
		if (_moreButton.gameObject.activeInHierarchy)
		{
			_moreButton.Click();
		}
	}

	private void ProcessClientInventoryUpdate(IEnumerable<ClientInventoryUpdateReportItem> inventoryUpdate)
	{
		IEnumerable<AetherizedCardInformation> enumerable = inventoryUpdate.SelectMany((ClientInventoryUpdateReportItem reportItem) => reportItem.aetherizedCards);
		IEnumerable<int> source = inventoryUpdate.SelectMany((ClientInventoryUpdateReportItem reportItem) => reportItem.delta.cardsAdded);
		if (enumerable.Count() == 0 && source.Count() > 0)
		{
			enumerable = source.Select((int grpId) => new AetherizedCardInformation
			{
				grpId = grpId
			}).ToList();
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.RewardType = RewardType.Card;
		RewardPrefabs payload = _assetLookupSystem.TreeLoader.LoadTree<RewardPrefabs>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		foreach (AetherizedCardInformation item in enumerable)
		{
			RewardDisplayCard component = AssetLoader.Instantiate(payload.Prefab, _rewardsContainer).GetComponent<RewardDisplayCard>();
			component.gameObject.SetActive(value: false);
			CardData data = ((item.gemsAwarded <= 0) ? new CardData(null, _cardDatabase.CardDataProvider.GetCardPrintingById((uint)item.grpId)) : CardDataExtensions.CreateRewardsCard(_cardDatabase, item.goldAwarded, item.gemsAwarded, item.set));
			CDCMetaCardView cDCMetaCardView = UnityEngine.Object.Instantiate(_cardPrefab, component.transform);
			cDCMetaCardView.Init(_cardDatabase, _cardViewBuilder);
			cDCMetaCardView.SetData(data);
			Meta_CDC cardView = cDCMetaCardView.CardView;
			Transform obj = cardView.transform;
			obj.SetParent(component.CardParent1.transform, worldPositionStays: false);
			obj.localScale = Vector3.one * _cardScale;
			component.card = cardView;
			component.SetRarity(cardView.Model.Rarity, cardView.Model.Rarity);
			cDCMetaCardView.Holder = _cardHolder;
			_cardHolderCollection.Add(cDCMetaCardView.Card, 1);
			CardRolloverZoomHandler component2 = component.GetComponent<CardRolloverZoomHandler>();
			component2.ZoomView = _zoomHandler;
			component2.Card = cDCMetaCardView.Card;
			component2.CardCollider = cardView.GetComponentInChildren<Collider>();
			component.AutoFlip = true;
			revealed.Enqueue(component);
		}
		_cardHolder.SetCards(_cardHolderCollection);
		StartCoroutine(RevealCardsCoroutine(_cardsPerScreen, _initialRevealDelay));
	}

	private IEnumerator RevealCardsCoroutine(int numberOfCardsPerScreen, float revealDelay)
	{
		for (int i = 0; i < _rewardsContainer.childCount; i++)
		{
			GameObject gameObject = _rewardsContainer.GetChild(i).gameObject;
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		yield return new WaitForSeconds(revealDelay);
		int i2 = 0;
		while (revealed.Count > 0 && i2 < numberOfCardsPerScreen)
		{
			i2++;
			RewardDisplayCard rewardDisplayCard = revealed.Dequeue();
			GameObject gameObject2 = rewardDisplayCard.gameObject;
			gameObject2.SetActive(value: true);
			AudioManager.PlayAudio(rewardDisplayCard.AutoFlip ? WwiseEvents.sfx_ui_main_rewards_wild_flipout : WwiseEvents.sfx_ui_main_rewards_card_flipout, gameObject2);
			yield return new WaitForSeconds(_sequenceDelay);
		}
	}

	public static ClientInventoryUpdateReportItem TEST_CreateTestInventoryUpdate()
	{
		ClientInventoryUpdateReportItem clientInventoryUpdateReportItem = new ClientInventoryUpdateReportItem();
		clientInventoryUpdateReportItem.aetherizedCards = new List<AetherizedCardInformation>
		{
			new AetherizedCardInformation
			{
				grpId = 68626
			},
			new AetherizedCardInformation
			{
				grpId = 69342
			},
			new AetherizedCardInformation
			{
				grpId = 69656
			},
			new AetherizedCardInformation
			{
				grpId = 69970
			},
			new AetherizedCardInformation
			{
				grpId = 70156
			},
			new AetherizedCardInformation
			{
				grpId = 70386
			},
			new AetherizedCardInformation
			{
				grpId = 70193
			},
			new AetherizedCardInformation
			{
				grpId = 70161
			},
			new AetherizedCardInformation
			{
				grpId = 70312
			},
			new AetherizedCardInformation
			{
				grpId = 70231
			}
		};
		clientInventoryUpdateReportItem.xpGained = 0;
		clientInventoryUpdateReportItem.delta = new InventoryDelta
		{
			gemsDelta = 0,
			goldDelta = 0,
			boosterDelta = new BoosterStack[0],
			cardsAdded = new int[10] { 68626, 69342, 69656, 69970, 70156, 70386, 70193, 70161, 70312, 70231 },
			decksAdded = new Guid[0],
			vanityItemsAdded = new string[0],
			vanityItemsRemoved = new string[0],
			vaultProgressDelta = 0.0m,
			wcCommonDelta = 0,
			wcUncommonDelta = 0,
			wcRareDelta = 0,
			wcMythicDelta = 0,
			artSkinsAdded = new ArtSkin[0],
			artSkinsRemoved = new ArtSkin[0],
			voucherItemsDelta = new VoucherStack[0]
		};
		return clientInventoryUpdateReportItem;
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			if (_openButton.gameObject.activeInHierarchy)
			{
				_openButton.Click();
			}
			if (_moreButton.gameObject.activeInHierarchy)
			{
				_moreButton.Click();
			}
			return true;
		}
		return false;
	}
}
