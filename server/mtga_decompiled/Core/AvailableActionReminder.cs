public class AvailableActionReminder : NPEReminder
{
	public ActionReminderType aaType;

	public AvailableActionReminder(ActionReminderType type, MTGALocalizedString text, float tooltipTime, float sparkyDispatchTime)
		: base(text, tooltipTime, sparkyDispatchTime)
	{
		aaType = type;
	}
}
