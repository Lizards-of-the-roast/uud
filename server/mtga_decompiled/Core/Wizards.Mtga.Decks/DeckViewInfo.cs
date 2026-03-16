using System;
using System.Collections.Generic;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wizards.Mtga.Decks;

public class DeckViewInfo
{
	public Guid deckId;

	public CardData sleeveData;

	public Client_Deck deck;

	public ClientVanitySelectionsV3 accountCosmeticDefaults;

	public string deckName;

	public List<ManaColor> manaColors = new List<ManaColor>();

	public ArtCrop crop;

	public string deckImageAssetPath;

	public bool useHistoricLabel;

	public bool isFavorite;

	public Guid? NetDeckFolderId;

	public bool IsNetDeck;

	private readonly Dictionary<string, DeckDisplayInfo> _deckValidationStateByFormat = new Dictionary<string, DeckDisplayInfo>();

	public DateTime LastPlayed;

	public DateTime LastUpdated;

	public string DeckFormat;

	private string DefaultDeckFormat => Pantry.Get<FormatManager>().GetDefaultFormat()?.FormatName;

	public DeckDisplayInfo GetValidationForFormat(string formatName)
	{
		if (string.IsNullOrEmpty(formatName))
		{
			formatName = (string.IsNullOrWhiteSpace(DeckFormat) ? DefaultDeckFormat : DeckFormat);
		}
		if (!_deckValidationStateByFormat.TryGetValue(formatName, out var value))
		{
			IColorChallengeStrategy colorChallenge = Pantry.Get<IColorChallengeStrategy>();
			DeckFormat safeFormat = Pantry.Get<FormatManager>().GetSafeFormat(formatName);
			value = DeckValidationUtils.CalculateDisplayInfo(deck, colorChallenge, safeFormat, WrapperController.Instance.InventoryManager, Pantry.Get<ITitleCountManager>(), WrapperController.Instance.CardDatabase, Languages.ActiveLocProvider, Pantry.Get<IEmergencyCardBansProvider>(), Pantry.Get<ISetMetadataProvider>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DesignerMetadataProvider>());
			_deckValidationStateByFormat.Add(formatName, value);
		}
		return value;
	}
}
