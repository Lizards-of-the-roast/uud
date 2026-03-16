using System.Collections.Generic;
using Core.Shared.Code.Providers;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.MDN;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

namespace Wizards.Mtga.Format;

public static class FormatUtilitiesClient
{
	public static List<DeckFormat> GetActiveFormats(FormatManager formatManager, EventManager eventManager)
	{
		List<DeckFormat> list = new List<DeckFormat>(formatManager.EvergreenFormats);
		foreach (EventContext eventContext in eventManager.EventContexts)
		{
			string deckSelectFormat = eventContext.PlayerEvent.EventUXInfo.DeckSelectFormat;
			if (deckSelectFormat != null && !list.ContainsFormatByName(deckSelectFormat))
			{
				list.Add(formatManager.GetSafeFormat(deckSelectFormat));
			}
		}
		return list;
	}

	public static bool ContainsFormatByName(this ICollection<DeckFormat> formats, string formatName)
	{
		if (string.IsNullOrWhiteSpace(formatName))
		{
			return false;
		}
		foreach (DeckFormat format in formats)
		{
			if (format.FormatName == formatName)
			{
				return true;
			}
		}
		return false;
	}

	public static string[] GetBannedFormatsName(uint printingDataTitleId, List<DeckFormat> formats)
	{
		List<string> list = new List<string>();
		foreach (DeckFormat format in formats)
		{
			if (format != null && format.IsCardBanned(printingDataTitleId))
			{
				string localizedName = format.GetLocalizedName();
				if (!list.Contains(localizedName))
				{
					list.Add(localizedName);
				}
			}
		}
		return list.ToArray();
	}

	public static string[] GetRestrictedFormatsNames(uint printingDataTitleId, List<DeckFormat> formats)
	{
		List<string> list = new List<string>();
		foreach (DeckFormat format in formats)
		{
			if (format != null && format.IsCardRestricted(printingDataTitleId))
			{
				string localizedName = format.GetLocalizedName();
				if (!list.Contains(localizedName))
				{
					list.Add(localizedName + "," + format.GetRestrictedQuotaMax(printingDataTitleId));
				}
			}
		}
		return list.ToArray();
	}

	public static bool UseHistoricLabel(Client_Deck deck, CardDatabase cardDb, FormatManager formatManager)
	{
		IEmergencyCardBansProvider emergencyCardBanProvider = Pantry.Get<IEmergencyCardBansProvider>();
		ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		CosmeticsProvider cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		DesignerMetadataProvider designerMetadataProvider = Pantry.Get<DesignerMetadataProvider>();
		if (!IsClientDeckLegalInFormat(emergencyCardBanProvider, setMetadataProvider, cosmeticsProvider, designerMetadataProvider, cardDb, formatManager, deck, "Historic"))
		{
			return false;
		}
		if (IsClientDeckLegalInFormat(emergencyCardBanProvider, setMetadataProvider, cosmeticsProvider, designerMetadataProvider, cardDb, formatManager, deck, "Standard"))
		{
			return false;
		}
		if (IsClientDeckLegalInFormat(emergencyCardBanProvider, setMetadataProvider, cosmeticsProvider, designerMetadataProvider, cardDb, formatManager, deck, "Alchemy"))
		{
			return false;
		}
		return true;
	}

	private static bool IsClientDeckLegalInFormat(IEmergencyCardBansProvider emergencyCardBanProvider, ISetMetadataProvider setMetadataProvider, CosmeticsProvider cosmeticsProvider, DesignerMetadataProvider designerMetadataProvider, CardDatabase cardDb, FormatManager formatManager, Client_Deck deck, string formatName)
	{
		return DeckValidationHelper.CalculateIsDeckLegal(formatManager.GetSafeFormat(formatName), deck, cardDb, emergencyCardBanProvider, setMetadataProvider, cosmeticsProvider, designerMetadataProvider).IsValid;
	}

	public static int FormatSortOrderComparator(DeckFormat a, DeckFormat b)
	{
		return a.SortOrder.CompareTo(b.SortOrder);
	}
}
