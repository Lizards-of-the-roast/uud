using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using UnityEngine;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public static class DeckBuilderWidgetUtilities
{
	public enum CommanderType
	{
		NoCommander,
		CompleteCommander,
		PartnerableCommander
	}

	private static readonly int BlueHash = Animator.StringToHash("Blue");

	public static bool HasChangesInCurrentDeck(DeckBuilderContext context, DeckBuilderModel model)
	{
		if (!context.IsEditingDeck)
		{
			return false;
		}
		if (WrapperDeckBuilder.HasCachedDeck())
		{
			return true;
		}
		DeckInfo deck = context.Deck;
		DeckInfo serverModel = model.GetServerModel();
		if (deck.name != serverModel.name)
		{
			return true;
		}
		if (deck.deckTileId != serverModel.deckTileId)
		{
			return true;
		}
		if (deck.deckArtId != serverModel.deckArtId)
		{
			return true;
		}
		if (deck.format != serverModel.format)
		{
			return true;
		}
		if (deck.cardBack != serverModel.cardBack)
		{
			return true;
		}
		if (deck.avatar != serverModel.avatar)
		{
			return true;
		}
		if (deck.pet != serverModel.pet)
		{
			return true;
		}
		if (!AreStringListsEqual(deck.emotes, serverModel.emotes))
		{
			return true;
		}
		if (!AreCardListsEqual(deck.mainDeck, serverModel.mainDeck))
		{
			return true;
		}
		if (!AreCardListsEqual(deck.sideboard, serverModel.sideboard))
		{
			return true;
		}
		if (!AreCardListsEqual(deck.commandZone, serverModel.commandZone))
		{
			return true;
		}
		if (!AreCardSkinListsEqual(deck.cardSkins, serverModel.cardSkins))
		{
			return true;
		}
		if (deck.companion?.Id != serverModel.companion?.Id || deck.companion?.Quantity != serverModel.companion?.Quantity)
		{
			return true;
		}
		return false;
		static bool AreCardListsEqual(List<CardInDeck> a, List<CardInDeck> b)
		{
			CardInDeck[] array = (from c in a
				where c.Quantity != 0
				orderby c.Id
				select c).ToArray();
			CardInDeck[] array2 = (from c in b
				where c.Quantity != 0
				orderby c.Id
				select c).ToArray();
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int num = 0; num < array.Length; num++)
			{
				if (array[num].Id != array2[num].Id)
				{
					return false;
				}
				if (array[num].Quantity != array2[num].Quantity)
				{
					return false;
				}
			}
			return true;
		}
		static bool AreCardSkinListsEqual(List<CardSkin> a, List<CardSkin> b)
		{
			if (a.Count != b.Count)
			{
				return false;
			}
			CardSkin[] array = a.OrderBy((CardSkin s) => s.GrpId).ToArray();
			CardSkin[] array2 = b.OrderBy((CardSkin s) => s.GrpId).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				if (array[num].GrpId != array2[num].GrpId)
				{
					return false;
				}
				if (string.CompareOrdinal(array[num].CCV, array2[num].CCV) != 0)
				{
					return false;
				}
			}
			return true;
		}
		static bool AreStringListsEqual(List<string> a, List<string> b)
		{
			if (a == null && b == null)
			{
				return true;
			}
			if (a == null || b == null || a.Count != b.Count)
			{
				return false;
			}
			return a.SequenceEqual(b);
		}
	}

	public static bool HasUnassignedCardSkinsInDeck(CosmeticsProvider cosmeticsProvider, IPreferredPrintingDataProvider preferredPrintingDataProvider, ICardDatabaseAdapter cardDatabase, DeckBuilderContext context, DeckBuilderModelProvider modelProvider)
	{
		if (context.IsSideboarding)
		{
			return false;
		}
		foreach (IGrouping<uint, CardPrintingQuantity> item in from i in modelProvider.Model.GetAllFilteredCards()
			group i by i.Printing.TitleId)
		{
			CardPrintingQuantity cardPrintingQuantity = item.OrderByDescending((CardPrintingQuantity i) => i.Printing.GrpId).First();
			(CardPrintingData, string) replacementStyle = DeckBuilderModelProvider.GetReplacementStyle(cardDatabase, cosmeticsProvider, cardPrintingQuantity.Printing);
			PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)item.Key);
			foreach (CardPrintingQuantity item2 in item)
			{
				if (modelProvider.Model.GetCardSkin(item2.Printing.GrpId) == null)
				{
					if (preferredPrintingForTitleId == null && replacementStyle.Item2 != null)
					{
						return true;
					}
					if (preferredPrintingForTitleId != null && (preferredPrintingForTitleId.printingGrpId != item2.Printing.GrpId || preferredPrintingForTitleId.styleCode != null))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static IReadOnlyList<CardPrintingQuantity> GetDisplayedPool(DeckBuilderContext context, DeckBuilderModel model, bool listViewShowingSideboard)
	{
		IReadOnlyList<CardPrintingQuantity> readOnlyList = (context.IsReadOnly ? (listViewShowingSideboard ? model.AppliedFilteredSideboard : model.AppliedFilteredMainDeck) : ((context.StartingMode != DeckBuilderMode.ReadOnlyCollection) ? model.GetFilteredPool() : model.GetCardCollection()));
		if (context.StartingMode == DeckBuilderMode.ReadOnlyCollection && context.CardPoolOverride != null)
		{
			readOnlyList = readOnlyList.Where((CardPrintingQuantity item) => context.CardPoolOverride.ContainsKey(item.Printing.GrpId)).ToList();
		}
		return readOnlyList;
	}

	public static CommanderType HasCommanderSet(DeckBuilderContext context, DeckBuilderModel model)
	{
		if (context.IsEditingDeck && context.Format.FormatIncludesCommandZone && model.GetFilteredCommandZone().Count == 1 && model.GetFilteredCommandZone()[0].Printing.HasPartnerAbility())
		{
			return CommanderType.PartnerableCommander;
		}
		if (context.IsEditingDeck && context.Format.FormatIncludesCommandZone && model.GetFilteredCommandZone().Count > 0)
		{
			return CommanderType.CompleteCommander;
		}
		return CommanderType.NoCommander;
	}

	public static IEnumerable<CardFilterType> FetchFiltersBasedOnColorsInDeckModel(DeckBuilderModel model)
	{
		CardColorFlags flags = CardColorFlags.None;
		foreach (CardPrintingQuantity allFilteredCard in model.GetAllFilteredCards())
		{
			flags |= allFilteredCard.Printing.ColorIdentityFlags;
		}
		if ((flags & CardColorFlags.White) == CardColorFlags.White)
		{
			yield return CardFilterType.White;
		}
		if ((flags & CardColorFlags.Blue) == CardColorFlags.Blue)
		{
			yield return CardFilterType.Blue;
		}
		if ((flags & CardColorFlags.Black) == CardColorFlags.Black)
		{
			yield return CardFilterType.Black;
		}
		if ((flags & CardColorFlags.Red) == CardColorFlags.Red)
		{
			yield return CardFilterType.Red;
		}
		if ((flags & CardColorFlags.Green) == CardColorFlags.Green)
		{
			yield return CardFilterType.Green;
		}
		if (flags != CardColorFlags.None)
		{
			yield return CardFilterType.Color_Colorless;
		}
	}

	public static List<DeckFormat> GetAvailableFormats(List<DeckFormat> allFormats, List<EventContext> events, DeckFormat currentContextFormat)
	{
		List<DeckFormat> list = new List<DeckFormat>();
		foreach (DeckFormat allFormat in allFormats)
		{
			if (allFormat.IsEvergreen)
			{
				list.Add(allFormat);
			}
			else
			{
				if (allFormat.FormatType != MDNEFormatType.Constructed)
				{
					continue;
				}
				bool num = events?.Exists(allFormat.FormatName, delegate(EventContext e, string formatName)
				{
					if (e.PlayerEvent?.EventUXInfo.DeckSelectFormat == formatName)
					{
						IPlayerEvent playerEvent = e.PlayerEvent;
						if (playerEvent == null)
						{
							return true;
						}
						return !playerEvent.EventInfo.IsPreconEvent;
					}
					return false;
				}) ?? false;
				bool flag = currentContextFormat == allFormat;
				if (num || flag)
				{
					list.Add(allFormat);
				}
			}
		}
		return list;
	}

	public static void UpdateDeckBladeParticles(MonoBehaviour animatorParent)
	{
		Animator animator = null;
		if (animatorParent != null && animatorParent.isActiveAndEnabled)
		{
			animator = animatorParent.GetComponentInChildren<Animator>();
		}
		if (animator != null)
		{
			animator.SetBool(BlueHash, value: false);
		}
	}

	public static bool CanUseLargeCards(DeckBuilderContext context, DeckBuilderLayoutState layoutState)
	{
		if (context.Mode != DeckBuilderMode.ReadOnlyCollection && layoutState.LayoutInUse == DeckBuilderLayout.Column)
		{
			if (layoutState.CanUseLargeCardsInColumnView)
			{
				return !layoutState.IsColumnViewExpanded;
			}
			return false;
		}
		return true;
	}

	public static bool OwnsOrHasSkinInPool(CosmeticsProvider cosmeticsProvider, DeckBuilderModel model, ICardDatabaseAdapter cardDatabase, uint artId, string skinCode)
	{
		if (!cosmeticsProvider.OwnsSkin(artId, skinCode))
		{
			return model.CardSkinIsOverridden(cardDatabase, artId, skinCode);
		}
		return true;
	}

	public static void CloneDeckWithEventFormat(DecksManager deckManager, FormatManager formatManager, DeckBuilderContext context, Action<bool> showOrHide)
	{
		deckManager.CreateDeck(deckManager.GetDeck(context.Deck.id), DeckActionType.Cloned.ToString(), forceCopy: true).ThenOnMainThread(delegate(Promise<Client_DeckSummary> p)
		{
			if (p.Successful)
			{
				Client_DeckSummary result = p.Result;
				Client_Deck deck = deckManager.GetDeck(result.DeckId);
				deck.Summary.Format = context.EventFormat;
				DeckBuilderContext context2 = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(deck), context.Event)
				{
					Format = formatManager.GetSafeFormat(deck.Summary.Format)
				};
				Pantry.Get<DeckBuilderContextProvider>().Context = context2;
				showOrHide(obj: true);
			}
			else
			{
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Add_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Add_Error_Text"));
			}
		});
	}

	public static void Generic_OnMouseOver()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
	}

	public static void CardBackButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		Pantry.Get<DeckBuilderActionsHandler>().OpenDeckDetailsCosmeticsSelector(Pantry.Get<CardDatabase>(), DisplayCosmeticsTypes.Sleeve);
	}

	public static PagesMetaCardViewDisplayInformation CreatePreviewCard(CardPrintingData printingData, string skinCode)
	{
		return new PagesMetaCardViewDisplayInformation
		{
			Card = printingData,
			AvailableTitleCount = 1u,
			AvailablePrintingCount = 1u,
			UsedPrintingCount = 1u,
			Max = 1,
			Tint = PagesMetaCardView.Tint.None,
			QuantityStyle = PagesMetaCardView.QuantityDisplayStyle.Pips,
			PipsStyle = PagesMetaCardView.PipsDisplayStyle.Card,
			Skin = skinCode,
			UseNewTag = false
		};
	}
}
