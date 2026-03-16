using AssetLookupTree;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class WorkflowTranslator_Replay : IWorkflowTranslator
{
	private readonly IWorkflowTranslation<IntermissionRequest> _intermissionTranslation;

	public WorkflowTranslator_Replay(IContext context, AssetLookupSystem assetLookupSystem)
	{
		_intermissionTranslation = new IntermissionTranslation(context, assetLookupSystem);
	}

	public WorkflowBase Translate(BaseUserRequest req)
	{
		if (!(req is IntermissionRequest req2))
		{
			return new ReplayWorkflow(req);
		}
		return _intermissionTranslation.Translate(req2);
	}
}
