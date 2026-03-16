using System;
using GreClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG.JsonConversion;

public class PlayerConfigConverter : JsonConverter<GreClient.Network.PlayerConfig>
{
	private const uint CURRENT_VERSION = 1u;

	public override void WriteJson(JsonWriter writer, GreClient.Network.PlayerConfig value, JsonSerializer serializer)
	{
		JObject jObject = new JObject();
		jObject.Add("Version", 1u);
		jObject.Add("Name", value.Name);
		jObject.Add("PlayerType", (int)value.PlayerType);
		jObject.Add("PlayerId", value.PlayerId);
		jObject.Add("Deck", JObject.FromObject(value.Deck, serializer));
		jObject.Add("CardStyles", JArray.FromObject(value.CardStyles));
		jObject.Add("FamiliarStrategy", (int)value.FamiliarStrategy);
		jObject.Add("StartingLife", value.StartingLife);
		jObject.Add("StartingHandSize", value.StartingHandSize);
		jObject.Add("ShuffleRestriction", (int)value.ShuffleRestriction);
		jObject.Add("TreeOfCongress", value.TreeOfCongress);
		jObject.Add("StartingPlayer", value.StartingPlayer);
		jObject.Add("Emblems", JArray.FromObject(value.Emblems));
		jObject.Add("DeckDirectory", value.DeckDirectory);
		jObject.Add("Avatar", value.Avatar);
		jObject.Add("Sleeve", value.Sleeve);
		jObject.Add("Pet", JObject.FromObject(value.Pet));
		jObject.Add("Title", value.Title);
		jObject.Add("Rank", JObject.FromObject(value.Rank, serializer));
		jObject.WriteTo(writer);
	}

	public override GreClient.Network.PlayerConfig ReadJson(JsonReader reader, Type objectType, GreClient.Network.PlayerConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		uint num = jObject.GetValue("Version")?.Value<uint>() ?? 0;
		int num2 = jObject.GetValue("PlayerType")?.Value<int>() ?? 0;
		string playerId = jObject.GetValue("PlayerId")?.Value<string>() ?? string.Empty;
		GreClient.Network.DeckConfig deck = jObject.GetValue("Deck")?.ToObject<GreClient.Network.DeckConfig>() ?? GreClient.Network.DeckConfig.Default();
		(uint, string)[] cardStyles = jObject.GetValue("CardStyles")?.ToObject<(uint, string)[]>(serializer) ?? Array.Empty<(uint, string)>();
		int familiarStrategy = jObject.GetValue("FamiliarStrategy")?.Value<int>() ?? 1;
		int shuffleRestriction = jObject.GetValue("ShuffleRestriction")?.Value<int>() ?? 0;
		uint startingLife = jObject.GetValue("StartingLife")?.Value<uint>() ?? 20;
		uint startingHandSize = jObject.GetValue("StartingHandSize")?.Value<uint>() ?? 7;
		bool treeOfCongress = jObject.GetValue("TreeOfCongress")?.Value<bool>() ?? false;
		bool startingPlayer = jObject.GetValue("StartingPlayer")?.Value<bool>() ?? false;
		uint[] emblems = jObject.GetValue("Emblems")?.ToObject<uint[]>() ?? Array.Empty<uint>();
		string deckDirectory = jObject.GetValue("DeckDirectory")?.Value<string>() ?? string.Empty;
		string avatar = jObject.GetValue("Avatar")?.Value<string>() ?? "Avatar_Basic_AjaniGoldmane";
		string sleeve = jObject.GetValue("Sleeve")?.Value<string>() ?? string.Empty;
		(string, string) pet = jObject.GetValue("Pet")?.ToObject<(string, string)>() ?? (string.Empty, string.Empty);
		string title = jObject.GetValue("Title")?.ToObject<string>() ?? "NoTitle";
		string playerName = GetPlayerName(jObject, (num2 == 0) ? "Local Player" : "Opponent", num >= 1);
		RankConfig rankConfig = GetRankConfig(jObject, serializer, num >= 1);
		return new GreClient.Network.PlayerConfig(playerName, (PlayerType)num2, playerId, deck, cardStyles, (FamiliarStrategyType)familiarStrategy, (ShuffleRestriction)shuffleRestriction, startingLife, startingHandSize, treeOfCongress, startingPlayer, emblems, deckDirectory, avatar, sleeve, pet, title, rankConfig);
	}

	private static string GetPlayerName(JObject jsonObject, string defaultValue, bool expectValue)
	{
		if (expectValue)
		{
			return jsonObject.GetValue("Name")?.Value<string>() ?? defaultValue;
		}
		return defaultValue;
	}

	private static RankConfig GetRankConfig(JObject jsonObject, JsonSerializer serializer, bool expectValue)
	{
		if (expectValue)
		{
			return jsonObject.GetValue("Rank")?.ToObject<RankConfig>(serializer) ?? RankConfig.Default;
		}
		return RankConfig.Default;
	}
}
