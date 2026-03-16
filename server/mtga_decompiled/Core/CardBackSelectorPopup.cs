using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Cosmetics;
using DG.Tweening;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

public class CardBackSelectorPopup : PopupBase, IScrollHandler, IEventSystemHandler, ICosmeticSelector<string>
{
	[SerializeField]
	private CustomButton _selectButton;

	[SerializeField]
	private CustomButton _cancelButton;

	[SerializeField]
	private CustomButton _favoriteButton;

	[SerializeField]
	private CustomButton _storeButton;

	[SerializeField]
	private CustomButton _makeAccountDefaultButton;

	[SerializeField]
	private CardBackSelector _cardSelectorPrefab;

	[SerializeField]
	private RectTransform _cardSelectorParent;

	[SerializeField]
	private float _selectorWidthInLayout = 360f;

	[SerializeField]
	private float _selectorMoveDuration = 0.35f;

	[SerializeField]
	private Ease _selectorMoveEase = Ease.OutQuint;

	[SerializeField]
	private bool _selfScroll = true;

	[SerializeField]
	private Scrollbar _scrollbar;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	private int _currentSelector;

	private List<CardBackSelector> _selectors = new List<CardBackSelector>();

	private Action<Action> _storeAction;

	private IBILogger _logger;

	private CardDatabase _cardDatabase;

	private string _defaultSleeve = "";

	private Action _onDefaultCallback;

	private CosmeticsProvider _cosmeticsProvider;

	private string _currentSleeve;

	private bool _isDefaultSelector;

	private StoreManager _storeManager;

	public event Action<string> OnSelected;

	protected override void Awake()
	{
		base.Awake();
		_selectButton.OnClick.AddListener(SelectButton_OnClick);
		_selectButton.OnMouseover.AddListener(Button_OnMouseover);
		_makeAccountDefaultButton.OnClick.AddListener(SelectButton_OnClick);
		_makeAccountDefaultButton.OnMouseover.AddListener(Button_OnMouseover);
		if (_cancelButton != null)
		{
			_cancelButton.OnClick.AddListener(CancelButton_OnClick);
			_cancelButton.OnMouseover.AddListener(Button_OnMouseover);
		}
		if (_favoriteButton != null)
		{
			_favoriteButton.OnClick.AddListener(FavoriteButton_OnClick);
			_favoriteButton.OnMouseover.AddListener(Button_OnMouseover);
		}
		if (_storeButton != null)
		{
			_storeButton.OnClick.AddListener(StoreButton_OnClick);
			_storeButton.OnMouseover.AddListener(Button_OnMouseover);
		}
		_cardSelectorParent.DestroyChildren();
	}

	public void Init(ICardRolloverZoom zoomHandler, IBILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CosmeticsProvider cosmeticsProvider, StoreManager storeManager)
	{
		_cosmeticsProvider = cosmeticsProvider;
		_cardHolder.RolloverZoomView = zoomHandler;
		_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_cardDatabase = cardDatabase;
		_logger = logger;
		_storeManager = storeManager;
	}

	public void SetData(string currentSleeve, string defaultSleeve)
	{
		_currentSleeve = currentSleeve;
		_defaultSleeve = defaultSleeve;
	}

	public void Open(bool showDefaultInterface = true)
	{
		base.gameObject.SetActive(value: true);
		_favoriteButton.gameObject.SetActive(showDefaultInterface);
		StartCoroutine(ShowYield(_currentSleeve, GetCardBackSelectorDisplayDataList(_cosmeticsProvider, _storeManager), _defaultSleeve));
		_isDefaultSelector = showDefaultInterface;
	}

	private static List<CardBackSelectorDisplayData> GetCardBackSelectorDisplayDataList(CosmeticsProvider cosmeticsProvider, StoreManager storeManager)
	{
		List<CardBackSelectorDisplayData> list = new List<CardBackSelectorDisplayData>();
		list.AddRange(cosmeticsProvider.PlayerOwnedSleeves.Select((CosmeticsSleeveEntry item) => new CardBackSelectorDisplayData(item.Id, collected: true)));
		if (storeManager != null)
		{
			list.AddRange(from item in storeManager.Sleeves
				where item.HasRemainingPurchases
				select new CardBackSelectorDisplayData(item.PrefabIdentifier, collected: false, item.StoreSection, item.Id));
			list.AddRange(from item in storeManager.CardbackCatalog.Values
				where item.StoreBundles?[0].HasRemainingPurchases ?? false
				select new CardBackSelectorDisplayData(item.Id, collected: false, item.StoreSection, item.StoreBundles?.FirstOrDefault()?.Id));
		}
		return list;
	}

