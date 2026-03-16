using System;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class SpaceDataConverter : JsonConverter<SpaceData>
{
	public override void WriteJson(JsonWriter writer, SpaceData value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Space");
		writer.WriteValue((int)value.Space);
		writer.WritePropertyName("ReverseIfOpponent");
		writer.WriteValue(value.ReverseIfOpponent);
		writer.WriteEndObject();
	}

	public override SpaceData ReadJson(JsonReader reader, Type objectType, SpaceData existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new SpaceData();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			if (!(text == "Space"))
			{
				if (!(text == "ReverseIfOpponent"))
				{
					throw new InvalidOperationException("SpaceDataConverter parsing error, unhandled property: " + text);
				}
				existingValue.ReverseIfOpponent = reader.ReadAsBoolean().Value;
			}
			else
			{
				existingValue.Space = (RelativeSpace)Enum.Parse(typeof(RelativeSpace), reader.ReadAsString(), ignoreCase: true);
			}
		}
		return existingValue;
	}
}
