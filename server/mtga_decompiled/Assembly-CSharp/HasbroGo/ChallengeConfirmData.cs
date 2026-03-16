using System;
using Newtonsoft.Json;

namespace HasbroGo;

[Serializable]
public class ChallengeConfirmData
{
	[JsonProperty("displayName")]
	public string DisplayName;

	[JsonProperty("deck")]
	public string Deck;

	public ChallengeConfirmData(string displayName, string deck)
	{
		DisplayName = displayName;
		Deck = deck;
	}
}
