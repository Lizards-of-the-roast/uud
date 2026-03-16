using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Decks;
using Core.Meta.MainNavigation.BoosterChamber;
using Core.Meta.MainNavigation.Store;
using EventPage.Components.NetworkModels;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.Events;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class SealedBoosterOpenAnimation : MonoBehaviour
{
	[SerializeField]
	private Animator _openAnimation;

	[SerializeField]
	private GameObject _boosterFan;

	[SerializeField]
	private CustomButton _openButton;

	[SerializeField]
	private BoosterMetaCardView _cardPrefab;

	[SerializeField]
	private BoosterMetaCardHolder _cardHolder;

	[SerializeField]
	private CustomButton _doneButton;

	[SerializeField]
	private Localize _doneText;

	[SerializeField]
	private Transform _boosterContainer;

	[SerializeField]
	private Renderer _openingBoosterRenderer;

	[SerializeField]
	private BoosterOpenToScrollListController _boosterOpenToScrollListController;

	private DeckBuilderContext _context;

	private int _gemsAdded;

	private ContentControllerRewards _rewardsPanel;

	private List<BoosterMetaCardView> _allCardViews;

	private AssetLookupSystem _assetLookupSystem;

	private IAccountClient _accountClient;

	private ISetMetadataProvider _setMetadataProvider;

	private InventoryManager _inventoryManager;

	private readonly List<Material> _trackedMaterials = new List<Material>();

	private static readonly int DismissCards = Animator.StringToHash("DismissCards");

	private static readonly int Merge = Animator.StringToHash("Merge");

	private static readonly int OpenBooster = Animator.StringToHash("OpenBooster");

	private static readonly int PacksToCards = Animator.StringToHash("PacksToCards");

	private static readonly int Sealed = Animator.StringToHash("Sealed");

	private static readonly int CardsComplete = Animator.StringToHash("CardsComplete");

	private static readonly int Rarity = Animator.StringToHash("Rarity");

	private static readonly int CardCount = Animator.StringToHash("CardCount");

	private static readonly int EmptyHub = Animator.StringToHash("empty hub");

	private void Awake()
	{
		_openButton.OnClick.AddListener(OpenButton_OnClick);
		_doneButton.OnClick.AddListener(DoneButton_OnClick);
	}

	private void OpenButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		_openAnimation.SetBool(Sealed, value: false);
		_openAnimation.SetTrigger(Merge);
		_openAnimation.SetTrigger(PacksToCards);
		_openButton.gameObject.SetActive(value: false);
	}

	public void Init(DeckBuilderContext context, int gemsAdded, ContentControllerRewards rewardsPanel, ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CosmeticsProvider cosmetics, AssetLookupSystem assetLookupSystem, ISetMetadataProvider setMetadataProvider, InventoryManager inventoryManager)
	{
		_context = context;
		_gemsAdded = gemsAdded;
		_rewardsPanel = rewardsPanel;
		_assetLookupSystem = assetLookupSystem;
		_setMetadataProvider = setMetadataProvider;
		_inventoryManager = inventoryManager;
		BoosterMetaCardViewPool boosterMetaCardViewPool = new BoosterMetaCardViewPool(_cardPrefab, cardDatabase, cardViewBuilder);
		StartCoroutine(PreloadCards(boosterMetaCardViewPool));
		_boosterOpenToScrollListController.gameObject.SetActive(value: true);
		_boosterOpenToScrollListController.Init(cardDatabase, cardViewBuilder, zoomHandler, DoneButton_OnClick, boosterMetaCardViewPool);
		_openAnimation.SetBool(Sealed, value: true);
		_doneButton.gameObject.SetActive(value: false);
		_doneButton.Interactable = true;
		_openButton.gameObject.SetActive(value: true);
		_doneButton.OnClick.RemoveListener(DoneButton_OnClick);
		_accountClient = Pantry.Get<IAccountClient>();
		_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_cardHolder.RolloverZoomView = zoomHandler;
		_cardHolder.ShowHighlight = (MetaCardView cardView) => false;
		FreeMaterials();
		int collationId = 0;
		Renderer[] componentsInChildren = _boosterContainer.GetComponentsInChildren<Renderer>();
		EventPage.Components.NetworkModels.BoosterPacksDisplayData boosterPacksDisplayData = _context.Event.PlayerEvent.EventUXInfo.EventComponentData?.BoosterPacksDisplay;
		List<FactionSealedUXInfo> list = _context?.Event?.PlayerEvent?.EventUXInfo.FactionSealedUXInfo;
		if (list != null && list.Count >= 1)
		{
			string factionName = _context.Event.PlayerEvent.CourseData.MadeChoice;
			boosterPacksDisplayData = new EventPage.Components.NetworkModels.BoosterPacksDisplayData
			{
				CardGrantTime = Wizards.MDN.CardGrantTime.PreMatch,
				CollationIds = list.FirstOrDefault((FactionSealedUXInfo info) => info.FactionInternalName == factionName)?.FactionCollations.Select((FactionCollation collation) => collation.CollationId).ToList()
			};
		}
		int num = Mathf.Min(componentsInChildren.Length, boosterPacksDisplayData?.CollationIds.Count ?? 0);
		for (int num2 = 0; num2 < num; num2++)
		{
			collationId = (int)boosterPacksDisplayData.CollationIds[num2];
			Material boosterMaterial = BoosterPayloadUtilities.GetBoosterMaterial(componentsInChildren[num2].sharedMaterial, collationId, showLimitedDecal: true, _assetLookupSystem);
			_trackedMaterials.Add(boosterMaterial);
			componentsInChildren[num2].material = boosterMaterial;
		}
		Material[] materials = _openingBoosterRenderer.materials;
		Material boosterMaterial2 = BoosterPayloadUtilities.GetBoosterMaterial(_openingBoosterRenderer.sharedMaterials[1], collationId, showLimitedDecal: true, _assetLookupSystem);
		_trackedMaterials.Add(boosterMaterial2);
		materials[1] = boosterMaterial2;
		_openingBoosterRenderer.materials = materials;
		Pantry.Get<DeckBuilderModelProvider>().ResetModel();
		CreateCardViews(cardDatabase, cosmetics);
	}

	private IEnumerator PreloadCards(BoosterMetaCardViewPool pool)
	{
		yield return pool.PreloadCards(base.transform);
	}

	public void BeginOpenAnimation()
	{
		_openAnimation.SetTrigger(OpenBooster);
		_boosterFan.SetActive(value: false);
	}

	public void StartBoosterOpenAnimationSequence()
	{
		_boosterOpenToScrollListController.StartBoosterOpenAnimationSequence(RevealSequenceComplete);
	}

	private void RevealSequenceComplete()
	{
		_boosterOpenToScrollListController.UpdateAllPlaceholderCardVisibity(visible: false);
		_openAnimation.Play(EmptyHub);
		_openAnimation.SetTrigger(CardsComplete);
		_doneButton.gameObject.SetActive(value: true);
		_doneText.gameObject.SetActive(value: true);
	}

	private void CreateCardViews(CardDatabase cardDatabase, CosmeticsProvider cosmetics)
	{
		List<Wizards.Mtga.FrontDoorModels.CardInDeck> sideboard = _context.Deck.sideboard;
		int result;
		List<CardSkin> list = (from s in _context.Event?.PlayerEvent?.CourseData?.CardStyles ?? new List<string>()
			select s.Split('.') into s
			where s.Length == 2 && int.TryParse(s[0], out result)
			select new CardSkin(0L, s[1], int.Parse(s[0]))).ToList();
		List<CardData> list2 = BoosterOpenCardDataHelper.GetCardDataOverride(cardDatabase);
		if (list2 == null)
		{
			list2 = new List<CardData>();
			for (int num = 0; num < sideboard.Count; num++)
			{
				Wizards.Mtga.FrontDoorModels.CardInDeck cardInDeck = sideboard[num];
				CardPrintingData printing = cardDatabase.CardDataProvider.GetCardPrintingById(cardInDeck.Id);
				string skinCode = list.Find((CardSkin s) => s.ArtId == printing.ArtId)?.CCV ?? cosmetics.GetHighestTierCollectedArtStyle(printing.ArtId);
				CardData item = CardDataExtensions.CreateSkinCard(cardInDeck.Id, cardDatabase, skinCode);
				for (int num2 = 0; num2 < cardInDeck.Quantity; num2++)
				{
					if (printing.Rarity == CardRarity.Rare || printing.Rarity == CardRarity.MythicRare)
					{
						list2.Add(item);
					}
				}
			}
		}
		List<CardDataAndRevealStatus> list3 = BoosterOpenCardDataHelper.AddRevealStatusAndRebalancedCardsToCardData(BoosterOpenCardDataHelper.SortCardsByRarity(list2), autoFlipEnabled: true);
		List<FactionSealedUXInfo> list4 = _context?.Event?.PlayerEvent?.EventUXInfo.FactionSealedUXInfo;
		if (list4 != null && list4.Count >= 1)
		{
			AddFactionTagToCard(_context.Event.PlayerEvent.CourseData, list4, list3);
		}
		BoosterOpenCardDataHelper.AddTags(list3, _inventoryManager, _setMetadataProvider);
		CardRarity rarity = list3.FirstOrDefault().CardData.Rarity;
		_openAnimation.SetInteger(CardCount, list3.Count);
		_openAnimation.SetInteger(Rarity, (int)rarity);
		_boosterOpenToScrollListController.SetCardsToDisplay(list3);
		_boosterOpenToScrollListController.UpdateAllPlaceholderCardVisibity(visible: true);
	}

	private void AddFactionTagToCard(CourseData courseData, List<FactionSealedUXInfo> fsUXInfo, List<CardDataAndRevealStatus> sortedCards)
	{
		string factionChoice = courseData.MadeChoice;
		FactionSealedUXInfo factionSealedUXInfo = fsUXInfo.Find((FactionSealedUXInfo f) => f.FactionInternalName == factionChoice);
		Dictionary<uint, HashSet<uint>> dictionary = new Dictionary<uint, HashSet<uint>>();
		foreach (CollationCardPool item in courseData.CardPoolByCollation)
		{
			if (dictionary.ContainsKey(item.CollationId))
			{
				if (dictionary.TryGetValue(item.CollationId, out var value))
				{
					value.UnionWith(item.CardPool.Distinct().ToHashSet());
				}
			}
			else
			{
				dictionary.Add(item.CollationId, item.CardPool.Distinct().ToHashSet());
			}
		}
		foreach (FactionCollation factionCollation in factionSealedUXInfo.FactionCollations)
		{
			if (string.IsNullOrWhiteSpace(factionCollation.HangarLoc) || !dictionary.TryGetValue(factionCollation.CollationId, out var value2))
			{
				continue;
			}
			foreach (CardDataAndRevealStatus sortedCard in sortedCards)
			{
				if (value2.Contains(sortedCard.CardData.GrpId))
				{
					sortedCard.factionTag = Languages.ActiveLocProvider.GetLocalizedText(factionCollation.HangarLoc);
				}
			}
		}
	}

	private void DoneButton_OnClick()
	{
		_doneButton.Interactable = false;
		_boosterOpenToScrollListController.ClearCardsOffScreen();
		MDNPlayerPrefs.SetHasOpenedSealedPool(_accountClient.AccountInformation?.PersonaID, _context.Event.PlayerEvent.CourseData.Id, value: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		StartCoroutine(Coroutine_GoToDeckBuilder());
	}

	private IEnumerator Coroutine_GoToDeckBuilder()
	{
		_openAnimation.SetTrigger(DismissCards);
		if (_gemsAdded > 0)
		{
			ClientInventoryUpdateReportItem t = new ClientInventoryUpdateReportItem
			{
				delta = 
				{
					gemsDelta = _gemsAdded
				}
			};
			yield return _rewardsPanel.AddAndDisplayRewardsCoroutine(t, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/GemCard/GemReward"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/GemCard/GemRewardDescription"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			yield return new WaitUntil(() => !_rewardsPanel.Visible);
		}
		_context.Deck.id = Guid.NewGuid();
		SceneLoader.GetSceneLoader().GoToDeckBuilder(_context);
	}

	public void FreeMaterials()
	{
		foreach (Material trackedMaterial in _trackedMaterials)
		{
			BoosterPayloadUtilities.FreeBoosterMaterial(trackedMaterial);
		}
		_trackedMaterials.Clear();
	}

	public void Cleanup()
	{
		_boosterOpenToScrollListController.StopBoosterOpenAnimationSequence();
		_boosterOpenToScrollListController.Cleanup();
		_openAnimation.Play(EmptyHub);
	}

	public void OnDestroy()
	{
		FreeMaterials();
		Cleanup();
	}
}
