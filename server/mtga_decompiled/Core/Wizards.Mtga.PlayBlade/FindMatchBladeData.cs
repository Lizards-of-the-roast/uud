using System.Collections.Generic;
using Wizards.Mtga.Decks;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public class FindMatchBladeData
{
	private readonly IBladeModel _model;

	public Dictionary<PlayBladeQueueType, List<BladeQueueInfo>> Queues => _model.Queues ?? new Dictionary<PlayBladeQueueType, List<BladeQueueInfo>>();

	public List<DeckViewInfo> Decks => _model.Decks ?? new List<DeckViewInfo>();

	public FindMatchBladeData(IBladeModel model)
	{
		_model = model;
	}
}
