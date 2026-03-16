using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Shared.Code.CardFilters;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

namespace Core.Code.Decks;

public class DeckBuilderVisualsUpdater
{
	public enum SideboardType
	{
		AllModes,
		Traditional
	}

	private class VisualCardData
	{
		public bool IsOverRestriction { get; set; }

		public bool Banned { get; set; }

		public uint OwnedMainCount { get; set; }

		public uint UnownedMainCount { get; set; }

		public uint OwnedBoardCount { get; set; }

		public uint UnownedBoardCount { get; set; }

		public string SkinCode { get; set; }
	}

	private List<CardPrintingData> _unsuggestedCardsInPool;

	private List<CardPrintingData> _suggestedCardsInDeck;

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderModel Model => ModelProvider.Model;

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	private DeckBuilderCardFilterProvider FilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private DeckBuilderPreferredPrintingState PreferredPrintingState => Pantry.Get<DeckBuilderPreferredPrintingState>();

	private CompanionUtil CompanionUtil => Pantry.Get<CompanionUtil>();

	private InventoryManager InventoryManager => Pantry.Get<InventoryManager>();

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	private AssetLookupSystem AssetLookupSystem => Pantry.Get<AssetLookupManager>().AssetLookupSystem;

	private ICardDatabaseAdapter CardDatabase => Pantry.Get<ICardDatabaseAdapter>();

	private IEmergencyCardBansProvider EmergencyCardBansProvider => Pantry.Get<IEmergencyCardBansProvider>();

	public List<CardPrintingData> UnsuggestedCardsInPool
	{
		get
		{
			return _unsuggestedCardsInPool ?? (_unsuggestedCardsInPool = new List<CardPrintingData>());
		}
		set
		{
			_unsuggestedCardsInPool = value;
		}
	}

	public List<CardPrintingData> SuggestedCardsInDeck => _suggestedCardsInDeck ?? (_suggestedCardsInDeck = new List<CardPrintingData>());

	public int MainDeckCurrentSize => (int)(Model.GetTotalMainDeckSize() + Model.GetTotalCommandSize());

	public int MainDeckMaxSize
	{
		get
		{
			if (!ContextProvider.Context.IsReadOnly)
			{
				return CompanionUtil.GetMinMainDeckCards(ContextProvider.Context.Format);
			}
			return 0;
		}
	}

	public int SideboardCurrentSize => (int)Model.GetTotalSideboardSize();

	public int SideboardMaxSize
	{
		get
		{
			if (!ContextProvider.Context.IsReadOnly)
			{
				return ContextProvider.Context.Format.MaxSideboardCards;
			}
			return 0;
		}
	}

	public event Action<IReadOnlyList<PagesMetaCardViewDisplayInformation>, bool, bool> OnPoolViewRefreshed;

	public event Action PreUpdateAllDeckVisuals;

	public event Action OnSubButtonsUpdated;

	public event Action<int, int> MainDeckCountVisualsUpdated;

	public event Action<List<ListMetaCardViewDisplayInformation>, CardFilter> MainDeckColumnVisualsUpdated;

	public event Action<List<ListMetaCardViewDisplayInformation>> MainDeckListVisualsUpdated;

	public event Action<int, int> SideboardCountVisualsUpdated;

	public event Action<List<ListMetaCardViewDisplayInformation>, SortType[], bool> SideboardVisualsUpdated;

	public event Action<ListMetaCardViewDisplayInformation, PagesMetaCardViewDisplayInformation, bool> CommanderViewUpdated;

	public event Action<ListMetaCardViewDisplayInformation, PagesMetaCardViewDisplayInformation, bool> PartnerViewUpdated;

	public event Action<ListMetaCardViewDisplayInformation, PagesMetaCardViewDisplayInformation, bool> CompanionViewUpdated;

	public event Action PreOnLayoutUpdated;

	public event Action OnLayoutUpdated;

