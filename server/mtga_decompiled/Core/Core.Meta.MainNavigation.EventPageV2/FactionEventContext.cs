using System.Collections.Generic;
using EventPage;
using EventPage.Components;
using Wizards.MDN;
using Wizards.Unification.Models.Events;

namespace Core.Meta.MainNavigation.EventPageV2;

public class FactionEventContext
{
	public Dictionary<string, FactionSealedUXInfo> FactionInfo = new Dictionary<string, FactionSealedUXInfo>();

	public static string NO_FACTION_SELECTED = "NO_FACTION_SELECTED";

	public string EventId { get; set; }

	public EventContext EventContext { get; set; }

	public LossDetailsComponentController LossController { get; set; }

	public ObjectiveTrackComponentController ObjectiveTrackController { get; set; }

	public string SelectedFaction { get; set; } = NO_FACTION_SELECTED;

	public EventPageScaffolding PostPurchaseScaffolding { get; set; }

	public EventComponentManager PostPurchaseComponentManager { get; set; }

	public uint NormalPack { get; set; }

	public string DefaultBackground { get; set; }
}
