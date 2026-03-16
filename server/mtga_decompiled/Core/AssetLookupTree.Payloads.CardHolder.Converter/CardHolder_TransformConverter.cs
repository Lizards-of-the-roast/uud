using System;
using AssetLookupTree.Json.Extensions;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace AssetLookupTree.Payloads.CardHolder.Converter;

[Preserve]
public class CardHolder_TransformConverter : JsonConverter<CardHolder_Transform>
{
	public override CardHolder_Transform ReadJson(JsonReader reader, Type objectType, CardHolder_Transform existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new CardHolder_Transform();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			if (!(reader.Value is string text) || !(text == "OffsetData"))
			{
				continue;
			}
			while (reader.Read() && reader.TokenType != JsonToken.EndObject)
			{
				switch (reader.Value as string)
				{
				case "PositionOffset":
					reader.Read();
					existingValue.OffsetData.PositionOffset = reader.ReadAsVector3();
					break;
				case "RotationOffset":
					reader.Read();
					existingValue.OffsetData.RotationOffset = reader.ReadAsVector3();
					break;
				case "RotationIsWorld":
					existingValue.OffsetData.RotationIsWorld = reader.ReadAsBoolean().Value;
					break;
				case "ScaleMultiplier":
					reader.Read();
					existingValue.OffsetData.ScaleMultiplier = reader.ReadAsVector3();
					break;
				case "ScaleIsWorld":
					existingValue.OffsetData.ScaleIsWorld = reader.ReadAsBoolean().Value;
					break;
				}
			}
		}
		return existingValue;
	}

	public override void WriteJson(JsonWriter writer, CardHolder_Transform value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("OffsetData");
		writer.WriteStartObject();
		writer.WritePropertyName("PositionOffset");
		writer.WriteVector3(value.OffsetData.PositionOffset);
		writer.WritePropertyName("RotationOffset");
		writer.WriteVector3(value.OffsetData.RotationOffset);
		writer.WritePropertyName("RotationIsWorld");
		writer.WriteValue(value.OffsetData.RotationIsWorld);
		writer.WritePropertyName("ScaleMultiplier");
		writer.WriteVector3(value.OffsetData.ScaleMultiplier);
		writer.WritePropertyName("ScaleIsWorld");
		writer.WriteValue(value.OffsetData.ScaleIsWorld);
		writer.WriteEndObject();
		writer.WriteEndObject();
	}
}