	private IEnumerator ShowYield(string currentCardBack, List<CardBackSelectorDisplayData> cardBacks, string defaultSleeve = "")
	{
		_defaultSleeve = defaultSleeve;
		if (currentCardBack == null)
		{
			currentCardBack = defaultSleeve;
		}
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		List<CardBackSelectorDisplayData> cardBacksToShow = new List<CardBackSelectorDisplayData>(cardBacks);
		cardBacksToShow.Insert(0, new CardBackSelectorDisplayData("CardBack_Default", collected: true));
		_currentSelector = 0;
		int index = 0;
		while (index < cardBacksToShow.Count)
		{
			CardBackSelectorDisplayData cardBackSelectorDisplayData = cardBacksToShow[index];
			CardData data = CardDataExtensions.CreateSkinCard(0u, _cardDatabase, null, cardBackSelectorDisplayData.Name, faceDown: true);
			CardBackSelector cardBackSelector;
			if (index < _selectors.Count)
			{
				cardBackSelector = _selectors[index];
				cardBackSelector.CDC.SetModel(data);
				cardBackSelector.CardView.SetData(data);
			}
			else
			{
				cardBackSelector = UnityEngine.Object.Instantiate(_cardSelectorPrefab, _cardSelectorParent);
				cardBackSelector.CardView.Init(_cardHolder.CardDatabase, _cardHolder.CardViewBuilder);
				cardBackSelector.CardView.SetData(data);
				cardBackSelector.CardView.Holder = _cardHolder;
				cardBackSelector.CDC = cardBackSelector.CardView.CardView;
				CDCMetaCardView cardView = cardBackSelector.CardView;
				cardView.OnClicked = (Action<MetaCardView>)Delegate.Combine(cardView.OnClicked, new Action<MetaCardView>(CardSelector_OnClick));
				_selectors.Add(cardBackSelector);
			}
			cardBackSelector.CardBack = cardBackSelectorDisplayData.Name;
			cardBackSelector.Collected = cardBackSelectorDisplayData.Collected;
			cardBackSelector.ListingId = cardBackSelectorDisplayData.ListingId;
			cardBackSelector.StoreSection = cardBackSelectorDisplayData.StoreSection;
			cardBackSelector.gameObject.SetActive(value: true);
			if (currentCardBack == cardBackSelectorDisplayData.Name)
			{
				_currentSelector = index;
			}
			yield return null;
			int num = index + 1;
			index = num;
		}
		for (int i = cardBacksToShow.Count; i < _selectors.Count; i++)
		{
			_selectors[i].gameObject.SetActive(value: false);
		}
		Show();
		yield return null;
		if (_selfScroll)
		{
			DOTween.Kill(_cardSelectorParent);
			_cardSelectorParent.DOAnchorPosX((float)(-_currentSelector) * _selectorWidthInLayout, 0f);
		}
		if (_scrollbar != null)
		{
			_scrollbar.value = 1f;
		}
		RefreshAnimators();
	}

	public void Close()
	{
		base.Hide();
	}

	protected override void Hide()
	{
		base.Hide();
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_out", AudioManager.Default);
	}

	private void RefreshAnimators()
	{
		string defaultSleeve = _defaultSleeve;
		for (int i = 0; i < _selectors.Count; i++)
		{
			_selectors[i].Animator.SetBool("Select", i == _currentSelector);
			_selectors[i].Animator.SetBool("Favorite", _selectors[i].CardBack == defaultSleeve);
			_selectors[i].Animator.SetBool("Locked", !_selectors[i].Collected);
		}
		bool flag = _selectors[_currentSelector].CardBack == defaultSleeve;
		bool collected = _selectors[_currentSelector].Collected;
		if (_favoriteButton != null)
		{
			_favoriteButton.Interactable = !flag && collected;
		}
		if (_storeButton != null)
		{
			_selectButton.gameObject.SetActive(collected);
			_storeButton.gameObject.SetActive(!collected);
		}
		ShowButtons(!collected);
	}

