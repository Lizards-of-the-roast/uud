using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;
using Wizards.Mtga.Utils;

namespace AssetLookupTree;

public class EvaluatorBase_StringConverter : JsonConverter
{
	private readonly StringCache _stringCache;

	public EvaluatorBase_StringConverter(StringCache stringCache)
	{
		_stringCache = stringCache;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			writer.WritePropertyName(fieldInfo.Name);
			serializer.Serialize(writer, fieldInfo.GetValue(value), fieldInfo.FieldType);
		}
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EvaluatorBase_String evaluatorBase_String = (existingValue as EvaluatorBase_String) ?? ((EvaluatorBase_String)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "ExpectedValue":
				evaluatorBase_String.ExpectedValue = _stringCache.Get(reader.ReadAsString());
				continue;
			case "Operation":
				evaluatorBase_String.Operation = (StringOperationType)Enum.Parse(typeof(StringOperationType), reader.ReadAsString(), ignoreCase: true);
				continue;
			case "ExpectedResult":
				evaluatorBase_String.ExpectedResult = reader.ReadAsBoolean().Value;
				continue;
			}
			FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				reader.Read();
				field.SetValue(evaluatorBase_String, serializer.Deserialize(reader, field.FieldType));
				continue;
			}
			throw new InvalidOperationException("EvaluatorBase_StringConverter parsing error, unhandled property: " + text);
		}
		return evaluatorBase_String;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.BaseType == typeof(EvaluatorBase_String);
	}
}
