using System.Collections.Generic;
using EventPage.Components.NetworkModels;
using Wizards.Arena.Enums.Event;
using Wizards.Unification.Models.Events;

namespace Wotc.Mtga.Events;

public interface IEventUXInfo
{
	string PublicEventName { get; }

	string TitleLocKey { get; }

	int DisplayPriority { get; }

	Dictionary<string, string> Parameters { get; }

	string Group { get; }

	EventBladeBehavior EventBladeBehavior { get; }

	bool HasEventPage { get; }

	bool PrioritizeBannerIfPlayerHasToken { get; }

	string DeckSelectFormat { get; set; }

	List<FactionSealedUXInfo> FactionSealedUXInfo { get; set; }

	bool OpenedFromPlayBlade { get; set; }

	EventPage.Components.NetworkModels.EventComponentData EventComponentData { get; }

	List<string> DynamicFilterTagIds { get; }
}
