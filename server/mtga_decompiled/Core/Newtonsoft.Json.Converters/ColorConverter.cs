using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Newtonsoft.Json.Converters;

[Preserve]
public class ColorConverter : JsonConverter<Color>
{
	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
	{
		Formatting formatting = writer.Formatting;
		writer.Formatting = Formatting.None;
		writer.WriteStartArray();
		writer.WriteValue(value.r);
		writer.WriteValue(value.g);
		writer.WriteValue(value.b);
		writer.WriteValue(value.a);
		writer.WriteEndArray();
		writer.Formatting = formatting;
	}

	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		float[] array = new float[4];
		if (reader.TokenType == JsonToken.StartArray)
		{
			int num = 0;
			while (reader.TokenType != JsonToken.EndArray && num < 4)
			{
				double? num2 = reader.ReadAsDouble();
				array[num] = (num2.HasValue ? ((float)num2.Value) : 0f);
				num++;
			}
			while (reader.TokenType != JsonToken.EndArray)
			{
				reader.Read();
			}
		}
		return new Color(array[0], array[1], array[2], array[3]);
	}
}
