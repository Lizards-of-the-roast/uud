using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene.Interactions;

public class AdornCardsTranslation : IWorkflowTranslation<StringInputRequest>
{
	private readonly IDatabaseUtilities _databaseUtilities;

	private readonly IBrowserController _browserController;

	public AdornCardsTranslation(IDatabaseUtilities databaseUtilities, IBrowserController browserController)
	{
		_databaseUtilities = databaseUtilities;
		_browserController = browserController;
	}

	public WorkflowBase Translate(StringInputRequest req)
	{
		return new AdornCardWorkflow(req, _browserController, _databaseUtilities);
	}
}
