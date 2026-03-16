using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace EsportsCrowd;

public struct HypeEvent
{
	public readonly HashSet<CrowdPersonBehaviour> AffectedPersons;

	public readonly CardColor? Affinity;

	public float Value;

	public HypeEvent(CardColor? affinity, float value)
	{
		AffectedPersons = new HashSet<CrowdPersonBehaviour>();
		Affinity = affinity;
		Value = value;
	}

	public HypeEvent(HypeEvent other)
	{
		AffectedPersons = new HashSet<CrowdPersonBehaviour>(other.AffectedPersons);
		Affinity = other.Affinity;
		Value = other.Value;
	}
}
