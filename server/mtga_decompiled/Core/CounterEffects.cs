using System.Collections.Generic;
using AssetLookupTree;

public readonly struct CounterEffects
{
	public readonly string PrefabPath;

	public readonly List<AudioEvent> AudioEvents;

	public readonly VfxData CardVFX;

	public CounterEffects(string prefabPath, List<AudioEvent> audioEvents, VfxData cardVfx = null)
	{
		PrefabPath = prefabPath;
		AudioEvents = audioEvents;
		CardVFX = cardVfx;
	}
}
