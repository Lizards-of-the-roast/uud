using System;
using Newtonsoft.Json;

namespace HasbroGo;

[Serializable]
public class ChallengeSendData
{
	[JsonProperty("displayName")]
	public string DisplayName;

	[JsonProperty("deck")]
	public string Deck;

	public ChallengeSendData(string displayName, string deck)
	{
		DisplayName = displayName;
		Deck = deck;
	}
}
