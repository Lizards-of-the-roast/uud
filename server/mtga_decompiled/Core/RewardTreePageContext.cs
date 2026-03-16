using Wizards.MDN;

public class RewardTreePageContext
{
	public string TrackName;

	public PostMatchContext PostMatchContext;

	public EventContext EventContext;

	public string PreviousSceneForBI;

	public RewardTreePageContext(string trackName, PostMatchContext postMatchContext, EventContext eventContext, NavContentType previousSceneForBI)
		: this(trackName, postMatchContext, eventContext, previousSceneForBI.ToString())
	{
	}

	public RewardTreePageContext(string trackName, PostMatchContext postMatchContext, EventContext eventContext, string previousSceneForBI)
	{
		TrackName = trackName;
		PostMatchContext = postMatchContext;
		EventContext = eventContext;
		PreviousSceneForBI = previousSceneForBI;
	}
}
