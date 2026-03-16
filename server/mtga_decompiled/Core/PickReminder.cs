using System.Collections.Generic;

public class PickReminder : NPEReminder
{
	public List<uint> KeyCreatures = new List<uint>();

	public PickReminder(MTGALocalizedString text, float tooltipTime, float sparkyDispatchTime, params uint[] choices)
		: base(text, tooltipTime, sparkyDispatchTime)
	{
		foreach (uint item in choices)
		{
			KeyCreatures.Add(item);
		}
	}
}
