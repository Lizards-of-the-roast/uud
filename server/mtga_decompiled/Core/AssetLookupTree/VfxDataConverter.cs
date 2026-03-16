using System;
using System.Reflection;
using AssetLookupTree.Evaluators;
using Newtonsoft.Json;
using Wizards.Mtga.Utils;

namespace AssetLookupTree;

public class VfxDataConverter : JsonConverter<VfxData>
{
	private readonly StringCache _stringCache;

	public VfxDataConverter(StringCache stringCache)
	{
		_stringCache = stringCache;
	}

	public override void WriteJson(JsonWriter writer, VfxData value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("ActivationType");
		writer.WriteValue((int)value.ActivationType);
		if (value.LoopingKey != null)
		{
			writer.WritePropertyName("LoopingKey");
			writer.WriteValue(value.LoopingKey);
		}
		writer.WritePropertyName("CanSurviveZoneTransfer");
		writer.WriteValue(value.CanSurviveZoneTransfer);
		writer.WritePropertyName("SpaceData");
		serializer.Serialize(writer, value.SpaceData);
		writer.WritePropertyName("ParentToSpace");
		writer.WriteValue(value.ParentToSpace);
		writer.WritePropertyName("IgnoreDedupe");
		writer.WriteValue(value.IgnoreDedupe);
		writer.WritePropertyName("TurnWideDataOptions");
		writer.WriteValue((int)value.TurnWideDataOptions);
		writer.WritePropertyName("PrefabData");
		serializer.Serialize(writer, value.PrefabData);
		writer.WritePropertyName("Offset");
		serializer.Serialize(writer, value.Offset);
		writer.WritePropertyName("PlayOnAttachmentStack");
		writer.WriteValue(value.PlayOnAttachmentStack);
		writer.WritePropertyName("PlayOnStackChildren");
		writer.WriteValue(value.PlayOnStackChildren);
		writer.WritePropertyName("HideIfNotTopOfStack");
		writer.WriteValue(value.HideIfNotTopOfStack);
		writer.WritePropertyName("AddParentZPositionToOffset");
		writer.WriteValue(value.AddParentZPositionToOffset);
		if (value.DelayData != null)
		{
			writer.WritePropertyName("DelayData");
			WriteDelay(value.DelayData, writer, serializer);
		}
		writer.WriteEndObject();
	}

	private void WriteDelay(VfxDelayData delay, JsonWriter writer, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Time");
		writer.WriteValue(delay.Time);
		writer.WritePropertyName("ConditionType");
		writer.WriteValue(delay.Condition?.GetType().FullName);
		writer.WritePropertyName("Condition");
		serializer.Serialize(writer, delay.Condition);
		writer.WriteEndObject();
	}

	public override VfxData ReadJson(JsonReader reader, Type objectType, VfxData existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (existingValue == null)
		{
			existingValue = new VfxData();
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			switch (text)
			{
			case "ActivationType":
				existingValue.ActivationType = (VfxActivationType)Enum.Parse(typeof(VfxActivationType), reader.ReadAsString());
				break;
			case "LoopingKey":
				existingValue.LoopingKey = _stringCache.Get(reader.ReadAsString());
				break;
			case "CanSurviveZoneTransfer":
				existingValue.CanSurviveZoneTransfer = reader.ReadAsBoolean().Value;
				break;
			case "SpaceData":
				reader.Read();
				existingValue.SpaceData = serializer.Deserialize<SpaceData>(reader);
				break;
			case "ParentToSpace":
				existingValue.ParentToSpace = reader.ReadAsBoolean().Value;
				break;
			case "IgnoreDedupe":
				existingValue.IgnoreDedupe = reader.ReadAsBoolean().Value;
				break;
			case "TurnWideDataOptions":
				existingValue.TurnWideDataOptions = (TurnWideDataOptions)Enum.Parse(typeof(TurnWideDataOptions), reader.ReadAsString());
				break;
			case "PrefabData":
				reader.Read();
				existingValue.PrefabData = serializer.Deserialize<VfxPrefabData>(reader);
				break;
			case "Offset":
				reader.Read();
				existingValue.Offset = serializer.Deserialize<OffsetData>(reader);
				break;
			case "PlayOnAttachmentStack":
				existingValue.PlayOnAttachmentStack = reader.ReadAsBoolean().Value;
				break;
			case "PlayOnStackChildren":
				existingValue.PlayOnStackChildren = reader.ReadAsBoolean().Value;
				break;
			case "HideIfNotTopOfStack":
				existingValue.HideIfNotTopOfStack = reader.ReadAsBoolean().Value;
				break;
			case "DelayData":
				existingValue.DelayData = DeserializeDelay(reader, serializer);
				break;
			case "AddParentZPositionToOffset":
				existingValue.AddParentZPositionToOffset = reader.ReadAsBoolean().Value;
				break;
			default:
				throw new InvalidOperationException("OffsetDataConverter parsing error, unhandled property: " + text);
			}
		}
		return existingValue;
	}

	private VfxDelayData DeserializeDelay(JsonReader reader, JsonSerializer serializer)
	{
		Assembly assembly = Assembly.GetAssembly(typeof(AssetLookupTree<>));
		VfxDelayData vfxDelayData = new VfxDelayData();
		string text = null;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			switch (reader.Value as string)
			{
			case "Time":
				vfxDelayData.Time = (float)reader.ReadAsDouble().Value;
				break;
			case "ConditionType":
				text = reader.ReadAsString();
				break;
			case "Condition":
				if (text != null)
				{
					reader.Read();
					vfxDelayData.Condition = (IEvaluator)serializer.Deserialize(reader, assembly.GetType(text));
				}
				break;
			}
		}
		return vfxDelayData;
	}
}
