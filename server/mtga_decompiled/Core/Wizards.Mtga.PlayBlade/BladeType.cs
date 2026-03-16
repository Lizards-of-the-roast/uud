using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Wizards.Mtga.PlayBlade;

[JsonConverter(typeof(StringEnumConverter))]
public enum BladeType
{
	Invalid = -1,
	Event,
	FindMatch,
	LastPlayed
}
