using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;

namespace AssetLookupTree;

public class EvaluatorBase_ListConverter : JsonConverter
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
		Type type = objectType.BaseType.GenericTypeArguments[0];
		object obj = existingValue ?? objectType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "ExpectedValues":
			{
				object value = objectType.GetField("ExpectedValues").GetValue(obj);
				MethodInfo method = value.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
				if (type.IsEnum)
				{
					reader.Read();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						if (reader.ValueType == typeof(string))
						{
							method.Invoke(value, new object[1] { Enum.Parse(type, (string)reader.Value) });
							continue;
						}
						if (reader.ValueType.IsPrimitive)
						{
							method.Invoke(value, new object[1] { Enum.ToObject(type, Convert.ToInt32(reader.Value)) });
							continue;
						}
						throw new InvalidOperationException("EvaluatorBase_ListConverter parsing error, unhandled enum type: " + type.Name);
					}
				}
				else
				{
					reader.Read();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						method.Invoke(value, new object[1] { serializer.Deserialize(reader, type) });
					}
				}
				break;
			}
			case "Operation":
				objectType.GetField("Operation").SetValue(obj, (SetOperationType)Enum.Parse(typeof(SetOperationType), reader.ReadAsString(), ignoreCase: true));
				break;
			case "MinCount":
				objectType.GetField("MinCount").SetValue(obj, reader.ReadAsInt32().Value);
				break;
			case "ExpectedResult":
				objectType.GetField("ExpectedResult").SetValue(obj, reader.ReadAsBoolean().Value);
				break;
			default:
			{
				FieldInfo field = objectType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					reader.Read();
					field.SetValue(obj, serializer.Deserialize(reader, field.FieldType));
					break;
				}
				throw new InvalidOperationException("EvaluatorBase_ListConverter parsing error, unhandled property: " + text);
			}
			}
		}
		return obj;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType?.BaseType?.GetGenericTypeDefinition() == typeof(EvaluatorBase_List<>);
	}
}
