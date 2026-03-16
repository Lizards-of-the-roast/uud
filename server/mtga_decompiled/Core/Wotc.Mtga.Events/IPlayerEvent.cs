using System;
using System.Collections.Generic;
using Wizards.Arena.Enums.UILayout;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.MDN.Services.Models.Event;
using Wizards.Models;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public interface IPlayerEvent
{
	IEventInfo EventInfo { get; }

	IEventUXInfo EventUXInfo { get; }

	DeckFormat Format { get; }

	CourseData CourseData { get; }

	bool InPlayingMatchesModule { get; }

	int GamesPlayed { get; }

	bool HasUnclaimedRewards { get; }

	int CurrentWins { get; }

	int CurrentLosses { get; }

	string MatchMakingName { get; }

	List<DTO_JumpStartSelection> CurrentChoices { get; }

	List<DTO_JumpStartSelection> PacketsChosen { get; }

	List<DTO_JumpStartSelection> HistoricalChoices { get; }

	List<uint> CollationIds { get; }

	CardGrantTime? CardGrantTime { get; }

	int AvgPodmakingSec { get; }

	List<uint> Emblems { get; }

	int MaxLosses { get; }

	string DefaultTemplateName { get; }

	UILayoutInfo UILayoutOptions { get; }

	LayoutDeckButtonBehavior DeckButtonBehavior { get; }

	bool ShowCopyDecksButton { get; }

	int MaxWins { get; }

	List<uint> CardPool { get; }

	MatchWinCondition WinCondition { get; }

	bool HasPrize(int? wins);

	void UpgradeDeck(UpgradePacket upgradePacket);

	bool ShowInPlayblade(ClientPlayerInventory inventory);

	Promise<Client_Deck> DeckFormattedForEventSubmission(Client_Deck deck);

	Promise<ICourseInfoWrapper> SetChoice(string choice);

	Promise<ICourseInfoWrapper> SubmitEventChoice(string choice, ChoiceType type);

	Promise<ICourseInfoWrapper> JoinAndPay(EventEntryCurrencyType currency, string eventChoice);

	Promise<Client_Deck> SubmitEventDeck(Client_Deck deck);

	Promise<ICourseInfoWrapper> SubmitEventDeckFromChoices(Client_Deck deck);

	Promise<ICourseInfoWrapper> ResignFromEvent();

	Promise<ICourseInfoWrapper> DropFromEvent();

	Promise<ICourseInfoWrapper> GetEventCourse();

	Promise<ICourseInfoWrapper> ClaimNoGatePrize();

	Promise<ICourseInfoWrapper> ClaimPrize();

	Promise<string> JoinNewMatchQueue();

	List<Guid> GetEventDeckIds(bool validate, out List<Guid> invalidDecks);

	bool TracksGameCount(out int gamesLeft);

	List<RewardDisplayData> GetRewardDisplayData();

	string GetLocalizedText(EventTextType textType);
}
