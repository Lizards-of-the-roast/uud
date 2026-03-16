using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Wizards.Mtga.Utils;

namespace Wizards.Mtga.Assets;

public class AssetFileInfoConverter : JsonConverter<AssetFileInfo>
{
	private readonly StringCache _stringCache;

	private readonly List<string> _dependencyCache = new List<string>(100);

	private readonly List<string> _indexedAssetsCache = new List<string>(100);

	public AssetFileInfoConverter()
		: this(new StringCache())
	{
	}

	public AssetFileInfoConverter(StringCache stringCache)
	{
		_stringCache = stringCache;
	}

	public override AssetFileInfo ReadJson(JsonReader reader, Type objectType, AssetFileInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		_dependencyCache.Clear();
		_indexedAssetsCache.Clear();
		string name = null;
		string oldName = null;
		long length = 0L;
		long compressedLength = 0L;
		AssetFileWrapperType wrapperType = AssetFileWrapperType.None;
		AssetPriority priority = AssetPriority.General;
		byte[] sha256Hash = null;
		uint? crc = null;
		string assetType = "AssetBundle";
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			switch (reader.Value as string)
			{
			case "Name":
				name = _stringCache.Get(reader.ReadAsString());
				break;
			case "AssetType":
				assetType = string.Intern(reader.ReadAsString());
				break;
			case "Length":
				length = Convert.ToInt64(reader.ReadAsDecimal().Value);
				break;
			case "CompressedLength":
				compressedLength = Convert.ToInt64(reader.ReadAsDecimal().GetValueOrDefault());
				break;
			case "wrapper":
			{
				string text2 = reader.ReadAsString();
				if (text2 == "gz")
				{
					AssetFileWrapperType assetFileWrapperType = AssetFileWrapperType.Gzip;
					wrapperType = assetFileWrapperType;
					break;
				}
				throw new FormatException("Unknown asset wrapper type: " + text2);
			}
			case "Priority":
			{
				priority = (Enum.TryParse<AssetPriority>(reader.ReadAsString(), out var result2) ? result2 : AssetPriority.General);
				break;
			}
			case "sha256":
				sha256Hash = reader.ReadAsBytes();
				break;
			case "crc":
			{
				string text = reader.ReadAsString();
				if (!uint.TryParse(text, NumberStyles.HexNumber, null, out var result))
				{
					throw new FormatException("'" + text + "' is not a valid hexadecimal number.");
				}
				crc = result;
				break;
			}
			case "IndexedAssets":
				reader.Read();
				ReadAsInternedList(reader, _indexedAssetsCache);
				break;
			case "Dependencies":
				reader.Read();
				ReadAsInternedList(reader, _dependencyCache);
				break;
			case "OldName":
				oldName = _stringCache.Get(reader.ReadAsString());
				break;
			default:
				reader.Read();
				serializer.Deserialize(reader);
				break;
			}
		}
		reader.Read();
		return new AssetFileInfo(name, length, priority, _indexedAssetsCache.ToArray(), _dependencyCache.ToArray(), assetType, wrapperType, compressedLength, sha256Hash, crc, oldName);
	}

	private void ReadAsInternedList(JsonReader reader, List<string> list)
	{
		reader.Read();
		while (reader.TokenType != JsonToken.EndArray)
		{
			list.Add(_stringCache.Get((string)reader.Value));
			reader.Read();
		}
	}

	public override void WriteJson(JsonWriter writer, AssetFileInfo value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Name");
		writer.WriteValue(value.Name);
		if (value.AssetType != "AssetBundle")
		{
			writer.WritePropertyName("AssetType");
			writer.WriteValue(value.AssetType);
		}
		writer.WritePropertyName("Length");
		writer.WriteValue(value.Length);
		if (value.CompressedLength > 0)
		{
			writer.WritePropertyName("CompressedLength");
			writer.WriteValue(value.CompressedLength);
		}
		if (value.WrapperType != AssetFileWrapperType.None)
		{
			writer.WritePropertyName("wrapper");
			if (value.WrapperType != AssetFileWrapperType.Gzip)
			{
				throw new ArgumentException($"Unknown wrapper type: {value.WrapperType}");
			}
			string value2 = "gz";
			writer.WriteValue(value2);
		}
		writer.WritePropertyName("Priority");
		writer.WriteValue(value.Priority);
		if (value.Sha256Hash != null)
		{
			writer.WritePropertyName("sha256");
			serializer.Serialize(writer, value.Sha256Hash, typeof(byte[]));
		}
		if (value.Crc32.HasValue)
		{
			writer.WritePropertyName("crc");
			serializer.Serialize(writer, value.Crc32.Value.ToString("x8"));
		}
		writer.WritePropertyName("IndexedAssets");
		serializer.Serialize(writer, value.IndexedAssets);
		string[] dependencies = value.Dependencies;
		if (dependencies != null && dependencies.Any())
		{
			writer.WritePropertyName("Dependencies");
			serializer.Serialize(writer, value.Dependencies);
		}
		writer.WriteEndObject();
	}
}
