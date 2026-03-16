using System;
using System.Collections.Generic;

public static class IEmoteDataProviderExtensions
{
	public static bool TryGetEmoteData(this IEmoteDataProvider provider, string id, out EmoteData emoteData)
	{
		emoteData = provider?.GetEmoteData(id);
		return emoteData != null;
	}

	public static IEnumerable<EmoteData> GetEmoteData(this IEmoteDataProvider provider, IEnumerable<string> ids)
	{
		foreach (string item in ids ?? Array.Empty<string>())
		{
			if (provider.TryGetEmoteData(item, out var emoteData))
			{
				yield return emoteData;
			}
		}
	}
}