	public static DeckBuilderVisualsUpdater Create()
	{
		return new DeckBuilderVisualsUpdater();
	}

	public void RefreshPoolView(bool scrollToTop, uint? collapseTitleId)
	{
		Dictionary<uint, uint> unownedCardSlots = Model.GetCardsNeededToFinishDeck().ToDictionary((KeyValuePair<uint, CardPrintingQuantity> k) => k.Value.Printing.GrpId, (KeyValuePair<uint, CardPrintingQuantity> v) => v.Value.Quantity);
		var (arg, arg2) = GetCardPoolViewInfo(unownedCardSlots, collapseTitleId);
		this.OnPoolViewRefreshed?.Invoke(arg, scrollToTop, arg2);
		PreferredPrintingState.ExpandAllOnce = false;
	}

	public (IReadOnlyList<PagesMetaCardViewDisplayInformation> DisplayInfos, bool UseNewTags) GetCardPoolViewInfo(Dictionary<uint, uint> unownedCardSlots, uint? collapsingTitleId = null)
	{
		return GetCardPoolViewInfo(ContextProvider.Context, Model, CosmeticsProvider, CardDatabase, InventoryManager, LayoutState, FilterProvider.Filter, PreferredPrintingState, EmergencyCardBansProvider, FilterProvider, unownedCardSlots, Pantry.Get<StoreManager>().CardSkinCatalog, collapsingTitleId);
	}

	private static (IReadOnlyList<PagesMetaCardViewDisplayInformation> DisplayInfos, bool UseNewTags) GetCardPoolViewInfo(DeckBuilderContext context, DeckBuilderModel model, CosmeticsProvider cosmeticsProvider, ICardDatabaseAdapter cardDatabase, InventoryManager inventoryManager, DeckBuilderLayoutState layoutProvider, IReadOnlyCardFilter filter, DeckBuilderPreferredPrintingState prefPrintingState, IEmergencyCardBansProvider emergencyCardBansProvider, DeckBuilderCardFilterProvider deckBuilderCardFilterProvider, Dictionary<uint, uint> unownedCardSlots, CardSkinCatalog cardSkinCatalog, uint? collapsingTitleId)
	{
		List<PagesMetaCardViewDisplayInformation> list = new List<PagesMetaCardViewDisplayInformation>();
		if (context.IsEditingDeck && !context.IsReadOnly && (!context.IsConstructed || !context.IsSideboarding) && filter.IsSet(CardFilterType.Type_Land))
		{
			PagesMetaCardViewDisplayInformation item = new PagesMetaCardViewDisplayInformation
			{
				UseCustomAutoLandsToggleObject = true
			};
			list.Add(item);
		}
		bool flag = !context.IsLimited && !context.IsSideboarding;
		bool showUnlimitedBasics = (!context.IsConstructed || !context.IsSideboarding) && !context.IsReadOnly;
		bool showInfiniteAnyNumbers = !context.IsLimited && !context.IsSideboarding && !context.IsReadOnly;
		bool valueOrDefault = context.Event?.PlayerEvent?.EventInfo?.AllowUncollectedCards == true;
		bool flag2 = !context.IsLimited && !context.IsSideboarding;
		EventContext eventContext = context.Event;
		bool useFactionTags = eventContext != null && eventContext.PlayerEvent?.CourseData?.CardPoolByCollation?.Select((CollationCardPool col) => col.CollationId).Distinct().Count() > 1;
		IReadOnlyList<CardPrintingQuantity> displayedPool = DeckBuilderWidgetUtilities.GetDisplayedPool(context, model, layoutProvider.IsListViewSideboarding);
		IEnumerable<IGrouping<uint, CardPrintingQuantity>> obj = (flag ? (from p in displayedPool
			group p by p.Printing.TitleId) : (from p in displayedPool
			group p by (!p.Printing.IsBasicLand) ? p.Printing.GrpId : p.Printing.TitleId));
		bool flag3 = deckBuilderCardFilterProvider.Filter.IsSet(CardFilterType.Type_Land);
		foreach (IGrouping<uint, CardPrintingQuantity> item2 in obj)
		{
			item2.Deconstruct(out var key, out var grouped);
			uint titleId = key;
			IEnumerable<CardPrintingQuantity> poolEntries = grouped;
			bool isExpanded = (flag || (!flag && flag3)) && prefPrintingState.IsExpanded(titleId);
			list = PreferredPrintingUtilities.GatherDisplayInfosForTitle(titleId, list, collapsingTitleId, context, model, cardSkinCatalog, cosmeticsProvider, filter, cardDatabase, inventoryManager, emergencyCardBansProvider, isExpanded, flag, showUnlimitedBasics, showInfiniteAnyNumbers, valueOrDefault, flag2, useFactionTags, poolEntries, unownedCardSlots, prefPrintingState.ExpandAllOnce, prefPrintingState.ExpandedPoolCards);
		}
		return (DisplayInfos: list, UseNewTags: flag2);
	}

