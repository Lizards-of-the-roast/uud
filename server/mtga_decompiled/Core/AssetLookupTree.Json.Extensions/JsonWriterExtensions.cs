using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree.Json.Extensions;

public static class JsonWriterExtensions
{
	public static void WriteVector2(this JsonWriter writer, Vector2 data)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(data.x);
		writer.WritePropertyName("y");
		writer.WriteValue(data.y);
		writer.WriteEndObject();
	}

	public static void WriteVector3(this JsonWriter writer, Vector3 data)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(data.x);
		writer.WritePropertyName("y");
		writer.WriteValue(data.y);
		writer.WritePropertyName("z");
		writer.WriteValue(data.z);
		writer.WriteEndObject();
	}

	public static void WriteVector4(this JsonWriter writer, Vector4 data)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(data.x);
		writer.WritePropertyName("y");
		writer.WriteValue(data.y);
		writer.WritePropertyName("z");
		writer.WriteValue(data.z);
		writer.WritePropertyName("w");
		writer.WriteValue(data.w);
		writer.WriteEndObject();
	}
}
