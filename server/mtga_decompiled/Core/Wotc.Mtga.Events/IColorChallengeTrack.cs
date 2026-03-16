using System.Collections.Generic;
using Wizards.Mtga.Decks;

namespace Wotc.Mtga.Events;

public interface IColorChallengeTrack
{
	string Name { get; }

	List<Client_ColorChallengeMatchNode> Nodes { get; }

	bool Completed { get; }

	int UnlockedMatchNodeCount { get; }

	Client_DeckSummary DeckSummary { get; }

	Client_ColorChallengeMatchNode CurrentMatchNode(string lastSelectedMatchNodeId);

	bool IsNodeNextToUnlock(string id);

	bool IsNodeCompleted(string id);
}