	public void UpdateAllDeckVisuals()
	{
		DeckBuilderContext context = ContextProvider.Context;
		UpdateSubButtons();
		this.PreUpdateAllDeckVisuals?.Invoke();
		bool checkCommanderColors = DeckBuilderWidgetUtilities.HasCommanderSet(context, Model) != DeckBuilderWidgetUtilities.CommanderType.NoCommander;
		CardColorFlags commanderColors = Model.GetCommanderColors();
		IReadOnlyList<CardPrintingQuantity> filteredSideboard = Model.GetFilteredSideboard();
		UpdateCommanderView(isPartner: false);
		LayoutState.UpdateSideboardVisibility();
		UpdateCompanionView(showCompanion: false);
		bool allowSearchWhenExpanded = PlatformUtils.IsDesktop() || PlatformUtils.IsAspectRatio4x3();
		Dictionary<uint, uint> unownedSlotCounts = Model.GetCardsNeededToFinishDeck().ToDictionary((KeyValuePair<uint, CardPrintingQuantity> k) => k.Value.Printing.GrpId, (KeyValuePair<uint, CardPrintingQuantity> v) => v.Value.Quantity);
		Dictionary<uint, VisualCardData> visualCardData = BuildVisualCardData(context, unownedSlotCounts, EmergencyCardBansProvider, checkCommanderColors, commanderColors);
		UpdateMainDeckVisuals(context, visualCardData, CompanionUtil, FilterProvider, SuggestedCardsInDeck, LayoutState.IsColumnViewExpanded, allowSearchWhenExpanded);
		UpdateSideboardVisuals(context, visualCardData, filteredSideboard);
		this.SideboardCountVisualsUpdated?.Invoke(SideboardCurrentSize, SideboardMaxSize);
	}

	private Dictionary<uint, VisualCardData> BuildVisualCardData(DeckBuilderContext context, Dictionary<uint, uint> unownedSlotCounts, IEmergencyCardBansProvider emergencyCardBansProvider, bool checkCommanderColors, CardColorFlags commanderColors)
	{
		return (from c in Model.GetAllFilteredCards()
			group c by c.Printing.GrpId into c
			select c.First().Printing).ToDictionary((CardPrintingData printing) => printing.GrpId, (CardPrintingData printing) => CalculateVisualCardData(context, emergencyCardBansProvider, checkCommanderColors, commanderColors, printing, unownedSlotCounts));
	}

	public void UpdateSubButtons()
	{
		DeckBuilderContext context = ContextProvider.Context;
		if (Model != null && context.Mode != DeckBuilderMode.ReadOnlyCollection)
		{
			this.OnSubButtonsUpdated?.Invoke();
		}
	}

