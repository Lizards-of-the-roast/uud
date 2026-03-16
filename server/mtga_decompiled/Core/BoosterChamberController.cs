using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Store;
using Core.BI;
using Core.Code.Promises;
using Core.Meta.MainNavigation.BoosterChamber;
using Core.Meta.MainNavigation.Store.Utils;
using Core.Shared.Code.Utilities;
using DG.Tweening;
using GreClient.CardData;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Models.ClientModels;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

[RequireComponent(typeof(Animator))]
public class BoosterChamberController : NavContentController, IScrollHandler, IEventSystemHandler
{
	[Header("Prefabs")]
	[SerializeField]
	private SealedBoosterView _sealedBoosterPackPrefab;

	[SerializeField]
	private BoosterMetaCardView _cardPrefab;

	[SerializeField]
	private GameObject _wildcardRewardsPrefab;

	[Header("Children")]
	[SerializeField]
	private BoosterMetaCardHolder _cardHolder;

	[SerializeField]
	private ICardRolloverZoom _zoomHandler;

	[SerializeField]
	private CardViewBuilder _cardViewBuilder;

	[SerializeField]
	private SetLogo _setLogo;

	[SerializeField]
	private WildcardTrack _wildcardTrack;

	[SerializeField]
	private GameObject _boosterCarouselLayout;

	[SerializeField]
	private GameObject _openTenButton;

	[SerializeField]
	private Animator _openingBoosterPackAnimator;

	[SerializeField]
	private Renderer _openingBoosterPackRenderer;

	[SerializeField]
	private GameObject _noPacksParent;

	[SerializeField]
	private GameObject _wildcardRewardsContainer;

	[SerializeField]
	private SwipePageTurnModule _swipeModule;

	[Header("Visual Adjustments")]
	[SerializeField]
	private float _carouselStartXPos;

	[SerializeField]
	private float _carouselWidth = 450f;

	[SerializeField]
	private float _boosterWidthInLayout = 200f;

	[SerializeField]
	private float _boosterWidthInLayoutWhileSelected = 450f;

	[SerializeField]
	private float _wildcardRewardsDelay1 = 1f;

	[SerializeField]
	private float _wildcardRewardsDelay2 = 2f;

	[SerializeField]
	private float _wildcardRewardsZOffset = -0.5f;

	[SerializeField]
	private float _scrollSelectDelay = 0.42f;

	[SerializeField]
	private Ease _packMoveEase = Ease.OutBack;

	[Tooltip("The delay until Moz on the cards is enabled and when New tags are shown")]
	[SerializeField]
	private float _boosterPackAnimationTime = 2.5f;

	[SerializeField]
	private float _displayNewBoosterDelayScale = 0.1f;

	[Header("Scroll List Open Sequence Refactor")]
	[SerializeField]
	private BoosterOpenToScrollListController _boosterOpenToScrollListController;

	private BoosterMetaCardViewPool _boosterMetaCardViewPool;

	private AssetLookupSystem _assetLookupSystem;

	private IBILogger _biLogger;

	private IFrontDoorConnectionServiceWrapper _fdc;

	private CardDatabase _cardDatabase;

	private InventoryManager _inventoryManager;

	private Action<bool> _onBoosterCrack;

	private IUnityObjectPool _objectPool;

	private SparkyTourState _sparkyTourState;

	private RendererReferenceLoader _openingBoosterPackRendererLoader;

	private bool _cardsArePreloaded;

	private Animator _boosterChamberAnimator;

	private SealedBoosterView _selectedBoosterView;

	private readonly List<SealedBoosterView> _boosterViews = new List<SealedBoosterView>();

	private readonly List<BoosterVoucherView> _voucherViews = new List<BoosterVoucherView>();

	private bool _autoRevealRareCard;

	private string _setCode = string.Empty;

	private int _centeredBoosterIndex = -1;

	private float _boosterSelectionTimer;

	private bool _scrolling;

	private bool _ignoreInput;

	private bool _boostersIsDisabled;

	private bool _readyToShow;

	private VoucherDataProvider _voucherDataProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private CustomButton _wildCardButton;

	private static readonly int OpenBooster = Animator.StringToHash("OpenBooster");

	private static readonly int DismissCardsTrigger = Animator.StringToHash("DismissCards");

	private static readonly int PacksToCards = Animator.StringToHash("PacksToCards");

	private static readonly int CardsComplete = Animator.StringToHash("CardsComplete");

	private static readonly int CardsRevealed = Animator.StringToHash("CardsRevealed");

	private static readonly int HaveBoosters = Animator.StringToHash("haveBoosters");

	private static readonly int CardCount = Animator.StringToHash("CardCount");

	private static readonly int Rarity = Animator.StringToHash("Rarity");

	private static readonly int EmptyHub = Animator.StringToHash("empty hub");

	private static readonly int PresentOn = Animator.StringToHash("PresentON");

	private static readonly int PresentOff = Animator.StringToHash("PresentOFF");

	private static readonly int HoverOff = Animator.StringToHash("HoverOff");

	private static readonly int Outro = Animator.StringToHash("Outro");

