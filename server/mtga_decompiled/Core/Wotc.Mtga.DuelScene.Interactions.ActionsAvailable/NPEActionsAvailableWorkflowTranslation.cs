using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class NPEActionsAvailableWorkflowTranslation : IWorkflowTranslation<ActionsAvailableRequest>
{
	private readonly GameManager _gameManager;

	private readonly IContext _context;

	private readonly NPEDirector _director;

	public NPEActionsAvailableWorkflowTranslation(GameManager gameManager, IContext context, NPEDirector director)
	{
		_gameManager = gameManager;
		_context = context ?? NullContext.Default;
		_director = director;
	}

	public WorkflowBase Translate(ActionsAvailableRequest req)
	{
		return new ActionsAvailableWorkflow_NPE(req, _gameManager, _context, _director);
	}
}