	private void UpdateMainDeckVisuals(DeckBuilderContext context, Dictionary<uint, VisualCardData> visualCardData, CompanionUtil companionUtil, DeckBuilderCardFilterProvider deckBuilderCardFilterProvider, List<CardPrintingData> suggestedCardsInDeck, bool isColumnViewExpanded, bool allowSearchWhenExpanded)
	{
		List<ListMetaCardViewDisplayInformation> list = new List<ListMetaCardViewDisplayInformation>();
		List<ListMetaCardViewDisplayInformation> list2 = new List<ListMetaCardViewDisplayInformation>();
		foreach (CardPrintingQuantity item in Model.GetFilteredMainDeck())
		{
			IEnumerable<AbilityHangerData> cardInvalidHanger;
			bool invalid = !companionUtil.IsDeckCardValid(item.Printing, Model.GetCompanion(), AssetLookupSystem, CardDatabase.GreLocProvider, out cardInvalidHanger);
			AbilityHangerData[] contextualHangers = cardInvalidHanger.ToArray();
			bool suggested = false;
			if (!context.IsSideboarding && isColumnViewExpanded)
			{
				suggested = suggestedCardsInDeck.Contains(item.Printing);
			}
			visualCardData.TryGetValue(item.Printing.GrpId, out var value);
			if (value == null)
			{
				throw new Exception("Visual card data is missing! What the heck!");
			}
			if (value.OwnedMainCount != 0)
			{
				list.Add(new ListMetaCardViewDisplayInformation
				{
					Card = item.Printing,
					Banned = (value.Banned || value.IsOverRestriction),
					Invalid = invalid,
					Suggested = suggested,
					ContextualHangers = contextualHangers,
					Quantity = value.OwnedMainCount,
					Unowned = false,
					SkinCode = value.SkinCode
				});
			}
			if (value.UnownedMainCount != 0)
			{
				list.Add(new ListMetaCardViewDisplayInformation
				{
					Card = item.Printing,
					Banned = (value.Banned || value.IsOverRestriction),
					Invalid = invalid,
					Suggested = suggested,
					ContextualHangers = contextualHangers,
					Quantity = value.UnownedMainCount,
					Unowned = true,
					SkinCode = value.SkinCode
				});
			}
			list2.Add(new ListMetaCardViewDisplayInformation
			{
				Card = item.Printing,
				Quantity = item.Quantity,
				Banned = (value.Banned || value.IsOverRestriction),
				Invalid = invalid,
				Suggested = suggested,
				ContextualHangers = contextualHangers,
				Unowned = (value.UnownedMainCount != 0),
				SkinCode = value.SkinCode
			});
		}
		this.MainDeckCountVisualsUpdated?.Invoke(MainDeckCurrentSize, MainDeckMaxSize);
		this.MainDeckColumnVisualsUpdated?.Invoke(list2, (isColumnViewExpanded && allowSearchWhenExpanded) ? deckBuilderCardFilterProvider.TextFilter : null);
		this.MainDeckListVisualsUpdated?.Invoke(list);
	}

