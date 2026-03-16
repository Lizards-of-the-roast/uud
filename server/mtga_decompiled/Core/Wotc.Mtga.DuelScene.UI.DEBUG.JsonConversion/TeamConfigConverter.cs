using System;
using GreClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wotc.Mtga.DuelScene.UI.DEBUG.JsonConversion;

public class TeamConfigConverter : JsonConverter<TeamConfig>
{
	public override void WriteJson(JsonWriter writer, TeamConfig value, JsonSerializer serializer)
	{
		JObject jObject = new JObject();
		jObject.Add("Players", JArray.FromObject(value.Players, serializer));
		jObject.WriteTo(writer);
	}

	public override TeamConfig ReadJson(JsonReader reader, Type objectType, TeamConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return new TeamConfig(JObject.Load(reader).GetValue("Players")?.ToObject<PlayerConfig[]>(serializer) ?? Array.Empty<PlayerConfig>());
	}
}
