using System;
using System.Collections.Generic;

public readonly struct ChatterConfig
{
	public readonly float IdleTimer;

	public readonly IReadOnlyList<ChatterPair> IdleChatterOptions;

	public readonly float ThinkingTimer;

	public readonly IReadOnlyList<ChatterPair> ThinkingChatterOptions;

	public readonly IReadOnlyList<(uint, IReadOnlyList<ChatterPair>)> GameStartChatterOptions;

	public readonly IReadOnlyList<(string, IReadOnlyList<ChatterPair>)> EmoteReplyChatterOptions;

	public readonly IReadOnlyList<ChatterPair> CreatureDeathChatterOptions;

	public ChatterConfig(float idleTimer, IReadOnlyList<ChatterPair> idleChatterOptions, float thinkingTimer, IReadOnlyList<ChatterPair> thinkingChatterOptions, IReadOnlyList<ChatterPair> creatureDeathChatterOptions, IReadOnlyList<GameStartChatterBucket> gameStartChatterOptions, IReadOnlyList<EmoteReplyChatterBucket> emoteReplyChatterOptions)
	{
		IdleTimer = idleTimer;
		IdleChatterOptions = idleChatterOptions ?? Array.Empty<ChatterPair>();
		ThinkingTimer = thinkingTimer;
		ThinkingChatterOptions = thinkingChatterOptions ?? Array.Empty<ChatterPair>();
		CreatureDeathChatterOptions = creatureDeathChatterOptions ?? Array.Empty<ChatterPair>();
		List<(uint, IReadOnlyList<ChatterPair>)> list = new List<(uint, IReadOnlyList<ChatterPair>)>();
		foreach (GameStartChatterBucket item in gameStartChatterOptions ?? Array.Empty<GameStartChatterBucket>())
		{
			list.Add((item.minimumCardsInHand, item.stringAudioPairs));
		}
		GameStartChatterOptions = list;
		List<(string, IReadOnlyList<ChatterPair>)> list2 = new List<(string, IReadOnlyList<ChatterPair>)>();
		foreach (EmoteReplyChatterBucket item2 in emoteReplyChatterOptions ?? Array.Empty<EmoteReplyChatterBucket>())
		{
			list2.Add((item2.emoteToReplyTo, item2.stringAudioPairs));
		}
		EmoteReplyChatterOptions = list2;
	}
}
