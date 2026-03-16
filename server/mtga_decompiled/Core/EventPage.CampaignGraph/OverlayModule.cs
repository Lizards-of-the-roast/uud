using System;
using Wotc.Mtga;

namespace EventPage.CampaignGraph;

public abstract class OverlayModule : EventModule
{
	public Action OnEnabled;

	public Action OnDisabled;

	private void OnEnable()
	{
		OnEnabled?.Invoke();
	}

	private void OnDisable()
	{
		OnDisabled?.Invoke();
	}

	public virtual void SetZoomHandler(ICardRolloverZoom cardRolloverZoom)
	{
	}
}