	private static readonly int OpenOutro = Animator.StringToHash("OpenOutro");

	private static readonly int FinalPosX = Animator.StringToHash("FinalPosX");

	private static readonly int FinalPosY = Animator.StringToHash("FinalPosY");

	private static readonly int GoToNavBarStartTime = Animator.StringToHash("GoToNavBarStartTime");

	public override NavContentType NavContentType => NavContentType.BoosterChamber;

	private SealedBoosterView CenteredBoosterView
	{
		get
		{
			if (_centeredBoosterIndex < 0 || _centeredBoosterIndex >= _boosterViews.Count)
			{
				return null;
			}
			return _boosterViews[_centeredBoosterIndex];
		}
	}

	public bool ThereIsABoosterOpened { get; private set; }

	public bool OkToSelectBooster
	{
		get
		{
			if (!_ignoreInput)
			{
				if (!_openingBoosterPackAnimator.GetCurrentAnimatorStateInfo(0).IsName("null"))
				{
					return _openingBoosterPackAnimator.GetCurrentAnimatorStateInfo(0).IsName("Packs");
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsReadyToShow => _readyToShow;

	public void Instantiate(ICardRolloverZoom zoomHandler, CardViewBuilder cardViewBuilder, AssetLookupSystem assetLookupSystem, IFrontDoorConnectionServiceWrapper fdc, IBILogger biLogger, CardDatabase cardDatabase, VoucherDataProvider voucherDataProvider, ISetMetadataProvider setMetadataProvider, InventoryManager inventoryManager, Action<bool> onBoosterCrack, IUnityObjectPool objectPool, CustomButton wildCardButton, SparkyTourState sparkyTourState)
	{
		_zoomHandler = zoomHandler;
		_cardHolder.RolloverZoomView = _zoomHandler;
		_cardViewBuilder = cardViewBuilder;
		_assetLookupSystem = assetLookupSystem;
		_fdc = fdc;
		_biLogger = biLogger;
		_cardDatabase = cardDatabase;
		_voucherDataProvider = voucherDataProvider;
		_setMetadataProvider = setMetadataProvider;
		_inventoryManager = inventoryManager;
		_onBoosterCrack = onBoosterCrack;
		_objectPool = objectPool;
		_wildCardButton = wildCardButton;
		_sparkyTourState = sparkyTourState;
		_setLogo.Init();
		_openingBoosterPackRendererLoader = new RendererReferenceLoader(_openingBoosterPackRenderer);
		_boosterChamberAnimator = GetComponent<Animator>();
		_cardHolder.EnsureInit(cardDatabase, _cardViewBuilder);
		_cardHolder.RolloverZoomView = _zoomHandler;
		_cardHolder.ShowHighlight = (MetaCardView cardView) => false;
		_wildcardTrack.Init(cardDatabase, cardViewBuilder);
		_boosterMetaCardViewPool = new BoosterMetaCardViewPool(_cardPrefab, _cardDatabase, _cardViewBuilder);
		_boosterOpenToScrollListController.Init(cardDatabase, cardViewBuilder, _zoomHandler, DismissCards, _boosterMetaCardViewPool);
		if ((bool)_swipeModule)
		{
			_swipeModule.onSwipeLeft.AddListener(PreviousBooster);
			_swipeModule.onSwipeRight.AddListener(NextBooster);
		}
	}

	public override void OnBeginOpen()
	{
		_boosterChamberAnimator.enabled = true;
		_boosterCarouselLayout.GetComponent<RectTransform>().DOAnchorPosX(_carouselStartXPos, 0.01f);
		StartCoroutine(Coroutine_Show());
		_ignoreInput = false;
		Languages.LanguageChangedSignal.Listeners += OnLocalize;
		_wildcardTrack.SetTrackPosition(_inventoryManager.Inventory.wcTrackPosition);
		if (_fdc != null)
		{
			_fdc.RegisterKillswitchNotification(OnKillSwitch);
			OnKillSwitch(_fdc.Killswitch);
		}
	}

	public override void OnBeginClose()
	{
		Languages.LanguageChangedSignal.Listeners -= OnLocalize;
		_fdc?.UnRegisterKillswitchNotification(OnKillSwitch);
		if (ThereIsABoosterOpened)
		{
			ThereIsABoosterOpened = false;
		}
		CleanupBoosters();
		CleanUpCards();
		_boosterOpenToScrollListController.StopBoosterOpenAnimationSequence();
		_boosterOpenToScrollListController.Cleanup();
		_cardsArePreloaded = false;
		_boosterChamberAnimator.Play(EmptyHub);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_rare_appear_forceoff, base.gameObject);
		SelfCleanup[] componentsInChildren = _wildcardRewardsContainer.GetComponentsInChildren<SelfCleanup>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ManualCleanup();
		}
	}

	public override void OnFinishClose()
	{
		base.OnFinishClose();
		_setLogo.Cleanup();
		_openingBoosterPackRendererLoader?.Cleanup();
	}

	private void OnKillSwitch(Client_KillSwitchNotification killswitch)
	{
		_boostersIsDisabled = killswitch?.IsBoosterDisabled ?? false;
	}

	private void OnLocalize()
	{
		RefreshLogoAndBooster();
	}

	private void Update()
	{
		if (!(_boosterSelectionTimer > 0f))
		{
			return;
		}
		_boosterSelectionTimer -= Time.deltaTime;
		if (_boosterSelectionTimer <= 0f)
		{
			if (CenteredBoosterView != null)
			{
				SelectBooster(CenteredBoosterView);
			}
			else
			{
				UnPresentAllBoosters();
			}
			SetLogoRefresh();
		}
	}

	private void CleanupBoosters(SealedBoosterView boosterToCleanup = null)
	{
		_boosterOpenToScrollListController.ClearCardsOffScreen();
		if (null != boosterToCleanup)
		{
			_boosterViews.Remove(boosterToCleanup);
			UnityEngine.Object.Destroy(boosterToCleanup.gameObject);
			SelectBooster();
			return;
		}
		foreach (SealedBoosterView boosterView in _boosterViews)
		{
			UnityEngine.Object.Destroy(boosterView.gameObject);
		}
		_boosterViews.Clear();
		foreach (BoosterVoucherView voucherView in _voucherViews)
		{
			UnityEngine.Object.Destroy(voucherView.gameObject);
		}
		_voucherViews.Clear();
		_centeredBoosterIndex = -1;
		_setLogo.IsVisible = false;
	}

	private void DisplayBoosters(int previousID)
	{
		CleanupBoosters();
		AudioManager.SetRTPCValue("booster_packrollover", 0f);
		List<ClientBoosterInfo> obj = _inventoryManager.Inventory.boosters?.Where((ClientBoosterInfo x) => x.count > 0).ToList() ?? new List<ClientBoosterInfo>();
		obj.Sort(SortBoostersByCollationID);
		List<ClientBoosterInfo> list = _boosterViews.Select((SealedBoosterView x) => x._info).ToList();
		bool flag = false;
		foreach (ClientBoosterInfo item in obj)
		{
			if (!list.Contains(item))
			{
				ClientBoosterInfo iBoosterInfo = item;
				SealedBoosterView sealedBoosterView = UnityEngine.Object.Instantiate(_sealedBoosterPackPrefab, _boosterCarouselLayout.transform);
				sealedBoosterView.Instantiate(_inventoryManager, _assetLookupSystem);
				sealedBoosterView.SetData(this, iBoosterInfo, _setMetadataProvider);
				_boosterViews.Add(sealedBoosterView);
				float num = 0f;
				if (sealedBoosterView._info.collationId == previousID)
				{
					SelectBooster(sealedBoosterView);
					flag = true;
				}
				else if (_boosterViews.Count > 1)
				{
					num = (float)_boosterViews.Count * _displayNewBoosterDelayScale;
					StartCoroutine(DelayedTrigger(sealedBoosterView.BoosterAnimator, PresentOff, num));
				}
				sealedBoosterView.RefreshInfo(num);
			}
		}
		List<ClientVoucherDescription> vouchers = _inventoryManager.Inventory.vouchers;
		if (vouchers != null && vouchers.Count > 0)
		{
			vouchers.Sort((ClientVoucherDescription v1, ClientVoucherDescription v2) => v1.availableDate.CompareTo(v2.availableDate));
			foreach (ClientVoucherDescription item2 in vouchers)
			{
				if (item2.count > 0 && !_voucherViews.Exists(item2.referenceId, (BoosterVoucherView a, string refId) => a.VoucherId == refId))
				{
					Client_VoucherDefinition client_VoucherDefinition = _voucherDataProvider.VoucherDefinitionForId(item2.referenceId);
					SimpleLogUtils.LogErrorIfNull(client_VoucherDefinition, "Voucher Definition not found for id " + item2.referenceId);
					AltAssetReference<BoosterVoucherView> altAssetReference = VoucherUtils.VoucherRefForId<VoucherPayload, BoosterVoucherView>(_assetLookupSystem, client_VoucherDefinition?.PrefabName);
					if (altAssetReference != null)
					{
						BoosterVoucherView boosterVoucherView = AssetLoader.Instantiate(altAssetReference, _boosterCarouselLayout.transform);
						VoucherUtils.UpdateVoucherView(boosterVoucherView, client_VoucherDefinition, item2.count);
						_voucherViews.Add(boosterVoucherView);
					}
					else
					{
						Debug.LogError("Store display for voucher \"" + item2.referenceId + "\" missing!");
					}
				}
			}
		}
		if (_boosterViews.Count > 0)
		{
			_noPacksParent.SetActive(value: false);
			UpdateHaveBoosters(haveBoosters: true);
			if (!flag)
			{
				SelectBooster(_boosterViews[0]);
			}
			else if (_centeredBoosterIndex != 0)
			{
				StartCoroutine(DelayedTrigger(_boosterViews[0].BoosterAnimator, PresentOff, (float)_boosterViews.Count * _displayNewBoosterDelayScale));
			}
		}
		else if (vouchers == null || vouchers.Count((ClientVoucherDescription _) => _.count > 0) == 0)
		{
			_setLogo.IsVisible = false;
			_openTenButton.gameObject.SetActive(value: false);
			_noPacksParent.SetActive(value: true);
			UpdateHaveBoosters(haveBoosters: false);
		}
		else if (_boosterViews.Count == 0 && _voucherViews.Count > 0)
		{
			_setLogo.SetTexture(_voucherViews[0]);
			_openTenButton.gameObject.SetActive(value: false);
		}
	}

	private void UpdateHaveBoosters(bool haveBoosters)
	{
		_boosterChamberAnimator.SetBool(HaveBoosters, haveBoosters);
	}

	private int SortBoostersByCollationID(ClientBoosterInfo x, ClientBoosterInfo y)
	{
		int num = x.collationId % 10000;
		int num2 = y.collationId % 10000;
		if (num == num2)
		{
			return x.collationId.CompareTo(y.collationId);
		}
		return num2.CompareTo(num);
	}

	public void ClickedVoucherView(BoosterVoucherView voucherToSelect)
	{
		if (voucherToSelect == null || (_selectedBoosterView != null && _selectedBoosterView.BoosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_OpenOutro")))
		{
			return;
		}
		int centeredBoosterIndex = _centeredBoosterIndex;
		_centeredBoosterIndex = _voucherViews.IndexOf(voucherToSelect);
		if (_centeredBoosterIndex >= 0)
		{
			_centeredBoosterIndex += _boosterViews.Count();
			if (centeredBoosterIndex != _centeredBoosterIndex)
			{
				_scrolling = true;
				SetLogoOut();
				UnPresentAllBoosters();
				CenterIndex(_centeredBoosterIndex);
				_boosterSelectionTimer = _scrollSelectDelay;
				_openTenButton.gameObject.SetActive(value: false);
			}
		}
	}

	private bool BoosterIsKillSwitched()
	{
		if (_boostersIsDisabled)
		{
			SystemMessageManager.Instance.ShowOk((MTGALocalizedString)"MainNav/Killswitch/PopupError_Title", (MTGALocalizedString)"MainNav/Killswitch/PopupError_Message");
			return true;
		}
		return false;
	}

	public void ClickedBooster(SealedBoosterView boosterToSelect)
	{
		if (!OkToSelectBooster)
		{
			return;
		}
		if (boosterToSelect._presenting && !boosterToSelect.BoosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON"))
		{
			boosterToSelect.BoosterAnimator.SetTrigger(PresentOn);
			return;
		}
		SetLogoOut();
		int centeredBoosterIndex = _centeredBoosterIndex;
		_centeredBoosterIndex = _boosterViews.IndexOf(boosterToSelect);
		_boosterSelectionTimer = _scrollSelectDelay;
		if (centeredBoosterIndex == _centeredBoosterIndex)
		{
			_scrolling = false;
			SelectBooster(boosterToSelect);
		}
		else
		{
			_scrolling = true;
		}
		CenterIndex(_centeredBoosterIndex);
	}

	private void SelectBooster(SealedBoosterView boosterToSelect = null)
	{
		_boosterSelectionTimer = 0f;
		if (_boosterViews.Count <= 0)
		{
			_centeredBoosterIndex = -1;
			return;
		}
		if (null == boosterToSelect)
		{
			_centeredBoosterIndex = 0;
			boosterToSelect = _boosterViews[0];
		}
		_selectedBoosterView = boosterToSelect;
		if (_openingBoosterPackAnimator.GetCurrentAnimatorStateInfo(0).IsName("null"))
		{
			_setLogo.SetTexture(_selectedBoosterView);
		}
		if (boosterToSelect._presenting && !_scrolling)
		{
			if (!BoosterIsKillSwitched())
			{
				boosterToSelect.BoosterAnimator.SetTrigger(PresentOn, value: false);
				BeginOpenBoosterSequence(boosterToSelect);
			}
			return;
		}
		_scrolling = false;
		if (!boosterToSelect.BoosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON") && !boosterToSelect.BoosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON_Transition"))
		{
			boosterToSelect.BoosterAnimator.SetTrigger(PresentOn);
		}
		boosterToSelect._presenting = true;
		_centeredBoosterIndex = _boosterViews.IndexOf(boosterToSelect);
		UnPresentAllBoosters(_centeredBoosterIndex);
		RefreshOpenTenButton();
		CenterIndex(_centeredBoosterIndex);
	}

	private void UnPresentAllBoosters(int skipIndex = -1)
	{
		for (int i = 0; i < _boosterViews.Count; i++)
		{
			if (i != skipIndex)
			{
				SealedBoosterView sealedBoosterView = _boosterViews[i];
				sealedBoosterView._presenting = false;
				if (sealedBoosterView.BoosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON") || sealedBoosterView.BoosterAnimator.GetCurrentAnimatorStateInfo(0).IsName("CarouselBooster_PresentON_Transition"))
				{
					sealedBoosterView.BoosterAnimator.ResetTrigger(PresentOn);
					sealedBoosterView.BoosterAnimator.SetTrigger(PresentOff);
					sealedBoosterView.BoosterAnimator.SetTrigger(HoverOff);
				}
				sealedBoosterView.RefreshInfo();
			}
		}
		_openTenButton.SetActive(value: false);
	}

	private void CenterIndex(int boosterIndex)
	{
		int count = _boosterViews.Count;
		int count2 = _voucherViews.Count;
		int num = count + count2;
		boosterIndex = Mathf.Clamp(boosterIndex, 0, num - 1);
		float num2 = (_carouselStartXPos + _carouselWidth) / 2f;
		int num3 = Math.Min(boosterIndex, count);
		int num4 = ((boosterIndex > count) ? (boosterIndex - count) : 0);
		float num5 = (float)num3 * _boosterWidthInLayout;
		for (int i = 0; i < num4; i++)
		{
			num5 += _voucherViews[i].GetComponent<RectTransform>().rect.width;
		}
		float num6 = 0f;
		num6 = ((boosterIndex < count || count2 <= 0) ? (num6 + _boosterWidthInLayoutWhileSelected / 2f) : (num6 + _voucherViews[boosterIndex - count].GetComponent<RectTransform>().rect.width / 2f));
		num2 -= num5 + num6;
		_boosterCarouselLayout.GetComponent<RectTransform>().DOAnchorPosX(num2, 1f).SetEase(_packMoveEase);
	}

	private IBoosterChamberSetLogoInfo GetSetLogoTexture(int boosterIndex)
	{
		if (boosterIndex < 0 || _boosterViews.Count + _voucherViews.Count <= 0)
		{
			return null;
		}
		if (boosterIndex >= _boosterViews.Count)
		{
			return _voucherViews[boosterIndex - _boosterViews.Count];
		}
		return _boosterViews[boosterIndex];
	}

	public void OpenTen()
	{
		if (OkToSelectBooster && !_scrolling && !BoosterIsKillSwitched())
		{
			ThereIsABoosterOpened = true;
			_setLogo.IsVisible = false;
			_ignoreInput = true;
			int quantityToOpen = Math.Min(10, _selectedBoosterView._info.count);
			StartCoroutine(Coroutine_OpenBoosters(quantityToOpen, _selectedBoosterView));
			_ignoreInput = false;
		}
	}

	private void BeginOpenBoosterSequence(SealedBoosterView boosterToOpen)
	{
		ThereIsABoosterOpened = true;
		_setLogo.IsVisible = false;
		StartCoroutine(Coroutine_OpenBoosters(1, boosterToOpen));
	}

	private IEnumerator Coroutine_OpenBoosters(int quantityToOpen, SealedBoosterView boosterToOpen)
	{
		boosterToOpen.RefreshInfo();
		int originalQuantity = boosterToOpen._info.count;
		if (_setCode != boosterToOpen.SetCode)
		{
			AudioManager.SetRTPCValue("booster_packrollover", 0f);
			AudioManager.SetRTPCValue("boosterpack_" + _setCode, 0f);
			_setCode = boosterToOpen.SetCode;
			AudioManager.SetRTPCValue("booster_packrollover", 100f);
			AudioManager.SetRTPCValue("boosterpack_" + _setCode, 100f);
		}
		_ignoreInput = true;
		_onBoosterCrack?.Invoke(obj: true);
		Promise<InventoryInfoShared> crackBoosterPromise = Pantry.Get<IInventoryServiceWrapper>().CrackBooster(boosterToOpen._info.collationId.ToString(), quantityToOpen);
		yield return new WaitUntil(() => crackBoosterPromise.IsDone);
		_onBoosterCrack?.Invoke(obj: false);
		_ignoreInput = false;
		if (!crackBoosterPromise.Successful || crackBoosterPromise.Result == null)
		{
			MTGALocalizedString mTGALocalizedString = "MainNav/General/ErrorTitle";
			MTGALocalizedString mTGALocalizedString2 = "MainNav/BoosterChamber/Booster_Open_Failure";
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText(mTGALocalizedString), Languages.ActiveLocProvider.GetLocalizedText(mTGALocalizedString2), showCancel: false, delegate
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			});
			if (crackBoosterPromise.State == PromiseState.Timeout)
			{
				BIEventType.OpenBoosterError.SendWithDefaults(("Error", "Timeout on OpenBooster request"), ("BoosterId", boosterToOpen._info.collationId.ToString()));
			}
			else if (!crackBoosterPromise.IsConnectionError)
			{
				BIEventType.OpenBoosterError.SendWithDefaults(("Error", "Timeout on OpenBooster request"), ("Error", crackBoosterPromise.Error.Message), ("BoosterId", boosterToOpen._info.collationId.ToString()));
			}
		}
		else
		{
			_inventoryManager.SetWcTrackPosition(crackBoosterPromise.Result.wcTrackPosition);
			if (crackBoosterPromise.Result.cardsOpened != null)
			{
				VisuallyOpenBooster(originalQuantity, quantityToOpen, crackBoosterPromise.Result);
			}
		}
	}

	private void CleanUpCards()
	{
		_boosterOpenToScrollListController.ClearCardsOffScreen();
	}

	public void StartBoosterOpenAnimationSequence()
	{
		_boosterOpenToScrollListController.StartBoosterOpenAnimationSequence(BoosterOpenAnimationComplete);
	}

	private void BoosterOpenAnimationComplete()
	{
		_boosterOpenToScrollListController.UpdateAllPlaceholderCardVisibity(visible: false);
		_boosterChamberAnimator.Play(EmptyHub);
		_boosterChamberAnimator.SetTrigger(CardsComplete);
		UpdateRevealed();
	}

	private void ListenForRevealedUpdate(CardDataAndRevealStatus card)
	{
		card.OnRevealed = (Action)Delegate.Combine(card.OnRevealed, new Action(UpdateRevealed));
	}

	private void UpdateRevealed()
	{
		int unrevealedCardCount = _boosterOpenToScrollListController.GetUnrevealedCardCount();
		_boosterChamberAnimator.SetBool(CardsRevealed, unrevealedCardCount <= 0);
	}

	private void VisuallyOpenBooster(int originalQuantity, int quantityOpened, InventoryInfoShared inventoryInfo)
	{
		CleanUpCards();
		_openTenButton.SetActive(value: false);
		_autoRevealRareCard = false;
		CardDatabase cardDatabase = _cardDatabase;
		List<DTO_CosmeticArtStyleEntry> cardStyles = inventoryInfo.Changes.SelectMany((InventoryChangeShared c) => c.ArtStyles).ToList();
		IEnumerable<CardData> enumerable = BoosterOpenCardDataHelper.GetCardDataOverride(cardDatabase, inventoryInfo.cardsOpened);
		if (enumerable == null)
		{
			enumerable = inventoryInfo.cardsOpened.Select(delegate(CrackBoostersCardInfo c)
			{
				if (!c.addedToInventory && c.gemsAwarded > 0)
				{
					_autoRevealRareCard = true;
					return CardDataExtensions.CreateRewardsCard(_cardDatabase, c.goldAwarded, c.gemsAwarded, c.set, "Booster");
				}
				CardPrintingData cardPrinting = cardDatabase.CardDataProvider.GetCardPrintingById(c.grpId);
				DTO_CosmeticArtStyleEntry dTO_CosmeticArtStyleEntry = ((cardPrinting == null) ? null : cardStyles.FirstOrDefault((DTO_CosmeticArtStyleEntry s) => s.ArtId == cardPrinting.ArtId));
				if (dTO_CosmeticArtStyleEntry != null)
				{
					cardStyles.Remove(dTO_CosmeticArtStyleEntry);
					return CardDataExtensions.CreateSkinCard(c.grpId, cardPrinting, _cardDatabase, dTO_CosmeticArtStyleEntry.Variant);
				}
				return new CardData(null, cardPrinting);
			});
		}
		int num = 0;
		CardRarity cardRarity = CardRarity.None;
		List<CardDataAndRevealStatus> list = BoosterOpenCardDataHelper.AddRevealStatusAndRebalancedCardsToCardData(BoosterOpenCardDataHelper.SortCardsByRarity(enumerable.ToList()), MDNPlayerPrefs.GetBoosterPackOpenAutoReveal(), MDNPlayerPrefs.GetBoosterPackOpenSkipAnimation());
		BoosterOpenCardDataHelper.AddTags(list, _inventoryManager, _setMetadataProvider);
		foreach (CardDataAndRevealStatus item in list)
		{
			if (!item.AutoReveal && !item.Revealed)
			{
				ListenForRevealedUpdate(item);
			}
		}
		_boosterOpenToScrollListController.SetCardsToDisplay(list);
		num = list.Count;
		cardRarity = list.FirstOrDefault().CardData.Rarity;
		_boosterOpenToScrollListController.UpdateAllPlaceholderCardVisibity(visible: true);
		_boosterChamberAnimator.SetTrigger(PacksToCards);
		foreach (SealedBoosterView boosterView in _boosterViews)
		{
			if (boosterView != _selectedBoosterView)
			{
				boosterView.BoosterAnimator.SetTrigger(Outro);
			}
		}
		foreach (BoosterVoucherView voucherView in _voucherViews)
		{
			voucherView.gameObject.SetActive(value: false);
			voucherView.GetComponent<Animator>().SetTrigger(Outro);
		}
		_selectedBoosterView.BoosterAnimator.SetTrigger(OpenOutro);
		SetLogoOut();
		_openingBoosterPackRendererLoader.Cleanup();
		_openingBoosterPackRendererLoader.SetPropertyBlockTexture(1, "_MainTex", _selectedBoosterView.BoosterBackgroundTexturePath);
		_openingBoosterPackRendererLoader.SetPropertyBlockTexture(1, "_Decal1", _selectedBoosterView.BoosterSetLogoTexturePath);
		_openingBoosterPackRendererLoader.ApplyPropertyBlocks();
		AudioSequenceOpen();
		_openingBoosterPackAnimator.SetInteger(CardCount, num);
		_openingBoosterPackAnimator.SetInteger(Rarity, (int)cardRarity);
		_openingBoosterPackAnimator.SetTrigger(OpenBooster);
		StartCoroutine(DelayEnableCardZoom(_boosterPackAnimationTime));
		_selectedBoosterView._info.count = originalQuantity - quantityOpened;
		_selectedBoosterView.RefreshInfo(quantityOpened);
		StartCoroutine(ShowWildcardReward(inventoryInfo));
		_wildcardTrack.StopTrackSounds(5f);
		if (_selectedBoosterView == null || _selectedBoosterView._info == null || _selectedBoosterView._info.count <= 0)
		{
			_boosterViews.Remove(_selectedBoosterView);
			UnityEngine.Object.Destroy(_selectedBoosterView.gameObject, 3f);
		}
		CenterIndex(_centeredBoosterIndex);
	}

	private IEnumerator ShowWildcardReward(InventoryInfoShared inventoryInfo)
	{
		yield return new WaitForSeconds(_wildcardRewardsDelay1);
		_wildcardTrack.UpdateTrackPosition(inventoryInfo.wcTrackPosition);
		if (inventoryInfo.wildCardTrackMythics + inventoryInfo.wildCardTrackRares + inventoryInfo.wildCardTrackUnCommons <= 0m)
		{
			yield return null;
		}
		yield return new WaitForSeconds(_wildcardRewardsDelay2);
		CardDatabase cardDatabase = _cardDatabase;
		List<int> redeemedWildcardGrpIds = new List<int>();
		for (int i = 0; (decimal)i < inventoryInfo.wildCardTrackUnCommons; i++)
		{
			redeemedWildcardGrpIds.Add(8);
		}
		for (int j = 0; (decimal)j < inventoryInfo.wildCardTrackRares; j++)
		{
			redeemedWildcardGrpIds.Add(7);
		}
		for (int k = 0; (decimal)k < inventoryInfo.wildCardTrackMythics; k++)
		{
			redeemedWildcardGrpIds.Add(6);
		}
		Vector3 wildcardNavPos = _wildCardButton.transform.position;
		bool hasShownWildcardReward = false;
		for (int wildCardAmount = 0; wildCardAmount < redeemedWildcardGrpIds.Count; wildCardAmount++)
		{
			GameObject gameObject = _objectPool.PopObject(_wildcardRewardsPrefab);
			gameObject.transform.SetParent(_wildcardRewardsContainer.transform, worldPositionStays: false);
			gameObject.transform.ZeroOut();
			gameObject.AddComponent<SelfCleanup>().SetLifetime(3f, SelfCleanup.CleanupType.SharedPool);
			Transform child = gameObject.transform.GetChild(0);
			child.DestroyChildren();
			gameObject.transform.localPosition += new Vector3(0f, 0f, (float)wildCardAmount * _wildcardRewardsZOffset);
			Animator component = gameObject.gameObject.GetComponent<Animator>();
			Vector3 vector = new Vector3(component.GetFloat(FinalPosX), component.GetFloat(FinalPosY), 0f);
			Vector3 vector2 = child.TransformVector(vector);
			Vector3 vector3 = wildcardNavPos - child.position + vector2;
			Vector3 position = gameObject.transform.position;
			Vector3 vector4 = position + vector3;
			vector4 = new Vector3(vector4.x, vector4.y, position.z);
			float num = component.GetFloat(GoToNavBarStartTime);
			float length = component.runtimeAnimatorController.animationClips.First((AnimationClip clip) => clip.name == "WildCardReward").length;
			DOTween.Sequence().Append(gameObject.transform.DOMove(vector4, length - num).SetDelay(num));
			CardData cardData = new CardData(null, cardDatabase.CardDataProvider.GetCardPrintingById((uint)redeemedWildcardGrpIds[wildCardAmount]));
			CDCMetaCardView cDCMetaCardView = _cardViewBuilder.CreateCDCMetaCardView(cardData, child);
			cDCMetaCardView.transform.ZeroOut();
			if (!hasShownWildcardReward)
			{
				hasShownWildcardReward = true;
				_wildcardTrack.RewardWildcard();
			}
			if (cardData.Rarity == CardRarity.MythicRare)
			{
				_wildcardTrack.RewardRare();
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_flip_mythic_rare, cDCMetaCardView.gameObject);
			}
			else if (cardData.Rarity == CardRarity.Rare)
			{
				_wildcardTrack.RewardRare();
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_flip_rare, cDCMetaCardView.gameObject);
			}
			else
			{
				_wildcardTrack.RewardUncommon();
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_flip_common, cDCMetaCardView.gameObject);
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	private void RefreshOpenTenButton()
	{
		if (_selectedBoosterView != null && _selectedBoosterView._info != null && _selectedBoosterView._info.count >= 2)
		{
			string key = ((_selectedBoosterView._info.count >= 10) ? "MainNav/BoosterChamber/Open10" : "MainNav/BoosterChamber/OpenAll");
			_openTenButton.GetComponentInChildren<Localize>().SetText(key);
			_openTenButton.gameObject.SetActive(value: true);
		}
		else
		{
			_openTenButton.gameObject.SetActive(value: false);
		}
	}

	public void SetLogoOut()
	{
		if (!ThereIsABoosterOpened)
		{
			_setLogo.SetLogoOut();
		}
	}

	private void SetLogoRefresh()
	{
		if (!ThereIsABoosterOpened)
		{
			IBoosterChamberSetLogoInfo setLogoTexture = GetSetLogoTexture(_centeredBoosterIndex);
			_setLogo.SetLogoRefresh(setLogoTexture);
			if (_selectedBoosterView != null)
			{
				_selectedBoosterView.UpdateHoverAnimation();
			}
		}
	}

	public void RefreshLogoAndBooster()
	{
		foreach (SealedBoosterView boosterView in _boosterViews)
		{
			boosterView.Refresh();
		}
		foreach (BoosterVoucherView voucherView in _voucherViews)
		{
			voucherView.Refresh();
		}
		SetLogoRefresh();
	}

	private IEnumerator DelayEnableCardZoom(float delay)
	{
		yield return new WaitForSeconds(delay);
		_cardHolder.RolloverZoomView.IsActive = true;
	}

	public void ModalFade_OnClick()
	{
	}

	private void DismissCards()
	{
		if (_ignoreInput)
		{
			return;
		}
		if (_sparkyTourState != null)
		{
			if (_sparkyTourState.ClientForcedToUnlock)
			{
				StartCoroutine(PlatformContext.GetReviewContext().Coroutine_RequestPlatformStoreReview());
			}
			else
			{
				_sparkyTourState.GetColorMasteryEventComplete.AsPromise().ThenOnMainThreadIfSuccess(delegate(bool colorMasteryEventComplete)
				{
					if (colorMasteryEventComplete)
					{
						StartCoroutine(PlatformContext.GetReviewContext().Coroutine_RequestPlatformStoreReview());
					}
				});
			}
		}
		_cardHolder.RolloverZoomView.IsActive = false;
		AudioManager.SetRTPCValue("booster_packrollover", 0f);
		AudioManager.SetRTPCValue("boosterpack_" + _setCode, 0f);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_accept_cards, base.gameObject);
		AudioManager.SetRTPCValue("booster_card_rollover", 0f);
		AudioManager.SetState("music", "menu_booster");
		ThereIsABoosterOpened = false;
		_openingBoosterPackAnimator.SetTrigger(DismissCardsTrigger);
		DisplayBoosters((_selectedBoosterView == null) ? (-1) : _selectedBoosterView.CollationId);
		_wildcardTrack.UpdateTrackPosition(_inventoryManager.Inventory.wcTrackPosition);
		_wildcardTrack.StopTrackSounds();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_card_rare_appear_forceoff, base.gameObject);
	}

	private IEnumerator DelayedTrigger(Animator animator, int triggerName, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		if (null != animator)
		{
			animator.SetTrigger(triggerName);
		}
	}

	private void AudioSequenceOpen()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_boost_pack_open_pack, base.gameObject);
	}

	public void OnScroll(PointerEventData eventData)
	{
		Scroll(eventData.scrollDelta.y);
	}

	private void NextBooster()
	{
		Scroll(1f);
	}

	private void PreviousBooster()
	{
		Scroll(-1f);
	}

	private void Scroll(float scrollDir)
	{
		if (OkToSelectBooster)
		{
			bool flag = false;
			int num = _boosterViews.Count - 1;
			if (!_openingBoosterPackAnimator.GetCurrentAnimatorStateInfo(0).IsName("PackOpening_CardsPresent"))
			{
				num += _voucherViews.Count;
			}
			if (scrollDir < 0f && _centeredBoosterIndex < num)
			{
				flag = true;
				_centeredBoosterIndex++;
			}
			else if (scrollDir > 0f && _centeredBoosterIndex > 0)
			{
				flag = true;
				_centeredBoosterIndex--;
			}
			if (flag)
			{
				_scrolling = true;
				SetLogoOut();
				_boosterSelectionTimer = _scrollSelectDelay;
				CenterIndex(_centeredBoosterIndex);
			}
		}
	}

	private IEnumerator Coroutine_Show()
	{
		_readyToShow = false;
		if (!_cardsArePreloaded)
		{
			_cardsArePreloaded = true;
			yield return _boosterMetaCardViewPool.PreloadCards(base.transform);
		}
		DisplayBoosters(-1);
		if (_boosterViews.Count + _voucherViews.Count > 0)
		{
			CenterIndex(_centeredBoosterIndex);
		}
		_readyToShow = true;
	}

	private void OnDestroy()
	{
		Languages.LanguageChangedSignal.Listeners -= OnLocalize;
		if (_fdc != null)
		{
			_fdc.UnRegisterKillswitchNotification(OnKillSwitch);
			_fdc = null;
		}
		_boosterMetaCardViewPool.Clear();
		_setLogo.Cleanup();
		_openingBoosterPackRendererLoader?.Cleanup();
	}
}
