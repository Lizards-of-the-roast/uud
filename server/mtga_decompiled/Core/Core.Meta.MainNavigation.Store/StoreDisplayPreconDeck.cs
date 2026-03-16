using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.BI;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN.Store;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Core.Meta.MainNavigation.Store;

public class StoreDisplayPreconDeck : StoreItemDisplay
{
	private const int _maxRetryCountFindingStoreItemBase = 60;

	[SerializeField]
	private MeshRenderer[] BoxMeshRenderers;

	[SerializeField]
	private Texture2D DefaultDeckImage;

	[SerializeField]
	private TooltipTrigger _tooltipTrigger;

	[SerializeField]
	private ManaSymbolView _symbolPrefab;

	[SerializeField]
	private DeckViewImages _deckViewImages;

	private Action<StoreItem, Client_PurchaseCurrencyType> _onDeckBoxClicked;

	private CardDatabase _cardDatabase;

	private Client_Deck _preconDeck;

	private DeckInfo _deckInfoBacker;

	private readonly List<CardDataForTile> _cardData = new List<CardDataForTile>();

	private StoreItemBase _storeItemBase;

	private StoreItem _storeItem;

	private DeckInfo _deckInfo
	{
		get
		{
			if (_deckInfoBacker == null)
			{
				_deckInfoBacker = DeckServiceWrapperHelpers.ToAzureModel(_preconDeck);
			}
			return _deckInfoBacker;
		}
	}

	public List<CardDataForTile> CardData => _cardData;

	private void Awake()
	{
		StartCoroutine(FindStoreItemBase());
	}

	private IEnumerator FindStoreItemBase()
	{
		int retryCount = 0;
		while (_storeItemBase == null)
		{
			yield return null;
			_storeItemBase = GetComponentInParent<StoreItemBase>();
			retryCount++;
			if (retryCount >= 60)
			{
				Debug.LogErrorFormat(base.gameObject, "Can not find a `StoreItemBase` in the parent objects of this precon deck. It is required for the background click functionality. Please look at, {0}.", base.gameObject.name);
				yield break;
			}
		}
		_storeItemBase.StoreItemBackgroundClicked += _onDeckBoxClicked;
		List<Sprite> list = new List<Sprite>();
		foreach (ManaColor item in from m in _preconDeck.GetDeckColors(_cardDatabase.CardDataProvider, isLimited: false)
			orderby m
			select m)
		{
			if (item == ManaColor.White)
			{
				list.Add(_deckViewImages.WhiteMana);
			}
			if (item == ManaColor.Blue)
			{
				list.Add(_deckViewImages.BlueMana);
			}
			if (item == ManaColor.Black)
			{
				list.Add(_deckViewImages.BlackMana);
			}
			if (item == ManaColor.Red)
			{
				list.Add(_deckViewImages.RedMana);
			}
			if (item == ManaColor.Green)
			{
				list.Add(_deckViewImages.GreenMana);
			}
		}
		foreach (Sprite item2 in list)
		{
			ManaSymbolView manaSymbolView = UnityEngine.Object.Instantiate(_symbolPrefab, _storeItemBase.ManaSymbolParent.GetComponentInChildren<HorizontalLayoutGroup>().transform, worldPositionStays: true);
			manaSymbolView.SymbolImage.sprite = item2;
			manaSymbolView.transform.ZeroOut();
			manaSymbolView.gameObject.SetActive(value: true);
		}
		_storeItemBase.ManaSymbolParent.gameObject.SetActive(value: true);
	}

