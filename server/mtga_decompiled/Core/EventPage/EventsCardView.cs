using UnityEngine;

namespace EventPage;

public class EventsCardView : CDCMetaCardView
{
	protected override bool ShowHighlight => false;

	protected override Bounds GetBounds()
	{
		return _cardCollider?.bounds ?? new Bounds(Vector3.zero, Vector3.zero);
	}
}
