using System;
using AssetLookupTree.Json.Extensions;
using UnityEngine;
using UnityEngine.Scripting;

namespace Newtonsoft.Json.Converters;

[Preserve]
public class Vector3Converter : JsonConverter<Vector3>
{
	public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return reader.ReadAsVector3(existingValue);
	}

	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		writer.WriteVector3(value);
	}
}
