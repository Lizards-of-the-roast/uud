using System;
using AssetLookupTree.Json.Extensions;
using UnityEngine;
using UnityEngine.Scripting;

namespace Newtonsoft.Json.Converters;

[Preserve]
public class Vector2Converter : JsonConverter<Vector2>
{
	public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return reader.ReadAsVector2(existingValue);
	}

	public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
	{
		writer.WriteVector2(value);
	}
}
