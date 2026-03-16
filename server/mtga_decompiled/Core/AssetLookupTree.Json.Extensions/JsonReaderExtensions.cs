using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree.Json.Extensions;

public static class JsonReaderExtensions
{
	public static Vector2 ReadAsVector2(this JsonReader reader, Vector2 existingValue = default(Vector2))
	{
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			switch (reader.Value as string)
			{
			case "x":
				existingValue.x = (float)reader.ReadAsDouble().Value;
				continue;
			case "y":
				existingValue.y = (float)reader.ReadAsDouble().Value;
				continue;
			}
			reader.Read();
			if (reader.TokenType == JsonToken.StartObject)
			{
				reader.ConsumeUnknownObject();
			}
			else if (reader.TokenType == JsonToken.StartArray)
			{
				reader.ConsumeUnknownArray();
			}
		}
		return existingValue;
	}

	public static Vector3 ReadAsVector3(this JsonReader reader, Vector3 existingValue = default(Vector3))
	{
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			switch (reader.Value as string)
			{
			case "x":
				existingValue.x = (float)reader.ReadAsDouble().Value;
				continue;
			case "y":
				existingValue.y = (float)reader.ReadAsDouble().Value;
				continue;
			case "z":
				existingValue.z = (float)reader.ReadAsDouble().Value;
				continue;
			}
			reader.Read();
			if (reader.TokenType == JsonToken.StartObject)
			{
				reader.ConsumeUnknownObject();
			}
			else if (reader.TokenType == JsonToken.StartArray)
			{
				reader.ConsumeUnknownArray();
			}
		}
		return existingValue;
	}

	public static Vector4 ReadAsVector4(this JsonReader reader, Vector4 existingValue = default(Vector4))
	{
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			switch (reader.Value as string)
			{
			case "x":
				existingValue.x = (float)reader.ReadAsDouble().Value;
				continue;
			case "y":
				existingValue.y = (float)reader.ReadAsDouble().Value;
				continue;
			case "z":
				existingValue.z = (float)reader.ReadAsDouble().Value;
				continue;
			case "w":
				existingValue.w = (float)reader.ReadAsDouble().Value;
				continue;
			}
			reader.Read();
			if (reader.TokenType == JsonToken.StartObject)
			{
				reader.ConsumeUnknownObject();
			}
			else if (reader.TokenType == JsonToken.StartArray)
			{
				reader.ConsumeUnknownArray();
			}
		}
		return existingValue;
	}

	public static void ConsumeUnknownObject(this JsonReader reader)
	{
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			if (reader.TokenType == JsonToken.StartObject)
			{
				reader.ConsumeUnknownObject();
			}
		}
	}

	public static void ConsumeUnknownArray(this JsonReader reader)
	{
		while (reader.Read() && reader.TokenType != JsonToken.EndArray)
		{
			if (reader.TokenType == JsonToken.StartArray)
			{
				reader.ConsumeUnknownArray();
			}
		}
	}
}
