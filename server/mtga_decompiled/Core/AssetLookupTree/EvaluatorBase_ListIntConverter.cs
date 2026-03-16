using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class EvaluatorBase_ListIntConverter : JsonConverter
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
		EvaluatorBase_List<int> evaluatorBase_List = (existingValue as EvaluatorBase_List<int>) ?? ((EvaluatorBase_List<int>)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "ExpectedValues":
				evaluatorBase_List.ExpectedValues.Clear();
				reader.Read();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					evaluatorBase_List.ExpectedValues.Add(Convert.ToInt32(reader.Value));
				}
				continue;
			case "Operation":
				evaluatorBase_List.Operation = (SetOperationType)Enum.Parse(typeof(SetOperationType), reader.ReadAsString());
				continue;
			case "MinCount":
				evaluatorBase_List.MinCount = reader.ReadAsInt32().Value;
				continue;
			case "ExpectedResult":
				evaluatorBase_List.ExpectedResult = reader.ReadAsBoolean().Value;
				continue;
			}
			FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				reader.Read();
				field.SetValue(evaluatorBase_List, serializer.Deserialize(reader, field.FieldType));
				continue;
			}
			throw new InvalidOperationException("EvaluatorBase_ListIntConverter parsing error, unhandled property: " + text);
		}
		return evaluatorBase_List;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.BaseType == typeof(EvaluatorBase_List<int>);
	}
}
