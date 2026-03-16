using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using AssetLookupTree.Evaluators.Utility;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class Utility_CompoundConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Utility_Compound utility_Compound = (Utility_Compound)value;
		writer.WriteStartObject();
		if (serializer.DefaultValueHandling == DefaultValueHandling.Include || utility_Compound.ExpectedResult)
		{
			writer.WritePropertyName("ExpectedResult");
			writer.WriteValue(utility_Compound.ExpectedResult);
		}
		writer.WritePropertyName("NestedEvaluators");
		writer.WriteStartArray();
		foreach (IEvaluator nestedEvaluator in utility_Compound.NestedEvaluators)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Type");
			writer.WriteValue(nestedEvaluator.GetType().FullName);
			writer.WritePropertyName("Value");
			serializer.Serialize(writer, nestedEvaluator, nestedEvaluator.GetType());
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Utility_Compound utility_Compound = (existingValue as Utility_Compound) ?? ((Utility_Compound)objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			if (!(text == "ExpectedResult"))
			{
				if (text == "NestedEvaluators")
				{
					Assembly assembly = Assembly.GetAssembly(typeof(AssetLookupTree<>));
					utility_Compound.NestedEvaluators.Clear();
					reader.Read();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						reader.Read();
						Type type = assembly.GetType(reader.ReadAsString());
						reader.Read();
						reader.Read();
						IEvaluator item = (IEvaluator)serializer.Deserialize(reader, type);
						reader.Read();
						utility_Compound.NestedEvaluators.Add(item);
					}
					utility_Compound.NestedEvaluators.TrimExcess();
				}
				else
				{
					FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field != null))
					{
						throw new InvalidOperationException("Utility_CompoundConverter parsing error, unhandled property: " + text);
					}
					reader.Read();
					field.SetValue(utility_Compound, serializer.Deserialize(reader, field.FieldType));
				}
			}
			else
			{
				utility_Compound.ExpectedResult = reader.ReadAsBoolean().Value;
			}
		}
		return utility_Compound;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.BaseType == typeof(Utility_Compound);
	}
}
