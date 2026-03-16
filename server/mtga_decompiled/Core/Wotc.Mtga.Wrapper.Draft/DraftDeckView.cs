using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Decks;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftDeckView : MonoBehaviour
{
	private const int k_DraftDeckMinCount = 40;

	[SerializeField]
	private DraftHeaderView _draftHeaderView;

	[SerializeField]
	private GameObject _mainHolderGameObject;

	[SerializeField]
	private GameObject _sideHolderGameObject;

	[SerializeField]
	private DeckMainTitlePanel _mainDeckPanel;

	[SerializeField]
	private DeckSideboardTitlePanel _sideboardDeckPanel;

	[SerializeField]
	private DeckViewImages _deckViewImages;

	[SerializeField]
	private GameObject _waitingOnPackTextObject;

	[SerializeField]
	private CustomButton _deckDetailsButton;

	[SerializeField]
	private DeckCostsSummary _deckCostsSummary;

	[SerializeField]
	private CustomButton _confirmPickButton;

	[SerializeField]
	private Localize _confirmPickButtonText;

	[SerializeField]
	private DraftTimer _draftTimer;

	[SerializeField]
	public Toggle ShowSideboardToggle;

	[SerializeField]
	private GameObject _sideboardToggleOn;

	[SerializeField]
	private GameObject _sideboardToggleOff;

	[SerializeField]
	private TMP_Text _toggleCardCountText;

	[SerializeField]
	private bool _canDragCards = true;

	[SerializeField]
	private bool _canClickCards;

	[SerializeField]
	private bool _sideboardControlsCardPicking;

	[SerializeField]
	private StaticColumnDropTarget _deckColumnViewDropTarget;

	private readonly Dictionary<string, string> _confirmButtonTextParams = new Dictionary<string, string>();

	private bool _isColumnView;

	private DraftContentController _draftContentController;

	public Action<MetaCardView> OnCardClicked;

	private IDraftMetaCardHolder _mainHolderInstance;

	private IDraftMetaCardHolder _sideHolderInstance;

	public Action<MetaCardView, MetaCardHolder> OnCardDropped;

	public Action OnDeckDetailsButtonClicked;

	public Action OnConfirmClicked;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private static readonly int HighlightHash = Animator.StringToHash("Highlight");

	private static readonly int PulseExtraHash = Animator.StringToHash("PulseExtra");

	private static readonly int FlashHash = Animator.StringToHash("Flash");

	public DraftHeaderView DraftHeaderView => _draftHeaderView;

	private IDraftMetaCardHolder _mainHolder
	{
		get
		{
			if (_mainHolderInstance == null)
			{
				_mainHolderInstance = _mainHolderGameObject.GetComponent<IDraftMetaCardHolder>();
			}
			if (_mainHolderInstance == null)
			{
				throw new ArgumentException("Serialized object for \"Main Holder Game Object\" must have a component that implements IDraftMetaCardHolder");
			}
			return _mainHolderInstance;
		}
	}

	private IDraftMetaCardHolder _sideHolder
	{
		get
		{
			if (_sideHolderInstance == null)
			{
				_sideHolderInstance = _sideHolderGameObject.GetComponent<IDraftMetaCardHolder>();
			}
			if (_sideHolderInstance == null)
			{
				throw new ArgumentException("Serialized object for \"Side Holder Game Object\" must have a component that implements IDraftMetaCardHolder");
			}
			return _sideHolderInstance;
		}
	}

	public string DeckBoxTexturePath { get; private set; }

	public ArtCrop DeckBoxTextureCrop { get; private set; }

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	public bool ShowDraftTimer
	{
		get
		{
			if (_draftTimer != null)
			{
				return _draftTimer.gameObject.activeSelf;
			}
			return false;
		}
		set
		{
			if (_draftTimer != null)
			{
				_draftTimer.gameObject.UpdateActive(value);
			}
		}
	}

	private void OnDestroy()
	{
		if (_mainHolder != null)
		{
			IDraftMetaCardHolder mainHolder = _mainHolder;
			mainHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(mainHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(HandleOnCardDropped));
			ListMetaCardHolder_Expanding listMetaCardHolder_Expanding = _mainHolder as ListMetaCardHolder_Expanding;
			if ((bool)listMetaCardHolder_Expanding)
			{
				listMetaCardHolder_Expanding.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(listMetaCardHolder_Expanding.OnCardRemoveClicked, new Action<MetaCardView>(HandleOnCardClicked));
				Pantry.Get<DeckBuilderModelProvider>().CardModifiedInPile -= CardModifiedInPile;
				Pantry.Get<DeckBuilderVisualsUpdater>().MainDeckListVisualsUpdated -= OnMainDeckListVisualsUpdated;
				if (ShowSideboardToggle != null)
				{
					LayoutState.SideboardVisibilityUpdated -= OnSideboardVisibilityUpdated;
				}
			}
		}
		if (_sideHolder != null)
		{
			IDraftMetaCardHolder sideHolder = _sideHolder;
			sideHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(sideHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(HandleOnCardDropped));
			ListMetaCardHolder_Expanding listMetaCardHolder_Expanding2 = _sideHolder as ListMetaCardHolder_Expanding;
			if ((bool)listMetaCardHolder_Expanding2)
			{
				listMetaCardHolder_Expanding2.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(listMetaCardHolder_Expanding2.OnCardRemoveClicked, new Action<MetaCardView>(HandleOnCardClicked));
			}
		}
		if ((bool)_sideboardDeckPanel)
		{
			DeckSideboardTitlePanel sideboardDeckPanel = _sideboardDeckPanel;
			sideboardDeckPanel.OnExpandChange = (Action)Delegate.Remove(sideboardDeckPanel.OnExpandChange, new Action(UpdateDestinationHighlight));
		}
	}

	private void OnValidate()
	{
		if (_mainHolderGameObject != null)
		{
			IDraftMetaCardHolder component = _mainHolderGameObject.GetComponent<IDraftMetaCardHolder>();
			if (component != null)
			{
				_mainHolderInstance = component;
			}
			else
			{
				Debug.LogError("Validaiton error on " + base.gameObject.name + ".\nMain Holder Game Object needs to have a component that implements IDraftMetaCardHolder.\nNulling reference.");
				_mainHolderGameObject = null;
			}
		}
		if (_sideHolderGameObject != null)
		{
			IDraftMetaCardHolder component2 = _sideHolderGameObject.GetComponent<IDraftMetaCardHolder>();
			if (component2 != null)
			{
				_mainHolderInstance = component2;
				return;
			}
			Debug.LogError("Validaiton error on " + base.gameObject.name + ".\nSide Holder Game Object needs to have a component that implements IDraftMetaCardHolder.\nNulling reference.");
			_sideHolderGameObject = null;
		}
	}

	public void Init(ICardRolloverZoom rolloverZoom, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, DraftContentController draftContentController)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_draftContentController = draftContentController;
		_mainHolder.EnsureInit(rolloverZoom, assetLookupSystem, cardDatabase, cardViewBuilder);
		_mainHolder.CanDragCards = (MetaCardView cardView) => _canDragCards;
		_mainHolder.CanSingleClickCards = (MetaCardView cardView) => _canClickCards;
		_mainHolder.CanDoubleClickCards = (MetaCardView cardView) => false;
		_mainHolder.CanDropCards = (MetaCardView cardView) => _canDragCards;
		IDraftMetaCardHolder mainHolder = _mainHolder;
		mainHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(mainHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(HandleOnCardDropped));
		ListMetaCardHolder_Expanding listMetaCardHolder_Expanding = _mainHolder as ListMetaCardHolder_Expanding;
		if ((bool)listMetaCardHolder_Expanding)
		{
			listMetaCardHolder_Expanding.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(listMetaCardHolder_Expanding.OnCardRemoveClicked, new Action<MetaCardView>(HandleOnCardClicked));
			Pantry.Get<DeckBuilderModelProvider>().CardModifiedInPile += CardModifiedInPile;
			Pantry.Get<DeckBuilderVisualsUpdater>().MainDeckListVisualsUpdated += OnMainDeckListVisualsUpdated;
			if (ShowSideboardToggle != null)
			{
				LayoutState.SideboardVisibilityUpdated += OnSideboardVisibilityUpdated;
			}
		}
		StaticColumnManager staticColumnManager = _mainHolder as StaticColumnManager;
		if ((bool)staticColumnManager)
		{
			staticColumnManager.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(staticColumnManager.OnCardClicked, new Action<MetaCardView>(HandleOnCardClicked));
			staticColumnManager.CanAddCard = (uint grpid) => true;
			_isColumnView = true;
		}
		_sideHolder.EnsureInit(rolloverZoom, assetLookupSystem, cardDatabase, cardViewBuilder);
		_sideHolder.CanDragCards = (MetaCardView cardView) => _canDragCards;
		_sideHolder.CanSingleClickCards = (MetaCardView cardView) => _canClickCards;
		_sideHolder.CanDoubleClickCards = (MetaCardView cardView) => false;
		_sideHolder.CanDropCards = (MetaCardView cardView) => _canDragCards;
		IDraftMetaCardHolder sideHolder = _sideHolder;
		sideHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(sideHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(HandleOnCardDropped));
		ListMetaCardHolder_Expanding listMetaCardHolder_Expanding2 = _sideHolder as ListMetaCardHolder_Expanding;
		if ((bool)listMetaCardHolder_Expanding2)
		{
			listMetaCardHolder_Expanding2.enabled = true;
			listMetaCardHolder_Expanding2.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(listMetaCardHolder_Expanding2.OnCardRemoveClicked, new Action<MetaCardView>(HandleOnCardClicked));
		}
		_deckDetailsButton.OnClick.AddListener(OnDeckTitlePanelClick);
		_confirmPickButton.gameObject.SetActive(value: true);
		_confirmPickButton.OnClick.AddListener(delegate
		{
			OnConfirmClicked?.Invoke();
		});
		UpdateConfirmButtonText();
		DeckSideboardTitlePanel sideboardDeckPanel = _sideboardDeckPanel;
		sideboardDeckPanel.OnExpandChange = (Action)Delegate.Combine(sideboardDeckPanel.OnExpandChange, new Action(UpdateDestinationHighlight));
		if (ShowSideboardToggle != null)
		{
			ShowSideboardToggle.onValueChanged.AddListener(ShowSideboardToggle_OnValueChanged);
			ShowSideboardToggle_OnValueChanged(ShowSideboardToggle.isOn);
		}
	}

	public void UpdateConfirmButtonText()
	{
		if (_confirmPickButtonText == null)
		{
			Debug.LogErrorFormat(base.gameObject, "The confirm button text was not set in the {0} prefabs. Please assign so that the text can be properly localized.", base.gameObject.name);
			return;
		}
		_confirmButtonTextParams["numberSelected"] = _draftContentController.NumberOfCardsCurrentlySelected.ToString();
		_confirmButtonTextParams["numberToSelect"] = _draftContentController.DraftPod?.PickNumCardsToTake.ToString();
		if (_draftContentController.DraftPod == null || _draftContentController.DraftPod.PickNumCardsToTake == 1 || _draftContentController.NumberOfCardsCurrentlySelected == _draftContentController.DraftPod.PickNumCardsToTake)
		{
			_confirmPickButtonText.SetText("EPP/RewardWeb/ConfirmPick", _confirmButtonTextParams);
		}
		else
		{
			_confirmPickButtonText.SetText("Social/Presence/Button_PackPick_PickX", _confirmButtonTextParams);
		}
	}

	public void CardModifiedInPile(DeckBuilderPile pile, CardData card)
	{
		if (pile != DeckBuilderPile.MainDeck)
		{
			return;
		}
		DeckBuilderLayoutState deckBuilderLayoutState = Pantry.Get<DeckBuilderLayoutState>();
		if (deckBuilderLayoutState.LayoutInUse == DeckBuilderLayout.List)
		{
			CardPrintingData cardPrintingById = Pantry.Get<ICardDatabaseAdapter>().CardDataProvider.GetCardPrintingById(card.GrpId);
			if (!deckBuilderLayoutState.IsListViewSideboarding)
			{
				((ListMetaCardHolder_Expanding)_mainHolder).ScrollToGrpId(cardPrintingById.GrpId);
			}
		}
	}

	public void OnMainDeckListVisualsUpdated(List<ListMetaCardViewDisplayInformation> displayInfos)
	{
		_mainHolder.SetCards(displayInfos);
	}

	public void OnSideboardVisibilityUpdated(bool showSideboard)
	{
		ShowSideboardToggle.gameObject.SetActive(showSideboard);
		ShowSideboardToggle.isOn = LayoutState.IsListViewSideboarding;
	}

	public void UpdateDeckVisual(Deck deck, DeckFormat deckFormat)
	{
		if (ShowSideboardToggle != null)
		{
			SetToggleCardCount(deck);
			_mainDeckPanel.SetActive(!IsShowingSideboard());
			_sideboardDeckPanel.SetActive(IsShowingSideboard());
		}
		_mainHolder.SetCards(deck.Main.ToTJDisplay(sort: true));
		_mainDeckPanel.SetCardCount(deck.Main.Sum((ICardCollectionItem i) => i.Quantity), deckFormat, 40, alwaysDefaultColor: true);
		deck.DeckTileId = deck.Main.FirstOrDefault((ICardCollectionItem x) => x.Quantity > 0)?.Card.GrpId ?? 0;
		string artPath = (DeckBoxTexturePath = ((deck.DeckArtId == 0) ? (_cardDatabase.CardDataProvider.GetCardPrintingById(deck.DeckTileId)?.ImageAssetPath ?? string.Empty) : _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(deck.DeckArtId)?.FirstOrDefault()?.ImageAssetPath));
		DeckBoxTextureCrop = _cardViewBuilder.CardMaterialBuilder.CropDatabase.GetCrop(artPath, "Normal") ?? ArtCrop.DEFAULT;
		_mainDeckPanel.SetDeckBoxTexture(artPath, DeckBoxTextureCrop);
		_sideHolder.SetCards(deck.Sideboard.ToTJDisplay(sort: true));
		_sideboardDeckPanel.SetCardCount(deck.Sideboard.Sum((ICardCollectionItem i) => i.Quantity), deckFormat.MaxSideboardCards);
		if (_deckCostsSummary != null)
		{
			_deckCostsSummary.SetDeck(deck.Main.ToCardPrintingQuantityList());
		}
		UpdateDestinationHighlight();
	}

	public void SetConfirmPickInteractable(bool value)
	{
		_confirmPickButton.Interactable = value;
	}

	public void UpdateDraftTimer(float secondsRemaining, float secondsTotal)
	{
		if (_draftTimer != null)
		{
			_draftTimer.UpdateTime(secondsRemaining, secondsTotal);
		}
	}

	public void ActivateWaitingOnPacksText(bool activate)
	{
		_waitingOnPackTextObject.gameObject.UpdateActive(activate);
	}

	public void HandleOnCardDropped(MetaCardView cardView, MetaCardHolder destination)
	{
		OnCardDropped?.Invoke(cardView, destination);
	}

	public void HandleOnCardClicked(MetaCardView cardView)
	{
		PulseHeader(_isColumnView ? IsCardInSideboard(cardView) : (!IsShowingSideboard()));
		if (ShowSideboardToggle != null)
		{
			ShowSideboardToggle.animator.SetTrigger(FlashHash);
		}
		OnCardClicked?.Invoke(cardView);
	}

	public void OnCardPicked()
	{
		if (_sideboardControlsCardPicking)
		{
			PulseHeader(!IsShowingSideboard());
		}
	}

	private void PulseHeader(bool pulseMainHeader)
	{
		Animator animator = (pulseMainHeader ? _mainDeckPanel.GetComponent<Animator>() : _sideboardDeckPanel.GetComponent<Animator>());
		if ((bool)animator)
		{
			animator.SetTrigger(PulseExtraHash);
		}
	}

	public void UpdateDestinationHighlight()
	{
		if (_sideboardControlsCardPicking)
		{
			Animator component = _mainDeckPanel.GetComponent<Animator>();
			Animator component2 = _sideboardDeckPanel.GetComponent<Animator>();
			if ((bool)component)
			{
				component.SetBool(HighlightHash, !IsShowingSideboard());
			}
			if ((bool)component2)
			{
				component2.SetBool(HighlightHash, IsShowingSideboard());
			}
		}
	}

	public List<MetaCardHolder> GetMainDeckMetaCardHolderList()
	{
		List<MetaCardHolder> metaCardHolderList = _mainHolder.GetMetaCardHolderList();
		if (metaCardHolderList.Count == 0)
		{
			metaCardHolderList.Add(_deckColumnViewDropTarget);
		}
		return metaCardHolderList;
	}

	public List<MetaCardHolder> GetSideboardMetaCardHolderList()
	{
		return _sideHolder.GetMetaCardHolderList();
	}

	public void ResetLanguage()
	{
		_mainHolder.ResetLanguage();
		_sideHolder.ResetLanguage();
	}

	private void ShowSideboardToggle_OnValueChanged(bool value)
	{
		_mainHolderGameObject.UpdateActive(!value);
		_sideboardToggleOn.UpdateActive(value);
		_sideHolderGameObject.UpdateActive(value);
		_sideboardToggleOff.UpdateActive(!value);
	}

	private void OnDeckTitlePanelClick()
	{
		if (_sideboardControlsCardPicking && _sideboardDeckPanel.Expanded)
		{
			_sideboardDeckPanel.SetExpand(expand: false);
		}
		else
		{
			OnDeckDetailsButtonClicked?.Invoke();
		}
	}

	private void SetToggleCardCount(Deck deck)
	{
		if (!(_toggleCardCountText == null))
		{
			int num = (IsShowingSideboard() ? deck.Main.Sum((ICardCollectionItem i) => i.Quantity) : deck.Sideboard.Sum((ICardCollectionItem i) => i.Quantity));
			int num2 = (IsShowingSideboard() ? 40 : (-1));
			_toggleCardCountText.text = ((num2 <= 0) ? $"({num})" : $"({num}/{num2})");
		}
	}

	public bool IsShowingSideboard()
	{
		if (ShowSideboardToggle != null)
		{
			return ShowSideboardToggle.isOn;
		}
		if (_sideboardControlsCardPicking)
		{
			return _sideboardDeckPanel.Expanded;
		}
		return false;
	}

	public bool IsCardInSideboard(MetaCardView cardView)
	{
		return GetSideboardMetaCardHolderList().Contains(cardView.Holder);
	}
}