	private VisualCardData CalculateVisualCardData(DeckBuilderContext context, IEmergencyCardBansProvider emergencyCardBansProvider, bool checkCommanderColors, CardColorFlags commanderColors, CardPrintingData printing, Dictionary<uint, uint> unownedSlotCounts)
	{
		int quantityInCardPool = (int)Model.GetQuantityInCardPool(printing.GrpId);
		uint quantityInWholeDeck = Model.GetQuantityInWholeDeck(printing.GrpId);
		uint quantityInMainDeck = Model.GetQuantityInMainDeck(printing.GrpId);
		uint quantityInSideboard = Model.GetQuantityInSideboard(printing.GrpId);
		string cardSkin = Model.GetCardSkin(printing.GrpId);
		bool flag = !string.IsNullOrWhiteSpace(cardSkin);
		bool flag2 = flag && !DeckBuilderWidgetUtilities.OwnsOrHasSkinInPool(CosmeticsProvider, Model, CardDatabase, printing.ArtId, cardSkin);
		bool isOverRestriction = context.IsOverRestriction(quantityInWholeDeck, printing.TitleId);
		bool banned = !context.IsSideboarding && (context.Format.IsCardBanned(printing.TitleId) || !context.Format.IsCardLegal(printing.TitleId) || emergencyCardBansProvider.IsTitleIdEmergencyBanned(printing.TitleId) || (checkCommanderColors && !DeckFormat.CardMatchesCommanderColors(printing, commanderColors)));
		uint num;
		uint num2;
		if (context.Event?.PlayerEvent?.EventInfo?.AllowUncollectedCards == true)
		{
			num = quantityInWholeDeck;
			num2 = 0u;
			goto IL_0198;
		}
		unownedSlotCounts.TryGetValue(printing.GrpId, out num2);
		int num3;
		int num4;
		if (!flag2)
		{
			if (quantityInCardPool == 0)
			{
				num3 = ((!flag) ? 1 : 0);
				if (num3 != 0)
				{
					goto IL_018c;
				}
			}
			else
			{
				num3 = 0;
			}
			num4 = (int)(quantityInWholeDeck - num2);
			goto IL_018d;
		}
		num3 = 1;
		goto IL_018c;
		IL_0198:
		uint num5 = Math.Min(num, quantityInMainDeck);
		uint ownedBoardCount = Math.Clamp(num - num5, 0u, quantityInSideboard);
		uint num6 = Math.Min(num2, quantityInSideboard);
		uint unownedMainCount = Math.Clamp(num2 - num6, 0u, quantityInMainDeck);
		return new VisualCardData
		{
			IsOverRestriction = isOverRestriction,
			Banned = banned,
			OwnedMainCount = num5,
			UnownedMainCount = unownedMainCount,
			OwnedBoardCount = ownedBoardCount,
			UnownedBoardCount = num6,
			SkinCode = cardSkin
		};
		IL_018d:
		num = (uint)num4;
		num2 = ((num3 != 0) ? quantityInWholeDeck : num2);
		goto IL_0198;
		IL_018c:
		num4 = 0;
		goto IL_018d;
	}

	private void UpdateSideboardVisuals(DeckBuilderContext context, Dictionary<uint, VisualCardData> visualCardData, IReadOnlyList<CardPrintingQuantity> filteredSideboard)
	{
		List<ListMetaCardViewDisplayInformation> list = new List<ListMetaCardViewDisplayInformation>();
		CardPrintingData companion = Model.GetCompanion();
		foreach (CardPrintingQuantity item in filteredSideboard)
		{
			visualCardData.TryGetValue(item.Printing.GrpId, out var value);
			if (value == null)
			{
				throw new Exception("Visual card data is missing! What the heck!");
			}
			uint num = value.OwnedBoardCount;
			uint num2 = value.UnownedBoardCount;
			if (companion != null && companion.GrpId == item.Printing.GrpId)
			{
				if (item.Quantity == 1)
				{
					continue;
				}
				if (value.OwnedBoardCount != 0)
				{
					num--;
				}
				else if (value.UnownedBoardCount != 0)
				{
					num2--;
				}
			}
			if (value.OwnedBoardCount != 0)
			{
				list.Add(new ListMetaCardViewDisplayInformation
				{
					Card = item.Printing,
					Banned = value.Banned,
					Quantity = num,
					Unowned = false,
					SkinCode = value.SkinCode
				});
			}
			if (value.UnownedBoardCount != 0)
			{
				list.Add(new ListMetaCardViewDisplayInformation
				{
					Card = item.Printing,
					Banned = (value.Banned || value.IsOverRestriction),
					Quantity = num2,
					Unowned = true,
					SkinCode = value.SkinCode
				});
			}
		}
		this.SideboardVisualsUpdated?.Invoke(list, Model.SideboardSortCriteria, context.IsReadOnly);
	}

