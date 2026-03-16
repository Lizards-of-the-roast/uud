using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class EvaluatorBase_IntToIntConverter : JsonConverter
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
		EvaluatorBase_IntToInt evaluatorBase_IntToInt = (existingValue as EvaluatorBase_IntToInt) ?? ((EvaluatorBase_IntToInt)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "ExpectedResult":
				evaluatorBase_IntToInt.ExpectedResult = reader.ReadAsBoolean().Value;
				continue;
			case "Operation":
				evaluatorBase_IntToInt.Operation = (IntToIntOperationType)Enum.Parse(typeof(IntToIntOperationType), reader.ReadAsString());
				continue;
			case "ValueOneModifier":
				evaluatorBase_IntToInt.ValueOneModifier = reader.ReadAsInt32().Value;
				continue;
			case "ValueTwoModifier":
				evaluatorBase_IntToInt.ValueTwoModifier = reader.ReadAsInt32().Value;
				continue;
			}
			FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				reader.Read();
				field.SetValue(evaluatorBase_IntToInt, serializer.Deserialize(reader, field.FieldType));
				continue;
			}
			throw new InvalidOperationException("EvaluatorBase_IntToIntConverter parsing error, unhandled property: " + text);
		}
		return evaluatorBase_IntToInt;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.BaseType == typeof(EvaluatorBase_IntToInt);
	}
}
