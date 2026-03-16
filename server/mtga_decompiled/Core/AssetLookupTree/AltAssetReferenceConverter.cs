using System;
using Newtonsoft.Json;
using Wizards.Mtga.Utils;

namespace AssetLookupTree;

public class AltAssetReferenceConverter : JsonConverter
{
	private readonly StringCache _stringCache;

	public AltAssetReferenceConverter(StringCache stringCache)
	{
		_stringCache = stringCache;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		AltAssetReference altAssetReference = (AltAssetReference)value;
		writer.WriteStartObject();
		writer.WritePropertyName("Guid");
		writer.WriteValue(altAssetReference.Guid);
		writer.WritePropertyName("RelativePath");
		writer.WriteValue(altAssetReference.RelativePath);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AltAssetReference altAssetReference = (existingValue as AltAssetReference) ?? ((AltAssetReference)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			if (!(text == "Guid"))
			{
				if (!(text == "RelativePath"))
				{
					throw new InvalidOperationException("SpaceDataConverter parsing error, unhandled property: " + text);
				}
				altAssetReference.RelativePath = _stringCache.Get(reader.ReadAsString());
			}
			else
			{
				altAssetReference.Guid = _stringCache.Get(reader.ReadAsString());
			}
		}
		return altAssetReference;
	}

	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(AltAssetReference)))
		{
			return objectType.BaseType == typeof(AltAssetReference);
		}
		return true;
	}
}
