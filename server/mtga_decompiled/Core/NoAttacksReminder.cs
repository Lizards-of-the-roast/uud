public class NoAttacksReminder : NPEReminder
{
	public NoAttacksReminder(MTGALocalizedString text, float tooltipTime, float sparkyDispatchTime)
		: base(text, tooltipTime, sparkyDispatchTime)
	{
	}

	public override void DispatchSparky(NPEController npeController, GameManager gameManager)
	{
		npeController.DesiredSparkySpot = SparkySpots.MinorButton;
	}
}
