using System;
using System.Collections.Generic;
using GreClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG.JsonConversion;

public class MatchConfigConverter : JsonConverter<GreClient.Network.MatchConfig>
{
	private readonly IReadOnlyList<GreClient.Network.TeamConfig> _defaultTeamConfig;

	public MatchConfigConverter(IReadOnlyList<GreClient.Network.TeamConfig> defaultTeamConfig)
	{
		_defaultTeamConfig = defaultTeamConfig;
	}

	public override void WriteJson(JsonWriter writer, GreClient.Network.MatchConfig value, JsonSerializer serializer)
	{
		JObject jObject = new JObject();
		jObject.Add("Name", value.Name);
		jObject.Add("Version", value.Version);
		jObject.Add("BattlefieldSelection", value.BattlefieldSelection);
		jObject.Add("GameType", (int)value.GameType);
		jObject.Add("GameVariant", (int)value.GameVariant);
		jObject.Add("WinCondition", (int)value.WinCondition);
		jObject.Add("MulliganType", (int)value.MulliganType);
		jObject.Add("UseSpecifiedSeed", value.UseSpecifiedSeed);
		jObject.Add("RngSeed", JArray.FromObject(value.RngSeed, serializer));
		jObject.Add("FreeMulligans", value.FreeMulligans);
		jObject.Add("MaxHandSize", value.MaxHandSize);
		jObject.Add("ShuffleRestriction", (int)value.ShuffleRestriction);
		jObject.Add("Timers", (int)value.Timers);
		jObject.Add("LandsPerTurn", value.LandsPerTurn);
		jObject.Add("Teams", JArray.FromObject(value.Teams, serializer));
		jObject.WriteTo(writer);
	}

	public override GreClient.Network.MatchConfig ReadJson(JsonReader reader, Type objectType, GreClient.Network.MatchConfig existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		string name = jObject.GetValue("Name")?.Value<string>() ?? "UNNAMED";
		uint version = jObject.GetValue("Version")?.Value<uint>() ?? 0;
		string battlefieldSelection = jObject.GetValue("BattlefieldSelection")?.Value<string>() ?? "TEST";
		int gameType = jObject.GetValue("GameType")?.Value<int>() ?? 1;
		int gameVariant = jObject.GetValue("GameVariant")?.Value<int>() ?? 1;
		int winCondition = jObject.GetValue("WinCondition")?.Value<int>() ?? 1;
		int mulliganType = jObject.GetValue("MulliganType")?.Value<int>() ?? 3;
		uint freeMulligans = jObject.GetValue("FreeMulligans")?.Value<uint>() ?? 1;
		bool useSpecifiedSeed = jObject.GetValue("UseSpecifiedSeed")?.Value<bool>() ?? false;
		IReadOnlyList<uint> readOnlyList = jObject.GetValue("RngSeed")?.ToObject<uint[]>();
		IReadOnlyList<uint> rngSeed = readOnlyList ?? GreClient.Network.MatchConfig.CreateNewRNGSeed();
		uint maxHandSize = jObject.GetValue("MaxHandSize")?.Value<uint>() ?? 7;
		int shuffleRestriction = jObject.GetValue("ShuffleRestriction")?.Value<int>() ?? 0;
		int timers = jObject.GetValue("Timers")?.Value<int>() ?? 0;
		uint landsPerTurn = jObject.GetValue("LandsPerTurn")?.Value<uint>() ?? 1;
		IReadOnlyList<GreClient.Network.TeamConfig> readOnlyList2 = jObject.GetValue("Teams")?.ToObject<GreClient.Network.TeamConfig[]>(serializer);
		IReadOnlyList<GreClient.Network.TeamConfig> teams = readOnlyList2 ?? _defaultTeamConfig;
		return new GreClient.Network.MatchConfig(name, version, battlefieldSelection, (GameType)gameType, (GameVariant)gameVariant, (MatchWinCondition)winCondition, (MulliganType)mulliganType, useSpecifiedSeed, rngSeed, freeMulligans, maxHandSize, (ShuffleRestriction)shuffleRestriction, (TimerPackage)timers, landsPerTurn, teams);
	}
}
