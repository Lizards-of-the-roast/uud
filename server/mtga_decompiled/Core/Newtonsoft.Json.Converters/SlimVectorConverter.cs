using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Newtonsoft.Json.Converters;

[Preserve]
public class SlimVectorConverter : JsonConverter
{
	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(Vector2)) && !(objectType == typeof(Vector3)))
		{
			return objectType == typeof(Vector4);
		}
		return true;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Formatting formatting = writer.Formatting;
		writer.Formatting = Formatting.None;
		writer.WriteStartArray();
		if (!(value is Vector2 vector))
		{
			if (!(value is Vector3 vector2))
			{
				if (value is Vector4 vector3)
				{
					writer.WriteValue(vector3.x);
					writer.WriteValue(vector3.y);
					writer.WriteValue(vector3.z);
					writer.WriteValue(vector3.w);
				}
			}
			else
			{
				writer.WriteValue(vector2.x);
				writer.WriteValue(vector2.y);
				writer.WriteValue(vector2.z);
			}
		}
		else
		{
			writer.WriteValue(vector.x);
			writer.WriteValue(vector.y);
		}
		writer.WriteEndArray();
		writer.Formatting = formatting;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
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
			if (objectType == typeof(Vector4))
			{
				return new Vector4(array[0], array[1], array[2], array[3]);
			}
			if (objectType == typeof(Vector3))
			{
				return new Vector3(array[0], array[1], array[2]);
			}
			if (objectType == typeof(Vector2))
			{
				return new Vector2(array[0], array[1]);
			}
			return null;
		}
		return serializer.Deserialize<object>(reader);
	}
}
