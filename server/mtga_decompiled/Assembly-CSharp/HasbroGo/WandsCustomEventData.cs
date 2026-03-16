using HasbroGo.Wands;
using Newtonsoft.Json;

namespace HasbroGo;

public class WandsCustomEventData : EventData
{
	[JsonProperty(PropertyName = "custom_double_field")]
	public double CustomDoubleField { get; set; }

	[JsonProperty(PropertyName = "custom_string_field")]
	public string CustomStringField { get; set; } = string.Empty;

	public override string GetEventName()
	{
		return "wands_custom_event_data";
	}
}
