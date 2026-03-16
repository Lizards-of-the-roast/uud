using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Providers;

namespace Wizards.MDN.Store;

public static class CardTileStoreCore
{
	public static Dictionary<(string, string), CardTileStoreViewModel> ViewModelsForStoreItem(List<CardDataForTile> cardDatas, InventoryManager inventoryManager, CosmeticsProvider cosmeticsProvider, ITitleCountManager titleCountManager)
	{
		Dictionary<(string, string), CardTileStoreViewModel> dictionary = new Dictionary<(string, string), CardTileStoreViewModel>();
		Dictionary<uint, CardPrintingQuantity> cardsNeededToFinishDeckForProration = GetCardsNeededToFinishDeckForProration(cardDatas, inventoryManager, titleCountManager);
		foreach (CardDataForTile cardData2 in cardDatas)
		{
			cardData2.Deconstruct(out var card, out var quantity, out var isArtStyle);
			CardData cardData = card;
			uint num = quantity;
			bool flag = isArtStyle;
			quantity = cardData.GrpId;
			CardTileStoreViewModel cardTileStoreViewModel = _viewModelForID(quantity.ToString(), cardData.SkinCode, dictionary);
			cardTileStoreViewModel.CardData = cardData;
			if (flag)
			{
				bool flag2 = cosmeticsProvider.OwnsSkin(cardData.Printing.ArtId, cardData.SkinCode);
				cardTileStoreViewModel.OwnedCount = (flag2 ? 1 : 0);
				cardTileStoreViewModel.UnownedCount = ((!flag2) ? 1 : 0);
			}
			else
			{
				CardPrintingQuantity value;
				int num2 = (int)(cardsNeededToFinishDeckForProration.TryGetValue(cardData.GrpId, out value) ? value.Quantity : 0);
				int ownedCount = (int)num - num2;
				if (cardData.Printing.IsBasicLand)
				{
					ownedCount = 0;
				}
				cardTileStoreViewModel.OwnedCount = ownedCount;
				cardTileStoreViewModel.UnownedCount = (int)num;
			}
			cardTileStoreViewModel.IsArtStyle = flag;
		}
		return dictionary;
	}

	public static Dictionary<uint, CardPrintingQuantity> GetCardsNeededToFinishDeckForProration(List<CardDataForTile> cardDatas, InventoryManager inventoryManager, ITitleCountManager titleCountManager)
	{
		Dictionary<uint, CardPrintingQuantity> dictionary = new Dictionary<uint, CardPrintingQuantity>();
		foreach (IGrouping<uint, CardDataForTile> item in from cd in cardDatas
			group cd by cd.Card.TitleId)
		{
			uint num = 0u;
			foreach (CardDataForTile item2 in item.ToList())
			{
				inventoryManager.Cards.TryGetValue(item2.Card.GrpId, out var value);
				if (value == 0 && !item2.IsArtStyle)
				{
					AddCount(dictionary, item2.Card.Printing, 1u);
					num++;
				}
			}
			titleCountManager.OwnedTitleCounts.TryGetValue(item.Key, out var value2);
			uint num2 = (uint)Math.Min(value2, 4);
			uint num3 = (uint)Math.Min(item.Sum((CardDataForTile t) => t.Quantity), 4);
			int num4 = Math.Clamp((int)(num3 - num2 - num), 0, (int)num3);
			if (num4 <= 0)
			{
				continue;
			}
			int num5 = num4;
			foreach (CardDataForTile item3 in item.OrderByDescending((CardDataForTile p) => p.Card.GrpId))
			{
				inventoryManager.Cards.TryGetValue(item3.Card.GrpId, out var value3);
				uint num6 = (uint)item3.Quantity;
				if (num6 > item3.Card.Printing.MaxCollected)
				{
					num6 = item3.Card.Printing.MaxCollected;
				}
				uint count = GetCount(dictionary, item3.Card.Printing);
				int num7 = (int)num6 - value3 - (int)count;
				if (num7 > 0)
				{
					int num8 = Math.Min(num7, num5);
					num5 -= num8;
					AddCount(dictionary, item3.Card.Printing, (uint)num8);
					if (num5 <= 0)
					{
						break;
					}
				}
			}
		}
		return dictionary;
	}

	private static void AddCount(Dictionary<uint, CardPrintingQuantity> dict, CardPrintingData card, uint quantity)
	{
		if (dict.TryGetValue(card.GrpId, out var value))
		{
			value.Quantity += quantity;
			return;
		}
		dict[card.GrpId] = new CardPrintingQuantity
		{
			Printing = card,
			Quantity = quantity
		};
	}

	private static uint GetCount(Dictionary<uint, CardPrintingQuantity> dict, CardPrintingData card)
	{
		if (!dict.TryGetValue(card.GrpId, out var value))
		{
			return 0u;
		}
		return value.Quantity;
	}

	public static List<CardDataForTile> CardDataForViews(List<StoreCardView> cardViews, List<Sku> skus)
	{
		List<CardDataForTile> list = new List<CardDataForTile>();
		foreach (StoreCardView cardView in cardViews)
		{
			if (cardView.IsFakeStyleCard)
			{
				continue;
			}
			CardData cardData = CardDataForView(cardView);
			if (cardData != null)
			{
				Sku sku = skus.Find((Sku sku2) => sku2.TreasureItem.ReferenceId == cardData.GrpId.ToString());
				CardDataForTile item = new CardDataForTile(cardData, (uint)(sku?.LimitRemaining ?? 0), isArtStyle: false);
				list.Add(item);
			}
		}
		return list;
	}

	private static CardData CardDataForView(StoreCardView cardView)
	{
		CardData cardData = ((cardView._copiedCard == null || cardView._copiedCard.Card == null) ? cardView.Card : cardView._copiedCard.Card);
		if (cardData != null && (cardData.IsFakeStyleCard || cardData.GrpId == 0))
		{
			return null;
		}
		return cardData;
	}

	public static CardTileStoreViewModel ViewModelForID(string id, string styleCode, Dictionary<(string, string), CardTileStoreViewModel> viewModelsHash)
	{
		return _viewModelForID(id, styleCode, viewModelsHash, 1);
	}

	public static int ExpectedTileCountForTreasureType(TreasureType treasureType)
	{
		return treasureType switch
		{
			TreasureType.GrantEntirePreConDeck => 15, 
			TreasureType.Card => 1, 
			TreasureType.ArtStyle => 1, 
			_ => 0, 
		};
	}

	public static CardTileStoreViewModel _viewModelForID(string id, string styleCode, Dictionary<(string, string), CardTileStoreViewModel> viewModelsHash, int unownedQuantity = 0)
	{
		if (!viewModelsHash.TryGetValue((id, styleCode), out var value))
		{
			value = new CardTileStoreViewModel
			{
				UnownedCount = unownedQuantity
			};
			viewModelsHash.Add((id, styleCode), value);
		}
		return value;
	}
}
