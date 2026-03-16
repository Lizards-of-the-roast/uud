using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.DeckBuilder;
using Core.Shared.Code.CardFilters;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public static class PreferredPrintingUtilities
{
	public static List<PagesMetaCardViewDisplayInformation> GatherDisplayInfosForTitle(uint titleId, List<PagesMetaCardViewDisplayInformation> displayInfos, uint? collapsingCardTitleIdForPageScroll, DeckBuilderContext context, DeckBuilderModel model, CardSkinCatalog cardSkinCatalog, CosmeticsProvider cosmeticsProvider, IReadOnlyCardFilter filter, ICardDatabaseAdapter cardDatabase, InventoryManager inventoryManager, IEmergencyCardBansProvider emergencyCardBansProvider, bool isExpanded, bool showCardIfAllInDeck, bool showUnlimitedBasics, bool showInfiniteAnyNumbers, bool allowUnowned, bool useNewTags, bool useFactionTags, IEnumerable<CardPrintingQuantity> poolEntries, Dictionary<uint, uint> unownedCardSlots, bool expandAllAtOnce, HashSet<uint> expandedPoolCards)
	{
		IOrderedEnumerable<CardOrStyleEntry> poolEntries2 = poolEntries.Select(ToCardOrStyleEntryLocal).OrderByDescending((CardOrStyleEntry entry) => entry, PreferredCardComparer.Instance);
		IReadOnlyList<CardOrStyleEntry> readOnlyList = IncludeStylesOfPoolEntries(cardSkinCatalog, cosmeticsProvider, cardDatabase, context, model, filter, poolEntries2);
		IOrderedEnumerable<CardOrStyleEntry> orderedEnumerable = readOnlyList.OrderByDescending((CardOrStyleEntry pe) => pe, PreferredCardComparer.Instance);
		bool poolContainsNewCards = false;
		IEnumerable<CardOrStyleEntry> source;
		if (!isExpanded)
		{
			using IEnumerator<CardOrStyleEntry> enumerator = orderedEnumerable.GetEnumerator();
			enumerator.MoveNext();
			CardOrStyleEntry[] array = new CardOrStyleEntry[1] { enumerator.Current };
			while (enumerator.MoveNext())
			{
				CardOrStyleEntry current = enumerator.Current;
				if (inventoryManager != null)
				{
					int value = 0;
					inventoryManager.CardsToTagNew?.TryGetValue(current.PrintingQuantity.Printing.GrpId, out value);
					if (value > 0)
					{
						poolContainsNewCards = true;
					}
				}
			}
			source = array;
		}
		else
		{
			source = orderedEnumerable;
		}
		int count = readOnlyList.Count;
		if (expandAllAtOnce && count > 1)
		{
			expandedPoolCards.Add(readOnlyList.First().PrintingQuantity.Printing.TitleId);
		}
		FactionSealedUXInfo factionSealedUXInfo = new FactionSealedUXInfo();
		string factionTag = "";
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		Dictionary<uint, string> dictionary2 = new Dictionary<uint, string>();
		List<FactionSealedUXInfo> list = context.Event?.PlayerEvent?.EventUXInfo?.FactionSealedUXInfo;
		if (list != null && list.Count >= 1)
		{
			CourseData courseData = context.Event.PlayerEvent.CourseData;
			string factionChoice = courseData.MadeChoice;
			factionSealedUXInfo = list.FirstOrDefault((FactionSealedUXInfo f) => f.FactionInternalName == factionChoice);
			if (factionSealedUXInfo == null)
			{
				throw new Exception("FactionUxInfo does not match MadeChoice, ensure MadeChoice on event entry uses FactionInternalName");
			}
			Dictionary<uint, HashSet<uint>> dictionary3 = new Dictionary<uint, HashSet<uint>>();
			foreach (CollationCardPool item in courseData.CardPoolByCollation)
			{
				if (dictionary3.TryGetValue(item.CollationId, out var value2))
				{
					value2.UnionWith(item.CardPool.Distinct().ToHashSet());
				}
				else
				{
					dictionary3.Add(item.CollationId, item.CardPool.Distinct().ToHashSet());
				}
			}
			foreach (var (value3, hashSet2) in dictionary3)
			{
				foreach (uint item2 in hashSet2)
				{
					dictionary.TryAdd(item2, value3);
				}
			}
			foreach (FactionCollation factionCollation in factionSealedUXInfo.FactionCollations)
			{
				dictionary2.TryAdd(factionCollation.CollationId, factionCollation.HangarLoc);
			}
		}
		foreach (var item3 in source.Select((CardOrStyleEntry item, int idx) => (value: item, idx: idx)))
		{
			var (poolEntry, forEntryIdx) = item3;
			if (CardUtilities.IsWildcard(poolEntry.PrintingQuantity.Printing.GrpId))
			{
				continue;
			}
			PagesMetaCardView.ExpandedDisplayStyle expandStyle = ExpandedMode(forEntryIdx, isExpanded, count);
			unownedCardSlots.TryGetValue(poolEntry.PrintingQuantity.Printing.GrpId, out var value4);
			uint num2 = 0u;
			uint num3 = 0u;
			uint remainingTitleCount = 0u;
			if (titleId != 0)
			{
				num2 = Math.Min(model.GetQuantityInCardPoolByTitle(titleId), (uint)context.MaxCardsByTitle);
				num3 = model.GetQuantityInWholeDeckByTitle(titleId);
				remainingTitleCount = (uint)Math.Clamp((int)(num2 - num3), 0, context.MaxCardsByTitle);
			}
			uint quantity = poolEntry.PrintingQuantity.Quantity;
			uint quantityInWholeDeckIncludingStyle = model.GetQuantityInWholeDeckIncludingStyle(poolEntry.PrintingQuantity.Printing.GrpId, poolEntry.StyleInformation?.StyleCode);
			bool useNewTags2 = string.IsNullOrEmpty(poolEntry.StyleInformation?.StyleCode) && useNewTags;
			bool useFactionTag = false;
			if (useFactionTags && dictionary.TryGetValue(poolEntry.PrintingQuantity.Printing.GrpId, out var value5) && dictionary2.TryGetValue(value5, out var value6) && !string.IsNullOrEmpty(value6))
			{
				factionTag = Languages.ActiveLocProvider.GetLocalizedText(value6);
				useFactionTag = true;
			}
			PagesMetaCardViewDisplayInformation pagesMetaCardViewDisplayInformation = PoolEntryToDisplayInfo(context, emergencyCardBansProvider, in poolEntry, allowUnowned, showUnlimitedBasics, showCardIfAllInDeck, showInfiniteAnyNumbers, useNewTags2, useFactionTag, factionTag, poolContainsNewCards, context.IsReadOnly, num3, num2, remainingTitleCount, quantity, quantityInWholeDeckIncludingStyle, value4, expandStyle);
			if (pagesMetaCardViewDisplayInformation != null)
			{
				if (pagesMetaCardViewDisplayInformation.IsSideboardingOrLimited && model.TryGetCardSkin(poolEntry.PrintingQuantity.Printing.GrpId, out var ccv))
				{
					pagesMetaCardViewDisplayInformation.Card = cardDatabase.CardDataProvider.GetCardPrintingById(pagesMetaCardViewDisplayInformation.Card.GrpId, ccv);
				}
				if (collapsingCardTitleIdForPageScroll.HasValue && poolEntry.PrintingQuantity.Printing.TitleId == collapsingCardTitleIdForPageScroll.Value)
				{
					pagesMetaCardViewDisplayInformation.IsCollapsing = true;
				}
				displayInfos.Add(pagesMetaCardViewDisplayInformation);
			}
		}
		return displayInfos;
		CardOrStyleEntry ToCardOrStyleEntryLocal(CardPrintingQuantity pe)
		{
			return ToCardOrStyleEntry(context, model, pe);
		}
	}

	private static PagesMetaCardViewDisplayInformation PoolEntryToDisplayInfo(DeckBuilderContext context, IEmergencyCardBansProvider emergencyCardBansProvider, in CardOrStyleEntry poolEntry, bool allowUnowned, bool showUnlimitedBasics, bool showCardIfAllInDeck, bool showInfiniteAnyNumbers, bool useNewTags, bool useFactionTag, string factionTag, bool poolContainsNewCards, bool isPoolPreview, uint usedTitleCount, uint availableTitleCount, uint remainingTitleCount, uint availablePrintingCount, uint usedPrintingCount, uint unownedPrintingCount, PagesMetaCardView.ExpandedDisplayStyle expandStyle)
	{
		CardPrintingData printing = poolEntry.PrintingQuantity.Printing;
		bool flag = false;
		bool flag2 = false;
		if ((printing.IsBasicLandUnlimited && showUnlimitedBasics) || (printing.AlternateDeckLimit.HasValue && showInfiniteAnyNumbers))
		{
			flag = (((int?)poolEntry.StyleInformation?.IsOwnedStyle) ?? ((int)availablePrintingCount)) != 0 && availableTitleCount >= printing.MaxCollected;
		}
		if (printing.IsBasicLandUnlimited)
		{
			flag2 = availablePrintingCount == 0;
		}
		bool showActualLandCount = (printing.IsBasicLandUnlimited && !showUnlimitedBasics) || ((context.IsLimited || context.IsSideboarding) && printing.IsBasicLandInclusive);
		if (usedPrintingCount >= availablePrintingCount && !showCardIfAllInDeck && !flag)
		{
			return null;
		}
		if (flag2 && allowUnowned && context.IsConstructed)
		{
			return null;
		}
		PagesMetaCardView.Tint tint = TintForPoolEntry(emergencyCardBansProvider, context, in poolEntry, usedPrintingCount, allowUnowned);
		int num = MaxForPoolEntry(context, usedPrintingCount, in poolEntry, showActualLandCount, expandStyle);
		PagesMetaCardView.QuantityDisplayStyle quantityStyle = QuantityStyleForPoolEntry(context.StartingMode, isPoolPreview, flag, usedPrintingCount, num);
		if (!showCardIfAllInDeck && !poolEntry.PrintingQuantity.Printing.IsBasicLandUnlimited)
		{
			availablePrintingCount = (uint)Math.Max((int)availablePrintingCount - usedPrintingCount, 0L);
			usedPrintingCount = 0u;
			availableTitleCount = availablePrintingCount;
			remainingTitleCount = availablePrintingCount;
			if (num > availablePrintingCount)
			{
				num = (int)availablePrintingCount;
			}
		}
		if (num > context.MaxCardsByTitle)
		{
			num = context.MaxCardsByTitle;
		}
		PagesMetaCardView.PipsDisplayStyle pipsStyle = ((poolEntry.StyleInformation.HasValue && expandStyle != PagesMetaCardView.ExpandedDisplayStyle.Stacked && !context.IsLimited && !context.IsSideboarding) ? PagesMetaCardView.PipsDisplayStyle.Skin : PagesMetaCardView.PipsDisplayStyle.Card);
		return new PagesMetaCardViewDisplayInformation
		{
			Card = poolEntry.PrintingQuantity.Printing,
			RemainingTitleCount = remainingTitleCount,
			UsedTitleCount = usedTitleCount,
			AvailableTitleCount = availableTitleCount,
			AvailablePrintingCount = availablePrintingCount,
			UnownedPrintingCount = unownedPrintingCount,
			UsedPrintingCount = usedPrintingCount,
			Max = num,
			Tint = tint,
			QuantityStyle = quantityStyle,
			PipsStyle = pipsStyle,
			ExpandedStyle = expandStyle,
			Skin = poolEntry.StyleInformation?.StyleCode,
			UseNewTag = useNewTags,
			UseFactionTag = useFactionTag,
			FactionTag = factionTag,
			PoolContainsNewCards = poolContainsNewCards,
			IsSideboardingOrLimited = (context.IsLimited || context.IsSideboarding)
		};
	}

	public static PagesMetaCardView.ExpandedDisplayStyle ExpandedMode(int forEntryIdx, bool isExpanded, int entriesCount)
	{
		if (entriesCount <= 1)
		{
			return PagesMetaCardView.ExpandedDisplayStyle.Solo;
		}
		if (!isExpanded)
		{
			return PagesMetaCardView.ExpandedDisplayStyle.Stacked;
		}
		if (forEntryIdx == 0)
		{
			return PagesMetaCardView.ExpandedDisplayStyle.Expanded_First;
		}
		if (forEntryIdx == entriesCount - 1)
		{
			return PagesMetaCardView.ExpandedDisplayStyle.Expanded_Last;
		}
		return PagesMetaCardView.ExpandedDisplayStyle.Expanded_Mid;
	}

	public static PagesMetaCardView.Tint TintForPoolEntry(IEmergencyCardBansProvider emergencyCardBansProvider, DeckBuilderContext context, in CardOrStyleEntry poolEntry, uint used, bool allowUnowned)
	{
		uint titleId = poolEntry.PrintingQuantity.Printing.TitleId;
		if (emergencyCardBansProvider.IsTitleIdEmergencyBanned(titleId))
		{
			return PagesMetaCardView.Tint.Red;
		}
		if (context.IsEditingDeck)
		{
			if (context.Format.IsCardBanned(titleId))
			{
				return PagesMetaCardView.Tint.Red;
			}
			if (context.Format.IsCardRestricted(titleId) && used >= context.Format.GetRestrictedQuotaMax(titleId))
			{
				return PagesMetaCardView.Tint.Red;
			}
		}
		if (poolEntry.StyleInformation.HasValue)
		{
			if (!poolEntry.StyleInformation.Value.IsOwnedStyle)
			{
				return PagesMetaCardView.Tint.Grey;
			}
			return PagesMetaCardView.Tint.None;
		}
		if (!(poolEntry.PrintingQuantity.Quantity != 0 || allowUnowned))
		{
			return PagesMetaCardView.Tint.Grey;
		}
		return PagesMetaCardView.Tint.None;
	}

	private static int MaxForPoolEntry(DeckBuilderContext context, uint used, in CardOrStyleEntry poolEntry, bool showActualLandCount, PagesMetaCardView.ExpandedDisplayStyle expandStyle)
	{
		uint maxCollected = poolEntry.PrintingQuantity.Printing.MaxCollected;
		int num = (int)maxCollected;
		if (expandStyle != PagesMetaCardView.ExpandedDisplayStyle.Stacked && expandStyle != PagesMetaCardView.ExpandedDisplayStyle.Solo && (poolEntry.PrintingQuantity.Quantity == 0 || (poolEntry.StyleInformation.HasValue && !poolEntry.StyleInformation.Value.IsOwnedStyle)) && !context.IsLimited && !context.IsSideboarding)
		{
			num = Math.Min((int)used, context.MaxCardsByTitle);
		}
		if (num > maxCollected && !showActualLandCount)
		{
			num = (int)maxCollected;
		}
		return num;
	}

	public static PagesMetaCardView.QuantityDisplayStyle QuantityStyleForPoolEntry(DeckBuilderMode mode, bool isPoolPreview, bool showInfinite, uint used, int max)
	{
		PagesMetaCardView.QuantityDisplayStyle result = PagesMetaCardView.QuantityDisplayStyle.Pips;
		if (mode == DeckBuilderMode.ReadOnlyCollection)
		{
			result = PagesMetaCardView.QuantityDisplayStyle.None;
		}
		else if (showInfinite)
		{
			result = PagesMetaCardView.QuantityDisplayStyle.Infinity;
		}
		else if (isPoolPreview && used > max)
		{
			result = PagesMetaCardView.QuantityDisplayStyle.Number;
		}
		return result;
	}

	private static IReadOnlyList<CardOrStyleEntry> IncludeStylesOfPoolEntries(CardSkinCatalog cardSkinCatalog, CosmeticsProvider cosmeticsProvider, ICardDatabaseAdapter cardDatabase, DeckBuilderContext context, DeckBuilderModel model, IReadOnlyCardFilter filter, IOrderedEnumerable<CardOrStyleEntry> poolEntries)
	{
		if (cardSkinCatalog == null || context.IsSideboarding || context.IsLimited)
		{
			return poolEntries.ToArray();
		}
		bool flag = filter.IsSet(CardFilterType.Collection_Collected) && !filter.IsSet(CardFilterType.Collection_Uncollected);
		bool flag2 = filter.IsSet(CardFilterType.Collection_Uncollected) && !filter.IsSet(CardFilterType.Collection_Collected);
		List<CardOrStyleEntry> list = new List<CardOrStyleEntry>();
		HashSet<uint> hashSet = new HashSet<uint>();
		foreach (CardOrStyleEntry poolEntry in poolEntries)
		{
			list.Add(poolEntry);
			uint titleId = poolEntry.PrintingQuantity.Printing.TitleId;
			if (!hashSet.Add(titleId))
			{
				continue;
			}
			foreach (IGrouping<uint, CardPrintingData> item2 in (from i in cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(titleId)
				where i.IsPrimaryCard && CardUtilities.CanCardExistInDeck(i)
				orderby i.GrpId descending
				group i by i.ArtId).ToList())
			{
				item2.Deconstruct(out var key, out var grouped);
				uint num = key;
				List<CardPrintingData> list2 = grouped.ToList();
				if (!cardSkinCatalog.TryGetSkins(num, out var skinList))
				{
					continue;
				}
				foreach (string item3 in skinList.Select((ArtStyleEntry i) => i.Variant).ToHashSet())
				{
					if (item3 == null)
					{
						continue;
					}
					bool flag3 = DeckBuilderWidgetUtilities.OwnsOrHasSkinInPool(cosmeticsProvider, model, cardDatabase, num, item3);
					if ((flag && !flag3) || (flag2 && flag3))
					{
						continue;
					}
					foreach (CardPrintingData item4 in list2)
					{
						if (!item4.Tags.Contains(MetaDataTag.Style_RETRO) || !(item3 == "DA"))
						{
							CardOrStyleEntry item = new CardOrStyleEntry(styleInformation: new StyleInformation?(new StyleInformation(item3, flag3)), printingQuantity: new CardPrintingQuantity
							{
								Printing = item4,
								Quantity = (flag3 ? 1u : 0u)
							});
							list.Add(item);
							break;
						}
					}
				}
			}
		}
		return list;
	}

	private static CardOrStyleEntry ToCardOrStyleEntry(DeckBuilderContext context, DeckBuilderModel model, CardPrintingQuantity pe)
	{
		string ccv;
		return new CardOrStyleEntry(pe, ((context.IsLimited || context.IsSideboarding) && model.TryGetCardSkin(pe.Printing.GrpId, out ccv)) ? new StyleInformation?(new StyleInformation(ccv, isOwnedStyle: true)) : ((StyleInformation?)null));
	}
}
