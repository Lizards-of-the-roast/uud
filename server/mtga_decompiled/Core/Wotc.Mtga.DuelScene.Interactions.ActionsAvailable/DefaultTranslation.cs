using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class DefaultTranslation : IWorkflowTranslation<ActionsAvailableRequest>
{
	private readonly GameManager _gameManager;

	private readonly IContext _context;

	public DefaultTranslation(GameManager gameManager, IContext context)
	{
		_gameManager = gameManager;
		_context = context ?? NullContext.Default;
	}

	public WorkflowBase Translate(ActionsAvailableRequest req)
	{
		return new ActionsAvailableWorkflow(req, _gameManager, _context);
	}
}
