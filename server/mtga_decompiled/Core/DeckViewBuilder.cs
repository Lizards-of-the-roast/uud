using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckViewBuilder
{
	private ICardBuilder<Meta_CDC> _metaCardViewBuilder;

	private IUnityObjectPool _unityObjectPool;

	private AssetLookupSystem _assetLookupSystem;

	private CardViewBuilder _cardViewBuilder;

	private CardDatabase _cardDatabase;

	private IClientLocProvider _clientLocManager;

	private IColorChallengeStrategy _colorChallengeStrategy;

	private CosmeticsProvider _cosmeticsProvider;

	public static DeckViewBuilder Create()
	{
		DeckViewBuilder obj = new DeckViewBuilder
		{
			_unityObjectPool = Pantry.Get<IUnityObjectPool>(),
			_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem,
			_cardViewBuilder = Pantry.Get<CardViewBuilder>()
		};
		obj._metaCardViewBuilder = new MetaCardBuilder(obj._cardViewBuilder);
		obj._cardDatabase = Pantry.Get<ICardDatabaseAdapter>() as CardDatabase;
		obj._clientLocManager = Pantry.Get<IClientLocProvider>();
		obj._colorChallengeStrategy = Pantry.Get<IColorChallengeStrategy>();
		obj._cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		return obj;
	}

	public DeckView CreateDeckView(Client_Deck deck, Transform parent)
	{
		return CreateDeckView(CreateDeckViewInfoFromDeckSummary(deck), parent);
	}

	public DeckView CreateDeckView(DeckViewInfo deckViewInfo, Transform parent)
	{
		_assetLookupSystem.Blackboard.Clear();
		DeckViewPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<DeckViewPrefab>().GetPayload(_assetLookupSystem.Blackboard);
		DeckView component = _unityObjectPool.PopObject(payload.PrefabPath, parent).GetComponent<DeckView>();
		component.gameObject.transform.ZeroOut();
		component.Initialize(_metaCardViewBuilder, _unityObjectPool, _assetLookupSystem);
		component.SetDeckModel(deckViewInfo);
		return component;
	}

	public void ReleaseDeckView(DeckView deckView)
	{
		if ((bool)deckView)
		{
			_unityObjectPool.PushObject(deckView.gameObject);
		}
	}

	public List<DeckViewInfo> CreateDeckViewInfos(List<Client_Deck> decks)
	{
		List<DeckViewInfo> list = new List<DeckViewInfo>();
		decks = decks.OrderByDescending((Client_Deck d) => (!(d.Summary.LastPlayed > d.Summary.LastUpdated)) ? d.Summary.LastUpdated : d.Summary.LastPlayed).ToList();
		foreach (Client_Deck deck in decks)
		{
			DeckViewInfo item = CreateDeckViewInfoFromDeckSummary(deck);
			list.Add(item);
		}
		return list;
	}

	public DeckViewInfo CreateDeckViewInfoFromDeckSummary(Client_Deck deck)
	{
		string localizedDeckName = Utils.GetLocalizedDeckName(deck.Summary.Name, _clientLocManager);
		return CreateDeckViewInfo(deck, localizedDeckName);
	}

	public DeckViewInfo CreateDeckViewInfoFromEvent(IPlayerEvent playerEvent)
	{
		Client_Deck courseDeck = playerEvent.CourseData.CourseDeck;
		string localizedDeckName = ((courseDeck.Summary.Name == null) ? playerEvent.GetDeckName(_clientLocManager) : Utils.GetLocalizedDeckName(courseDeck.Summary.Name, _clientLocManager));
		return CreateDeckViewInfo(courseDeck, localizedDeckName);
	}

	private DeckViewInfo CreateDeckViewInfo(Client_Deck deck, string localizedDeckName)
	{
		Client_DeckSummary summary = deck.Summary;
		FormatManager formatManager = WrapperController.Instance.FormatManager;
		summary.Name = localizedDeckName;
		CardArtTextureLoader textureLoader = _cardViewBuilder.CardMaterialBuilder.TextureLoader;
		IArtCropProvider cropDatabase = _cardViewBuilder.CardMaterialBuilder.CropDatabase;
		string text = ((summary.DeckArtId == 0) ? null : _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(summary.DeckArtId)?.FirstOrDefault()?.ImageAssetPath);
		if (text == null)
		{
			text = _cardDatabase.CardDataProvider.GetCardPrintingById(summary.DeckTileId)?.ImageAssetPath;
		}
		SimpleLogUtils.LogWarningIfNull(text, $"Deck {summary.DeckId} is missing a deckImageAssetPath! (from deck art ID {summary.DeckArtId} and tile ID {summary.DeckTileId})");
		bool isLimited = FormatUtilities.IsLimited(formatManager.GetSafeFormat(summary.Format).FormatType);
		HashSet<ManaColor> deckColors = deck.GetDeckColors(_cardDatabase.CardDataProvider, isLimited);
		return new DeckViewInfo
		{
			deckId = summary.DeckId,
			sleeveData = CardDataExtensions.CreateSkinCard(0u, _cardDatabase, "", summary.CardBack ?? _cosmeticsProvider._vanitySelections.cardBackSelection, faceDown: true),
			deck = deck,
			accountCosmeticDefaults = _cosmeticsProvider._vanitySelections,
			crop = cropDatabase.GetCrop(text, "Normal"),
			deckImageAssetPath = textureLoader.GetCardArtPath(text),
			deckName = localizedDeckName,
			manaColors = deckColors.ToList(),
			isFavorite = summary.IsFavorite,
			useHistoricLabel = FormatUtilitiesClient.UseHistoricLabel(deck, _cardDatabase, formatManager),
			LastPlayed = summary.LastPlayed,
			LastUpdated = summary.LastUpdated,
			DeckFormat = summary.Format,
			NetDeckFolderId = summary.NetDeckFolderId,
			IsNetDeck = summary.IsNetDeck
		};
	}
}
