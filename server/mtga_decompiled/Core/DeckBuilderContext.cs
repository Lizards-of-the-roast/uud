using System;
using System.Collections.Generic;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;

public class DeckBuilderContext
{
	public EventContext Event;

	public string SetToFilter;

	public readonly DeckInfo Deck;

	public readonly bool IsSideboarding;

	public readonly bool IsFirstEdit;

	public bool IsAmbiguousFormat;

	public readonly DeckBuilderMode StartingMode;

	public DeckBuilderMode Mode;

	public readonly Guid ChallengeId;

	public readonly Dictionary<uint, uint> CardPoolOverride;

	public readonly Dictionary<uint, string> CardSkinOverride;

	public readonly DeckSelectContext DeckSelectContext;

	public readonly bool IsCachingEnabled;

	public readonly string PreconContext;

	public readonly bool IsPlayblade;

	public readonly string EventFormat;

	public readonly bool IsInvalidForEventFormat;

	public readonly bool IsDrafting;

	public DeckFormat Format { get; set; }

	public bool IsEditingDeck => Deck != null;

	public bool IsConstructed
	{
		get
		{
			if (Format != null)
			{
				return Format.FormatType == MDNEFormatType.Constructed;
			}
			return false;
		}
	}

	public bool IsLimited
	{
		get
		{
			if (Format != null)
			{
				return FormatUtilities.IsLimited(Format.FormatType);
			}
			return false;
		}
	}

	public bool IsEvent => Event != null;

	public bool IsPlayQueueEvent
	{
		get
		{
			if (Event != null)
			{
				return !Event.PlayerEvent.EventUXInfo.HasEventPage;
			}
			return false;
		}
	}

	public bool IsColorChallengeEvent => Event?.PlayerEvent is IColorChallengePlayerEvent;

	public bool IsReadOnly => StartingMode == DeckBuilderMode.ReadOnly;

	public bool CanSaveInvalidDecks
	{
		get
		{
			if (!IsSideboarding && !IsLimited)
			{
				return !IsColorChallengeEvent;
			}
			return false;
		}
	}

	public bool SuggestLandAfterDeckLoad
	{
		get
		{
			if (IsLimited && IsFirstEdit)
			{
				return !Event.PlayerEvent.IsOmniscience();
			}
			return false;
		}
	}

	public int MaxCardsByTitle => (Format ?? Pantry.Get<FormatManager>().GetDefaultFormat()).MaxCardsByTitle;

	public bool CanCraft
	{
		get
		{
			if (!IsSideboarding && StartingMode != DeckBuilderMode.ReadOnlyCollection)
			{
				return !IsLimited;
			}
			return false;
		}
	}

	public bool OnlyShowPoolCards
	{
		get
		{
			if (!IsLimited && !IsSideboarding)
			{
				return StartingMode == DeckBuilderMode.ReadOnlyCollection;
			}
			return true;
		}
	}

	public bool ShouldShowCollectedToggles
	{
		get
		{
			if (IsEditingDeck)
			{
				if (IsConstructed)
				{
					return !IsSideboarding;
				}
				return false;
			}
			return true;
		}
	}

	public DeckBuilderContext(DeckInfo deck = null, EventContext evt = null, bool sideboarding = false, bool firstEdit = false, DeckBuilderMode startingMode = DeckBuilderMode.DeckBuilding, bool ambiguousFormat = false, Guid challengeId = default(Guid), Dictionary<uint, uint> cardPoolOverride = null, Dictionary<uint, string> cardSkinOverride = null, DeckSelectContext deckSelectContext = null, bool cachingEnabled = false, bool isPlayblade = false, string eventFormat = null, bool isInvalidForEventFormat = false, string preconDeckContext = null, bool isDrafting = false, string setToFilter = null)
	{
		Deck = deck;
		Event = evt;
		IsSideboarding = sideboarding;
		IsFirstEdit = firstEdit;
		StartingMode = startingMode;
		Mode = startingMode;
		IsAmbiguousFormat = ambiguousFormat;
		ChallengeId = challengeId;
		CardPoolOverride = cardPoolOverride;
		CardSkinOverride = cardSkinOverride;
		DeckSelectContext = deckSelectContext;
		IsCachingEnabled = cachingEnabled;
		IsPlayblade = isPlayblade;
		EventFormat = eventFormat;
		IsInvalidForEventFormat = isInvalidForEventFormat;
		PreconContext = preconDeckContext;
		IsDrafting = isDrafting;
		SetToFilter = setToFilter;
	}

	public bool ShowCraftingButtons(DecksManager deckManager)
	{
		if (CanCraftDeck(deckManager))
		{
			return CanCraft;
		}
		return false;
	}

	public bool IsOverRestriction(uint numInDeck, uint titleId)
	{
		if (!IsLimited && Format.IsCardRestricted(titleId))
		{
			return numInDeck > Format.GetRestrictedQuotaMax(titleId);
		}
		return false;
	}

	public bool CanCraftDeck(DecksManager deckManager)
	{
		if (IsLimited || PreconContext != null)
		{
			return false;
		}
		DeckInfo deck = Deck;
		object obj;
		if (deck != null)
		{
			_ = deck.id;
			if (0 == 0)
			{
				obj = deckManager?.GetDeck(Deck.id);
				goto IL_0042;
			}
		}
		obj = null;
		goto IL_0042;
		IL_0042:
		Client_Deck client_Deck = (Client_Deck)obj;
		if (client_Deck == null)
		{
			return true;
		}
		if (client_Deck.Summary.IsNetDeck || !IsReadOnly)
		{
			return CanCraft;
		}
		return false;
	}

	public (bool AllowUncollectedCards, IReadOnlyList<uint> CardPool) DeckValidationEventData()
	{
		(bool, IReadOnlyList<uint>)? tuple = Event?.PlayerEvent?.DeckValidationEventData();
		return (AllowUncollectedCards: tuple?.Item1 ?? false, CardPool: tuple?.Item2);
	}
}