	private void UpdateCommanderView(bool isPartner)
	{
		DeckBuilderContext context = ContextProvider.Context;
		ListMetaCardViewDisplayInformation arg = null;
		PagesMetaCardViewDisplayInformation arg2 = null;
		bool flag = false;
		if (context.Format != null && context.Format.FormatIncludesCommandZone)
		{
			CardPrintingQuantity cardPrintingQuantity = Model.GetFilteredCommandZone().FirstOrDefault();
			if (isPartner)
			{
				cardPrintingQuantity = Model.GetFilteredCommandZone().Skip(1).FirstOrDefault();
			}
			arg = new ListMetaCardViewDisplayInformation();
			if (cardPrintingQuantity != null)
			{
				int num = (int)Model.GetQuantityInCardPool(cardPrintingQuantity.Printing.GrpId);
				bool flag2 = num >= cardPrintingQuantity.Printing.MaxCollected;
				if (num > context.Format.MaxCardsByTitle)
				{
					num = context.Format.MaxCardsByTitle;
				}
				int quantity = (int)cardPrintingQuantity.Quantity;
				int num2 = quantity - num;
				if (num > quantity)
				{
					num = quantity;
				}
				if (cardPrintingQuantity.Printing.AlternateDeckLimit.HasValue && flag2)
				{
					num = quantity;
					num2 = 0;
				}
				if (context.Event?.PlayerEvent?.EventInfo?.AllowUncollectedCards == true)
				{
					num = quantity;
					num2 = 0;
				}
				string cardSkin = Model.GetCardSkin(cardPrintingQuantity.Printing.GrpId);
				(CardPrintingData BasePrinting, CardPrintingData DirectPrinting) basePrinting = SpecializeUtilities.GetBasePrinting(CardDatabase.CardDataProvider, cardPrintingQuantity.Printing.GrpId);
				CardPrintingData item = basePrinting.BasePrinting;
				CardPrintingData item2 = basePrinting.DirectPrinting;
				bool banned = !context.IsSideboarding && (context.Format.IsCardBanned(item.TitleId) || !context.Format.IsCardLegal(item.TitleId) || EmergencyCardBansProvider.IsTitleIdEmergencyBanned(cardPrintingQuantity.Printing.TitleId) || !context.Format.AllowedCommander(item));
				IEnumerable<AbilityHangerData> cardInvalidHanger;
				bool invalid = !CompanionUtil.IsDeckCardValid(cardPrintingQuantity.Printing, Model.GetCompanion(), AssetLookupSystem, CardDatabase.GreLocProvider, out cardInvalidHanger);
				arg = new ListMetaCardViewDisplayInformation
				{
					Card = cardPrintingQuantity.Printing,
					VisualCard = item2,
					Quantity = cardPrintingQuantity.Quantity,
					Banned = banned,
					Invalid = invalid,
					ContextualHangers = cardInvalidHanger.ToArray(),
					Unowned = (num2 > 0),
					SkinCode = cardSkin
				};
				if (context.IsReadOnly)
				{
					arg2 = DeckBuilderWidgetUtilities.CreatePreviewCard(cardPrintingQuantity.Printing, cardSkin);
				}
				if (cardPrintingQuantity.Printing.HasPartnerAbility() && !isPartner)
				{
					flag = true;
				}
			}
		}
		(isPartner ? this.PartnerViewUpdated : this.CommanderViewUpdated)?.Invoke(arg, arg2, context.Mode == DeckBuilderMode.ReadOnly);
		if (flag)
		{
			UpdateCommanderView(isPartner: true);
		}
	}

