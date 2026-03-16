using System;
using AssetLookupTree.Json.Extensions;
using UnityEngine;
using UnityEngine.Scripting;

namespace Newtonsoft.Json.Converters;

[Preserve]
public class Vector4Converter : JsonConverter<Vector4>
{
	public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return reader.ReadAsVector4(existingValue);
	}

	public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
	{
		writer.WriteVector4(value);
	}
}
