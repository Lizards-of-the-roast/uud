using System;

[Serializable]
public class AudioEvent
{
	public float Delay;

	public string WwiseEventName = string.Empty;

	public bool PlayOnGlobal;

	public AudioEvent(string eventName = "", float delay = 0f)
	{
		Delay = delay;
		WwiseEventName = eventName;
	}
}
