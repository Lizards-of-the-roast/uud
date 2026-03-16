using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;

namespace Core.Code.Decks;

public class DeckBuilderCardFilterProvider : IDisposable
{
	private CardFilter _filter;

	private bool? _isAutoSuggestLandsToggleOn;

	public IReadOnlyCardFilter Filter => _filter ?? (_filter = InitializeFilter(FilterValueChangeSource.Miscellaneous));

	public CardFilter PreCraftingFilter { get; private set; }

	public CardFilter TextFilter { get; private set; }

	public bool IsDeckFilterToggleOn { get; set; }

	public bool IsCompanionToggleOn { get; set; }

	public bool IsAutoSuggestLandsToggleOn
	{
		get
		{
			bool valueOrDefault = _isAutoSuggestLandsToggleOn == true;
			if (!_isAutoSuggestLandsToggleOn.HasValue)
			{
				valueOrDefault = InitializeAutoSuggestLandsToggle(ContextProvider.Context);
				_isAutoSuggestLandsToggleOn = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
		set
		{
			_isAutoSuggestLandsToggleOn = value;
			this.AutoSuggestLandFilterToggleSet?.Invoke(value);
		}
	}

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	public event Action<bool> AutoSuggestLandFilterToggleSet;

	public event Action<Transform> OnReparentAutoSuggestLand;

	public event Action<FilterValueChangeSource, CardFilterType, bool> FilterValueChanged;

	public event Action<IReadOnlyCardFilter> OnFilterReset;

	public event Action<IReadOnlyCardFilter> OnApplyFilters;

	public event Action<List<Func<CardFilterGroup, CardFilterGroup>>> OnUpdatedFilterFunctions;

	public static DeckBuilderCardFilterProvider Create()
	{
		return new DeckBuilderCardFilterProvider();
	}

	public void ReparentAutoSuggestLand(Transform transform)
	{
		this.OnReparentAutoSuggestLand?.Invoke(transform);
	}

	private DeckBuilderCardFilterProvider()
	{
		ContextProvider.OnContextSet += ResetInitializeAutoSuggestLandsToggle;
		ModelProvider.TitleInPileChangedQuantity += OnTitleInPileChangedQuantity;
	}

	public void SetFilter(FilterValueChangeSource source, CardFilterType type, bool value)
	{
		_filter.Set(type, value);
		this.FilterValueChanged?.Invoke(source, type, value);
	}

	public void SetFilterSearchText(string searchText)
	{
		_filter.SearchText = searchText;
	}

	public void ResetAndApplyFilters(FilterValueChangeSource source)
	{
		InitializeFilter(source);
		ApplyFilters(source);
		this.OnFilterReset?.Invoke(Filter);
	}

	public void ApplyFilters(FilterValueChangeSource source)
	{
		DeckBuilderModel model = Pantry.Get<DeckBuilderModelProvider>().Model;
		DeckBuilderContext context = Pantry.Get<DeckBuilderContextProvider>().Context;
		InventoryManager inventoryManager = Pantry.Get<InventoryManager>();
		ITitleCountManager titleCountManager = Pantry.Get<ITitleCountManager>();
		model.ReSortSkipFilters();
		CardMatcher.CardMatcherMetadata metadata = new CardMatcher.CardMatcherMetadata
		{
			TitleIdsToNumberOwned = titleCountManager?.OwnedTitleCounts
		};
		List<Func<CardFilterGroup, CardFilterGroup>> filterFunctions = new List<Func<CardFilterGroup, CardFilterGroup>>();
		bool flag = Filter.IsSet(CardFilterType.Type_Land);
		Func<CardPrintingData, bool> filterForDeckBuilder = Pantry.Get<CompanionUtil>().GetFilterForDeckBuilder(context, model);
		bool showCompanionFilterToggle = filterForDeckBuilder != null;
		(List<Func<CardFilterGroup, CardFilterGroup>> FilterFuncs, IEnumerable<CardFilterType> FilterTypesToSet) allFilterFunctions = GetAllFilterFunctions(filterFunctions, Filter, context, model, filterForDeckBuilder, flag, IsDeckFilterToggleOn, IsCompanionToggleOn, showCompanionFilterToggle);
		(filterFunctions, _) = allFilterFunctions;
		foreach (CardFilterType item in allFilterFunctions.FilterTypesToSet)
		{
			_filter.Set(item);
			this.FilterValueChanged?.Invoke(source, item, arg3: true);
		}
		filterFunctions.AddRange(_filter.GetFilterFunctions(metadata));
		model.FilterPool(filterFunctions);
		if (context.IsReadOnly)
		{
			model.ApplyFilterMainDeck(filterFunctions);
			model.ApplyFilterSideboard(filterFunctions);
		}
		else
		{
			model.ApplyDeckBaseFilters();
		}
		if (!context.IsLimited && !context.IsSideboarding)
		{
			if (flag)
			{
				model.SortPool(SortType.BasicLandsFirst, SortType.IsNew, SortType.ColorOrder, SortType.CMCWithXLast, SortType.Title);
			}
			else
			{
				if (inventoryManager != null && inventoryManager.cacheNeedsRefreshed)
				{
					DeckViewUtilities.ClearNewCardSortedCache();
					inventoryManager.SetCacheNeedsRefreshed(refresh: false);
				}
				model.SortPool(CardSorter.CardPoolNewCardsFirstSort);
			}
		}
		else if (flag)
		{
			model.SortPool(SortType.BasicLandsFirst, SortType.ColorOrder, SortType.CMCWithXLast, SortType.Title);
		}
		else
		{
			model.SortPool(SortType.ColorOrder, SortType.CMCWithXLast, SortType.Title);
		}
		VisualsUpdater.RefreshPoolView(scrollToTop: true, null);
		this.OnApplyFilters?.Invoke(Filter);
		this.OnUpdatedFilterFunctions?.Invoke(filterFunctions);
	}

	private static (List<Func<CardFilterGroup, CardFilterGroup>> FilterFuncs, IEnumerable<CardFilterType> FilterTypesToSet) GetAllFilterFunctions(List<Func<CardFilterGroup, CardFilterGroup>> filterFunctions, IReadOnlyCardFilter filter, DeckBuilderContext context, DeckBuilderModel model, Func<CardPrintingData, bool> companionFilter, bool filteringOnLand, bool isDeckFilterToggleOn, bool isCompanionToggleOn, bool showCompanionFilterToggle)
	{
		bool flag = filter.IsSet(CardFilterType.Rarity_BasicLand);
		if (filteringOnLand || context.IsReadOnly || (context.IsConstructed && context.IsSideboarding))
		{
			if (flag)
			{
				filterFunctions.Add(delegate(CardFilterGroup cards)
				{
					for (int num = cards.Cards.Count - 1; num >= 0; num--)
					{
						if (cards.Cards[num].PassedFilter && !cards.Cards[num].Card.IsBasicLand)
						{
							cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.NotLand);
						}
					}
					return cards;
				});
			}
		}
		else if (flag)
		{
			filterFunctions.Add(delegate(CardFilterGroup cards)
			{
				for (int num = cards.Cards.Count - 1; num >= 0; num--)
				{
					if (!cards.Cards[num].Card.IsBasicLand)
					{
						cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.NotLand);
					}
				}
				return cards;
			});
		}
		else if (!context.IsConstructed)
		{
			filterFunctions.Add(delegate(CardFilterGroup cards)
			{
				for (int num = cards.Cards.Count - 1; num >= 0; num--)
				{
					if (cards.Cards[num].Card.IsBasicLandUnlimited)
					{
						cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.IsLandUnlimited);
					}
				}
				return cards;
			});
		}
		else
		{
			filterFunctions.Add(delegate(CardFilterGroup cards)
			{
				for (int num = cards.Cards.Count - 1; num >= 0; num--)
				{
					if (cards.Cards[num].Card.IsBasicLand)
					{
						cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.IsLand);
					}
				}
				return cards;
			});
		}
		List<CardFilterType> list = new List<CardFilterType>();
		if (context.IsEditingDeck && !context.IsReadOnly)
		{
			filterFunctions.Add(delegate(CardFilterGroup cards)
			{
				for (int num = cards.Cards.Count - 1; num >= 0; num--)
				{
					if (!context.IsSideboarding && !context.Format.IsCardLegal(cards.Cards[num].Card.TitleId) && !context.Format.IsCardBanned(cards.Cards[num].Card.TitleId))
					{
						cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.Format);
					}
				}
				return cards;
			});
			switch (DeckBuilderWidgetUtilities.HasCommanderSet(context, model))
			{
			case DeckBuilderWidgetUtilities.CommanderType.CompleteCommander:
			{
				CardColorFlags colors = model.GetCommanderColors();
				filterFunctions.Add(delegate(CardFilterGroup cards)
				{
					for (int num = cards.Cards.Count - 1; num >= 0; num--)
					{
						if (!DeckFormat.CardMatchesCommanderColors(cards.Cards[num].Card, colors))
						{
							cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.Commander_Color);
						}
					}
					return cards;
				});
				break;
			}
			case DeckBuilderWidgetUtilities.CommanderType.PartnerableCommander:
				if (!filter.IsSet(CardFilterType.Commanders))
				{
					break;
				}
				filterFunctions.Add(delegate(CardFilterGroup cards)
				{
					for (int num = cards.Cards.Count - 1; num >= 0; num--)
					{
						IReadOnlyList<CardPrintingQuantity> filteredCommandZone = model.GetFilteredCommandZone();
						if (!cards.Cards[num].Card.HasPartnerAbilityCompatibleWithCommanders(filteredCommandZone))
						{
							cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.Commander_NotPartner);
						}
					}
					return cards;
				});
				if (HasNoOtherOwnedPartnerCards(filter, model))
				{
					list.Add(CardFilterType.Collection_Uncollected);
				}
				break;
			}
			if (context.Format.FormatIncludesCommandZone && filter.IsSet(CardFilterType.Commanders) && context.Format.AllowedCommanders.Count > 0)
			{
				filterFunctions.Add(delegate(CardFilterGroup cards)
				{
					for (int num = cards.Cards.Count - 1; num >= 0; num--)
					{
						if (!context.Format.AllowedCommander(cards.Cards[num].Card))
						{
							cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.Commander_Hidden);
						}
					}
					return cards;
				});
			}
			if (showCompanionFilterToggle && isCompanionToggleOn)
			{
				filterFunctions.Add(delegate(CardFilterGroup cards)
				{
					for (int num = cards.Cards.Count - 1; num >= 0; num--)
					{
						if (!companionFilter(cards.Cards[num].Card))
						{
							cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.Companion_Check);
						}
					}
					return cards;
				});
			}
			if (isDeckFilterToggleOn)
			{
				filterFunctions.Add(delegate(CardFilterGroup cards)
				{
					for (int num = cards.Cards.Count - 1; num >= 0; num--)
					{
						if (model.GetQuantityInWholeDeckByTitle(cards.Cards[num].Card) == 0)
						{
							cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.Quantity);
						}
					}
					return cards;
				});
				list.Add(CardFilterType.Collection_Collected);
				list.Add(CardFilterType.Collection_Uncollected);
			}
		}
		return (FilterFuncs: filterFunctions, FilterTypesToSet: list);
	}

	private static bool HasNoOtherOwnedPartnerCards(IReadOnlyCardFilter filter, DeckBuilderModel model)
	{
		if (!filter.IsSet(CardFilterType.Collection_Uncollected))
		{
			return model.GetFilteredPool().Count((CardPrintingQuantity card) => card.Printing.HasPartnerAbility()) <= 1;
		}
		return false;
	}

	private CardFilter InitializeFilter(FilterValueChangeSource source)
	{
		DeckBuilderContext context = Pantry.Get<DeckBuilderContextProvider>().Context;
		DeckBuilderModel model = Pantry.Get<DeckBuilderModelProvider>().Model;
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		HashSet<CardFilterType> hashSet = new HashSet<CardFilterType>();
		_filter = new CardFilter(cardDatabase, cardDatabase.GreLocProvider, setMetadataProvider);
		TextFilter = new CardFilter(cardDatabase, cardDatabase.GreLocProvider, setMetadataProvider);
		hashSet.Add(CardFilterType.Collection_Collected);
		if (context.Mode == DeckBuilderMode.ReadOnlyCollection)
		{
			PreCraftingFilter = Filter.Copy();
			hashSet.Add(CardFilterType.Collection_Uncollected);
		}
		else if (context.Mode != DeckBuilderMode.DeckBuilding)
		{
			PreCraftingFilter = Filter.Copy();
		}
		if (context.IsReadOnly)
		{
			hashSet.Add(CardFilterType.Collection_Uncollected);
		}
		if (context.IsSideboarding)
		{
			hashSet.Add(CardFilterType.Collection_Uncollected);
		}
		if (context.Event?.PlayerEvent?.EventInfo?.AllowUncollectedCards == true)
		{
			hashSet.Add(CardFilterType.Collection_Uncollected);
		}
		if (context.IsConstructed && !context.IsSideboarding && !context.IsFirstEdit)
		{
			foreach (CardFilterType item in DeckBuilderWidgetUtilities.FetchFiltersBasedOnColorsInDeckModel(model))
			{
				hashSet.Add(item);
			}
		}
		if (context.IsEditingDeck && context.Format.FormatIncludesCommandZone && model.GetFilteredCommandZone().Count == 0)
		{
			hashSet.Add(CardFilterType.Commanders);
		}
		else if (context.IsEditingDeck && context.Format.FormatIncludesCommandZone && model.GetFilteredCommandZone().Count == 1 && model.GetFilteredCommandZone()[0].Printing.HasPartnerAbility())
		{
			hashSet.Add(CardFilterType.Commanders);
		}
		foreach (CardFilterType item2 in hashSet)
		{
			_filter.Set(item2);
			this.FilterValueChanged?.Invoke(source, item2, arg3: true);
		}
		return _filter;
	}

	public void SetPreCraftingFilter()
	{
		PreCraftingFilter = Filter.Copy();
	}

	private void ResetInitializeAutoSuggestLandsToggle(DeckBuilderContext context)
	{
		_isAutoSuggestLandsToggleOn = null;
	}

	private static bool InitializeAutoSuggestLandsToggle(DeckBuilderContext context)
	{
		if (!context.IsConstructed || !context.IsFirstEdit)
		{
			return context.SuggestLandAfterDeckLoad;
		}
		return true;
	}

	private void OnTitleInPileChangedQuantity(DeckBuilderPile pile, CardData cardData)
	{
		if (pile == DeckBuilderPile.MainDeck)
		{
			if (BasicLandSuggester.PrintingIsBasicLandOrWaste(cardData))
			{
				IsAutoSuggestLandsToggleOn = false;
			}
			if (IsAutoSuggestLandsToggleOn)
			{
				BasicLandSuggester.SuggestLand();
				VisualsUpdater.UpdateAllDeckVisuals();
			}
		}
	}

	public void Dispose()
	{
		ContextProvider.OnContextSet -= ResetInitializeAutoSuggestLandsToggle;
		ModelProvider.TitleInPileChangedQuantity -= OnTitleInPileChangedQuantity;
	}
}
