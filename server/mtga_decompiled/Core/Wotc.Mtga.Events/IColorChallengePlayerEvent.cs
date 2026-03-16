using System.Collections.Generic;

namespace Wotc.Mtga.Events;

public interface IColorChallengePlayerEvent : IPlayerEvent
{
	IColorChallengeTrack CurrentTrack { get; }

	Client_ColorChallengeMatchNode CurrentMatchNode { get; }

	int CompletedGames { get; }

	int TotalGames { get; }

	List<string> CompletedTracks { get; }

	string SelectTrack(string trackName);

	void SelectMatchNode(string nodeId);
}
