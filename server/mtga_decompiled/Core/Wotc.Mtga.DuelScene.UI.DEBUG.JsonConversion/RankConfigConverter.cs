using System;
using GreClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wotc.Mtga.DuelScene.UI.DEBUG.JsonConversion;

public class RankConfigConverter : JsonConverter<RankConfig>
{
	public override void WriteJson(JsonWriter writer, RankConfig value, JsonSerializer serializer)
	{
		JObject jObject = new JObject();
		jObject.Add("Class", value.Class);
		jObject.Add("Tier", value.Tier);
		jObject.Add("MythicPercent", value.MythicPercent);
		jObject.Add("MythicPlacement", value.MythicPlacement);
		jObject.WriteTo(writer);
	}

	public override RankConfig ReadJson(JsonReader reader, Type objectType, RankConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		int rankClass = jObject.GetValue("Class")?.Value<int>() ?? 0;
		int rankTier = jObject.GetValue("Tier")?.Value<int>() ?? 0;
		float mythicPercent = jObject.GetValue("MythicPercent")?.Value<float>() ?? 0f;
		int mythicPlacement = jObject.GetValue("MythicPlacement")?.Value<int>() ?? 0;
		return new RankConfig(rankClass, rankTier, mythicPercent, mythicPlacement);
	}
}
