using System;
using System.Collections.Generic;
using System.Linq;
using EventPage.Components.NetworkModels;
using Wizards.Arena.Enums.UILayout;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.MDN.Services.Models.Event;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public class ColorChallengePlayerEvent : IColorChallengePlayerEvent, IPlayerEvent
{
	private class DefaultFactory : IGenericFactory<EventInfoV3, ICourseInfoWrapper, ColorChallengePlayerEvent>
	{
		public ColorChallengePlayerEvent Create(EventInfoV3 eventInfo, ICourseInfoWrapper course)
		{
			return new ColorChallengePlayerEvent(eventInfo, Pantry.Get<IAccountClient>().AccountInformation?.PersonaID);
		}
	}

	private IColorChallengeStrategy _strategy;

	private readonly string _personaId;

	public static readonly IGenericFactory<EventInfoV3, ICourseInfoWrapper, ColorChallengePlayerEvent> CurrentFactory = new DefaultFactory();

	private IColorChallengeStrategy Strategy => _strategy ?? (_strategy = Pantry.Get<IColorChallengeStrategy>());

	private ClientGraphDefinition Graph => Strategy.Graph;

	public IColorChallengeTrack CurrentTrack => Strategy.CurrentTrack;

	public Client_ColorChallengeMatchNode CurrentMatchNode => CurrentTrack?.CurrentMatchNode(_selectedMatchNodeId);

	private ClientNodeDefinition CurrentNodeDefinition
	{
		get
		{
			if (!Graph.Nodes.TryGetValue(CurrentMatchNode.Id, out var value))
			{
				return null;
			}
			return value;
		}
	}

	private string _selectedMatchNodeId { get; set; } = "";

	public int CompletedGames => Strategy.CompletedGames;

	public int TotalGames => Strategy.TotalGames;

	public List<string> CompletedTracks => Strategy.CompletedTracks;

	public string Name => Graph.Id;

	public IEventInfo EventInfo { get; }

	public bool InPlayingMatchesModule => !string.IsNullOrEmpty(_selectedMatchNodeId);

	public int GamesPlayed { get; }

	public bool HasUnclaimedRewards { get; }

	public int CurrentWins { get; }

	public int CurrentLosses { get; }

	public string DefaultTemplateName
	{
		get
		{
			if (PlatformUtils.IsHandheld())
			{
				if (PlatformUtils.GetCurrentAspectRatio() < 1.5f)
				{
					return "CampaignGraphEventTemplate_Handheld_4x3";
				}
				return "CampaignGraphEventTemplate_Handheld_16x9";
			}
			return "CampaignGraphEventTemplate";
		}
	}

	public LayoutDeckButtonBehavior DeckButtonBehavior => LayoutDeckButtonBehavior.Fixed;

	public UILayoutInfo UILayoutOptions { get; private set; }

	public MatchWinCondition WinCondition => MatchWinCondition.SingleElimination;

	public CourseData CourseData { get; private set; }

	public IEventUXInfo EventUXInfo { get; }

	public DeckFormat Format => Pantry.Get<FormatManager>().GetAllFormats().FirstOrDefault((DeckFormat f) => f.FormatName == EventUXInfo.DeckSelectFormat);

	public string MatchMakingName
	{
		get
		{
			if (CurrentMatchNode.IsPvpMatch)
			{
				return CurrentNodeDefinition.Configuration.QueueNodeConfig.QueueName;
			}
			return Name + "_" + CurrentMatchNode.Id;
		}
	}

	public List<DTO_JumpStartSelection> CurrentChoices { get; }

	public List<DTO_JumpStartSelection> PacketsChosen { get; }

	public List<DTO_JumpStartSelection> HistoricalChoices { get; }

	public List<uint> CollationIds { get; }

	public CardGrantTime? CardGrantTime { get; }

	public int AvgPodmakingSec { get; }

	public List<uint> Emblems { get; }

	public int MaxLosses { get; }

	public bool ShowCopyDecksButton { get; }

	public int MaxWins { get; }

	public List<uint> CardPool { get; }

	private ColorChallengePlayerEvent(EventInfoV3 eventInfo, string personaId)
	{
		_personaId = personaId;
		CourseData = new CourseData();
		UILayoutOptions = new UILayoutInfo
		{
			ResignBehavior = Wizards.MDN.LayoutResignBehavior.Hidden
		};
		EventInfo = new ColorChallengeEventInfo(eventInfo);
		EventUXInfo = new ColorChallengeUxInfo(eventInfo.EventUXInfo);
		_updateExternalData();
	}

	public string SelectTrack(string trackName)
	{
		string empty = string.Empty;
		if (_selectedMatchNodeId != empty)
		{
			SelectMatchNode(empty);
		}
		return CurrentTrack.Name;
	}

	public void SelectMatchNode(string nodeId)
	{
		if (!string.IsNullOrWhiteSpace(_personaId) && !string.IsNullOrWhiteSpace(nodeId))
		{
			MDNPlayerPrefs.SetCampaignEventSelectedNode(_personaId, Graph.Id, CurrentTrack.Name, nodeId);
			MDNPlayerPrefs.SetCampaignGraphSelectedEvent(_personaId, Graph.Id, CurrentTrack.Name);
		}
		_selectedMatchNodeId = (CurrentTrack.Nodes.Exists((Client_ColorChallengeMatchNode x) => x.Id == nodeId) ? nodeId : CurrentTrack.CurrentMatchNode(_selectedMatchNodeId).Id);
		_updateExternalData();
	}

	private void _updateExternalData()
	{
		if (CurrentMatchNode != null)
		{
			Guid deckIdForTrack = Strategy.GetDeckIdForTrack(CurrentTrack.Name);
			Client_Deck deckInfo = null;
			if (deckIdForTrack != default(Guid))
			{
				deckInfo = Pantry.Get<IPreconDeckServiceWrapper>().GetPreconDeck(deckIdForTrack);
			}
			CourseData.Update(deckInfo);
			if (EventInfo is ColorChallengeEventInfo colorChallengeEventInfo)
			{
				colorChallengeEventInfo.InternalEventName = (CurrentMatchNode.IsPvpMatch ? CurrentNodeDefinition.Configuration.QueueNodeConfig.QueueName : CurrentMatchNode.Id);
				colorChallengeEventInfo.UpdateDailyWeeklyRewards = CurrentMatchNode.IsPvpMatch;
				colorChallengeEventInfo.IsPreconEvent = !CurrentMatchNode.IsPvpMatch;
			}
		}
	}

	public bool HasPrize(int? wins)
	{
		return true;
	}

	public void UpgradeDeck(UpgradePacket upgradePacket)
	{
		throw new NotImplementedException();
	}

	public bool ShowInPlayblade(ClientPlayerInventory inventory)
	{
		return EventInfo.EventState != MDNEventState.NotActive;
	}

	public Promise<ICourseInfoWrapper> GetEventCourse()
	{
		throw new NotImplementedException();
	}

	public Promise<string> JoinNewMatchQueue()
	{
		return Strategy.JoinNewMatchQueue(_selectedMatchNodeId).Convert((ClientCampaignGraphState _) => (string)null);
	}

	public List<Guid> GetEventDeckIds(bool validate, out List<Guid> invalidDecks)
	{
		throw new NotImplementedException();
	}

	public List<RewardDisplayData> GetRewardDisplayData()
	{
		throw new NotImplementedException();
	}

	public string GetLocalizedText(EventTextType textType)
	{
		switch (textType)
		{
		case EventTextType.Title_MainNav:
			return "Events/Event_Title_" + Name;
		case EventTextType.Title_EventPage:
			return "Events/Event_Title_" + Strategy.CurrentTrack.Name;
		case EventTextType.Description:
			if (string.IsNullOrWhiteSpace(_selectedMatchNodeId))
			{
				return "Events/Event_Preview_Desc_" + Strategy.CurrentTrack.Name;
			}
			return "Events/Event_Desc_" + _selectedMatchNodeId;
		case EventTextType.Subtitle:
			if (!string.IsNullOrWhiteSpace(_selectedMatchNodeId))
			{
				return "Events/Event_Title_" + _selectedMatchNodeId;
			}
			break;
		}
		return "";
	}

	public Promise<Client_Deck> DeckFormattedForEventSubmission(Client_Deck deck)
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> SetChoice(string choice)
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> SubmitEventChoice(string choice, ChoiceType type)
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> JoinAndPay(EventEntryCurrencyType currency, string eventChoice)
	{
		throw new NotImplementedException();
	}

	public Promise<Client_Deck> SubmitEventDeck(Client_Deck deck)
	{
		CourseData.Update(deck);
		return new SimplePromise<Client_Deck>(deck);
	}

	public Promise<ICourseInfoWrapper> SubmitEventDeckFromChoices(Client_Deck deck)
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> ResignFromEvent()
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> DropFromEvent()
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> ClaimNoGatePrize()
	{
		throw new NotImplementedException();
	}

	public Promise<ICourseInfoWrapper> ClaimPrize()
	{
		throw new NotImplementedException();
	}

	public bool TracksGameCount(out int gamesLeft)
	{
		throw new NotImplementedException();
	}
}
