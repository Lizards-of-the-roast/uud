using System.Collections.Generic;
using AssetLookupTree;
using Pooling;
using UnityEngine;
using Wizards.MDN.DeckManager;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Inventory;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Cosmetics;

public class CosmeticsPerDeckView : MonoBehaviour
{
	[SerializeField]
	private Transform _displayItemGrid;

	[SerializeField]
	private Transform _selectorTransform;

	private Client_Deck _deckInfo;

	private CosmeticSelectorController _cosmeticSelectorController;

	private CosmeticsProvider _cosmeticsProvider;

	public void Init(Client_Deck deckInfo, ClientVanitySelectionsV3 accountDefaults, IClientLocProvider locMan, CosmeticsProvider cosmetics, AvatarCatalog avatarCatalog, PetCatalog petCatalog, AssetLookupSystem assetLookupSystem, IDeckSleeveProvider deckManager, ICardRolloverZoom zoomHandler, BILogger logger, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, List<CardBackSelectorDisplayData> cardBackSelectorDisplayData, IEmoteDataProvider emoteDataProvider, IUnityObjectPool objectPool)
	{
		_cosmeticSelectorController = GetComponent<CosmeticSelectorController>();
		_deckInfo = deckInfo;
		_cosmeticsProvider = cosmetics;
		_cosmeticSelectorController.Init(_displayItemGrid, _selectorTransform, locMan, cosmetics, avatarCatalog, petCatalog, assetLookupSystem, zoomHandler, logger, cardDatabase, cardViewBuilder, emoteDataProvider, objectPool, deckManager, null, isReadOnly: false);
		_cosmeticSelectorController.SetOnOpenCallback(HideRootUI);
		_cosmeticSelectorController.SetOnCloseCallback(ShowRootUI);
		if (deckInfo == null)
		{
			SetData(accountDefaults);
		}
		else
		{
			SetData(deckInfo, accountDefaults);
		}
	}

	private void SetData(ClientVanitySelectionsV3 accountDefaults)
	{
		_cosmeticSelectorController.SetData(accountDefaults);
	}

	private void SetData(Client_Deck deckInfo, ClientVanitySelectionsV3 accountDefaults)
	{
		_cosmeticSelectorController.SetData(deckInfo, accountDefaults);
	}

	public void ShowRootUI()
	{
		_displayItemGrid.gameObject.SetActive(value: true);
	}

	public void HideRootUI()
	{
		_displayItemGrid.gameObject.SetActive(value: false);
	}
}