	public void UpdateCompanionView(bool showCompanion)
	{
		DeckBuilderContext context = ContextProvider.Context;
		if (!showCompanion && Model.GetCompanion() != null)
		{
			showCompanion = true;
		}
		if (!showCompanion)
		{
			showCompanion = Model.GetFilteredMainDeck().Any((CardPrintingQuantity cq) => CompanionUtil.CardCanBeCompanion(cq.Printing));
		}
		if (!showCompanion)
		{
			showCompanion = Model.GetFilteredSideboard().Any((CardPrintingQuantity cq) => CompanionUtil.CardCanBeCompanion(cq.Printing));
		}
		ListMetaCardViewDisplayInformation arg = null;
		PagesMetaCardViewDisplayInformation arg2 = null;
		if (showCompanion)
		{
			CardPrintingData companion = Model.GetCompanion();
			arg = new ListMetaCardViewDisplayInformation();
			if (companion != null)
			{
				int num;
				if (Model.GetQuantityInCardPool(companion.GrpId) < 1)
				{
					EventContext eventContext = context.Event;
					num = ((eventContext != null && eventContext.PlayerEvent?.EventInfo?.AllowUncollectedCards == true) ? 1 : 0);
				}
				else
				{
					num = 1;
				}
				bool flag = (byte)num != 0;
				string cardSkin = Model.GetCardSkin(companion.GrpId);
				bool banned = false;
				bool invalid = false;
				IEnumerable<AbilityHangerData> source = Enumerable.Empty<AbilityHangerData>();
				if (!context.IsSideboarding)
				{
					banned = context.Format.IsCardBanned(companion.TitleId) || !context.Format.IsCardLegal(companion.TitleId) || EmergencyCardBansProvider.IsTitleIdEmergencyBanned(companion.TitleId);
					if (!CompanionUtil.CardCanBeCompanion(companion))
					{
						banned = true;
					}
					else if (DeckBuilderWidgetUtilities.HasCommanderSet(context, Model) != DeckBuilderWidgetUtilities.CommanderType.NoCommander && !DeckFormat.CardMatchesCommanderColors(companion, Model.GetCommanderColors()))
					{
						banned = true;
					}
					if (!CompanionUtil.IsValid)
					{
						invalid = true;
						source = new AbilityHangerData[1]
						{
							new AbilityHangerData
							{
								Header = "MainNav/DeckBuilder/CompanionInvalid_Title",
								Body = new MTGALocalizedString
								{
									Key = "MainNav/DeckBuilder/CompanionSelection_Body",
									Parameters = new Dictionary<string, string> { 
									{
										"conditionText",
										CompanionUtil.GetAbilityText(companion, CardDatabase.GreLocProvider)
									} }
								},
								BadgePath = CompanionUtil.GetCompanionIconPath(companion, AssetLookupSystem),
								Color = CompanionUtil.WarningYellow
							}
						};
					}
				}
				arg = new ListMetaCardViewDisplayInformation
				{
					Card = companion,
					Quantity = 1u,
					Banned = banned,
					Invalid = invalid,
					ContextualHangers = source.ToArray(),
					Unowned = !flag,
					SkinCode = cardSkin
				};
				if (context.IsReadOnly)
				{
					arg2 = DeckBuilderWidgetUtilities.CreatePreviewCard(companion, cardSkin);
				}
			}
		}
		this.CompanionViewUpdated?.Invoke(arg, arg2, context.Mode == DeckBuilderMode.ReadOnly);
	}

	public void UpdateView()
	{
		UpdateLayout();
		UpdateAllDeckVisuals();
	}

	public void UpdateLayout()
	{
		bool isColumnViewExpanded = LayoutState.IsColumnViewExpanded;
		bool flag = LayoutState.LayoutInUse != DeckBuilderLayout.Column;
		if (isColumnViewExpanded && flag)
		{
			LayoutState.IsColumnViewExpanded = false;
			UnsuggestedCardsInPool.Clear();
			SuggestedCardsInDeck.Clear();
		}
		this.PreOnLayoutUpdated?.Invoke();
		this.OnLayoutUpdated?.Invoke();
		Canvas.ForceUpdateCanvases();
	}
}
