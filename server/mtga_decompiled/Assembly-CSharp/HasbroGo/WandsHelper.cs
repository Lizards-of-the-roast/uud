using HasbroGoUnity.Wands;

namespace HasbroGo;

public static class WandsHelper
{
	public static void RecordCustomEvent(double inDouble, string inString)
	{
		WizWandsEvents.RecordEvent(new WandsCustomEventData
		{
			CustomDoubleField = inDouble,
			CustomStringField = inString
		});
	}
}
