using System;
using System.Collections.Generic;

namespace AssetLookupTree;

[Serializable]
public class SfxData
{
	public List<AudioEvent> AudioEvents = new List<AudioEvent>(1);

	public SfxData(List<AudioEvent> ae = null)
	{
		AudioEvents = ae ?? new List<AudioEvent>();
	}

	public bool isGlobalOnly()
	{
		List<AudioEvent> audioEvents = AudioEvents;
		if (audioEvents != null && audioEvents.Count > 0)
		{
			return AudioEvents.TrueForAll((AudioEvent x) => x?.PlayOnGlobal ?? false);
		}
		return false;
	}
}
