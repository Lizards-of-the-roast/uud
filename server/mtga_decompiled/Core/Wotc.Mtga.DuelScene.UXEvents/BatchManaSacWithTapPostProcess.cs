using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class BatchManaSacWithTapPostProcess : IUXEventGrouper
{
	private readonly IUXEventGrouper _internal = new BatchManaSacrificePostProcess(new SequenceValidatorAggregate(new SacrificeValidation(), new CardTappedValidation(), new ActivateManaValidation(), new ManaProducedValidation()));

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		_internal.GroupEvents(in startIdx, ref events);
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
