using System;
using Newtonsoft.Json;
using Wizards.Mtga.Utils;

namespace AssetLookupTree;

public class AudioEventConverter : JsonConverter<AudioEvent>
{
	private readonly StringCache _stringCache;

	public AudioEventConverter(StringCache stringCache)
	{
		_stringCache = stringCache;
	}

	public override void WriteJson(JsonWriter writer, AudioEvent value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Delay");
		writer.WriteValue(value.Delay);
		writer.WritePropertyName("WwiseEventName");
		writer.WriteValue(value.WwiseEventName);
		writer.WritePropertyName("PlayOnGlobal");
		writer.WriteValue(value.PlayOnGlobal);
		writer.WriteEndObject();
	}

	public override AudioEvent ReadJson(JsonReader reader, Type objectType, AudioEvent existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new AudioEvent();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "Delay":
				existingValue.Delay = (float)reader.ReadAsDouble().Value;
				break;
			case "WwiseEventName":
				existingValue.WwiseEventName = _stringCache.Get(reader.ReadAsString());
				break;
			case "PlayOnGlobal":
				existingValue.PlayOnGlobal = reader.ReadAsBoolean().Value;
				break;
			default:
				throw new InvalidOperationException("AudioEventConverter parsing error, unhandled property: " + text);
			}
		}
		return existingValue;
	}
}
