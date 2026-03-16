using System;
using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Network.ServiceWrappers;

public class MercantileEmoteEntryLoader : IEmoteEntryLoader
{
	private readonly IMercantileServiceWrapper _mercantile;

	public bool IsLoaded { get; private set; }

	public IReadOnlyCollection<ClientEmoteEntry> EmoteEntries { get; private set; }

	public event Action<IReadOnlyCollection<ClientEmoteEntry>> OnEmoteEntryLoaded;

	public MercantileEmoteEntryLoader(IMercantileServiceWrapper mercantile)
	{
		_mercantile = mercantile;
		_mercantile.OnEmoteCatalogUpdated += OnEmoteCatalogUpdated;
	}

	~MercantileEmoteEntryLoader()
	{
		_mercantile.OnEmoteCatalogUpdated -= OnEmoteCatalogUpdated;
	}

	public Promise<MercantileCollections> Load()
	{
		return _mercantile.GetMercantileCollections();
	}

	private void OnEmoteCatalogUpdated(EmoteCatalog emoteCatalog)
	{
		IsLoaded = true;
		List<EmoteEntry> list = new List<EmoteEntry>();
		foreach (EmoteEntry value in emoteCatalog.Values)
		{
			list.Add(value);
		}
		EmoteEntries = EmoteUtils.TranslateToClientEmoteEntries(list);
		this.OnEmoteEntryLoaded?.Invoke(EmoteEntries);
	}
}
