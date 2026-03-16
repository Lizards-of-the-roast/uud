using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Events;

public interface IColorChallengeStrategy
{
	ClientGraphDefinition Graph { get; }

	string TemplateKey { get; }

	bool InPlayingMatchesModule { get; }

	bool CurrentTrackCompleted { get; }

	Dictionary<string, IColorChallengeTrack> Tracks { get; }

	IColorChallengeTrack CurrentTrack { get; }

	bool ColorChallengeSkipped { get; }

	bool Initialized { get; }

	int CompletedGames { get; }

	int TotalGames { get; }

	List<string> CompletedTracks { get; }

	string CurrentTrackName { get; set; }

	event Action<int> OnCompletedGamesChanged;

	string SwitchTrack(string trackName);

	IEnumerator UpdateData();

	void GoToNextNode();

	Promise<ClientCampaignGraphState> Skip();

	Guid GetDeckIdForTrack(string trackName);

	Promise<ClientCampaignGraphState> JoinNewMatchQueue(string nodeId);

	bool TryGetDeckUpgradePacket(out Client_DeckUpgrade deckUpgrade);

	Task<bool> GetMilestoneStatus(string milestoneName);
}
