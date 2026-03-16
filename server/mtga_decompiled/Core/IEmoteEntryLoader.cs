using System;
using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wotc.Mtga.Client.Models.Mercantile;

public interface IEmoteEntryLoader
{
	bool IsLoaded { get; }

	IReadOnlyCollection<ClientEmoteEntry> EmoteEntries { get; }

	event Action<IReadOnlyCollection<ClientEmoteEntry>> OnEmoteEntryLoaded;

	Promise<MercantileCollections> Load();
}
