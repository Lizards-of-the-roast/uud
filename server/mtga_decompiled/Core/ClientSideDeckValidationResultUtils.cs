using System;
using System.Text;
using Wizards.Arena.DeckValidation.Client;
using Wotc.Mtga.Loc;

public static class ClientSideDeckValidationResultUtils
{
	public const string MinMainDeckRequiredLocParam = "MinMainDeckRequired";

	public const string MaxMainDeckRequiredLocParam = "MaxMainDeckRequired";

	public const string MaxTotalCardsLocParam = "MaxTotalCards";

	public const string MaxSideboardCardsLocParam = "MaxSideboardCards";

	public const string MaxSideboardCardsLocValue = "15";

	public static string GetInvalidReasons(this ClientSideDeckValidationResult self, IClientLocProvider locProvider)
	{
		StringBuilder stringBuilder = new StringBuilder();
		(string, string)[] locParams = new(string, string)[4]
		{
			("MinMainDeckRequired", self.MinMainDeckCards.ToString()),
			("MaxMainDeckRequired", self.MaxMainDeckCards.ToString()),
			("MaxTotalCards", self.MaxTotalCards.ToString()),
			("MaxSideboardCards", "15")
		};
		if (self.IsCommander && (self.NumberLessThanRequiredInMain != 0 || self.NumberMoreThanRequiredInMain != 0))
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_Brawl_NeedsX", locParams));
			stringBuilder.Append(Environment.NewLine);
		}
		if (!self.IsCommander && self.NumberLessThanRequiredInMain != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_BelowX", locParams));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.HasMoreThanXInDeck)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_MoreThanXInMain", locParams));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberMoreThanXInSideboard != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_MoreThanXInSideboard", locParams));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberMoreThan0InSideboard != 0)
		{
			string localizedText = locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_MoreThan0InSideboard");
			stringBuilder.AppendLine(localizedText);
		}
		if (self.NumberBannedCards != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasBannedCards"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberEmergencyBannedCards != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasTempBannedCards"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberTooManyCardsByTitle != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_MoreThan4Copies"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberNonFormatCard != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_NotLegal"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.HasUnOwnedCards)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasUnowned"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberUnknownCards != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/Deckbuilder/DeckBuilder_InvalidDeckReasons_HasUnknownCards"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.HasCommanderButShouldNot)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasCommanderButShouldNot"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.HasNoCommanderButShould)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasNoCommanderButShould"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.HasInvalidCommander)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasInvalidCommander"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.NumberInvalidCardsForCommander != 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasInvalidCardsForCommander"));
			stringBuilder.Append(Environment.NewLine);
		}
		if (self.CardTitlesOverRestrictedListQuota.Count > 0)
		{
			stringBuilder.Append(locProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_RestrictedQuotaExceeded"));
			stringBuilder.Append(Environment.NewLine);
		}
		return stringBuilder.ToString();
	}
}
