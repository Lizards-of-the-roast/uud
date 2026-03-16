using System;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Shared.Code.AutoPlay.TimedReplays;

internal class ProtoMessageConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(IMessage).IsAssignableFrom(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		string json = JsonConvert.SerializeObject(new ExpandoObjectConverter().ReadJson(reader, objectType, existingValue, serializer));
		IMessage message = (IMessage)Activator.CreateInstance(objectType);
		return JsonParser.Default.Parse(json, message.Descriptor);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteRawValue(JsonFormatter.Default.Format((IMessage)value));
	}
}
