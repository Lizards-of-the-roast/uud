using System.Collections.Generic;

public interface IEmoteDataProvider
{
	EmoteData GetEmoteData(string id);

	void Update(IReadOnlyCollection<ClientEmoteEntry> emoteEntries);

	IReadOnlyCollection<EmoteData> GetAllEmoteData();

	IReadOnlyCollection<EmoteData> GetDefaultEmoteData();
}