	private void ShowButtons(bool selectionsIsLocked)
	{
		_favoriteButton.gameObject.SetActive(_isDefaultSelector && !selectionsIsLocked);
		_makeAccountDefaultButton.gameObject.SetActive(!_isDefaultSelector && !selectionsIsLocked);
		_selectButton.gameObject.SetActive(_isDefaultSelector && !selectionsIsLocked);
		_storeButton.gameObject.SetActive(selectionsIsLocked);
	}

	private void Button_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public override void OnEnter()
	{
		SelectButton_OnClick();
	}

	public override void OnEscape()
	{
		_cardHolder.RolloverZoomView.CardRolledOff(null, alwaysRollOff: true);
		CancelButton_OnClick();
	}

	private void SelectButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_select", base.gameObject);
		_currentSleeve = _selectors[_currentSelector].CardBack;
		this.OnSelected?.Invoke(_selectors[_currentSelector].CardBack);
		Hide();
	}

	private void FavoriteButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		string cardBack = _selectors[_currentSelector].CardBack;
		string defaultSleeve = _defaultSleeve;
		BI_SetDefaultVanity(VanityItemType.cardback.ToString(), cardBack, defaultSleeve);
		_cosmeticsProvider.SetCardbackSelection(cardBack).ThenOnMainThread(delegate(Promise<PreferredCosmetics> p)
		{
			if (p.Successful)
			{
				OnDefaultSet();
			}
		});
	}

	private void OnDefaultSet()
	{
		string cardBack = _selectors[_currentSelector].CardBack;
		_onDefaultCallback?.Invoke();
		_defaultSleeve = cardBack;
		RefreshAnimators();
	}

	private void BI_SetDefaultVanity(string vanityType, string newDefaultSleeve, string oldDefaultSleeve)
	{
		SetDefaultVanity payload = new SetDefaultVanity
		{
			EventTime = DateTime.UtcNow,
			VanityType = vanityType,
			VanityName = newDefaultSleeve,
			PrevVanityName = oldDefaultSleeve,
			SelectionMethod = ""
		};
		_logger.Send(ClientBusinessEventType.SetDefaultVanity, payload);
	}

	private void StoreButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		StoreTabType storeTab = _selectors[_currentSelector].StoreSection switch
		{
			EStoreSection.Bundles => StoreTabType.Bundles, 
			EStoreSection.ProgressionTracks => StoreTabType.Featured, 
			_ => StoreTabType.Cosmetics, 
		};
		if (_selectors[_currentSelector].ListingId != null)
		{
			Action obj = delegate
			{
				SceneLoader.GetSceneLoader().GoToStoreItem(_selectors[_currentSelector].ListingId, storeTab, "clicked store button from card back selection");
			};
			_storeAction(obj);
		}
		else
		{
			Action obj2 = delegate
			{
				SceneLoader.GetSceneLoader().GoToStore(storeTab, "clicked store button from card back selection");
			};
			_storeAction(obj2);
		}
	}

	private void CancelButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
		Hide();
	}

	private void CardSelector_OnClick(MetaCardView selector)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_choose", base.gameObject);
		SetCurrentSelector(_selectors.FindIndex((CardBackSelector x) => x.CardView == selector));
	}

	private void SetCurrentSelector(int currentSelector)
	{
		_currentSelector = currentSelector;
		AudioManager.PlayAudio("sfx_ui_hovercard", base.gameObject);
		if (_selfScroll)
		{
			DOTween.Kill(_cardSelectorParent);
			_cardSelectorParent.DOAnchorPosX((float)(-_currentSelector) * _selectorWidthInLayout, _selectorMoveDuration).SetEase(_selectorMoveEase).SetTarget(_cardSelectorParent);
		}
		RefreshAnimators();
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (_selfScroll)
		{
			if (eventData.scrollDelta.y < 0f && _currentSelector < _selectors.Count - 1)
			{
				SetCurrentSelector(_currentSelector + 1);
			}
			if (eventData.scrollDelta.y > 0f && _currentSelector > 0)
			{
				SetCurrentSelector(_currentSelector - 1);
			}
		}
	}

	public void OnStoreClicked(Action<Action> storeAction)
	{
		_storeAction = storeAction;
	}

	public void SetOnDefaultCallback(Action onDefaultCallback)
	{
		_onDefaultCallback = onDefaultCallback;
	}

	public void SetCallbacks(Action<string> onSelected, Action onHide)
	{
		this.OnSelected = onSelected;
		OnHide = onHide;
	}
}