	public void Init(StoreItem item, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, Action<StoreItem, Client_PurchaseCurrencyType> onDeckBoxClicked)
	{
		_cardDatabase = cardDatabase;
		_onDeckBoxClicked = onDeckBoxClicked;
		_storeItem = item;
		_preconDeck = Pantry.Get<IPreconDeckServiceWrapper>().GetPreconDeck(new Guid(item.Skus[0].TreasureItem.ReferenceId));
		foreach (KeyValuePair<EDeckPile, List<Client_DeckCard>> item3 in _preconDeck.Contents.Piles.OrderBy((KeyValuePair<EDeckPile, List<Client_DeckCard>> kvp) => kvp.Key switch
		{
			EDeckPile.CommandZone => 0, 
			EDeckPile.Main => 1, 
			EDeckPile.Sideboard => 2, 
			_ => 3, 
		}))
		{
			item3.Deconstruct(out var _, out var value);
			foreach (Client_DeckCard card in value)
			{
				int num = _cardData.FindIndex((CardDataForTile cardDataForTile2) => cardDataForTile2.Card.GrpId == card.Id);
				if (num != -1)
				{
					var (card2, num3, isArtStyle) = (CardDataForTile)(ref _cardData[num]);
					_cardData[num] = new CardDataForTile(card2, num3 + card.Quantity, isArtStyle);
				}
				else
				{
					CardDataForTile item2 = new CardDataForTile(new CardData(null, cardDatabase.CardDataProvider.GetCardPrintingById(card.Id)), card.Quantity, isArtStyle: false);
					_cardData.Add(item2);
				}
			}
		}
		_tooltipTrigger.LocString = "MainNav/Store/Decks/Contents_Click";
		StoreCardView[] componentsInChildren = GetComponentsInChildren<StoreCardView>();
		for (int num4 = 0; num4 < componentsInChildren.Length; num4++)
		{
			StoreCardView storeCardView = componentsInChildren[num4].CreateCard(_cardData[num4].Card, cardDatabase, cardViewBuilder);
			storeCardView.OnClicked = (Action<MetaCardView>)Delegate.Combine(storeCardView.OnClicked, new Action<MetaCardView>(OnCardViewClicked));
			CardViews.Add(storeCardView);
		}
		MeshRendererReferenceLoader[] array = new MeshRendererReferenceLoader[BoxMeshRenderers.Length];
		for (int num5 = 0; num5 < BoxMeshRenderers.Length; num5++)
		{
			array[num5] = new MeshRendererReferenceLoader(BoxMeshRenderers[num5]);
		}
		string cardArtPath = cardViewBuilder.CardMaterialBuilder.TextureLoader.GetCardArtPath(cardDatabase.CardDataProvider.GetCardPrintingById(_preconDeck.Summary.DeckTileId)?.ImageAssetPath);
		DeckBoxUtil.SetDeckBoxTexture(cardArtPath, cardViewBuilder.CardMaterialBuilder.CropDatabase.GetCrop(cardArtPath, "Normal"), array, DefaultDeckImage);
	}

	protected override void OnDestroy()
	{
		_storeItemBase.StoreItemBackgroundClicked -= _onDeckBoxClicked;
		_storeItemBase.StoreItemBackgroundClicked -= OnStoreItemBackgroundClicked;
		foreach (StoreCardView cardView in CardViews)
		{
			cardView.OnClicked = (Action<MetaCardView>)Delegate.Remove(cardView.OnClicked, new Action<MetaCardView>(OnCardViewClicked));
		}
	}

	public void PurchaseConfirmationVisibilityChanged(bool confirmationShowing)
	{
		if (_storeItemBase == null || _onDeckBoxClicked == null)
		{
			return;
		}
		if (confirmationShowing)
		{
			_storeItemBase.StoreItemBackgroundClicked -= _onDeckBoxClicked;
			_storeItemBase.StoreItemBackgroundClicked += OnStoreItemBackgroundClicked;
		}
		else
		{
			_storeItemBase.StoreItemBackgroundClicked -= OnStoreItemBackgroundClicked;
			_storeItemBase.StoreItemBackgroundClicked += _onDeckBoxClicked;
		}
		foreach (StoreCardView cardView in CardViews)
		{
			if (confirmationShowing)
			{
				cardView.OnClicked = (Action<MetaCardView>)Delegate.Remove(cardView.OnClicked, new Action<MetaCardView>(OnCardViewClicked));
			}
			else
			{
				cardView.OnClicked = (Action<MetaCardView>)Delegate.Combine(cardView.OnClicked, new Action<MetaCardView>(OnCardViewClicked));
			}
		}
	}

	private void OnCardViewClicked(MetaCardView cardView)
	{
		if (_storeItemBase != null)
		{
			_storeItemBase.OnBackgroundElementClicked(Client_PurchaseCurrencyType.Gem);
		}
	}

	private void OnStoreItemBackgroundClicked(StoreItem storeItem, Client_PurchaseCurrencyType currencyType)
	{
		CallDeckBoxBuilder();
	}

	public void CallDeckBoxBuilder()
	{
		BIEventType.ButtonPressed.SendWithDefaults(("ButtonIdentifier", "PreconDeckPreviewButton"), ("DynamicButtonId", _storeItem.PurchasingId));
		_deckInfo.name = Wotc.Mtga.Loc.Utils.GetLocalizedDeckName(_deckInfo.name);
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		DeckInfo deckInfo = _deckInfo;
		string format = _preconDeck.Summary.Format;
		string id = _storeItem.Id;
		sceneLoader.GoToDeckBuilder(new DeckBuilderContext(deckInfo, null, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnly, ambiguousFormat: false, default(Guid), null, null, null, cachingEnabled: false, isPlayblade: false, format, isInvalidForEventFormat: false, id));
	}
}
