using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree;

public class VfxPrefabDataConverter : JsonConverter<VfxPrefabData>
{
	public override void WriteJson(JsonWriter writer, VfxPrefabData value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("StartTime");
		writer.WriteValue(value.StartTime);
		writer.WritePropertyName("CleanupAfterTime");
		writer.WriteValue(value.CleanupAfterTime);
		writer.WritePropertyName("SkipSelfCleanup");
		writer.WriteValue(value.SkipSelfCleanup);
		writer.WritePropertyName("AllPrefabs");
		writer.WriteStartArray();
		foreach (AltAssetReference<GameObject> allPrefab in value.AllPrefabs)
		{
			serializer.Serialize(writer, allPrefab);
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	public override VfxPrefabData ReadJson(JsonReader reader, Type objectType, VfxPrefabData existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new VfxPrefabData();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "StartTime":
				existingValue.StartTime = (float)reader.ReadAsDouble().Value;
				break;
			case "CleanupAfterTime":
				existingValue.CleanupAfterTime = (float)reader.ReadAsDouble().Value;
				break;
			case "SkipSelfCleanup":
				existingValue.SkipSelfCleanup = reader.ReadAsBoolean().Value;
				break;
			case "AllPrefabs":
				existingValue.AllPrefabs = new List<AltAssetReference<GameObject>>(1);
				reader.Read();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					existingValue.AllPrefabs.Add(serializer.Deserialize<AltAssetReference<GameObject>>(reader));
				}
				existingValue.AllPrefabs.TrimExcess();
				break;
			default:
				throw new InvalidOperationException("VfxPrefabDataConverter parsing error, unhandled property: " + text);
			}
		}
		return existingValue;
	}
}
