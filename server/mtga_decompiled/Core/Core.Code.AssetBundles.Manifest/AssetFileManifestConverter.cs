using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

public class AssetFileManifestConverter : JsonConverter<AssetFileManifest>
{
	public override AssetFileManifest ReadJson(JsonReader reader, Type objectType, AssetFileManifest? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		List<AssetFileInfo> list = new List<AssetFileInfo>(1000);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			switch (reader.Value as string)
			{
			case "FormatVersion":
			case "EncryptionKey":
				reader.Read();
				reader.Skip();
				break;
			case "Assets":
				reader.Read();
				reader.Read();
				while (reader.TokenType != JsonToken.EndArray)
				{
					AssetFileInfo assetFileInfo = serializer.Deserialize<AssetFileInfo>(reader);
					if (assetFileInfo != null)
					{
						list.Add(assetFileInfo);
					}
				}
				break;
			}
		}
		reader.Read();
		return new AssetFileManifest(AssetPriority.Automatic, list);
	}

	public override void WriteJson(JsonWriter writer, AssetFileManifest? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		writer.WriteStartObject();
		writer.WritePropertyName("Assets");
		writer.WriteStartArray();
		foreach (AssetFileInfo value2 in value.AssetFileInfoByName.Values)
		{
			serializer.Serialize(writer, value2);
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
