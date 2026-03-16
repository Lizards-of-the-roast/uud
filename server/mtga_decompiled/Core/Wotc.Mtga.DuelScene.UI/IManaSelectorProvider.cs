using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI;

public interface IManaSelectorProvider
{
	int CurrentSelection { get; }

	uint MaxSelections { get; }

	bool AllSelectionsComplete { get; }

	IReadOnlyCollection<ManaColor> SelectedColors { get; }

	int ValidSelectionCount { get; }

	IEnumerable<ManaColorSelector.ManaProducedData> ValidSelections { get; }

	bool WillTap { get; }

	uint? CurrentConstantCount { get; }

	ManaColorSelector.ManaProducedData GetElementAt(int index);

	bool ContainsColor(ManaColor color);

	void Select(ManaColor color);

	void Cleanup();

	uint? GetConstantCountForSelection(int index);

	uint? GetBranchingSelectionCount(int index, ManaColor color);
}
