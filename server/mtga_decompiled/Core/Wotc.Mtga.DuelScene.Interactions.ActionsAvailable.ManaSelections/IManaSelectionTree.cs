using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public interface IManaSelectionTree
{
	uint SourceId { get; }

	IEnumerable<ManaColor> SelectableColors { get; }

	IEnumerable<ManaColor> SelectionTotal { get; }

	uint SelectionCount { get; }

	bool AllSelectionsComplete { get; }

	bool WillTap { get; }

	uint? BranchingSelectionCount(ManaColor color);

	uint AmountForColor(ManaColor color);

	uint? ConstantManaCount();

	(Action, ManaPaymentOption) GetPaymentOption();

	IEnumerable<(Action, ManaPaymentOption)> GetAllPaymentOptions();

	void Prune(IManaSelectionTree previousTree);

	void Next(ManaColor color);

	void Undo();
}
