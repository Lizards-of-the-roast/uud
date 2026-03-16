using System;
using System.Collections.Generic;
using System.Linq;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public class LastPlayedBladeData
{
	private readonly IBladeModel _model;

	private const int ENTRIES_TO_DISPLAY = 4;

	public List<RecentlyPlayedInfo> RecentlyPlayed => (_model.RecentlyPlayed ?? new List<RecentlyPlayedInfo>()).Skip(Math.Max(0, (_model.RecentlyPlayed?.Count ?? 0) - 4)).ToList();

	public Dictionary<PlayBladeQueueType, List<BladeQueueInfo>> Queues => _model.Queues ?? new Dictionary<PlayBladeQueueType, List<BladeQueueInfo>>();

	public LastPlayedBladeData(IBladeModel model)
	{
		_model = model;
	}
}
