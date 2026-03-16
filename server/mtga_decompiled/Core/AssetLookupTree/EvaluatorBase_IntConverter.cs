using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class EvaluatorBase_IntConverter : JsonConverter
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
		EvaluatorBase_Int evaluatorBase_Int = (existingValue as EvaluatorBase_Int) ?? ((EvaluatorBase_Int)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "ExpectedResult":
				evaluatorBase_Int.ExpectedResult = reader.ReadAsBoolean().Value;
				continue;
			case "MinExpectedResult":
				evaluatorBase_Int.MinExpectedResult = reader.ReadAsInt32().Value;
				continue;
			case "MaxExpectedResult":
				evaluatorBase_Int.MaxExpectedResult = reader.ReadAsInt32().Value;
				continue;
			case "Operation":
				evaluatorBase_Int.Operation = (IntOperationType)Enum.Parse(typeof(IntOperationType), reader.ReadAsString());
				continue;
			}
			FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				reader.Read();
				field.SetValue(evaluatorBase_Int, serializer.Deserialize(reader, field.FieldType));
				continue;
			}
			throw new InvalidOperationException("EvaluatorBase_IntConverter parsing error, unhandled property: " + text);
		}
		return evaluatorBase_Int;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.BaseType == typeof(EvaluatorBase_Int);
	}
}
