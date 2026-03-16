using System;
using System.Collections.Generic;

public class EmoteDatabase : IEmoteDataProvider
{
	private readonly Dictionary<string, EmoteData> _emoteData = new Dictionary<string, EmoteData>();

	private readonly Dictionary<string, EmoteData> _defaultEmoteData = new Dictionary<string, EmoteData>();

	public EmoteDatabase(IReadOnlyCollection<ClientEmoteEntry> emoteEntries)
	{
		Update(emoteEntries);
	}

	public void Update(IReadOnlyCollection<ClientEmoteEntry> emoteEntries)
	{
		foreach (ClientEmoteEntry item in (IEnumerable<ClientEmoteEntry>)(((object)emoteEntries) ?? ((object)Array.Empty<ClientEmoteEntry>())))
		{
			EmoteData emoteData = item.ToEmoteData();
			_emoteData[emoteData.Id] = emoteData;
			if (item.IsDefault)
			{
				_defaultEmoteData[emoteData.Id] = emoteData;
			}
		}
	}

	public EmoteData GetEmoteData(string id)
	{
		return _emoteData.GetValueOrDefault(id);
	}

	public IReadOnlyCollection<EmoteData> GetAllEmoteData()
	{
		return _emoteData.Values;
	}

	public IReadOnlyCollection<EmoteData> GetDefaultEmoteData()
	{
		return _defaultEmoteData.Values;
	}
}
