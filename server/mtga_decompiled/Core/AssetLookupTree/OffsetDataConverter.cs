using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree;

public class OffsetDataConverter : JsonConverter<OffsetData>
{
	public override void WriteJson(JsonWriter writer, OffsetData value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("PositionOffset");
		serializer.Serialize(writer, value.PositionOffset);
		writer.WritePropertyName("RotationOffset");
		serializer.Serialize(writer, value.RotationOffset);
		writer.WritePropertyName("RotationIsWorld");
		writer.WriteValue(value.RotationIsWorld);
		writer.WritePropertyName("ScaleMultiplier");
		serializer.Serialize(writer, value.ScaleMultiplier);
		writer.WritePropertyName("ScaleIsWorld");
		writer.WriteValue(value.ScaleIsWorld);
		writer.WriteEndObject();
	}

	public override OffsetData ReadJson(JsonReader reader, Type objectType, OffsetData existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new OffsetData();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "PositionOffset":
				reader.Read();
				existingValue.PositionOffset = serializer.Deserialize<Vector3>(reader);
				break;
			case "RotationOffset":
				reader.Read();
				existingValue.RotationOffset = serializer.Deserialize<Vector3>(reader);
				break;
			case "RotationIsWorld":
				existingValue.RotationIsWorld = reader.ReadAsBoolean().Value;
				break;
			case "ScaleMultiplier":
				reader.Read();
				existingValue.ScaleMultiplier = serializer.Deserialize<Vector3>(reader);
				break;
			case "ScaleIsWorld":
				existingValue.ScaleIsWorld = reader.ReadAsBoolean().Value;
				break;
			default:
				throw new InvalidOperationException("OffsetDataConverter parsing error, unhandled property: " + text);
			}
		}
		return existingValue;
	}
}
