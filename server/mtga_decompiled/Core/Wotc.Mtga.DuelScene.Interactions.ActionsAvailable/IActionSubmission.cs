using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IActionSubmission
{
	void SubmitAction(GreInteraction interaction);

	void SubmitAction(Action action);
}
