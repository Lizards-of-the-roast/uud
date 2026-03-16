using System;
using GreClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wotc.Mtga.DuelScene.UI.DEBUG.JsonConversion;

public class DeckConfigConverter : JsonConverter<DeckConfig>
{
	public override void WriteJson(JsonWriter writer, DeckConfig value, JsonSerializer serializer)
	{
		JObject jObject = new JObject();
		jObject.Add("Name", value.Name);
		jObject.Add("Deck", JArray.FromObject(value.Deck));
		jObject.Add("Sideboard", JArray.FromObject(value.Sideboard));
		jObject.Add("Commander", JArray.FromObject(value.Commander));
		jObject.Add("Companion", value.Companion);
		jObject.WriteTo(writer);
	}

	public override DeckConfig ReadJson(JsonReader reader, Type objectType, DeckConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		string name = jObject.GetValue("Name")?.Value<string>() ?? "UNNAMED";
		uint[] deck = jObject.GetValue("Deck")?.ToObject<uint[]>() ?? Array.Empty<uint>();
		uint[] sideboard = jObject.GetValue("Sideboard")?.ToObject<uint[]>() ?? Array.Empty<uint>();
		uint[] commander = jObject.GetValue("Commander")?.ToObject<uint[]>() ?? Array.Empty<uint>();
		uint companion = jObject.GetValue("Companion").Value<uint>();
		return new DeckConfig(name, deck, sideboard, commander, companion);
	}
}
