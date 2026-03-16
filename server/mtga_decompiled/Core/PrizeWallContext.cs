using Wizards.MDN;

public class PrizeWallContext
{
	public NavContentType BackContentType;

	public EventContext EventContext;

	public ProgressionTrackPageContext MasteryPassContext;

	public PrizeWallContext(NavContentType backContentType, EventContext eventContext = null, ProgressionTrackPageContext masteryPassContext = null)
	{
		BackContentType = backContentType;
		EventContext = eventContext;
		MasteryPassContext = masteryPassContext;
	}
}
