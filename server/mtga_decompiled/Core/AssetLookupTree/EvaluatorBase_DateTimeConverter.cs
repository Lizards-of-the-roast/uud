using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class EvaluatorBase_DateTimeConverter : JsonConverter
{
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
		EvaluatorBase_DateTime evaluatorBase_DateTime = (existingValue as EvaluatorBase_DateTime) ?? ((EvaluatorBase_DateTime)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "MinExpectedResult":
				reader.Read();
				evaluatorBase_DateTime.MinExpectedResult = serializer.Deserialize<long>(reader);
				continue;
			case "MaxExpectedResult":
				reader.Read();
				evaluatorBase_DateTime.MaxExpectedResult = serializer.Deserialize<long>(reader);
				continue;
			case "Operation":
				evaluatorBase_DateTime.Operation = (DateTimeOperationType)Enum.Parse(typeof(DateTimeOperationType), reader.ReadAsString());
				continue;
			}
			FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				reader.Read();
				field.SetValue(evaluatorBase_DateTime, serializer.Deserialize(reader, field.FieldType));
				continue;
			}
			throw new InvalidOperationException("EvaluatorBase_DateTimeConverter parsing error, unhandled property: " + text);
		}
		return evaluatorBase_DateTime;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.BaseType == typeof(EvaluatorBase_DateTime);
	}
}
