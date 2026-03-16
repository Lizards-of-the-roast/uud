using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class SfxDataConverter : JsonConverter<SfxData>
{
	public override void WriteJson(JsonWriter writer, SfxData value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("AudioEvents");
		writer.WriteStartArray();
		foreach (AudioEvent audioEvent in value.AudioEvents)
		{
			serializer.Serialize(writer, audioEvent);
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	public override SfxData ReadJson(JsonReader reader, Type objectType, SfxData existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new SfxData();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			if (text == "AudioEvents")
			{
				SfxData sfxData = existingValue;
				if (sfxData.AudioEvents == null)
				{
					sfxData.AudioEvents = new List<AudioEvent>(1);
				}
				existingValue.AudioEvents.Clear();
				reader.Read();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					existingValue.AudioEvents.Add(serializer.Deserialize<AudioEvent>(reader));
				}
				existingValue.AudioEvents.TrimExcess();
				continue;
			}
			throw new InvalidOperationException("SfxDataConverter parsing error, unhandled property: " + text);
		}
		return existingValue;
	}
}
