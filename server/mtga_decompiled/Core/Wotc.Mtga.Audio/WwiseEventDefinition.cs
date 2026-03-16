namespace Wotc.Mtga.Audio;

public readonly struct WwiseEventDefinition
{
	public readonly string GUID;

	public readonly string EventName;

	public readonly string PckName;

	public WwiseEventDefinition(string guid, string eventName, string pckName)
	{
		GUID = guid;
		EventName = eventName;
		PckName = pckName;
	}
}
