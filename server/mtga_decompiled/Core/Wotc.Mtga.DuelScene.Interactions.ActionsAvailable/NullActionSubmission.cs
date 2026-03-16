using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class NullActionSubmission : IActionSubmission
{
	public static readonly IActionSubmission Default = new NullActionSubmission();

	public void SubmitAction(GreInteraction interaction)
	{
	}

	public void SubmitAction(Action action)
	{
	}
}
